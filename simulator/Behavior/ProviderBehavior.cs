namespace ProviderSimulator.Behavior;

internal sealed record ProviderBehavior(BehaviorMode Mode, double FailureRate, int LatencyMs);

internal sealed record ChangeBehaviorRequest(BehaviorMode Mode, double? FailureRate, int? LatencyMs);

internal sealed record BehaviorStatus(BehaviorMode Mode, double FailureRate, int LatencyMs, long RequestsReceived);
