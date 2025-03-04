using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AiUo.Configuration;
using AiUo.Hosting.Services;

namespace AiUo.Hosting;

public class AiUoHostedService : BackgroundService
{
    private readonly ILogger _logger;
    private IAiUoHostRegisterService _registerService;
    private IAiUoHostTimerService _timerService;
    private IAiUoHostLifetimeService _lifetimeService;
    public AiUoHostedService(ILogger<AiUoHostedService> logger, IAiUoHostRegisterService registerService, IAiUoHostTimerService timerService, IAiUoHostLifetimeService lifetimeService, IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        _registerService = registerService;
        _timerService = timerService;
        _lifetimeService = lifetimeService;
        // 1-StartAsync
        // 2-ApplicationStarted
        // 3-ApplicationStopping
        // 4-StopAsync
        // 5-ApplicationStopped
        lifetime.ApplicationStarted.Register(OnStarted);
        lifetime.ApplicationStopping.Register(OnStopping);
        lifetime.ApplicationStopped.Register(OnStopped);
        if (registerService.RegisterEnabled)
        {
            if (_timerService != null)
            {
                if (ConfigUtil.Host.HeartbeatInterval > 0)
                {
                    _timerService.Register(new AiUoHostTimerItem
                    {
                        Id = "IAiUoHostTimerService.Heartbeat",
                        Title = "Host心跳",
                        Interval = ConfigUtil.Host.HeartbeatInterval,
                        ExecuteCount = 0,
                        TryCount = -1,
                        Callback = (stoppingToken) => _registerService.Heartbeat()
                    });
                }
                if (ConfigUtil.Host.HeathInterval > 0)
                {
                    _timerService.Register(new AiUoHostTimerItem
                    {
                        Id = "IAiUoHostTimerService.Health",
                        Title = "Host检查",
                        Interval = ConfigUtil.Host.HeathInterval,
                        ExecuteCount = 0,
                        TryCount = -1,
                        Callback = (stoppingToken) => _registerService.Health()
                    });
                }
            }
        }
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        //_logger.LogDebug("启动 => 1. Host OnStarting");
        foreach (var item in _lifetimeService?.StartingTasks)
        {
            await item.Invoke();
        }
        await base.StartAsync(cancellationToken);
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //_logger.LogDebug("启动 => 2. Host ExecuteAsync");
        await _timerService?.StartAsync(stoppingToken);
    }
    private void OnStarted()
    {
        //_logger.LogDebug("启动 => 3. Host OnStarted");
        _registerService.Register().GetTaskResult();
        foreach (var item in _lifetimeService?.StartedTasks)
        {
            item.Invoke().GetTaskResult();
        }
        //_logger.LogDebug("==== 启动应用程序结束 ====");
    }
    private void OnStopping()
    {
        _logger.LogDebug("停止 => 4. Host OnStopping");
        if (_registerService.RegisterEnabled)
        {
            _registerService.Deregistering().GetTaskResult();
            _timerService?.Deregister(new List<string> {
                "IAiUoHostTimerService.Heartbeat",
                "IAiUoHostTimerService.Health"
            });
        }
        foreach (var item in _lifetimeService?.StoppingTasks)
        {
            item.Invoke().GetTaskResult();
        }
    }
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("停止 => 5. Host StopAsync");
        await _timerService?.StopAsync();
        if (_registerService.RegisterEnabled)
        {
            _registerService.Deregistered().GetTaskResult();
        }
        await base.StopAsync(cancellationToken);
    }
    private void OnStopped()
    {
        _logger.LogDebug("停止 => 6. Host OnStopped");
        foreach (var item in _lifetimeService?.StoppedTasks)
        {
            item.Invoke().GetTaskResult();
        }
    }
}