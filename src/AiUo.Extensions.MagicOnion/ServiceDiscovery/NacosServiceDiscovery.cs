using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nacos.V2;
using Nacos.V2.Naming.Dtos;
using Nacos.V2.Naming;
using Nacos.V2.Naming.Event;

namespace AiUo.Extensions.MagicOnion.ServiceDiscovery;

/// <summary>
/// Nacos服务发现实现
/// </summary>
public class NacosServiceDiscovery : IServiceDiscovery, IDisposable
{
    private readonly INacosNamingService _namingService;
    private readonly ILogger<NacosServiceDiscovery>? _logger;
    private readonly Dictionary<string, ServiceChangeCallback> _subscriptions = new();
    private readonly Dictionary<string, EventListener> _eventListeners = new();
    private bool _disposed;

    /// <summary>
    /// 初始化Nacos服务发现
    /// </summary>
    /// <param name="namingService">Nacos命名服务</param>
    /// <param name="logger">日志记录器</param>
    public NacosServiceDiscovery(INacosNamingService namingService, ILogger<NacosServiceDiscovery>? logger = null)
    {
        _namingService = namingService ?? throw new ArgumentNullException(nameof(namingService));
        _logger = logger;
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
            _logger?.LogDebug("从Nacos获取服务实例列表：{ServiceName}", serviceName);
            var instances = await _namingService.GetAllInstances(serviceName);
            return instances.Select(ConvertToServiceInstance).ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "从Nacos获取服务实例列表失败：{ServiceName}", serviceName);
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
            _logger?.LogInformation("向Nacos注册服务实例：{ServiceName} {InstanceId} {Host}:{Port}", 
                instance.ServiceName, instance.InstanceId, instance.Host, instance.Port);
                
            var nacosInstance = new Instance
            {
                ServiceName = instance.ServiceName,
                Ip = instance.Host,
                Port = instance.Port,
                InstanceId = instance.InstanceId,
                Metadata = instance.Metadata,
                Weight = instance.Weight,
                Healthy = instance.IsHealthy,
                Enabled = true
            };
            
            await _namingService.RegisterInstance(instance.ServiceName, nacosInstance);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "向Nacos注册服务实例失败：{ServiceName} {InstanceId}", 
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
            _logger?.LogInformation("从Nacos注销服务实例：{ServiceName} {InstanceId}", 
                instance.ServiceName, instance.InstanceId);
                
            await _namingService.DeregisterInstance(
                instance.ServiceName,
                instance.Host,
                instance.Port,
                instance.InstanceId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "从Nacos注销服务实例失败：{ServiceName} {InstanceId}", 
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
        if (_subscriptions.ContainsKey(serviceName))
        {
            _logger?.LogWarning("服务{ServiceName}已经订阅，将覆盖现有订阅", serviceName);
            await UnsubscribeAsync(serviceName);
        }

        try
        {
            _logger?.LogInformation("订阅Nacos服务变更：{ServiceName}", serviceName);
            _subscriptions[serviceName] = callback;
            
            var listener = new EventListener(async instances =>
            {
                var serviceInstances = instances.Select(ConvertToServiceInstance).ToList();
                _logger?.LogDebug("Nacos服务{ServiceName}发生变更，实例数量：{Count}", serviceName, serviceInstances.Count);
                await callback(serviceInstances);
            });
            
            _eventListeners[serviceName] = listener;
            await _namingService.Subscribe(serviceName, listener);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "订阅Nacos服务变更失败：{ServiceName}", serviceName);
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
        if (!_subscriptions.ContainsKey(serviceName))
        {
            _logger?.LogWarning("服务{ServiceName}未订阅，无需取消", serviceName);
            return;
        }

        try
        {
            _logger?.LogInformation("取消订阅Nacos服务变更：{ServiceName}", serviceName);
            
            if (_eventListeners.TryGetValue(serviceName, out var listener))
            {
                await _namingService.Unsubscribe(serviceName, listener);
                _eventListeners.Remove(serviceName);
            }
            
            _subscriptions.Remove(serviceName);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "取消订阅Nacos服务变更失败：{ServiceName}", serviceName);
            throw;
        }
    }

    /// <summary>
    /// 将Nacos实例转换为服务实例
    /// </summary>
    /// <param name="instance">Nacos实例</param>
    /// <returns>服务实例</returns>
    private static ServiceInstance ConvertToServiceInstance(Instance instance)
    {
        return new ServiceInstance
        {
            ServiceName = instance.ServiceName,
            InstanceId = instance.InstanceId,
            Host = instance.Ip,
            Port = instance.Port,
            Metadata = instance.Metadata ?? new Dictionary<string, string>(),
            IsHealthy = instance.Healthy,
            Weight = instance.Weight
        };
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        foreach (var serviceName in _subscriptions.Keys.ToList())
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
        _eventListeners.Clear();
        _disposed = true;
        
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Nacos事件监听器
    /// </summary>
    private class EventListener : IEventListener
    {
        private readonly Func<List<Instance>, Task> _callback;

        public EventListener(Func<List<Instance>, Task> callback)
        {
            _callback = callback;
        }

        public Task OnEvent(IEvent @event)
        {
            if (@event is Nacos.V2.Naming.Event.InstancesChangeEvent instancesChangeEvent)
            {
                return _callback(instancesChangeEvent.Hosts);
            }

            return Task.CompletedTask;
        }
    }
}