using Microsoft.Extensions.Options;
using ProviderSimulator.Behavior;

namespace ProviderSimulator.Rates;

internal sealed record RateResponse(string Base, string Target, decimal Rate);

internal sealed class RateCatalog(IOptions<SimulatorOptions> options)
{
    private const int Decimals = 6;

    private readonly Dictionary<string, decimal> _usdRates =
        new(options.Value.UsdRates, StringComparer.OrdinalIgnoreCase);

    public RateResponse? Quote(string baseCurrency, string targetCurrency)
    {
        if (!_usdRates.TryGetValue(baseCurrency, out var baseRate) ||
            !_usdRates.TryGetValue(targetCurrency, out var targetRate))
        {
            return null;
        }

        var jitter = 1 + ((decimal)Random.Shared.NextDouble() * 2 - 1) * (decimal)options.Value.RateJitter;
        var rate = decimal.Round(targetRate / baseRate * jitter, Decimals);

        return new RateResponse(baseCurrency.ToUpperInvariant(), targetCurrency.ToUpperInvariant(), rate);
    }
}
