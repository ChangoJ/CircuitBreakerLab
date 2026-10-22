namespace CircuitBreakerLab.Domain.CircuitBreaker;

public sealed record CircuitEvent(CircuitEventType Type, CircuitState StateAfter, string Detail, DateTimeOffset OccurredAt);
