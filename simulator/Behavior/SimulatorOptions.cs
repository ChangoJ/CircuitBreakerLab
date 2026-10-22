using System.ComponentModel.DataAnnotations;

namespace ProviderSimulator.Behavior;

internal sealed class SimulatorOptions
{
    public const string SectionName = "Simulator";

    public BehaviorMode DefaultMode { get; init; }

    [Range(0.0, 1.0)]
    public double DefaultFailureRate { get; init; }

    [Range(0, 60000)]
    public int DefaultLatencyMs { get; init; }

    [Range(0.0, 0.5)]
    public double RateJitter { get; init; }

    [Required]
    [MinLength(2)]
    public Dictionary<string, decimal> UsdRates { get; init; } = [];
}
