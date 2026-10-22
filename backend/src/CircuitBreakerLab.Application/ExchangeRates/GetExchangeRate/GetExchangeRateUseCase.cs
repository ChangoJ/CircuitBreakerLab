using CircuitBreakerLab.Application.Ports;
using CircuitBreakerLab.Domain.ExchangeRates;

namespace CircuitBreakerLab.Application.ExchangeRates.GetExchangeRate;

internal sealed class GetExchangeRateUseCase(
    IExchangeRateProvider provider,
    IExchangeRateCache cache,
    ICircuitBreakerMonitor monitor) : IGetExchangeRateUseCase
{
    public async Task<ExchangeRateResult> ExecuteAsync(GetExchangeRateQuery query, CancellationToken cancellationToken)
    {
        var pair = CurrencyPair.Create(query.BaseCurrency, query.TargetCurrency);

        try
        {
            var rate = await provider.GetAsync(pair, cancellationToken);
            await cache.SaveAsync(rate, cancellationToken);
            return ExchangeRateResult.Live(rate, monitor.CurrentState);
        }
        catch (ProviderUnavailableException exception)
        {
            return await FallbackAsync(pair, exception.Reason, cancellationToken);
        }
    }

    private async Task<ExchangeRateResult> FallbackAsync(
        CurrencyPair pair,
        ProviderFailureReason reason,
        CancellationToken cancellationToken)
    {
        var cached = await cache.FindAsync(pair, cancellationToken);

        if (cached is null)
        {
            return ExchangeRateResult.Unavailable(pair, reason, monitor.CurrentState);
        }

        monitor.RecordFallbackServed(pair);
        return ExchangeRateResult.Fallback(cached, reason, monitor.CurrentState);
    }
}
