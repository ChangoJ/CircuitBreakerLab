using CircuitBreakerLab.Domain.CircuitBreaker;

namespace CircuitBreakerLab.Application.CircuitBreaker.GetCircuitStatus;

public interface IGetCircuitStatusUseCase
{
    CircuitSnapshot Execute();
}
