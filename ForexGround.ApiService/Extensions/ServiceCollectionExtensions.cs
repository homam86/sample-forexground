using ForexGround.ApiService.Providers;
using ForexGround.ApiService.Providers.Frankfurter;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    const string BaseUrl = "https://api.frankfurter.dev/v1";

    public static IServiceCollection AddFrankfurterApi(this IServiceCollection services)
    {
        services.AddRefitClient<IFrankfurterApiClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(BaseUrl));

        services.AddScoped<IForexProvider, DummyForexProvider>();
        services.AddScoped<IForexProvider, FrankfurterForexProvider>();

        services.AddScoped<IForexProviderFactory, ForexProviderFactory>();

        return services;
    }

    
    public static IServiceCollection AddAndConfigureApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
        });

        return services;
    }
}
