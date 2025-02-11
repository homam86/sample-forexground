namespace ForexGround.ApiService.Providers;

public interface IForexProvider
{
    string Name { get; }

    Task<decimal> GetExchangeRate(string fromCurrency, string toCurrency);

    IAsyncEnumerable<ForexValue> GetCurrencyPrices(string baseCurrency);

    IAsyncEnumerable<ForexValue> GetCurrencyPrices(string baseCurrency, DateTime startDate, DateTime? endDate = null);
}

public record ForexValue(string Currency, string BaseCurrency, DateTime Date, decimal ExchangeRate);
