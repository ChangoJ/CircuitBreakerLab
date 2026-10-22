using CircuitBreakerLab.Domain.Common;

namespace CircuitBreakerLab.Domain.ExchangeRates;

public sealed class InvalidCurrencyCodeException(string? value)
    : DomainException($"'{value}' is not a valid ISO 4217 currency code.");

public sealed class SameCurrencyPairException(CurrencyCode currency)
    : DomainException($"Base and target currency cannot be the same ('{currency}').");

public sealed class InvalidExchangeRateException(decimal rate)
    : DomainException($"Exchange rate must be greater than zero (received {rate}).");
