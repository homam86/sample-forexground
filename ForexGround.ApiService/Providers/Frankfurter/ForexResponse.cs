namespace ForexGround.ApiService.Providers.Frankfurter;

public class ForexResponse
{
    public required string Base { get; set; }

    public required string Date { get; set; }

    public Dictionary<string, decimal> Rates { get; set; } = [];

}
