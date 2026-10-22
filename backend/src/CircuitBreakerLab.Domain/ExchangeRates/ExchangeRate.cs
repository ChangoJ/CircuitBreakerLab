namespace CircuitBreakerLab.Domain.ExchangeRates;

public sealed class ExchangeRate
{
    public CurrencyPair Pair { get; }

    public decimal Rate { get; }

    public DateTimeOffset RetrievedAt { get; }

    private ExchangeRate(CurrencyPair pair, decimal rate, DateTimeOffset retrievedAt)
    {
        Pair = pair;
        Rate = rate;
        RetrievedAt = retrievedAt;
    }

    public static ExchangeRate Create(CurrencyPair pair, decimal rate, DateTimeOffset retrievedAt)
    {
        if (rate <= 0)
        {
            throw new InvalidExchangeRateException(rate);
        }

        return new ExchangeRate(pair, rate, retrievedAt);
    }
}
