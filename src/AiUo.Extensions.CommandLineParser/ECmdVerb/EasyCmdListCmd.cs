using CommandLine;
using AiUo.Collections;

namespace AiUo.Extensions.CommandLineParser;

/// <summary>
/// 简易命令列表
/// </summary>
public class EasyCmdListCmd : VerbCommand<EasyCmdListOptions>
{
    public override void Execute()
    {
        if (!File.Exists(CmdLineUtil.ConfigFileName))
        {
            Console.WriteLine($"简易命令配置文件{CmdLineUtil.ConfigFileName}不存在");
            return;
        }
        Console.WriteLine("============ 简易命令列表 ============");
        int verbLen = 0;
        int argsLen = 0;
        var cmds = EasyCmdUtil.EasyCmds.Values;
        cmds.ForEach(item =>
        {
            verbLen = Math.Max(verbLen, StringUtil.GetStringWidth(item.Verb));
            var arg = item.Options?.Split(' ')?.FirstOrDefault("");
            argsLen = Math.Max(argsLen, StringUtil.GetStringWidth(arg));
        });
        int i = 0;
        foreach (var cmd in EasyCmdUtil.EasyCmds.Values)
        {
            i++;
            var arg = cmd.Options?.Split(' ')?.FirstOrDefault("");
            Console.WriteLine($"{{0,2}}> [verb]: {{1,-{verbLen}}} [opts]: {{2,-{argsLen}}} [desc]: {{3}}"
                , i, cmd.Verb, arg, cmd.Desc);
        }
        Console.WriteLine();
    }
}

[Verb("ecmd-list", HelpText = "显示配置文件（默认tcmd.json）中定义的简易命令列表")]
public class EasyCmdListOptions { }