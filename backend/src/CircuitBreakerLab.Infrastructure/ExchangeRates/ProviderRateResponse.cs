namespace CircuitBreakerLab.Infrastructure.ExchangeRates;

internal sealed record ProviderRateResponse(string Base, string Target, decimal Rate);
