using ForexGround.ApiService.Providers;
using ForexGround.ApiService.Providers.Frankfurter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace ForexGround.ApiService.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ForexController : ControllerBase
{
    private static readonly string[] BannedCurrencies = ["TRY", "PLN", "THB", "MXN"];

    private readonly ILogger _logger;
    private readonly IForexProvider _forexProvider;

    public ForexController(ILogger<ForexController> logger,
        IForexProviderFactory forexProviderFactory)
    {
        _logger = logger;
        _forexProvider = forexProviderFactory.GetProvider("Frankfurter");
    }

    [HttpGet("{currency}")]
    //[Authorize(Roles = "Agent")]
    [OutputCache(PolicyName = PolicyNames.Default)]
    public async IAsyncEnumerable<ForexValue> GetAsync(string currency)
    {
        var exchangeRates = _forexProvider.GetCurrencyPrices(currency);
        await foreach (var rate in exchangeRates)
        {
            yield return rate;
        }
    }

    [HttpGet("{src}/exchange/{dst}")]
    [Authorize(Roles = "Agent")]
    [OutputCache(PolicyName = PolicyNames.Default)]
    public async Task<ActionResult<decimal>> GetExchangeAsync(string src, string dst, [FromQuery] decimal amount = 1)
    {
        src = src.ToUpper();
        dst = dst.ToUpper();
        if (IsBannedCurrency(src))
        {
            return BadRequest($"Currency '{src}' is not supported");
        }
        
        if (IsBannedCurrency(src) || IsBannedCurrency(dst))
        {
            return BadRequest($"Currency '{dst}' is not supported");
        }

        if (amount <= 0)
        {
            return BadRequest("Amount must be greater than 0");
        }

        var result = await _forexProvider.GetExchangeRate(src, dst);

        return Ok(result * amount);
    }

    [HttpGet("{currency}/history")]
    [Authorize(Roles = "Agent")]
    [OutputCache(PolicyName = PolicyNames.Default)]
    public async IAsyncEnumerable<ForexValue> GetHistoryAsync(string currency, [FromQuery] ForexHistoryRequest request)
    {
        // TODO: Add pagination
        var startDate = request.StartDate.ToDateTime(TimeOnly.MinValue);
        var endDate = request.EndDate?.ToDateTime(TimeOnly.MinValue);

        var exchangeRates = _forexProvider.GetCurrencyPrices(currency, startDate, endDate);
        await foreach (var rate in exchangeRates)
        {
            yield return rate;
        }
    }

    private static bool IsBannedCurrency(string currency)
    {
        currency = currency.ToUpper();
        return BannedCurrencies.Contains(currency);
    }
}
