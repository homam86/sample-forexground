using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace ForexGround.ApiService.Providers.Frankfurter;

public class FrankfurterApiService : IFrankfurterApiService
{
    private readonly ILogger _logger;
    private readonly IFrankfurterApiClient _client;

    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

    public FrankfurterApiService(ILogger<FrankfurterApiService> logger,
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

    public Task<ForexResponse> GetRateAsync(string baseCurrency, string[] currencies)
    {
        var symbols = currencies.Length > 0 ? string.Join(",", currencies) : null;

        return CallApi(() => _client.GetLatestRatesAsync(baseCurrency, symbols));
    }

    public Task<ForexResponseByDate> GetHistoryAsync(string baseCurrency, DateOnly startDate, DateOnly? endDate, string[] currencies)
    {
        var symbols = currencies.Length > 0 ? string.Join(",", currencies) : null;
        var dateRange = $"{startDate:yyyy-MM-dd}..";
        if (endDate.HasValue)
        {
            dateRange += $"{endDate:yyyy-MM-dd}";
        }

        return CallApi(() => _client.GetHistoricalRatesAsync(baseCurrency, dateRange, symbols));
    }

    private Task<TResponse> CallApi<TResponse>(Func<Task<TResponse>> apiCall)
    {
        return _circuitBreakerPolicy.ExecuteAsync(() => _retryPolicy.ExecuteAsync(apiCall));
    }
}
