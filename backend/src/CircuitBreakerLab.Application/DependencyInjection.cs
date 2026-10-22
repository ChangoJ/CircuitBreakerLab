using CircuitBreakerLab.Application.CircuitBreaker.GetCircuitStatus;
using CircuitBreakerLab.Application.CircuitBreaker.IsolateCircuit;
using CircuitBreakerLab.Application.CircuitBreaker.ResetCircuit;
using CircuitBreakerLab.Application.ExchangeRates.GetExchangeRate;
using Microsoft.Extensions.DependencyInjection;

namespace CircuitBreakerLab.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IGetExchangeRateUseCase, GetExchangeRateUseCase>();
        services.AddScoped<IGetCircuitStatusUseCase, GetCircuitStatusUseCase>();
        services.AddScoped<IResetCircuitUseCase, ResetCircuitUseCase>();
        services.AddScoped<IIsolateCircuitUseCase, IsolateCircuitUseCase>();
        return services;
    }
}
