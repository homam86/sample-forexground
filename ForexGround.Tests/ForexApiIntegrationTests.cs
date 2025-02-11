using System.Net.Http.Json;

namespace ForexGround.Tests;

public class ForexApiIntegrationTests
{
    record ForexValue(string Currency, string BaseCurrency, DateTime Date, decimal ExchangeRate);

    [Fact]
    public async Task GetPriceList_ReturnsSuccessStatusCode()
    {
        var appHost =
            await DistributedApplicationTestingBuilder.CreateAsync<Projects.ForexGround_AppHost>();
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var resourceNotificationService =
            app.Services.GetRequiredService<ResourceNotificationService>();
        await resourceNotificationService
                .WaitForResourceAsync("apiservice", KnownResourceStates.Running)
                .WaitAsync(TimeSpan.FromSeconds(30));

        var httpClient = app.CreateHttpClient("apiservice");
        var response = await httpClient.GetAsync("/api/v1/forex/eur");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var forecasts = await response.Content.ReadFromJsonAsync<IEnumerable<ForexValue>>();
        Assert.NotNull(forecasts);
        Assert.True(forecasts.Count() > 0);
    }
}