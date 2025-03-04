namespace AiUo.Extensions.SshNet.Workers;

public class SshExecCmd : WorkerBase
{
    public string Command { get; set; }
    public SshExecCmd(string command, SshClientEx client = null) : base(client) 
    {
        Command = command;
    }
    public override void Execute()
    {
        OutputText($"执行 {Command} 开始", true);
        ExecuteCmd(Command);
        OutputText($"执行 {Command} 结束", true);
    }
}