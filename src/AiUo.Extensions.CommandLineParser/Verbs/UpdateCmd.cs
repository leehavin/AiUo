using CommandLine;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using AiUo.CliWrap;
using AiUo.Linux;
using AiUo.Logging;

namespace AiUo.Extensions.CommandLineParser;

public class UpdateCmd : VerbCommand<UpdateOptions>
{
    public const string NUGET_SOURCE = "NUGET_SOURCE";
    public override void Execute()
    {
        var id = Assembly.GetEntryAssembly().GetName().Name.ToLower();
        var sb = new StringBuilder($"dotnet tool update {id} --no-cache");
        if (Options.IsGlobal)
            sb.Append(" --global");
        else
            sb.Append(" --tool-path");
        // source
        var env_source = Environment.GetEnvironmentVariable(NUGET_SOURCE);
        if (string.IsNullOrEmpty(Options.Source))
        {
            Options.Source = env_source;
        }
        else
        {
            if (env_source != Options.Source)
            {
                LogUtil.Info($"设置环境变量 {NUGET_SOURCE}：{Options.Source}");
                if (AiUoUtil.IsWindowsOS)
                {
#pragma warning disable CA1416 // 验证平台兼容性
                    using var identity = WindowsIdentity.GetCurrent();
                    var principal = new WindowsPrincipal(identity);
                    var isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
                    if (!isElevated)
                    {
                        throw new Exception("需要管理员权限，请使用管理员权限重新运行。");
                    }
                    Environment.SetEnvironmentVariable(NUGET_SOURCE, Options.Source, EnvironmentVariableTarget.User);
#pragma warning restore CA1416 // 验证平台兼容性
                }
                else
                {
                    LinuxUtil.SetEnvironmentVariable(NUGET_SOURCE, Options.Source);
                }
                LogUtil.Info($"{NUGET_SOURCE}={Environment.GetEnvironmentVariable(NUGET_SOURCE)}");
            }
        }
        if (!string.IsNullOrEmpty(Options.Source))
            sb.Append($" --add-source {Options.Source}");
        // version
        if (!string.IsNullOrEmpty(Options.Version))
            sb.Append($" --version {Options.Version}");
        sb.Append(" -v m");
        var cmd = sb.ToString();
        LogUtil.Info($"执行命令：{cmd}");
        if (AiUoUtil.IsWindowsOS)
        {
            ExecuteWinCmd(cmd, DotNetToolUtil.ToolPath);
        }
        else
        {
            CliUtil.Execute(cmd);
        }
    }
    public static void ExecuteWinCmd(string cmd, string dir)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {cmd}",
            WorkingDirectory = dir,
            UseShellExecute = true,       //是否使用操作系统shell启动
            //RedirectStandardInput = false,  //接受来自调用程序的输入信息
            //RedirectStandardOutput = false, //由调用程序获取输出信息
            //RedirectStandardError = false,  //重定向标准错误输出
            //CreateNoWindow = true,         //不显示程序窗口
        };
        Process.Start(psi);
    }
}

[Verb("update", HelpText = "升级当前 dotnet tool 工具。如: update -s http://10.0.0.101:5555/v3/index.json")]
public class UpdateOptions
{
    [Option('g', "global", HelpText = "是否安装到全局，false表示安装或更新本地工具")]
    public bool IsGlobal { get; set; } = true;
    [Option('s', "source", HelpText = "工具所在 NuGet 包源")]
    public string Source { get; set; }
    [Option('v', "version", HelpText = "版本")]
    public string Version { get; set; }
}