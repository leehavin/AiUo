using Microsoft.Extensions.Configuration;
using AiUo.Configuration;

namespace AiUo.AspNet;

public class ClientSyncNotifySection : ConfigSection
{
    public override string SectionName => "ClientSyncNotify";
    public bool Enabled { get; set; }
    public string HeaderName { get; set; }
    public string NotifyProvider { get; set; }
    public override void Bind(IConfiguration configuration)
    {
        base.Bind(configuration);
    }
}