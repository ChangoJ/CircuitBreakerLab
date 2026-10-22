namespace CircuitBreakerLab.Application.CircuitBreaker.IsolateCircuit;

public interface IIsolateCircuitUseCase
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
