namespace AiUo.Extensions.SshNet.Workers;

public class InstallDotNetSdk : WorkerBase
{
    public string Version { get; set; }
    public InstallDotNetSdk(string version = "6.0", SshClientEx client = null) : base(client)
    {
        Version = version;
    }
    public override void Execute()
    {
        OutputText($"安装 dotnet-sdk-{Version} 开始", true);
        ExecuteCmd("sudo rpm -Uvh https://packages.microsoft.com/config/rhel/7/packages-microsoft-prod.rpm");
        ExecuteCmd($"sudo yum install -y dotnet-sdk-{Version} openssh-server unzip curl");
        OutputText($"安装 dotnet-sdk-{Version} 完成", true);
    }
}