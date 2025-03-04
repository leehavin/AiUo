using Microsoft.Extensions.Logging;

namespace AiUo.Extensions.CommandLineParser;

public class CmdLineConfig
{
    /// <summary>
    /// 输出日志级别
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
    /// <summary>
    /// debug参数，仅用于开发调试
    /// </summary>
    public string DebugArgs { get; set; }
    /// <summary>
    /// 没有参数时使用的默认参数
    /// </summary>
    public string DefaultArgs { get; set; }
    /// <summary>
    /// 配置文件名
    /// </summary>
    public string ConfigFile { get; set; } = "tcmd.json";
}