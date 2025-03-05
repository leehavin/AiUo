using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AiUo.Extensions.MagicOnion.ServiceDiscovery;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace AiUo.Extensions.MagicOnion;

/// <summary>
/// MagicOnion客户端工厂
/// </summary>
public class MagicOnionClientFactory : IDisposable
{
    private readonly MagicOnionOptions _options;
    private readonly ConcurrentDictionary<string, object> _clients = new();
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
    private readonly ILogger<MagicOnionClientFactory>? _logger;
    private readonly ConcurrentDictionary<string, GrpcChannel> _channelPool = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly CancellationTokenSource _idleMonitorCts = new();
    private readonly ConcurrentDictionary<string, DateTime> _lastAccessTime = new();
    private readonly ConcurrentDictionary<string, List<ServiceInstance>> _serviceInstances = new();
    private readonly ConcurrentDictionary<string, int> _roundRobinCounters = new();
    private readonly Random _random = new();
    private readonly IServiceDiscovery? _serviceDiscovery;
    private bool _disposed;

    /// <summary>
    /// 初始化MagicOnion客户端工厂
    /// </summary>
    /// <param name="options">MagicOnion配置选项</param>
    /// <param name="serviceDiscovery">服务发现接口（可选）</param>
    /// <param name="logger">日志记录器</param>
    public MagicOnionClientFactory(IOptions<MagicOnionOptions> options, IServiceDiscovery? serviceDiscovery = null, ILogger<MagicOnionClientFactory>? logger = null)
    {
        _options = options.Value;
        _logger = logger;
        _serviceDiscovery = serviceDiscovery;
        
        // 配置重试策略
        _retryPolicy = Policy
            .Handle<Exception>(ex => !(ex is RpcException rpcEx && 
                                     (rpcEx.StatusCode == StatusCode.InvalidArgument || 
                                      rpcEx.StatusCode == StatusCode.AlreadyExists ||
                                      rpcEx.StatusCode == StatusCode.FailedPrecondition)))
            .WaitAndRetryAsync(
                _options.MaxRetryCount,
                retryAttempt => TimeSpan.FromMilliseconds(_options.RetryInterval * Math.Pow(2, retryAttempt - 1)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger?.LogWarning(exception, "MagicOnion调用失败，正在进行第{RetryCount}次重试，等待{RetryInterval}ms", 
                        retryCount, timeSpan.TotalMilliseconds);
                });
                
        // 配置熔断器策略
        _circuitBreakerPolicy = Policy
            .Handle<Exception>(ex => !(ex is RpcException rpcEx && 
                                     (rpcEx.StatusCode == StatusCode.InvalidArgument || 
                                      rpcEx.StatusCode == StatusCode.AlreadyExists ||
                                      rpcEx.StatusCode == StatusCode.FailedPrecondition)))
            .CircuitBreakerAsync(
                _options.CircuitBreakerThreshold,
                TimeSpan.FromMilliseconds(_options.CircuitBreakerResetTimeout),
                onBreak: (ex, breakDelay) =>
                {
                    _logger?.LogError(ex, "MagicOnion服务熔断器已触发，熔断时长：{BreakDelay}ms", breakDelay.TotalMilliseconds);
                },
                onReset: () =>
                {
                    _logger?.LogInformation("MagicOnion服务熔断器已重置，服务恢复正常");
                },
                onHalfOpen: () =>
                {
                    _logger?.LogInformation("MagicOnion服务熔断器处于半开状态，正在尝试恢复服务");
                });
                
        // 启动空闲连接监控
        if (_options.IdleTimeout > 0)
        {
            Task.Run(MonitorIdleConnectionsAsync);
        }
        
        // 如果启用了服务发现，初始化服务发现
        if (_options.ServiceDiscovery.Enabled && _serviceDiscovery != null && !string.IsNullOrEmpty(_options.ServiceDiscovery.ServiceName))
        {
            InitializeServiceDiscoveryAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 创建MagicOnion客户端
    /// </summary>
    /// <typeparam name="TService">服务接口类型</typeparam>
    /// <param name="serverAddress">可选的服务器地址，如果不指定则使用默认配置</param>
    /// <returns>MagicOnion客户端</returns>
    public TService CreateClient<TService>(string? serverAddress = null)
        where TService : IMagicOnionService<TService>
    {
        string key = typeof(TService).FullName + (serverAddress ?? string.Empty);
        return (TService)_clients.GetOrAdd(key, _ =>
        {
            var channel = GetOrCreateChannel(serverAddress);
            UpdateLastAccessTime(serverAddress ?? _options.GetAddress());
            _logger?.LogDebug("创建MagicOnion客户端：{ServiceType}", typeof(TService).FullName);
            return MagicOnionClient.Create<TService>(channel);
        });
    }

    /// <summary>
    /// 初始化服务发现
    /// </summary>
    private async Task InitializeServiceDiscoveryAsync()
    {
        try
        {
            if (_serviceDiscovery == null || !_options.ServiceDiscovery.Enabled) return;
            
            string serviceName = _options.ServiceDiscovery.ServiceName;
            _logger?.LogInformation("初始化服务发现，服务名称：{ServiceName}", serviceName);
            
            // 获取初始服务实例列表
            var instances = await _serviceDiscovery.GetServiceInstancesAsync(serviceName);
            UpdateServiceInstances(serviceName, instances);
            
            // 订阅服务变更
            await _serviceDiscovery.SubscribeAsync(serviceName, instances =>
            {
                UpdateServiceInstances(serviceName, instances);
                return Task.CompletedTask;
            });
            
            // 定期刷新服务实例列表
            if (_options.ServiceDiscovery.RefreshInterval > 0)
            {
                _ = Task.Run(async () =>
                {
                    while (!_disposed)
                    {
                        try
                        {
                            await Task.Delay(_options.ServiceDiscovery.RefreshInterval);
                            if (_disposed) break;
                            
                            var refreshedInstances = await _serviceDiscovery.GetServiceInstancesAsync(serviceName);
                            UpdateServiceInstances(serviceName, refreshedInstances);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "刷新服务实例列表失败：{ServiceName}", serviceName);
                        }
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "初始化服务发现失败");
        }
    }
    
    /// <summary>
    /// 更新服务实例列表
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <param name="instances">服务实例列表</param>
    private void UpdateServiceInstances(string serviceName, List<ServiceInstance> instances)
    {
        try
        {
            // 过滤实例
            var filteredInstances = instances
                .Where(i => !_options.ServiceDiscovery.OnlyHealthyInstances || i.IsHealthy);
                
            // 应用自定义过滤器
            if (_options.ServiceDiscovery.InstanceFilter != null)
            {
                filteredInstances = filteredInstances.Where(_options.ServiceDiscovery.InstanceFilter);
            }
            
            var finalInstances = filteredInstances.ToList();
            
            _logger?.LogDebug("更新服务实例列表：{ServiceName}，实例数量：{Count}", serviceName, finalInstances.Count);
            _serviceInstances[serviceName] = finalInstances;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "更新服务实例列表失败：{ServiceName}", serviceName);
        }
    }
    
    /// <summary>
    /// 获取或创建gRPC通道
    /// </summary>
    /// <param name="serverAddress">服务器地址，如果为null则使用默认配置</param>
    /// <returns>gRPC通道</returns>
    private GrpcChannel GetOrCreateChannel(string? serverAddress = null)
    {
        string address;
        
        // 如果指定了服务器地址，直接使用
        if (serverAddress != null)
        {
            address = serverAddress;
        }
        // 如果启用了服务发现，从服务发现中获取地址
        else if (_options.ServiceDiscovery.Enabled && _serviceDiscovery != null && 
                 _serviceInstances.TryGetValue(_options.ServiceDiscovery.ServiceName, out var instances) && 
                 instances.Count > 0)
        {
            var instance = SelectServiceInstance(instances, _options.ServiceDiscovery.LoadBalancingStrategy);
            address = $"{instance.Host}:{instance.Port}";
            _logger?.LogDebug("使用服务发现选择服务器：{ServerAddress}", address);
        }
        // 如果启用了负载均衡且没有指定服务器地址，则从服务器列表中选择一个
        else if (_options.EnableLoadBalancing && _options.Servers.Count > 0)
        {
            // 简单的轮询负载均衡
            int serverIndex = Environment.TickCount % _options.Servers.Count;
            address = _options.Servers[serverIndex];
            _logger?.LogDebug("使用负载均衡选择服务器：{ServerAddress}", address);
        }
        else
        {
            address = _options.GetAddress();
        }
        
        return _channelPool.GetOrAdd(address, CreateChannel);
    }
    
    /// <summary>
    /// 创建gRPC通道
    /// </summary>
    /// <param name="address">服务器地址</param>
    /// <returns>gRPC通道</returns>
    private GrpcChannel CreateChannel(string address)
    {
        var channelOptions = new GrpcChannelOptions
        {
            MaxReceiveMessageSize = null, // 无限制
            MaxSendMessageSize = null, // 无限制
            DisposeHttpClient = true
        };

        // 添加连接超时设置
        if (_options.ConnectionTimeout > 0)
        {
            channelOptions.HttpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMilliseconds(_options.ConnectionTimeout)
            };
        }

        var uri = address.Contains("://") ? address : 
                 (_options.UseTls ? $"https://{address}" : $"http://{address}");

        _logger?.LogInformation("创建新的gRPC通道：{Uri}", uri);
        return GrpcChannel.ForAddress(uri, channelOptions);
    }

    /// <summary>
    /// 使用重试策略和熔断器执行操作
    /// </summary>
    /// <typeparam name="TResult">返回结果类型</typeparam>
    /// <param name="func">要执行的操作</param>
    /// <returns>操作结果</returns>
    public async Task<TResult> ExecuteWithPoliciesAsync<TResult>(Func<Task<TResult>> func)
    {
        return await _circuitBreakerPolicy.WrapAsync(_retryPolicy).ExecuteAsync(func);
    }
    
    /// <summary>
    /// 使用重试策略执行操作
    /// </summary>
    /// <typeparam name="TResult">返回结果类型</typeparam>
    /// <param name="func">要执行的操作</param>
    /// <returns>操作结果</returns>
    public async Task<TResult> ExecuteWithRetryAsync<TResult>(Func<Task<TResult>> func)
    {
        return await _retryPolicy.ExecuteAsync(func);
    }
    
    /// <summary>
    /// 更新通道最后访问时间
    /// </summary>
    /// <param name="address">服务器地址</param>
    private void UpdateLastAccessTime(string address)
    {
        _lastAccessTime[address] = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 监控并清理空闲连接
    /// </summary>
    private async Task MonitorIdleConnectionsAsync()
    {
        try
        {
            while (!_idleMonitorCts.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), _idleMonitorCts.Token);
                await CleanupIdleConnectionsAsync();
            }
        }
        catch (OperationCanceledException) when (_idleMonitorCts.IsCancellationRequested)
        {
            // 正常取消，不做处理
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "监控空闲连接时发生异常");
        }
    }
    
    /// <summary>
    /// 清理空闲连接
    /// </summary>
    private async Task CleanupIdleConnectionsAsync()
    {
        if (_options.IdleTimeout <= 0) return;
        
        try
        {
            await _semaphore.WaitAsync(_idleMonitorCts.Token);
            
            var now = DateTime.UtcNow;
            var idleThreshold = TimeSpan.FromMilliseconds(_options.IdleTimeout);
            var idleChannels = _lastAccessTime
                .Where(kvp => (now - kvp.Value) > idleThreshold)
                .Select(kvp => kvp.Key)
                .ToList();
                
            foreach (var address in idleChannels)
            {
                if (_channelPool.TryRemove(address, out var channel))
                {
                    _lastAccessTime.TryRemove(address, out _);
                    await channel.ShutdownAsync();
                    _logger?.LogInformation("已关闭空闲通道：{Address}", address);
                }
            }
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            _logger?.LogError(ex, "清理空闲连接时发生异常");
        }
        finally
        {
            if (_semaphore.CurrentCount == 0)
            {
                _semaphore.Release();
            }
        }
    }

    /// <summary>
    /// 根据负载均衡策略选择服务实例
    /// </summary>
    /// <param name="instances">服务实例列表</param>
    /// <param name="strategy">负载均衡策略</param>
    /// <returns>选择的服务实例</returns>
    private ServiceInstance SelectServiceInstance(List<ServiceInstance> instances, LoadBalancingStrategy strategy)
    {
        if (instances.Count == 0)
        {
            throw new InvalidOperationException("没有可用的服务实例");
        }
        
        if (instances.Count == 1)
        {
            return instances[0];
        }
        
        string serviceName = _options.ServiceDiscovery.ServiceName;
        
        switch (strategy)
        {
            case LoadBalancingStrategy.Random:
                return instances[_random.Next(instances.Count)];
                
            case LoadBalancingStrategy.WeightedRandom:
                // 计算权重总和
                double totalWeight = instances.Sum(i => i.Weight);
                double randomValue = _random.NextDouble() * totalWeight;
                double weightSum = 0;
                
                foreach (var instance in instances)
                {
                    weightSum += instance.Weight;
                    if (randomValue <= weightSum)
                    {
                        return instance;
                    }
                }
                
                return instances.Last();
                
            case LoadBalancingStrategy.WeightedRoundRobin:
                // 实现加权轮询
                int currentCount = _roundRobinCounters.GetOrAdd(serviceName, 0);
                
                // 计算最大公约数和权重总和
                double gcd = CalculateGCD(instances.Select(i => i.Weight).ToArray());
                double maxWeight = instances.Max(i => i.Weight);
                int weightSum2 = (int)instances.Sum(i => i.Weight / gcd);
                
                int index = currentCount % weightSum2;
                _roundRobinCounters[serviceName] = (currentCount + 1) % weightSum2;
                
                // 根据权重选择实例
                int currentWeight = 0;
                for (int i = 0; i < instances.Count; i++)
                {
                    currentWeight += (int)(instances[i].Weight / gcd);
                    if (index < currentWeight)
                    {
                        return instances[i];
                    }
                }
                
                return instances.First();
                
            case LoadBalancingStrategy.LeastConnections:
                // 最少连接策略，这里简化实现，实际应该跟踪每个实例的连接数
                return instances.OrderBy(i => _random.Next()).First();
                
            case LoadBalancingStrategy.RoundRobin:
            default:
                // 简单轮询
                int current = _roundRobinCounters.GetOrAdd(serviceName, 0);
                int nextIndex = current % instances.Count;
                _roundRobinCounters[serviceName] = (current + 1) % instances.Count;
                return instances[nextIndex];
        }
    }
    
    /// <summary>
    /// 计算最大公约数
    /// </summary>
    /// <param name="weights">权重数组</param>
    /// <returns>最大公约数</returns>
    private double CalculateGCD(double[] weights)
    {
        if (weights.Length == 0) return 1;
        if (weights.Length == 1) return weights[0];
        
        double result = weights[0];
        for (int i = 1; i < weights.Length; i++)
        {
            result = GCD(result, weights[i]);
        }
        
        return result;
    }
    
    /// <summary>
    /// 计算两个数的最大公约数
    /// </summary>
    /// <param name="a">第一个数</param>
    /// <param name="b">第二个数</param>
    /// <returns>最大公约数</returns>
    private double GCD(double a, double b)
    {
        // 为了处理小数，将数值放大后取整
        const int precision = 1000;
        int intA = (int)(a * precision);
        int intB = (int)(b * precision);
        
        while (intB != 0)
        {
            int temp = intB;
            intB = intA % intB;
            intA = temp;
        }
        
        return intA / (double)precision;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _idleMonitorCts.Cancel();
        _idleMonitorCts.Dispose();
        _semaphore.Dispose();

        foreach (var channel in _channelPool.Values)
        {
            try
            {
                channel.Dispose();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "释放gRPC通道时发生异常");
            }
        }

        _channelPool.Clear();
        _lastAccessTime.Clear();
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}