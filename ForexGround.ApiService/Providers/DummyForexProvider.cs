
namespace ForexGround.ApiService.Providers;

public class DummyForexProvider : IForexProvider
{
    private static readonly Random Random = new Random();
    private static readonly List<string> Currencies = new List<string> { "USD", "EUR", "JPY", "GBP", "AUD" };


    private static readonly List<ForexValue> DummyData = GenerateDummyData();

    public string Name => "Dummy";

    public async IAsyncEnumerable<ForexValue> GetCurrencyPrices(string baseCurrency)
    {
        foreach (var forexValue in DummyData.Where(d => d.Currency == baseCurrency))
        {
            yield return forexValue;
            await Task.Yield(); // Simulate async operation
        }
    }

    public async IAsyncEnumerable<ForexValue> GetCurrencyPrices(string baseCurrency, DateTime startDate, DateTime? endDate = null)
    {
        var end = endDate ?? DateTime.UtcNow;
        foreach (var forexValue in DummyData.Where(d => d.Currency == baseCurrency && d.Date >= startDate && d.Date <= end))
        {
            yield return forexValue;
            await Task.Yield(); // Simulate async operation
        }
    }

    public Task<decimal> GetExchangeRate(string fromCurrency, string toCurrency)
    {
        var val = (decimal)(Random.NextDouble() * (1.5 - 0.5) + 0.5);
        return Task.FromResult(val);
    }


    private static List<ForexValue> GenerateDummyData()
    {
        var dummyData = new List<ForexValue>();
        foreach (var currency in Currencies)
        {
            for (int i = 0; i < 30; i++)
            {
                var val = (decimal)(Random.NextDouble() * (1.5 - 0.5) + 0.5);
                var date = DateTime.UtcNow.AddDays(-i);
                dummyData.Add(new ForexValue(currency, "USD", date, val));
            }
        }
        return dummyData;
    }
}
