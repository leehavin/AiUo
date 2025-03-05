using System;
using System.Collections.Generic;
using AiUo.Configuration;
using AiUo.Extensions.MagicOnion.ServiceDiscovery;
using Microsoft.Extensions.Configuration;

namespace AiUo.Extensions.MagicOnion;

/// <summary>
/// MagicOnion服务配置选项
/// </summary>
public class MagicOnionOptions : ConfigSection
{
    public override string SectionName => "MagicOnion";
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 服务地址
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// 服务端口
    /// </summary>
    public int Port { get; set; } = 5000;

    /// <summary>
    /// 是否使用TLS/SSL
    /// </summary>
    public bool UseTls { get; set; } = false;

    /// <summary>
    /// 连接超时(毫秒)
    /// </summary>
    public int ConnectionTimeout { get; set; } = 10000;

    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// 重试间隔(毫秒)
    /// </summary>
    public int RetryInterval { get; set; } = 1000;

    /// <summary>
    /// 连接池大小
    /// </summary>
    public int PoolSize { get; set; } = 10;

    /// <summary>
    /// 空闲超时(毫秒)
    /// </summary>
    public int IdleTimeout { get; set; } = 60000;

    /// <summary>
    /// 熔断失败阈值
    /// </summary>
    public int CircuitBreakerThreshold { get; set; } = 5;

    /// <summary>
    /// 熔断重置时间(毫秒)
    /// </summary>
    public int CircuitBreakerResetTimeout { get; set; } = 30000;

    /// <summary>
    /// 是否启用负载均衡
    /// </summary>
    public bool EnableLoadBalancing { get; set; } = false;

    /// <summary>
    /// 负载均衡服务器列表
    /// </summary>
    public List<string> Servers { get; set; } = new List<string>();
    
    /// <summary>
    /// 服务发现配置
    /// </summary>
    public ServiceDiscoveryOptions ServiceDiscovery { get; set; } = new ServiceDiscoveryOptions();

    /// <summary>
    /// 获取服务地址
    /// </summary>
    /// <returns>完整的服务地址</returns>
    public string GetAddress()
    {
        return $"{Host}:{Port}";
    }
    
    public override void Bind(IConfiguration configuration)
    {
        base.Bind(configuration);
        ServiceDiscovery = configuration.GetSection(nameof(ServiceDiscovery)).Get<ServiceDiscoveryOptions>() ?? new ServiceDiscoveryOptions();
        Servers = configuration.GetSection(nameof(Servers)).Get<List<string>>() ?? new List<string>();
    }
}