namespace CircuitBreakerLab.Domain.ExchangeRates;

public sealed record CurrencyPair
{
    public CurrencyCode Base { get; }

    public CurrencyCode Target { get; }

    private CurrencyPair(CurrencyCode baseCurrency, CurrencyCode target)
    {
        Base = baseCurrency;
        Target = target;
    }

    public static CurrencyPair Create(string? baseCurrency, string? target)
    {
        var baseCode = CurrencyCode.Create(baseCurrency);
        var targetCode = CurrencyCode.Create(target);

        if (baseCode == targetCode)
        {
            throw new SameCurrencyPairException(baseCode);
        }

        return new CurrencyPair(baseCode, targetCode);
    }

    public override string ToString() => $"{Base}/{Target}";
}
