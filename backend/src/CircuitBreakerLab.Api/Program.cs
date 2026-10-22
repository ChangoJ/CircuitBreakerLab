using System.Text.Json.Serialization;
using CircuitBreakerLab.Api.Endpoints;
using CircuitBreakerLab.Api.ExceptionHandling;
using CircuitBreakerLab.Application;
using CircuitBreakerLab.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapHealthChecks("/health");
app.MapExchangeRateEndpoints();
app.MapCircuitBreakerEndpoints();

await app.RunAsync();
