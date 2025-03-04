using System;

namespace AiUo.CliWrap;

public class CliResult
{
    /// <summary>
    /// 退出码：0-成功 1-错误
    /// </summary>
    public int ExitCode { get; set; }
    public bool Success => ExitCode == 0;
    /// <summary>
    /// 起始时间
    /// </summary>
    public DateTimeOffset StartTime { get; set; }
    /// <summary>
    /// 退出时间
    /// </summary>
    public DateTimeOffset ExitTime { get; set; }
    /// <summary>
    /// 运行时间
    /// </summary>
    public TimeSpan RunTime { get; set; }
    public string Output { get; set; }
    public string Error { get; set; }
}