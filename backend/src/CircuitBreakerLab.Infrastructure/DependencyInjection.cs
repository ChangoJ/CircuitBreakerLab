using CircuitBreakerLab.Application.Ports;
using CircuitBreakerLab.Infrastructure.Configuration;
using CircuitBreakerLab.Infrastructure.ExchangeRates;
using CircuitBreakerLab.Infrastructure.Resilience;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace CircuitBreakerLab.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddOptions<ExchangeRateProviderOptions>()
            .BindConfiguration(ExchangeRateProviderOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<CircuitBreakerOptions>()
            .BindConfiguration(CircuitBreakerOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<CircuitBreakerMonitor>();
        services.AddSingleton<ICircuitBreakerMonitor>(provider => provider.GetRequiredService<CircuitBreakerMonitor>());
        services.AddSingleton<IExchangeRateCache, InMemoryExchangeRateCache>();

        services
            .AddHttpClient<IExchangeRateProvider, HttpExchangeRateProvider>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<ExchangeRateProviderOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            })
            .AddResilienceHandler(ExchangeRateResiliencePipeline.Name, ExchangeRateResiliencePipeline.Configure);

        return services;
    }
}
