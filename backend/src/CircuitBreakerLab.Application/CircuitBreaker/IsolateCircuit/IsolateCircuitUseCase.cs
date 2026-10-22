using CircuitBreakerLab.Application.Ports;

namespace CircuitBreakerLab.Application.CircuitBreaker.IsolateCircuit;

internal sealed class IsolateCircuitUseCase(ICircuitBreakerMonitor monitor) : IIsolateCircuitUseCase
{
    public Task ExecuteAsync(CancellationToken cancellationToken) => monitor.IsolateAsync(cancellationToken);
}
