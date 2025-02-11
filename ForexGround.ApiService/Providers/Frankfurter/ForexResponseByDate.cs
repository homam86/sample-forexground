using System.Text.Json.Serialization;

namespace ForexGround.ApiService.Providers.Frankfurter;

public class ForexResponseByDate
{
    public required string Base { get; set; }

    [JsonPropertyName("start_date")]
    public required string StartDate { get; set; }

    [JsonPropertyName("end_date")]
    public required string? EndDate { get; set; }

    public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; } = [];

}
