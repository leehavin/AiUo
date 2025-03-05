using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiUo.Extensions.MagicOnion.ServiceDiscovery;

/// <summary>
/// Consul服务发现实现
/// </summary>
public class ConsulServiceDiscovery : IServiceDiscovery, IDisposable
{
    private readonly ConsulClient _consulClient;
    private readonly ILogger<ConsulServiceDiscovery>? _logger;
    private readonly Dictionary<string, System.Threading.CancellationTokenSource> _subscriptions = new();
    private readonly Dictionary<string, ServiceChangeCallback> _callbacks = new();
    private bool _disposed;

    /// <summary>
    /// 初始化Consul服务发现
    /// </summary>
    /// <param name="options">Consul配置选项</param>
    /// <param name="logger">日志记录器</param>
    public ConsulServiceDiscovery(IOptions<ConsulServiceDiscoveryOptions> options, ILogger<ConsulServiceDiscovery>? logger = null)
    {
        var config = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;

        _consulClient = new ConsulClient(cfg =>
        {
            cfg.Address = new Uri(config.Address);
            
            if (!string.IsNullOrEmpty(config.Token))
            {
                cfg.Token = config.Token;
            }

            if (config.Datacenter != null)
            {
                cfg.Datacenter = config.Datacenter;
            }

            if (config.WaitTime.HasValue)
            {
                cfg.WaitTime = config.WaitTime.Value;
            }
        });
    }

    /// <summary>
    /// 获取服务实例列表
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <returns>服务实例列表</returns>
    public async Task<List<ServiceInstance>> GetServiceInstancesAsync(string serviceName)
    {
        try
        {
            _logger?.LogDebug("从Consul获取服务实例列表：{ServiceName}", serviceName);
            var queryResult = await _consulClient.Health.Service(serviceName, string.Empty, true);
            return queryResult.Response
                .Select(serviceEntry => ConvertToServiceInstance(serviceEntry, serviceName))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "从Consul获取服务实例列表失败：{ServiceName}", serviceName);
            return new List<ServiceInstance>();
        }
    }

    /// <summary>
    /// 注册服务实例
    /// </summary>
    /// <param name="instance">服务实例信息</param>
    /// <returns>注册结果</returns>
    public async Task RegisterServiceAsync(ServiceInstance instance)
    {
        try
        {
            _logger?.LogInformation("向Consul注册服务实例：{ServiceName} {InstanceId} {Host}:{Port}", 
                instance.ServiceName, instance.InstanceId, instance.Host, instance.Port);

            var registration = new AgentServiceRegistration
            {
                ID = instance.InstanceId,
                Name = instance.ServiceName,
                Address = instance.Host,
                Port = instance.Port,
                Tags = instance.Metadata?.Keys.ToArray(),
                Meta = instance.Metadata,
                Check = new AgentServiceCheck
                {
                    // 默认使用HTTP健康检查
                    HTTP = $"http://{instance.Host}:{instance.Port}/health",
                    Interval = TimeSpan.FromSeconds(10),
                    Timeout = TimeSpan.FromSeconds(5),
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
                }
            };

            // 如果元数据中包含健康检查配置，则使用自定义配置
            if (instance.Metadata != null)
            {
                if (instance.Metadata.TryGetValue("health_check_type", out var checkType))
                {
                    switch (checkType.ToLower())
                    {
                        case "grpc":
                            if (instance.Metadata.TryGetValue("health_check_path", out var grpcPath))
                            {
                                registration.Check = new AgentServiceCheck
                                {
                                    GRPC = $"{instance.Host}:{instance.Port}{grpcPath}",
                                    GRPCUseTLS = instance.Metadata.TryGetValue("use_tls", out var useTls) && 
                                                bool.TryParse(useTls, out var tlsValue) && tlsValue,
                                    Interval = TimeSpan.FromSeconds(10),
                                    Timeout = TimeSpan.FromSeconds(5),
                                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
                                };
                            }
                            break;
                        case "tcp":
                            registration.Check = new AgentServiceCheck
                            {
                                TCP = $"{instance.Host}:{instance.Port}",
                                Interval = TimeSpan.FromSeconds(10),
                                Timeout = TimeSpan.FromSeconds(5),
                                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
                            };
                            break;
                    }
                }
            }

            await _consulClient.Agent.ServiceRegister(registration);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "向Consul注册服务实例失败：{ServiceName} {InstanceId}", 
                instance.ServiceName, instance.InstanceId);
            throw;
        }
    }

    /// <summary>
    /// 注销服务实例
    /// </summary>
    /// <param name="instance">服务实例信息</param>
    /// <returns>注销结果</returns>
    public async Task DeregisterServiceAsync(ServiceInstance instance)
    {
        try
        {
            _logger?.LogInformation("从Consul注销服务实例：{ServiceName} {InstanceId}", 
                instance.ServiceName, instance.InstanceId);
                
            await _consulClient.Agent.ServiceDeregister(instance.InstanceId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "从Consul注销服务实例失败：{ServiceName} {InstanceId}", 
                instance.ServiceName, instance.InstanceId);
            throw;
        }
    }

    /// <summary>
    /// 订阅服务变更
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <param name="callback">变更回调</param>
    /// <returns>订阅结果</returns>
    public async Task SubscribeAsync(string serviceName, ServiceChangeCallback callback)
    {
        if (_callbacks.ContainsKey(serviceName))
        {
            _logger?.LogWarning("服务{ServiceName}已经订阅，将覆盖现有订阅", serviceName);
            await UnsubscribeAsync(serviceName);
        }

        try
        {
            _logger?.LogInformation("订阅Consul服务变更：{ServiceName}", serviceName);
            _callbacks[serviceName] = callback;

            // 启动服务监控
            // 初始查询获取当前服务实例
            var queryOptions = new QueryOptions { WaitIndex = 0 };
            var queryResult = await _consulClient.Health.Service(serviceName, string.Empty, true, queryOptions);
            var instances = queryResult.Response
                .Select(serviceEntry => ConvertToServiceInstance(serviceEntry, serviceName))
                .ToList();
            
            // 通知初始服务实例
            await callback(instances);
            
            // 启动长轮询监控服务变更
            var cancellationTokenSource = new System.Threading.CancellationTokenSource();
            _subscriptions[serviceName] = cancellationTokenSource;
            
            // 在后台任务中监控服务变更
            _ = Task.Run(async () =>
            {
                var waitIndex = queryResult.LastIndex;
                
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        queryOptions = new QueryOptions { WaitIndex = waitIndex };
                        var result = await _consulClient.Health.Service(serviceName, string.Empty, true, queryOptions);
                        
                        // 如果索引变化，表示服务列表有更新
                        if (result.LastIndex > waitIndex)
                        {
                            waitIndex = result.LastIndex;
                            var updatedInstances = result.Response
                                .Select(serviceEntry => ConvertToServiceInstance(serviceEntry, serviceName))
                                .ToList();
                                
                            _logger?.LogDebug("Consul服务{ServiceName}发生变更，实例数量：{Count}", serviceName, updatedInstances.Count);
                            await callback(updatedInstances);
                        }
                    }
                    catch (Exception ex) when (!cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        _logger?.LogError(ex, "监控Consul服务{ServiceName}变更时发生错误", serviceName);
                        await Task.Delay(1000, cancellationTokenSource.Token);
                    }
                }
            }, cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "订阅Consul服务变更失败：{ServiceName}", serviceName);
            throw;
        }
    }

    /// <summary>
    /// 取消订阅服务变更
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <returns>取消订阅结果</returns>
    public async Task UnsubscribeAsync(string serviceName)
    {
        if (!_callbacks.ContainsKey(serviceName))
        {
            _logger?.LogWarning("服务{ServiceName}未订阅，无需取消", serviceName);
            return;
        }

        try
        {
            _logger?.LogInformation("取消订阅Consul服务变更：{ServiceName}", serviceName);
            
            if (_subscriptions.TryGetValue(serviceName, out var cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                _subscriptions.Remove(serviceName);
            }
            
            _callbacks.Remove(serviceName);
            await Task.CompletedTask; // 保持接口一致性
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "取消订阅Consul服务变更失败：{ServiceName}", serviceName);
            throw;
        }
    }

    /// <summary>
    /// 将Consul服务条目转换为服务实例
    /// </summary>
    /// <param name="serviceEntry">Consul服务条目</param>
    /// <param name="serviceName">服务名称</param>
    /// <returns>服务实例</returns>
    private static ServiceInstance ConvertToServiceInstance(ServiceEntry serviceEntry, string serviceName)
    {
        var service = serviceEntry.Service;
        var health = serviceEntry.Checks.All(c => c.Status == HealthStatus.Passing);
        
        return new ServiceInstance
        {
            ServiceName = serviceName,
            InstanceId = service.ID,
            Host = service.Address,
            Port = service.Port,
            Metadata = service.Meta?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, string>(),
            IsHealthy = health,
            Weight = 1.0 // Consul没有内置权重概念，默认为1
        };
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        foreach (var serviceName in _callbacks.Keys.ToList())
        {
            try
            {
                UnsubscribeAsync(serviceName).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "释放资源时取消订阅服务失败：{ServiceName}", serviceName);
            }
        }

        _subscriptions.Clear();
        _callbacks.Clear();
        _consulClient.Dispose();
        _disposed = true;
        
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Consul服务发现配置选项
/// </summary>
public class ConsulServiceDiscoveryOptions
{
    /// <summary>
    /// Consul服务器地址
    /// </summary>
    public string Address { get; set; } = "http://localhost:8500";
    
    /// <summary>
    /// 访问令牌
    /// </summary>
    public string? Token { get; set; }
    
    /// <summary>
    /// 数据中心
    /// </summary>
    public string? Datacenter { get; set; }
    
    /// <summary>
    /// 等待时间
    /// </summary>
    public TimeSpan? WaitTime { get; set; }
}