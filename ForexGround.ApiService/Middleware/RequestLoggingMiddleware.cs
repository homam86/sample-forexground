using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ForexGround.ApiService.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // Call the next middleware in the pipeline
        await _next(context);

        stopwatch.Stop();

        var caller = context.User.Identity?.IsAuthenticated == true
            ? context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "Unknown"
            : "Anonymous";
        var endpoint = context.Request.Path;
        var duration = stopwatch.ElapsedMilliseconds;

        // TODO: Save the log to a persistent store
        _logger.LogInformation(">>>> User [{Caller}], Endpoint: {Endpoint}, Duration: {Duration}ms", caller, endpoint, duration);
    }
}
