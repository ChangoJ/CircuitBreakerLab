namespace CircuitBreakerLab.Application.Ports;

public sealed class ProviderUnavailableException(ProviderFailureReason reason, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public ProviderFailureReason Reason { get; } = reason;
}
