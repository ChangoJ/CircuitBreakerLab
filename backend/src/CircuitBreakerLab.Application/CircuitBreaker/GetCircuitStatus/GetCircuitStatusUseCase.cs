using CircuitBreakerLab.Application.Ports;
using CircuitBreakerLab.Domain.CircuitBreaker;

namespace CircuitBreakerLab.Application.CircuitBreaker.GetCircuitStatus;

internal sealed class GetCircuitStatusUseCase(ICircuitBreakerMonitor monitor) : IGetCircuitStatusUseCase
{
    public CircuitSnapshot Execute() => monitor.GetSnapshot();
}
