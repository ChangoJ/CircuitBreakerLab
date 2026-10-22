namespace CircuitBreakerLab.Domain.CircuitBreaker;

public enum CircuitState
{
    Closed,
    Open,
    HalfOpen,
    Isolated
}
