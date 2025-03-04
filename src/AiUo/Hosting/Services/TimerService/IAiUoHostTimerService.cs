using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AiUo.Hosting.Services;

/// <summary>
/// Host定时任务服务
/// </summary>
public interface IAiUoHostTimerService
{
    bool Register(AiUoHostTimerItem item, bool tryUpdate = false);
    bool Deregister(string id);
    bool Deregister(List<string> ids);
    Task StartAsync(CancellationToken stoppingToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}