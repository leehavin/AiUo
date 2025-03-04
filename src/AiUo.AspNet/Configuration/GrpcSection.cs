using Microsoft.Extensions.Configuration;

namespace AiUo.Configuration;

public class GrpcSection : ConfigSection
{
    public override string SectionName => "Grpc";
    public bool Enabled { get; set; }
    public bool AutoLoad { get; set; }
    /// <summary>
    /// Grpc服务类所在Assembly。见:https://github.com/protobuf-net/protobuf-net.Grpc
    /// </summary>
    public List<string> Assemblies { get; set; } = new();

    public override void Bind(IConfiguration configuration)
    {
        base.Bind(configuration);
        Assemblies = configuration?.GetSection("Assemblies")
            .Get<List<string>>() ?? new();
    }
}