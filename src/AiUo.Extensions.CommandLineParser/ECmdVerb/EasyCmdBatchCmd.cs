using CommandLine;

namespace AiUo.Extensions.CommandLineParser;

/// <summary>
/// 简易命令批处理执行
/// </summary>
public class EasyCmdBatchCmd : VerbCommand<EasyCmdBatchOptions>
{
    public override void Execute()
    {
        foreach (var verb in Options.EasyVerbs)
        {
            if (!EasyCmdUtil.EasyCmds.TryGetValue(verb, out EasyCmdListItem cmd))
                throw new Exception($"执行简易命令批处理时，没有找到建议命令名。verb:{verb}");
            var args = CmdLineUtil.RepareArgs(cmd.Options);
            CmdLineUtil.Parser.Run(args);
        }
    }
}
[Verb("ecmd-batch", HelpText = "执行配置文件（默认tcmd.json）中定义的简易命令批处理。如: ecmd-bach --verbs 1,2,3")]
public class EasyCmdBatchOptions 
{
    [Option('v', "verbs", Separator=',', HelpText = "批量执行的简易命令verb集合")]
    public IEnumerable<string> EasyVerbs { get; set; }
}