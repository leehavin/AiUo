using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AiUo.Configuration;
using AiUo.Logging;
using AiUo.Text;

namespace AiUo.Hosting.Services;

public class DefaultAiUoHostTimerService : IAiUoHostTimerService
{
    /// <summary>
    /// 停止服务时，等待运行中的timer任务的timeout时间
    /// </summary>
    private int _waitTasksTimeout;
    private int _minDelayInterval;

    private ConcurrentDictionary<string, AiUoHostTimerJob> _jobs = new();
    private readonly ConcurrentDictionary<int, Task> _taskDict = new();
    private CancellationTokenSource _stoppingCts = new CancellationTokenSource(); //终止通知
    private CancellationTokenSource _changeCts = new CancellationTokenSource();//改变通知

    public DefaultAiUoHostTimerService()
    {
        _waitTasksTimeout = ConfigUtil.Host.TimerWaitTimeout;
        _minDelayInterval = ConfigUtil.Host.TimerMinDelay;
    }

    #region Method
    /// <summary>
    /// 注册timer任务
    /// </summary>
    /// <param name="item"></param>
    /// <param name="tryUpdate"></param>
    /// <returns>如果tryUpdate=true，则返回false意味着update</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public bool Register(AiUoHostTimerItem item, bool tryUpdate = false)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item), $"DefaultAiUoHostTimerService.Register时item不能为空");
        if (item.Callback == null)
            throw new Exception("DefaultAiUoHostTimerService.Register时Callback不能为空");

        var ret = true;
        item.Id ??= ObjectId.NewId();
        var value = new AiUoHostTimerJob
        {
            Id = item.Id,
            Title = item.Title,
            Interval = item.Interval,
            ExecuteCount = item.ExecuteCount,
            TryCount = item.TryCount,
            Callback = item.Callback,
            Remain = item.Interval,
            Timestamp = DateTime.UtcNow.ToTimestamp(true)
        };
        if (tryUpdate)
        {
            _jobs.AddOrUpdate(item.Id, value, (k, v) =>
            {
                ret = false;
                return value;
            });
        }
        else
        {
            if (!_jobs.TryAdd(item.Id, value))
                throw new Exception($"DefaultAiUoHostTimerService.Register时Id重复,可使用RegisterOrUpdate。Id:{item.Id}");
        }
        _changeCts.Cancel();
        return ret;
    }

    public bool Deregister(List<string> ids)
    {
        var ret = false;
        foreach (var id in ids)
        {
            ret = _jobs.TryRemove(id, out _) || ret;
        }
        if (ret)
            _changeCts.Cancel();
        return ret;
    }
    /// <summary>
    /// 注销timer任务
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool Deregister(string id)
    {
        var ret = _jobs.TryRemove(id, out var job);
        if (ret)
            _changeCts.Cancel();
        return ret;
    }
    #endregion

    public async Task StartAsync(CancellationToken stoppingToken = default)
    {
        // 外层停止，通知内层停止
        stoppingToken.Register(() =>
        {
            _stoppingCts.Cancel();
        });
        LogUtil.Info("启动 => Host定时服务[IAiUoHostTimerService]");
        int interval = 0;
        while (!_stoppingCts.IsCancellationRequested)
        {
            int nextInterval = int.MaxValue;
            //var jobs = GetJobs().ToList().OrderBy(x => x.Timestamp);
            var jobs = GetJobs();
            foreach (var job in jobs)
            {
                try
                {
                    if (job.TryExecute(interval, _stoppingCts.Token, out Task task))
                    {
                        if (job.IsCycleEnd)//循环结束
                            _jobs.TryRemove(job.Id, out var _);
                        var taskId = task.GetHashCode();
                        _taskDict.TryAdd(taskId, task);
                        var _ = task.ContinueWith(t =>
                        {
                            // IsCompleted=true时, Task.Status=RanToCompletion,Canceled或者Faulted
                            if (t.Status == TaskStatus.RanToCompletion) // 成功
                            {
                                LogUtil.Debug("HostTimer任务执行成功: [{Id}]-{Title} Interval:{Interval} Count:{CurrentCount}/{ExecuteCount} Error:{ErrorCount}/{TryCount} ThreadId:{ThreadId}"
                                    , job.Id, job.Title, job.Interval, job.CurrentCount, job.ExecuteCount, job.ErrorCount, job.TryCount, Thread.CurrentThread.ManagedThreadId);
                                job.ErrorCount = 0;
                            }
                            else if (t.Status == TaskStatus.Canceled) // 取消
                            {
                                LogUtil.Warning("HostTimer任务执行取消: [{Id}]-{Title} Interval:{Interval} Count:{CurrentCount}/{ExecuteCount} Error:{ErrorCount}/{TryCount} ThreadId:{ThreadId}"
                                    , job.Id, job.Title, job.Interval, job.CurrentCount, job.ExecuteCount, job.ErrorCount, job.TryCount, Thread.CurrentThread.ManagedThreadId);
                                job.ErrorCount = 0;
                            }
                            else if (t.Status == TaskStatus.Faulted) // 失败
                            {
                                // 超过重试次数移除!
                                job.ErrorCount++;
                                LogUtil.Error(t.Exception, "HostTimer任务执行异常: [{Id}]-{Title} Interval:{Interval} Count:{CurrentCount}/{ExecuteCount} Error:{ErrorCount}/{TryCount} ThreadId:{ThreadId}"
                                    , job.Id, job.Title, job.Interval, job.CurrentCount, job.ExecuteCount, job.ErrorCount, job.TryCount, Thread.CurrentThread.ManagedThreadId);
                                if (job.TryCount == 0 || (job.TryCount > 0 && job.ErrorCount >= job.TryCount))
                                    _jobs.TryRemove(job.Id, out var _);
                            }
                            _taskDict.TryRemove(taskId, out var _);
                        });
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.Error(ex, $"DefaultAiUoHostTimerService.StartAsync执行中出现异常，必须处理!!! id:{job.Id} title:{job.Title}");
                }

                if (!job.IsCycleEnd)
                    nextInterval = Math.Min(nextInterval, job.Remain);
            }
            interval = Math.Max(_minDelayInterval, nextInterval);
            await Task.Delay(TimeSpan.FromMilliseconds(interval), _changeCts.Token).ContinueWith((t) =>
            {
                if (t.Status == TaskStatus.Canceled)
                {
                    interval = 0;
                    _changeCts = new CancellationTokenSource();
                }
            }, stoppingToken);
        }
    }
    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _stoppingCts.Cancel();
        _changeCts.Cancel();
        var tasks = GetTasks().ToArray();
        if (!Task.WaitAll(tasks, _waitTasksTimeout, cancellationToken))
        {
            foreach (var task in tasks)
            {
                if (!task.IsCompletedSuccessfully)
                {
                    LogUtil.Error(task.Exception, $"DefaultAiUoHostTimerService.StopAsync时异常: status: {task.Status}");
                }
            }
        }
        LogUtil.Info("停止 => Host定时服务[IAiUoHostTimerService]");
        return Task.CompletedTask;
    }

    #region Utils
    private IEnumerable<Task> GetTasks()
    {
        var enumtor = _taskDict.GetEnumerator();
        while (enumtor.MoveNext())
        {
            yield return enumtor.Current.Value;
        }
    }
    private IEnumerable<AiUoHostTimerJob> GetJobs()
    {
        var enumtor = _jobs.GetEnumerator();
        while (enumtor.MoveNext())
        {
            yield return enumtor.Current.Value;
        }
    }
    #endregion
}