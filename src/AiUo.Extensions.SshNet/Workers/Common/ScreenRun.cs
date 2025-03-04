using System.Collections.Generic;

namespace AiUo.Extensions.SshNet.Workers;

public class ScreenRun : WorkerBase
{
    public List<string> Commands { get; }
    public string Name { get; set; }
    public string ConfigFile { get; set; }
    public ScreenRun(List<string> commands, string name, string configFile=null, SshClientEx client = null) : base(client)
    {
        Commands = commands;
        Name = name;
        ConfigFile = configFile;
    }
    public override void Execute()
    {
        OutputText($"运行screen命令开始。name:{Name}", true);
        var cmds = new List<string>();
        var cmd = $"screen -S {Name} -L";
        if (ConfigFile != null)
            cmd = $"{cmd} -c {ConfigFile}";
        cmds.Add(cmd);
        cmds.AddRange(Commands);
        Client.ExecuteStream(cmds);
        OutputText($"运行screen命令结束。name:{Name}", true);
    }
}