using System.Text.Json.Serialization;
using ProviderSimulator.Behavior;
using ProviderSimulator.Rates;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<SimulatorOptions>()
    .BindConfiguration(SimulatorOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<BehaviorState>();
builder.Services.AddSingleton<RateCatalog>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.MapHealthChecks("/health");

app.MapGet("/rates", async (string? @base, string? target, BehaviorState state, RateCatalog catalog, CancellationToken cancellationToken) =>
{
    state.RegisterRequest();
    var behavior = state.Current;

    if (behavior.Mode == BehaviorMode.Slow)
    {
        await Task.Delay(behavior.LatencyMs, cancellationToken);
    }

    var shouldFail = behavior.Mode == BehaviorMode.Failing ||
        (behavior.Mode == BehaviorMode.Intermittent && Random.Shared.NextDouble() < behavior.FailureRate);

    if (shouldFail)
    {
        return Results.Problem("Simulated provider failure.", statusCode: StatusCodes.Status500InternalServerError);
    }

    if (string.IsNullOrWhiteSpace(@base) || string.IsNullOrWhiteSpace(target))
    {
        return Results.Problem("Query parameters 'base' and 'target' are required.", statusCode: StatusCodes.Status400BadRequest);
    }

    var quote = catalog.Quote(@base, target);

    return quote is null
        ? Results.Problem($"Pair {@base}/{target} is not supported.", statusCode: StatusCodes.Status404NotFound)
        : Results.Ok(quote);
});

var admin = app.MapGroup("/admin/behavior");

admin.MapGet("/", (BehaviorState state) => Results.Ok(state.Status()));

admin.MapPut("/", (ChangeBehaviorRequest request, BehaviorState state) => Results.Ok(state.Change(request)));

await app.RunAsync();
