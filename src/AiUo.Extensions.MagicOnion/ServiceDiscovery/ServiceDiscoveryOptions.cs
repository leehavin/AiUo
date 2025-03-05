using System;

namespace AiUo.Extensions.MagicOnion.ServiceDiscovery;

/// <summary>
/// 服务发现配置选项
/// </summary>
public class ServiceDiscoveryOptions
{
    /// <summary>
    /// 是否启用服务发现
    /// </summary>
    public bool Enabled { get; set; } = false;
    
    /// <summary>
    /// 服务发现类型
    /// </summary>
    public ServiceDiscoveryType Type { get; set; } = ServiceDiscoveryType.None;
    
    /// <summary>
    /// 服务名称
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;
    
    /// <summary>
    /// 服务刷新间隔(毫秒)
    /// </summary>
    public int RefreshInterval { get; set; } = 10000;
    
    /// <summary>
    /// 负载均衡策略
    /// </summary>
    public LoadBalancingStrategy LoadBalancingStrategy { get; set; } = LoadBalancingStrategy.RoundRobin;
    
    /// <summary>
    /// 是否只使用健康实例
    /// </summary>
    public bool OnlyHealthyInstances { get; set; } = true;
    
    /// <summary>
    /// 服务实例筛选器
    /// </summary>
    public Func<ServiceInstance, bool>? InstanceFilter { get; set; }
}

/// <summary>
/// 服务发现类型
/// </summary>
public enum ServiceDiscoveryType
{
    /// <summary>
    /// 无服务发现
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Nacos服务发现
    /// </summary>
    Nacos = 1,
    
    /// <summary>
    /// Consul服务发现
    /// </summary>
    Consul = 2
}

/// <summary>
/// 负载均衡策略
/// </summary>
public enum LoadBalancingStrategy
{
    /// <summary>
    /// 轮询
    /// </summary>
    RoundRobin = 0,
    
    /// <summary>
    /// 随机
    /// </summary>
    Random = 1,
    
    /// <summary>
    /// 加权轮询
    /// </summary>
    WeightedRoundRobin = 2,
    
    /// <summary>
    /// 加权随机
    /// </summary>
    WeightedRandom = 3,
    
    /// <summary>
    /// 最少连接
    /// </summary>
    LeastConnections = 4
}