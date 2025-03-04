using AiUo.Configuration;
using AiUo.Logging;

namespace AiUo.Extensions.CommandLineParser;

public static class CmdLineUtil
{
    /// <summary>
    /// 当前程序的配置文件，默认tcmd.json
    /// </summary>
    public static string ConfigFileName { get; private set; }
    public static MultiConfigFile ConfigFile { get; private set; }

    public static CmdLineParser Parser { get; private set; }
    /// <summary>
    /// 运行command程序
    /// </summary>
    /// <param name="config"></param>
    /// <param name="args"></param>
    public static void Run(CmdLineConfig config, string[] args)
    {
        config = config ?? new CmdLineConfig();
        if (string.IsNullOrEmpty(config.ConfigFile))
            config.ConfigFile = "tcmd.json";
        ConfigFileName = config.ConfigFile;
        ConfigFile = new MultiConfigFile(config.ConfigFile);
        OptionsUtil.ParseOptions(config);
        LogUtil.Debug($"[ConfigFile]: {config.ConfigFile} [DebugArgs]: {config.DebugArgs} [DefaultArgs]: {config.DefaultArgs}");
        //
        Parser = new CmdLineParser(config);
        Parser.Run(args);
    }
    internal static string[] RepareArgs(string argStr)
    {
        var argsList = new List<string>();
        foreach (var item in argStr.Split(" ", StringSplitOptions.RemoveEmptyEntries))
        {
            var arg = item.Trim().TrimEnd('\r', '\n');
            if (!string.IsNullOrEmpty(arg))
                argsList.Add(arg);
        }
        return argsList.ToArray();
    }
}