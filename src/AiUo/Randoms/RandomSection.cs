using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using AiUo.Randoms;
using AiUo.Collections;

namespace AiUo.Configuration;

public class RandomSection : ConfigSection
{
    public override string SectionName => "Random";
    public string DefaultProviderName { get; set; }
    public Dictionary<string, RandomProviderElement> Providers { get; set; }
    public override void Bind(IConfiguration configuration)
    {
        base.Bind(configuration);
        Providers = configuration.GetSection("Providers")
            .Get<Dictionary<string, RandomProviderElement>>() ?? new();
        Providers.ForEach(x => x.Value.Name = x.Key);
    }
}
public class RandomProviderElement
{
    public string Name { get; internal set; }
    public string RandomType { get; set; }
    public SamplingOptions Options { get; set; }
}