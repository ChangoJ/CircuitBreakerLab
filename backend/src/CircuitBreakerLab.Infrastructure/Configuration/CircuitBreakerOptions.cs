using System.ComponentModel.DataAnnotations;

namespace CircuitBreakerLab.Infrastructure.Configuration;

public sealed class CircuitBreakerOptions
{
    public const string SectionName = "CircuitBreaker";

    [Range(0.01, 1.0)]
    public double FailureRatio { get; init; }

    [Range(2, 1000)]
    public int MinimumThroughput { get; init; }

    [Range(1, 3600)]
    public int SamplingDurationSeconds { get; init; }

    [Range(1, 3600)]
    public int BreakDurationSeconds { get; init; }

    [Range(1, 1000)]
    public int EventHistorySize { get; init; }
}
