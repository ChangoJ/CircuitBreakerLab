using CircuitBreakerLab.Domain.ExchangeRates;

namespace CircuitBreakerLab.Application.Ports;

public interface IExchangeRateCache
{
    Task SaveAsync(ExchangeRate exchangeRate, CancellationToken cancellationToken);

    Task<ExchangeRate?> FindAsync(CurrencyPair pair, CancellationToken cancellationToken);
}
