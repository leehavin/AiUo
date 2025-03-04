namespace AiUo.Extensions.SshNet.Workers;

public class InstallDotNetTool : WorkerBase
{
    public string PackageId { get; set; }
    public string Source { get; set; }
    public InstallDotNetTool(string packageId, string source=null, SshClientEx client = null) : base(client)
    {
        PackageId = packageId;
        Source = source ?? "https://api.nuget.org/v3/index.json";
    }
    public override void Execute()
    {
        OutputText($"安装 DotNet Tool {PackageId} 开始", true);
        var cmd = $"dotnet tool update {PackageId} --no-cache -g --add-source {Source}";
        ExecuteCmd(cmd);
        OutputText($"安装 DotNet Tool {PackageId} 结束", true);
    }
}