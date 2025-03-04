using System.Threading;
using System.Threading.Tasks;
using AiUo.Logging;

namespace AiUo.Hosting.Services;

public class AiUoHostTimerJob : AiUoHostTimerItem
{
    /// <summary>
    /// 加入任务utc时间戳
    /// </summary>
    public long Timestamp { get; set; }

    private int _remain;
    /// <summary>
    /// 下次执行剩余毫秒数
    /// </summary>
    public int Remain { get { return _remain; } set { _remain = value; } }
    /// <summary>
    /// 是否应该立刻执行
    /// </summary>
    public bool CanExecute => Remain <= 0;

    /// <summary>
    /// 当前已执行次数
    /// </summary>
    public int CurrentCount = 0;
    /// <summary>
    /// 当前异常次数
    /// </summary>
    public int ErrorCount { get; set; }
    /// <summary>
    /// 是否循环完毕（移除Work）
    /// </summary>
    public bool IsCycleEnd => ExecuteCount > 0 && ExecuteCount <= CurrentCount;
    public bool TryExecute(int processInterval, CancellationToken stoppingToken, out Task task)
    {
        task = null;
        Interlocked.Add(ref _remain, -processInterval);
        if (!CanExecute)
            return false;
        Interlocked.Exchange(ref _remain, Interval);
        if (ExecuteCount > 0)
            Interlocked.Add(ref CurrentCount, 1);
        LogUtil.Debug("HostTimer任务执行开始: [{Id}]-{Title} Interval:{Interval} Count:{CurrentCount}/{ExecuteCount} Error:{ErrorCount}/{TryCount}"
            , Id, Title, Interval, CurrentCount, ExecuteCount, ErrorCount, TryCount);
        task = Task.Run(() => Callback(stoppingToken));
        return true;
    }
}