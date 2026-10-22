using Microsoft.Extensions.Options;

namespace ProviderSimulator.Behavior;

internal sealed class BehaviorState(IOptions<SimulatorOptions> options)
{
    private readonly Lock _sync = new();
    private long _requestsReceived;
    private ProviderBehavior _current = new(
        options.Value.DefaultMode,
        options.Value.DefaultFailureRate,
        options.Value.DefaultLatencyMs);

    public ProviderBehavior Current
    {
        get
        {
            lock (_sync)
            {
                return _current;
            }
        }
    }

    public long RegisterRequest() => Interlocked.Increment(ref _requestsReceived);

    public BehaviorStatus Status()
    {
        var behavior = Current;
        return new BehaviorStatus(behavior.Mode, behavior.FailureRate, behavior.LatencyMs, Interlocked.Read(ref _requestsReceived));
    }

    public BehaviorStatus Change(ChangeBehaviorRequest request)
    {
        lock (_sync)
        {
            _current = new ProviderBehavior(
                request.Mode,
                Math.Clamp(request.FailureRate ?? _current.FailureRate, 0.0, 1.0),
                Math.Max(request.LatencyMs ?? _current.LatencyMs, 0));
        }

        return Status();
    }
}
