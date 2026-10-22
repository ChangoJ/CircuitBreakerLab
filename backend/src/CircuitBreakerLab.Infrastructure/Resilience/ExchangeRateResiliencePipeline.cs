using CircuitBreakerLab.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

namespace CircuitBreakerLab.Infrastructure.Resilience;

internal static class ExchangeRateResiliencePipeline
{
    public const string Name = "exchange-rate-provider";

    public static void Configure(ResiliencePipelineBuilder<HttpResponseMessage> builder, ResilienceHandlerContext context)
    {
        var monitor = context.ServiceProvider.GetRequiredService<CircuitBreakerMonitor>();
        var circuit = context.ServiceProvider.GetRequiredService<IOptions<CircuitBreakerOptions>>().Value;
        var provider = context.ServiceProvider.GetRequiredService<IOptions<ExchangeRateProviderOptions>>().Value;

        builder
            .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = circuit.FailureRatio,
                MinimumThroughput = circuit.MinimumThroughput,
                SamplingDuration = TimeSpan.FromSeconds(circuit.SamplingDurationSeconds),
                BreakDuration = TimeSpan.FromSeconds(circuit.BreakDurationSeconds),
                StateProvider = monitor.StateProvider,
                ManualControl = monitor.ManualControl,
                OnOpened = args =>
                {
                    monitor.OnOpened(args.BreakDuration, args.IsManual, DescribeOutcome(args.Outcome));
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = _ =>
                {
                    monitor.OnHalfOpened();
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    monitor.OnClosed(args.IsManual);
                    return ValueTask.CompletedTask;
                }
            })
            .AddTimeout(TimeSpan.FromSeconds(provider.TimeoutSeconds));
    }

    private static string DescribeOutcome(Outcome<HttpResponseMessage> outcome) =>
        outcome.Exception?.GetType().Name
        ?? (outcome.Result is { } response ? $"HTTP {(int)response.StatusCode}" : "manual");
}
