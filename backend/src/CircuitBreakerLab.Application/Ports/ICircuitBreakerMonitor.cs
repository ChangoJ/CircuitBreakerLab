using CircuitBreakerLab.Domain.CircuitBreaker;
using CircuitBreakerLab.Domain.ExchangeRates;

namespace CircuitBreakerLab.Application.Ports;

public interface ICircuitBreakerMonitor
{
    CircuitState CurrentState { get; }

    CircuitSnapshot GetSnapshot();

    void RecordFallbackServed(CurrencyPair pair);

    Task ResetAsync(CancellationToken cancellationToken);

    Task IsolateAsync(CancellationToken cancellationToken);
}
