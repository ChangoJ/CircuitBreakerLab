namespace CircuitBreakerLab.Domain.ExchangeRates;

public sealed record CurrencyCode
{
    public const int Length = 3;

    public string Value { get; }

    private CurrencyCode(string value) => Value = value;

    public static CurrencyCode Create(string? value)
    {
        var normalized = value?.Trim().ToUpperInvariant();

        if (normalized is null || normalized.Length != Length || !normalized.All(char.IsAsciiLetter))
        {
            throw new InvalidCurrencyCodeException(value);
        }

        return new CurrencyCode(normalized);
    }

    public override string ToString() => Value;
}
