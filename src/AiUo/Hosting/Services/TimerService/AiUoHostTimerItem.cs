using System;
using System.Threading;
using System.Threading.Tasks;

namespace AiUo.Hosting.Services;

public class AiUoHostTimerItem
{
    /// <summary>
    /// 执行间隔
    /// </summary>
    public int Interval { get; set; }
    /// <summary>
    /// 执行次数 0-无限
    /// </summary>
    public int ExecuteCount { get; set; }
    /// <summary>
    /// 当异常时重试的次数(默认3次，0:不重试,-1:永远重试)
    /// </summary>
    public int TryCount { get; set; } = 3;
    public Func<CancellationToken, Task> Callback { get; set; }

    /// <summary>
    /// 标识，用于移除
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// 描述，用于日志
    /// </summary>
    public string Title { get; set; }
}