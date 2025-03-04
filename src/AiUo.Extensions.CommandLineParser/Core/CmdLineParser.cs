using CommandLine;
using System.Reflection;
using AiUo.Logging;
using AiUo.Reflection;
using AiUo.Collections;

namespace AiUo.Extensions.CommandLineParser;

/// <summary>
/// 命令行参数解析器
/// Main函数中加入代码，如:
///     AiUoUtil.Init(LogLevel.Error);
///     var config = new CmdLineConfig();
///     config.DefaultArgs = "list";
///     //config.DebugArgs = "";
///     new CmdLineParser(config).Run(args);
/// CmdLineParser.Run 执行判断顺序：
///     1）运行DebugArgs => CmdLineConfig.DebugArgs
///     2）运行简易命令(列表或执行) => CmdLineConfig.EasyCmdListConfigFile
///     3）运行DefaultArgs(无参数时使用的默认参数) => CmdLineConfig.DefaultArgs
///     4）运行预定义的Verb (如："--help", "--version")
///     5）运行自定义Verb
/// </summary>
public class CmdLineParser
{
    protected CmdLineConfig Config;
    private HashSet<string> _verbCache = new HashSet<string>();
    private Dictionary<Type, IVerbCommand> _verbCmdCache = new Dictionary<Type, IVerbCommand>();
    private static HashSet<string> _predefinedVerbs = new HashSet<string> { "--help", "--version", "help", "version" };

    internal CmdLineParser(CmdLineConfig config)
    {
        Config = config ?? new CmdLineConfig();
    }
    /// <summary>
    /// 执行判断顺序：
    ///     1）运行DebugArgs => CmdLineConfig.DebugArgs
    ///     2）运行简易命令(列表或执行) => CmdLineConfig.EasyCmdListConfigFile
    ///     3）运行DefaultArgs(无参数时使用的默认参数) => CmdLineConfig.DefaultArgs
    ///     4）运行预定义的Verb (如："--help", "--version")
    ///     5）运行自定义Verb
    /// </summary>
    /// <param name="args"></param>
    public void Run(string[] args)
    {
        try
        {
            LogUtil.Debug($"初始执行参数：{string.Join(' ', args)}");
            if (args != null && args.Length == 1)
            {
                if (args[0] == "-v")
                    args[0] = "--version";
                else if (args[0] == "-h")
                    args[0] = "--help";

            }
            // 处理运行DebugArgs => CmdLineConfig.DebugArgs
            if (RunDebugArgs(args))
                return;
            var types = GetVerbTypes();
            // 处理运行简易命令(列表或运行) => EasyCmdUtil.ConfigFile
            if (RunEasyCmd(args))
                return;
            // 处理运行DefaultArgs(无参数时使用的默认参数) => CmdLineConfig.DefaultArgs
            if (RunDefaultArgs(args))
                return;

            if (args != null && args.Length > 0)
                LogUtil.Debug($"最终执行参数：{string.Join(' ', args)}");
            var result = Parser.Default.ParseArguments(args, types);
            // 处理运行预定义的Verb (如："--help", "--version")
            if (RunPredefinedVerb(args))
                return;
            // 处理运行自定义Verb
            result.WithParsed(RunOptions)
                .WithNotParsed(errs =>
                {
                    var msg = $"异常: 命令参数无法解析。{string.Join(' ', args)}";
                    LogUtil.Error(msg);
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"异常: {ex.Message}");
            LogUtil.Error(ex, $"异常: {ex.Message}");
        }
    }
    private bool RunDebugArgs(string[] args)
    {
        if (!string.IsNullOrEmpty(Config.DebugArgs))
        {
            var debugArgs = Config.DebugArgs;
            Config.DebugArgs = null;//只执行一次
            Run(CmdLineUtil.RepareArgs(debugArgs));
            return true;
        }
        return false;
    }
    private bool RunDefaultArgs(string[] args)
    {
        if ((args == null || args.Length == 0) && !string.IsNullOrEmpty(Config.DefaultArgs))
        {
            Run(CmdLineUtil.RepareArgs(Config.DefaultArgs));
            return true;
        }
        return false;
    }
    private bool RunEasyCmd(string[] args)
    {
        EasyCmdUtil.ParseEasyCmds();
        EasyCmdUtil.EasyCmds.Values.ForEach(item =>
        {
            if (_verbCache.Contains(item.Verb))
                throw new Exception($"简易命令配置的verb与程序中定义的重复。verb: {item.Verb} desc: {item.Desc}");
            _verbCache.Add(item.Verb);

        });
        // run
        if (args != null && args.Length > 0 && EasyCmdUtil.EasyCmds.ContainsKey(args[0]))
        {
            var verb = args[0];
            LogUtil.Debug($"执行简易命令: {verb}");
            var item = EasyCmdUtil.EasyCmds[verb];
            var inputArgs = string.Join(' ', args.Skip(1)); //执行简易命令时，用户输入的动态参数
            var newArgs = CmdLineUtil.RepareArgs(item.Options + inputArgs);
            Run(newArgs);
            return true;
        }
        return false;
    }
    private bool RunPredefinedVerb(string[] args)
    {
        return (args == null || args.Length == 0) || (args.Length > 0 && _predefinedVerbs.Contains(args[0]));
    }
    private void RunOptions(object opt)
    {
        ParseVerbCommands();
        if (!_verbCmdCache.ContainsKey(opt.GetType()))
            throw new Exception($"执行时verb对应的Cmmmand未找到。verb: {opt}");
        var cmdObj = _verbCmdCache[opt.GetType()];
        cmdObj.SetOptions(opt);
        cmdObj.Execute();
    }

    #region Utils
    private void ParseVerbCommands()
    {
        _verbCmdCache.Clear();
        AddCaches(Assembly.GetExecutingAssembly().GetTypes());
        AddCaches(Assembly.GetEntryAssembly().GetTypes());
        void AddCaches(Type[] types)
        {
            foreach (var type in types)
            {
                if (type == typeof(VerbCommand<>))
                    continue;
                if (ReflectionUtil.IsSubclassOfGeneric(type, typeof(VerbCommand<>)))
                {
                    var verbType = type.BaseType.GenericTypeArguments[0];
                    var cmdObj = (IVerbCommand)ReflectionUtil.CreateInstance(type);
                    _verbCmdCache.Add(verbType, cmdObj);
                }
            }
        }
    }

    private Type[] GetVerbTypes()
    {
        _verbCache.Clear();
        var types = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.GetCustomAttribute<VerbAttribute>() != null);
        var typesExec = Assembly.GetEntryAssembly().GetTypes()
            .Where(t => t.GetCustomAttribute<VerbAttribute>() != null);
        var ret = Enumerable.Concat(types, typesExec).ToList();
        ret.Sort((x, y) =>
        {
            return x.GetCustomAttribute<VerbAttribute>().Name.CompareTo(y.GetCustomAttribute<VerbAttribute>().Name);
        });
        foreach (var type in ret)
        {
            var verbName = type.GetCustomAttribute<VerbAttribute>().Name;
            if (_verbCache.Contains(verbName))
                throw new Exception($"cmd命令定义的verb重复。verb: {verbName} type: {type.FullName}");
            _verbCache.Add(verbName);
        }
        return ret.ToArray();
    }
    #endregion
}