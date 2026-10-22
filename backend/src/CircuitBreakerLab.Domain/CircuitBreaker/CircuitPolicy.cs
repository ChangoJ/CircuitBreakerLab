namespace CircuitBreakerLab.Domain.CircuitBreaker;

public sealed record CircuitPolicy(
    double FailureRatio,
    int MinimumThroughput,
    TimeSpan SamplingDuration,
    TimeSpan BreakDuration,
    TimeSpan CallTimeout);
