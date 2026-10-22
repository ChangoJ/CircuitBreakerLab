using CircuitBreakerLab.Application.CircuitBreaker.GetCircuitStatus;
using CircuitBreakerLab.Application.CircuitBreaker.IsolateCircuit;
using CircuitBreakerLab.Application.CircuitBreaker.ResetCircuit;
using CircuitBreakerLab.Domain.CircuitBreaker;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CircuitBreakerLab.Api.Endpoints;

internal static class CircuitBreakerEndpoints
{
    public static IEndpointRouteBuilder MapCircuitBreakerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/circuit-breaker").WithTags("Circuit breaker");

        group.MapGet("/", GetStatus)
            .WithName("GetCircuitStatus")
            .WithSummary("Current state, policy, metrics and recent events of the circuit.");

        group.MapPost("/reset", ResetAsync)
            .WithName("ResetCircuit")
            .WithSummary("Manually closes the circuit.");

        group.MapPost("/isolate", IsolateAsync)
            .WithName("IsolateCircuit")
            .WithSummary("Manually opens the circuit until it is reset.");

        return app;
    }

    private static Ok<CircuitSnapshot> GetStatus(IGetCircuitStatusUseCase useCase) =>
        TypedResults.Ok(useCase.Execute());

    private static async Task<Ok<CircuitSnapshot>> ResetAsync(
        IResetCircuitUseCase useCase,
        IGetCircuitStatusUseCase status,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(cancellationToken);
        return TypedResults.Ok(status.Execute());
    }

    private static async Task<Ok<CircuitSnapshot>> IsolateAsync(
        IIsolateCircuitUseCase useCase,
        IGetCircuitStatusUseCase status,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(cancellationToken);
        return TypedResults.Ok(status.Execute());
    }
}
