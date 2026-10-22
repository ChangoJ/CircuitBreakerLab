namespace CircuitBreakerLab.Domain.CircuitBreaker;

public sealed record CircuitSnapshot(
    CircuitState State,
    DateTimeOffset? OpenUntil,
    CircuitPolicy Policy,
    CircuitMetrics Metrics,
    IReadOnlyList<CircuitEvent> RecentEvents);
