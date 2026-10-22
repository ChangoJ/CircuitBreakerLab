namespace CircuitBreakerLab.Application.ExchangeRates.GetExchangeRate;

public interface IGetExchangeRateUseCase
{
    Task<ExchangeRateResult> ExecuteAsync(GetExchangeRateQuery query, CancellationToken cancellationToken);
}
