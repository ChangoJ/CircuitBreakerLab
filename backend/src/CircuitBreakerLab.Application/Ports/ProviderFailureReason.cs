namespace CircuitBreakerLab.Application.Ports;

public enum ProviderFailureReason
{
    CircuitOpen,
    Timeout,
    ErrorResponse,
    Unreachable
}
