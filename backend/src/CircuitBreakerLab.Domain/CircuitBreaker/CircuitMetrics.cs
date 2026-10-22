namespace CircuitBreakerLab.Domain.CircuitBreaker;

public sealed record CircuitMetrics(long Successes, long Failures, long Rejections, long FallbacksServed)
{
    public long TotalCalls => Successes + Failures + Rejections;
}
