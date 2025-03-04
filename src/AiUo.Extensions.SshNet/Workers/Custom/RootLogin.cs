namespace AiUo.Extensions.SshNet.Workers;

/// <summary>
/// 设置当前用户使用root权限登录
/// </summary>
public class RootLogin : WorkerBase
{
    public RootLogin(SshClientEx client = null) : base(client)
    {
    }
    public override void Execute()
    {
        OutputText($"设置当前用户{Client.ConnectionInfo.Username}使用root权限登录开始", true);
        var content = ExecuteCmd("cat ~/.bash_profile").Result;
        if (!content.Contains("sudo su root"))
            ExecuteCmd("sudo sed -i '/PATH=/i\\sudo su root' ~/.bash_profile");
        ExecuteCmd("sudo sed -i 's/^Subsystem sftp.*/Subsystem sftp internal-sftp/' /etc/ssh/sshd_config");
        ExecuteCmd("sudo sed -ri 's/^/#/;s/sleep 10\"\\s +/ &\\n / ' /root/.ssh/authorized_keys");
        ExecuteCmd("sudo systemctl reload sshd");
        OutputText($"设置当前用户{Client.ConnectionInfo.Username}使用root权限登录结束", true);
    }
}