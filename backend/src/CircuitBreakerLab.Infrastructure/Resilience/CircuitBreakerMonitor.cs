using CircuitBreakerLab.Application.Ports;
using CircuitBreakerLab.Domain.CircuitBreaker;
using CircuitBreakerLab.Domain.ExchangeRates;
using CircuitBreakerLab.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CircuitBreakerManualControl = Polly.CircuitBreaker.CircuitBreakerManualControl;
using CircuitBreakerStateProvider = Polly.CircuitBreaker.CircuitBreakerStateProvider;
using PollyCircuitState = Polly.CircuitBreaker.CircuitState;

namespace CircuitBreakerLab.Infrastructure.Resilience;

internal sealed partial class CircuitBreakerMonitor : ICircuitBreakerMonitor
{
    private readonly Lock _sync = new();
    private readonly LinkedList<CircuitEvent> _events = new();
    private readonly int _eventHistorySize;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<CircuitBreakerMonitor> _logger;
    private long _successes;
    private long _failures;
    private long _rejections;
    private long _fallbacksServed;
    private DateTimeOffset? _openUntil;

    public CircuitBreakerMonitor(
        IOptions<CircuitBreakerOptions> circuitOptions,
        IOptions<ExchangeRateProviderOptions> providerOptions,
        TimeProvider timeProvider,
        ILogger<CircuitBreakerMonitor> logger)
    {
        var circuit = circuitOptions.Value;
        _eventHistorySize = circuit.EventHistorySize;
        _timeProvider = timeProvider;
        _logger = logger;
        Policy = new CircuitPolicy(
            circuit.FailureRatio,
            circuit.MinimumThroughput,
            TimeSpan.FromSeconds(circuit.SamplingDurationSeconds),
            TimeSpan.FromSeconds(circuit.BreakDurationSeconds),
            TimeSpan.FromSeconds(providerOptions.Value.TimeoutSeconds));
    }

    public CircuitPolicy Policy { get; }

    public CircuitBreakerStateProvider StateProvider { get; } = new();

    public CircuitBreakerManualControl ManualControl { get; } = new();

    public CircuitState CurrentState => StateProvider.CircuitState switch
    {
        PollyCircuitState.Open => CircuitState.Open,
        PollyCircuitState.HalfOpen => CircuitState.HalfOpen,
        PollyCircuitState.Isolated => CircuitState.Isolated,
        _ => CircuitState.Closed
    };

    public CircuitSnapshot GetSnapshot()
    {
        var metrics = new CircuitMetrics(
            Interlocked.Read(ref _successes),
            Interlocked.Read(ref _failures),
            Interlocked.Read(ref _rejections),
            Interlocked.Read(ref _fallbacksServed));

        lock (_sync)
        {
            var state = CurrentState;
            var openUntil = state == CircuitState.Open ? _openUntil : null;
            return new CircuitSnapshot(state, openUntil, Policy, metrics, [.. _events]);
        }
    }

    public Task ResetAsync(CancellationToken cancellationToken) => ManualControl.CloseAsync(cancellationToken);

    public Task IsolateAsync(CancellationToken cancellationToken) => ManualControl.IsolateAsync(cancellationToken);

    public void RecordSuccess(CurrencyPair pair)
    {
        Interlocked.Increment(ref _successes);
        Append(CircuitEventType.CallSucceeded, $"{pair} answered by the provider.");
    }

    public void RecordFailure(CurrencyPair pair, ProviderFailureReason reason)
    {
        Interlocked.Increment(ref _failures);
        Append(CircuitEventType.CallFailed, $"{pair} failed: {reason}.");
    }

    public void RecordRejection(CurrencyPair pair)
    {
        Interlocked.Increment(ref _rejections);
        Append(CircuitEventType.CallRejected, $"{pair} rejected immediately, the provider was not called.");
    }

    public void RecordFallbackServed(CurrencyPair pair)
    {
        Interlocked.Increment(ref _fallbacksServed);
        Append(CircuitEventType.FallbackServed, $"{pair} served from the last known value.");
    }

    public void OnOpened(TimeSpan breakDuration, bool isManual, string cause)
    {
        if (isManual)
        {
            lock (_sync)
            {
                _openUntil = null;
            }

            LogCircuitIsolated(_logger);
            Append(CircuitEventType.CircuitIsolated, "Circuit manually isolated, every call will be rejected.");
            return;
        }

        lock (_sync)
        {
            _openUntil = _timeProvider.GetUtcNow().Add(breakDuration);
        }

        LogCircuitOpened(_logger, breakDuration.TotalSeconds, cause);
        Append(
            CircuitEventType.CircuitOpened,
            $"Failure threshold reached ({cause}). Calls blocked for {breakDuration.TotalSeconds}s.");
    }

    public void OnHalfOpened()
    {
        LogCircuitHalfOpened(_logger);
        Append(CircuitEventType.CircuitHalfOpened, "Break elapsed, a trial call is allowed to test the provider.");
    }

    public void OnClosed(bool isManual)
    {
        lock (_sync)
        {
            _openUntil = null;
        }

        LogCircuitClosed(_logger, isManual);
        Append(
            CircuitEventType.CircuitClosed,
            isManual ? "Circuit manually reset." : "Trial call succeeded, the provider is healthy again.");
    }

    private void Append(CircuitEventType type, string detail)
    {
        var circuitEvent = new CircuitEvent(type, CurrentState, detail, _timeProvider.GetUtcNow());

        lock (_sync)
        {
            _events.AddFirst(circuitEvent);

            while (_events.Count > _eventHistorySize)
            {
                _events.RemoveLast();
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Circuit OPENED for {BreakSeconds}s. Cause: {Cause}")]
    private static partial void LogCircuitOpened(ILogger logger, double breakSeconds, string cause);

    [LoggerMessage(Level = LogLevel.Information, Message = "Circuit HALF-OPEN, next call is a trial")]
    private static partial void LogCircuitHalfOpened(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Circuit CLOSED. Manual: {IsManual}")]
    private static partial void LogCircuitClosed(ILogger logger, bool isManual);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Circuit ISOLATED manually")]
    private static partial void LogCircuitIsolated(ILogger logger);
}
