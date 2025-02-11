using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace ForexGround.ApiService.Providers.Frankfurter;

public class FrankfurterForexProvider : IForexProvider
{
    private readonly ILogger _logger;
    private readonly IFrankfurterApiClient _client;

    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

    private static readonly string[] BannedCurrencies = ["TRY", "PLN", "THB", "MXN"];

    #region Ctor()
    public FrankfurterForexProvider(ILogger<FrankfurterForexProvider> logger,
    IFrankfurterApiClient client)
    {
        _logger = logger;
        _client = client;

        /**
         *  We can add global resilience policies in Program.cs
         *  builder.Services.ConfigureHttpClientDefaults(http =>
         *  {
         *      http.AddStandardResilienceHandler();
         *  });
         */

        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning($"Retry {retryCount} encountered an error: {exception.Message}. Waiting {timeSpan} before next retry.");
                });

        _circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                2, // Break the circuit after 2 exceptions
                TimeSpan.FromMinutes(1),
                onBreak: (exception, breakDelay) =>
                {
                    _logger.LogWarning($"Circuit breaker triggered due to: {exception.Message}. Circuit will be open for {breakDelay}.");
                },
                onReset: () => _logger.LogInformation("Circuit breaker reset."),
                onHalfOpen: () => _logger.LogInformation("Circuit breaker is half-open, next call is a trial.")
            );
    }
    #endregion

    public string Name => "Frankfurter";

    public async Task<decimal> GetExchangeRate(string fromCurrency, string toCurrency)
    {
        fromCurrency = fromCurrency.ToUpper();
        toCurrency = toCurrency.ToUpper();

        var result = await CallApi(() => _client.GetLatestRatesAsync(fromCurrency, toCurrency));
        if (!result.Rates.ContainsKey(toCurrency))
        {
            throw new InvalidOperationException($"Currency '{toCurrency}' not found.");
        }

        return result.Rates[toCurrency];
    }

    public async IAsyncEnumerable<ForexValue> GetCurrencyPrices(string baseCurrency)
    {
        baseCurrency = baseCurrency.ToUpper();
        var result = await CallApi(() => _client.GetLatestRatesAsync(baseCurrency, null));
        foreach (var rate in result.Rates)
        {
            yield return new ForexValue(rate.Key, baseCurrency, DateTime.UtcNow, rate.Value);
        }
    }

    public async IAsyncEnumerable<ForexValue> GetCurrencyPrices(string baseCurrency, DateTime startDate, DateTime? endDate = null)
    {
        baseCurrency = baseCurrency.ToUpper();
        var dateRange = $"{startDate:yyyy-MM-dd}..";
        if (endDate.HasValue)
        {
            dateRange += $"{endDate:yyyy-MM-dd}";
        }

        var result = await CallApi(() => _client.GetHistoricalRatesAsync(baseCurrency, dateRange));

        foreach (var day in result.Rates)
        {
            foreach (var rate in day.Value)
            {
                yield return new ForexValue(rate.Key, baseCurrency, DateTime.Parse(day.Key), rate.Value);
            }
        }
    }

    private Task<TResponse> CallApi<TResponse>(Func<Task<TResponse>> apiCall)
    {
        return _circuitBreakerPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(apiCall));
    }

    private static bool IsBannedCurrency(string currency)
    {
        currency = currency.ToUpper();
        return BannedCurrencies.Contains(currency);
    }
}
