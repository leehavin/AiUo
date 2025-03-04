namespace AiUo.Extensions.SshNet.Workers;

/// <summary>
/// 打开root登录功能并设置密码
/// </summary>
public class RootPasswordSet : WorkerBase
{
    public string RootPassword { get; set; }
    public RootPasswordSet(string rootPassword, SshClientEx client = null) : base(client)
    {
        RootPassword = rootPassword;
    }
    public override void Execute()
    {
        OutputText("打开root登录功能并设置密码开始", true);
        // 打开root登录功能
        ExecuteCmd("sudo sed -i 's/^.*PermitRootLogin.*/PermitRootLogin yes/g' /etc/ssh/sshd_config");
        ExecuteCmd("sudo sed -i 's/^.*PasswordAuthentication.*/PasswordAuthentication yes/g' /etc/ssh/sshd_config");
        ExecuteCmd("sudo systemctl reload sshd");
        // 设置密码
        ExecuteCmd($"echo root:{RootPassword} |sudo chpasswd root");
        OutputText("打开root登录功能并设置密码结束", true);
    }
}