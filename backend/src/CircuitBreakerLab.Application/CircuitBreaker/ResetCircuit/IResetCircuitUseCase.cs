namespace CircuitBreakerLab.Application.CircuitBreaker.ResetCircuit;

public interface IResetCircuitUseCase
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
