using CircuitBreakerLab.Application.ExchangeRates.GetExchangeRate;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CircuitBreakerLab.Api.Endpoints;

internal static class ExchangeRateEndpoints
{
    public static IEndpointRouteBuilder MapExchangeRateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/exchange-rates").WithTags("Exchange rates");

        group.MapGet("/", GetExchangeRateAsync)
            .WithName("GetExchangeRate")
            .WithSummary("Gets an exchange rate through the circuit breaker, falling back to the last known value.")
            .Produces<ExchangeRateResult>(StatusCodes.Status503ServiceUnavailable)
            .ProducesValidationProblem();

        return app;
    }

    private static async Task<Results<Ok<ExchangeRateResult>, JsonHttpResult<ExchangeRateResult>>> GetExchangeRateAsync(
        [FromQuery(Name = "base")] string? baseCurrency,
        [FromQuery(Name = "target")] string? targetCurrency,
        IGetExchangeRateUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(new GetExchangeRateQuery(baseCurrency, targetCurrency), cancellationToken);

        return result.Source == RateSource.Unavailable
            ? TypedResults.Json(result, statusCode: StatusCodes.Status503ServiceUnavailable)
            : TypedResults.Ok(result);
    }
}
