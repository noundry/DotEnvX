using Microsoft.Extensions.Configuration;
using Noundry.DotEnvX.Core.Models;

namespace Noundry.DotEnvX.Core.Extensions;

public class DotEnvConfigurationSource : IConfigurationSource
{
    public DotEnvOptions? Options { get; set; }
    
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new DotEnvConfigurationProvider(Options);
    }
}

public class DotEnvConfigurationProvider : ConfigurationProvider
{
    private readonly DotEnvOptions? _options;
    
    public DotEnvConfigurationProvider(DotEnvOptions? options)
    {
        _options = options;
    }
    
    public override void Load()
    {
        var result = DotEnv.Config(_options);
        
        if (result.Error != null && _options?.Strict == true)
        {
            throw result.Error;
        }
        
        if (result.Parsed != null)
        {
            Data = new Dictionary<string, string?>(result.Parsed!);
        }
    }
}