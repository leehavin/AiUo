namespace AiUo.Extensions.SshNet.Workers;

public class SshKeysAllow : WorkerBase
{
    public SshKeysAllow(SshClientEx client = null) : base(client) { }
    public override void Execute()
    {
        OutputText("设置 authorized_keys 开始", true);
        ExecuteCmd("sudo mkdir ~/.ssh");
        ExecuteCmd("sudo chmod 700 ~/.ssh");
        ExecuteCmd("sudo touch ~/.ssh/authorized_keys");
        ExecuteCmd("sudo chmod 600 ~/.ssh/authorized_keys");
        OutputText("设置 authorized_keys 完成", true);
    }
}