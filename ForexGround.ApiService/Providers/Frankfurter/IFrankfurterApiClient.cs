using Refit;

namespace ForexGround.ApiService.Providers.Frankfurter;

public interface IFrankfurterApiClient
{
    [Get("/latest")]
    Task<ForexResponse> GetLatestRatesAsync(
        [AliasAs("base")] string? baseCurrency,
        [AliasAs("symbols")] string? symbols = null /*e.g: CHF,GBP*/
    );

    [Get("/{dateRange}")]
    Task<ForexResponseByDate> GetHistoricalRatesAsync([AliasAs("base")] string baseCurrency,
        string dateRange,
        string? symbols = null /*e.g: CHF,GBP*/
    );

}
