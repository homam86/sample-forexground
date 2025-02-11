using ForexGround.ApiService;

namespace Microsoft.Extensions.Hosting;

public static class HostBuilderExtensions
{
    public static IHostApplicationBuilder AddApiOutputCache(this IHostApplicationBuilder builder)
    {
        var expireDuration = TimeSpan.FromMinutes(5); // TODO: Make it configurable

        builder.Services.AddOutputCache(options =>
        {
            options.AddPolicy(PolicyNames.Default, builder => builder
                        .Tag("api-all")
                        .Expire(expireDuration));
        });

        return builder;
    }
}
