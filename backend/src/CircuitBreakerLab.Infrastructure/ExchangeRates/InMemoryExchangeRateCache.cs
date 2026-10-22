using System.Collections.Concurrent;
using CircuitBreakerLab.Application.Ports;
using CircuitBreakerLab.Domain.ExchangeRates;

namespace CircuitBreakerLab.Infrastructure.ExchangeRates;

internal sealed class InMemoryExchangeRateCache : IExchangeRateCache
{
    private readonly ConcurrentDictionary<CurrencyPair, ExchangeRate> _rates = new();

    public Task SaveAsync(ExchangeRate exchangeRate, CancellationToken cancellationToken)
    {
        _rates[exchangeRate.Pair] = exchangeRate;
        return Task.CompletedTask;
    }

    public Task<ExchangeRate?> FindAsync(CurrencyPair pair, CancellationToken cancellationToken) =>
        Task.FromResult(_rates.GetValueOrDefault(pair));
}
