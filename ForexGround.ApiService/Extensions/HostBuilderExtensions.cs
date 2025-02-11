using ForexGround.ApiService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

    public static IHostApplicationBuilder ConfigureAuth(this IHostApplicationBuilder builder)
    {
        var jwtKey = builder.Configuration["Jwt:Key"];
        var jwtIssuer = builder.Configuration["Jwt:Issuer"];
        var jwtAudience = builder.Configuration["Jwt:Audience"];

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
        });

        return builder;
    }
}
