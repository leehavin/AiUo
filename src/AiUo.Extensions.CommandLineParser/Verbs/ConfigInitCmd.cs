using CommandLine;

namespace AiUo.Extensions.CommandLineParser;

public class ConfigInitCmd : VerbCommand<ConfigInitOptions>
{
    public override void Execute()
    {
        if (File.Exists(CmdLineUtil.ConfigFileName))
        {
            ConsoleEx.Write($"配置文件{CmdLineUtil.ConfigFileName}已存在，是否覆盖(y/n):", ConsoleColor.Yellow);
            var input = Console.ReadLine();
            if (input.Trim().ToLower() != "y")
            {
                Console.WriteLine("操作已取消。");
                return;
            }
        }
        var config = CmdLineUtil.ConfigFile;
        config.SetSection(OptionsUtil.SECTION_NAME, OptionsUtil.Options);
        var ecmd = new List<EasyCmdListItem> {
            new EasyCmdListItem{
                Verb = "l",
                Options = "ecmd-list",
                Desc ="显示简易命令列表"
            }
        };
        config.SetSection(EasyCmdUtil.SECTION_NAME, ecmd);
        config.Save();
        Console.WriteLine($"生成配置文件 {config.ConfigFile}，内容如下：");
        Console.WriteLine(config.ToString());
    }
}
[Verb("config-init", HelpText = "在当前目录创建配置文件，默认tcmd.json")]
public class ConfigInitOptions
{
}