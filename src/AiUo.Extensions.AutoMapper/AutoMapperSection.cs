using Microsoft.Extensions.Configuration;

namespace AiUo.Configuration;

public class AutoMapperSection : ConfigSection
{
    public override string SectionName => "AutoMapper";
    public bool AutoLoad { get; set; }
    public List<string> Assemblies { get; set; } = new List<string>();
    public override void Bind(IConfiguration configuration)
    {
        base.Bind(configuration);
        Assemblies.Clear();
        Assemblies = configuration?.GetSection("Assemblies")
            .Get<List<string>>() ?? new();
    }
}