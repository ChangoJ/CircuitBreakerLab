using System.ComponentModel.DataAnnotations;

namespace CircuitBreakerLab.Infrastructure.Configuration;

public sealed class ExchangeRateProviderOptions
{
    public const string SectionName = "ExchangeRateProvider";

    [Required]
    [Url]
    public string BaseUrl { get; init; } = string.Empty;

    [Range(1, 60)]
    public int TimeoutSeconds { get; init; }
}
