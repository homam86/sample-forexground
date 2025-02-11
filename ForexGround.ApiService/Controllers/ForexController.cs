using ForexGround.ApiService.Providers.Frankfurter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Net;

namespace ForexGround.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ForexController : ControllerBase
{
    private static readonly string[] BannedCurrencies = ["TRY", "PLN", "THB", "MXN"];

    private readonly ILogger<ForexController> _logger;
    private readonly IFrankfurterApiService _frankfurterApiService;

    public ForexController(ILogger<ForexController> logger,
        IFrankfurterApiService frankfurterApiService)
    {
        _logger = logger;
        _frankfurterApiService = frankfurterApiService;
    }

    [HttpGet("{currency}")]
    [OutputCache(PolicyName = PolicyNames.Default)]
    public async Task<ForexResponse> GetAsync(string currency)
    {
        var result = await _frankfurterApiService.GetRateAsync(currency, []);
        return result;
    }

    [HttpGet("{src}/convert/{dst}")]
    [OutputCache(PolicyName = PolicyNames.Default)]
    public async Task<ActionResult<decimal>> GetExchangeAsync(string src, string dst, [FromQuery] decimal amount = 1)
    {
        dst = dst.ToUpper();
        if (IsBannedCurrency(dst))
        {
            return BadRequest($"Currency '{dst}' is not supported");
        }

        if (amount <= 0)
        {
            return BadRequest("Amount must be greater than 0");
        }

        var result = await _frankfurterApiService.GetRateAsync(src, [dst]);
        if (!result.Rates.ContainsKey(dst))
        {
            return StatusCode((int)HttpStatusCode.BadGateway, "Destination currency not found.");
        }

        return Ok(result.Rates[dst] * amount);
    }

    [HttpGet("{currency}/history")]
    [OutputCache(PolicyName = PolicyNames.Default)]
    public async Task<ActionResult<ForexResponse>> GetHistoryAsync(string currency, [FromQuery] ForexHistoryRequest request)
    {
        // TODO: Add pagination
        var result = await _frankfurterApiService.GetHistoryAsync(currency, request.StartDate, request.EndDate, []);

        return Ok(result);
    }

    private static bool IsBannedCurrency(string currency)
    {
        currency = currency.ToUpper();
        return BannedCurrencies.Contains(currency);
    }
}
