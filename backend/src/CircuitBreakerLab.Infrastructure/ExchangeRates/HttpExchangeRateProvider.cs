using System.Net.Http.Json;
using CircuitBreakerLab.Application.Ports;
using CircuitBreakerLab.Domain.ExchangeRates;
using CircuitBreakerLab.Infrastructure.Resilience;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace CircuitBreakerLab.Infrastructure.ExchangeRates;

internal sealed class HttpExchangeRateProvider(
    HttpClient httpClient,
    CircuitBreakerMonitor monitor,
    TimeProvider timeProvider) : IExchangeRateProvider
{
    private const string RatesEndpoint = "rates";

    public async Task<ExchangeRate> GetAsync(CurrencyPair pair, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(BuildUri(pair), cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw Fail(pair, ProviderFailureReason.ErrorResponse, $"Provider answered HTTP {(int)response.StatusCode}.");
            }

            var payload = await response.Content.ReadFromJsonAsync<ProviderRateResponse>(cancellationToken)
                ?? throw Fail(pair, ProviderFailureReason.ErrorResponse, "Provider returned an empty body.");

            var rate = ExchangeRate.Create(pair, payload.Rate, timeProvider.GetUtcNow());
            monitor.RecordSuccess(pair);
            return rate;
        }
        catch (BrokenCircuitException exception)
        {
            monitor.RecordRejection(pair);
            throw new ProviderUnavailableException(
                ProviderFailureReason.CircuitOpen,
                "Circuit is open: the call was rejected without reaching the provider.",
                exception);
        }
        catch (TimeoutRejectedException exception)
        {
            throw Fail(pair, ProviderFailureReason.Timeout, "Provider did not answer in time.", exception);
        }
        catch (HttpRequestException exception)
        {
            throw Fail(pair, ProviderFailureReason.Unreachable, "Provider is unreachable.", exception);
        }
    }

    private ProviderUnavailableException Fail(
        CurrencyPair pair,
        ProviderFailureReason reason,
        string message,
        Exception? innerException = null)
    {
        monitor.RecordFailure(pair, reason);
        return new ProviderUnavailableException(reason, message, innerException);
    }

    private static Uri BuildUri(CurrencyPair pair) =>
        new($"{RatesEndpoint}?base={pair.Base}&target={pair.Target}", UriKind.Relative);
}
