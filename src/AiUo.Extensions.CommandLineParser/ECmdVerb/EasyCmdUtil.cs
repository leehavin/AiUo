using AiUo.Configuration;

namespace AiUo.Extensions.CommandLineParser;

public static class EasyCmdUtil
{
    public const string SECTION_NAME = "ecmds";
    public static Dictionary<string, EasyCmdListItem> EasyCmds = new Dictionary<string, EasyCmdListItem>();
    public static void ParseEasyCmds()
    {
        EasyCmds.Clear();
        if (!File.Exists(CmdLineUtil.ConfigFileName))
            return;
        var cmds = new MultiConfigFile(CmdLineUtil.ConfigFileName).GetSection<List<EasyCmdListItem>>(SECTION_NAME);
        if (cmds == null)
            return;
        foreach (var cmd in cmds)
        {
            EasyCmds.Add(cmd.Verb, cmd);
        }
    }
}