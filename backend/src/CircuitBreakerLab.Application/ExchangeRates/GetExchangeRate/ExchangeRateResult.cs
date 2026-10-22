using CircuitBreakerLab.Application.Ports;
using CircuitBreakerLab.Domain.CircuitBreaker;
using CircuitBreakerLab.Domain.ExchangeRates;

namespace CircuitBreakerLab.Application.ExchangeRates.GetExchangeRate;

public sealed record ExchangeRateResult(
    string BaseCurrency,
    string TargetCurrency,
    decimal? Rate,
    DateTimeOffset? RetrievedAt,
    RateSource Source,
    ProviderFailureReason? FailureReason,
    CircuitState CircuitState)
{
    public static ExchangeRateResult Live(ExchangeRate rate, CircuitState state) =>
        new(rate.Pair.Base.Value, rate.Pair.Target.Value, rate.Rate, rate.RetrievedAt, RateSource.Live, null, state);

    public static ExchangeRateResult Fallback(ExchangeRate rate, ProviderFailureReason reason, CircuitState state) =>
        new(rate.Pair.Base.Value, rate.Pair.Target.Value, rate.Rate, rate.RetrievedAt, RateSource.Fallback, reason, state);

    public static ExchangeRateResult Unavailable(CurrencyPair pair, ProviderFailureReason reason, CircuitState state) =>
        new(pair.Base.Value, pair.Target.Value, null, null, RateSource.Unavailable, reason, state);
}
