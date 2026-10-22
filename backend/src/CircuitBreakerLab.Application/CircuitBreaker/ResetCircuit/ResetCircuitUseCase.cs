using CircuitBreakerLab.Application.Ports;

namespace CircuitBreakerLab.Application.CircuitBreaker.ResetCircuit;

internal sealed class ResetCircuitUseCase(ICircuitBreakerMonitor monitor) : IResetCircuitUseCase
{
    public Task ExecuteAsync(CancellationToken cancellationToken) => monitor.ResetAsync(cancellationToken);
}
