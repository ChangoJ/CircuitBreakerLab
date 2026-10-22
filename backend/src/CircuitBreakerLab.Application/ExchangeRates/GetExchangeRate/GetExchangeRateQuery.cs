namespace CircuitBreakerLab.Application.ExchangeRates.GetExchangeRate;

public sealed record GetExchangeRateQuery(string? BaseCurrency, string? TargetCurrency);
