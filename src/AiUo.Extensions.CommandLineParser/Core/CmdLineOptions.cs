using Microsoft.Extensions.Logging;

namespace AiUo.Extensions.CommandLineParser;

/// <summary>
/// 从配置文件中读取的配置信息，如果存在则覆盖传入的CmdLineConfig
/// </summary>
public class CmdLineOptions
{
    /// <summary>
    /// 输出日志级别
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
    /// <summary>
    /// 没有参数时使用的默认参数
    /// </summary>
    public string DefaultArgs { get; set; }
}