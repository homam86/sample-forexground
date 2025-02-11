namespace ForexGround.ApiService.Providers.Frankfurter
{
    public interface IFrankfurterApiService
    {
        Task<ForexResponseByDate> GetHistoryAsync(string baseCurrency, DateOnly startDate, DateOnly? endDate, string[] currencies);
        Task<ForexResponse> GetRateAsync(string baseCurrency, string[] currencies);
    }
}