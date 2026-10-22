using CircuitBreakerLab.Domain.ExchangeRates;

namespace CircuitBreakerLab.Application.Ports;

public interface IExchangeRateProvider
{
    Task<ExchangeRate> GetAsync(CurrencyPair pair, CancellationToken cancellationToken);
}
