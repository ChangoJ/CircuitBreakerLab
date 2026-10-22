namespace CircuitBreakerLab.Domain.CircuitBreaker;

public enum CircuitEventType
{
    CallSucceeded,
    CallFailed,
    CallRejected,
    FallbackServed,
    CircuitOpened,
    CircuitHalfOpened,
    CircuitClosed,
    CircuitIsolated
}
