namespace AiUo.Extensions.SshNet.Workers;

public class SELinuxControl : WorkerBase
{
    public bool Enabled { get; set; }
    public SELinuxControl(bool enabled, SshClientEx client = null) : base(client)
    {
        Enabled = enabled;
    }
    public override void Execute()
    {
        var value = Enabled ? "enforcing" : "disabled";
        OutputText($"设置 SELinux = {value} 开始", true);
        ExecuteCmd("sudo setenforce 0");
        ExecuteCmd($"sudo sed -i 's/^SELINUX=.*/SELINUX={value}/g' /etc/selinux/config");
        OutputText($"设置 SELinux = {value} 完成", true);
    }
}