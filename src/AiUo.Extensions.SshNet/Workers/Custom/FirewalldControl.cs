namespace AiUo.Extensions.SshNet.Workers;

public class FirewalldControl : WorkerBase
{
    public bool Enabled { get; set; }
    public FirewalldControl(bool enabled, SshClientEx client = null) : base(client)
    {
        Enabled = enabled;
    }
    public override void Execute()
    {
        var start = Enabled ? "start" : "stop";
        var enable = Enabled ? "enable" : "disable";
        OutputText($"设置 Firewalld = {enable} 开始", true);
        ExecuteCmd($"sudo systemctl {start} firewalld");
        ExecuteCmd($"sudo systemctl {enable} firewalld");
        OutputText($"设置 Firewalld = {enable} 结束", true);
    }
}