namespace ForexGround.ApiService.Providers;

public interface IForexProviderFactory
{
    IForexProvider GetProvider(string providerName);
}

public class ForexProviderFactory : IForexProviderFactory
{
    private readonly IEnumerable<IForexProvider> _providers;
    
    public ForexProviderFactory(IEnumerable<IForexProvider> providers)
    {
        _providers = providers;
    }

    public IForexProvider GetProvider(string providerName)
    {
        return _providers.FirstOrDefault(p => p.Name == providerName) 
            ?? throw new ArgumentException($"Unknown provider: {providerName}");
    }
}
