using System.Collections.Generic;
using System.Threading.Tasks;

namespace AiUo.Extensions.MagicOnion.ServiceDiscovery;

/// <summary>
/// 服务发现接口
/// </summary>
public interface IServiceDiscovery
{
    /// <summary>
    /// 获取服务实例列表
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <returns>服务实例列表</returns>
    Task<List<ServiceInstance>> GetServiceInstancesAsync(string serviceName);
    
    /// <summary>
    /// 注册服务实例
    /// </summary>
    /// <param name="instance">服务实例信息</param>
    /// <returns>注册结果</returns>
    Task RegisterServiceAsync(ServiceInstance instance);
    
    /// <summary>
    /// 注销服务实例
    /// </summary>
    /// <param name="instance">服务实例信息</param>
    /// <returns>注销结果</returns>
    Task DeregisterServiceAsync(ServiceInstance instance);
    
    /// <summary>
    /// 订阅服务变更
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <param name="callback">变更回调</param>
    /// <returns>订阅结果</returns>
    Task SubscribeAsync(string serviceName, ServiceChangeCallback callback);
    
    /// <summary>
    /// 取消订阅服务变更
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <returns>取消订阅结果</returns>
    Task UnsubscribeAsync(string serviceName);
}

/// <summary>
/// 服务变更回调委托
/// </summary>
/// <param name="instances">最新的服务实例列表</param>
public delegate Task ServiceChangeCallback(List<ServiceInstance> instances);

/// <summary>
/// 服务实例信息
/// </summary>
public class ServiceInstance
{
    /// <summary>
    /// 服务名称
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;
    
    /// <summary>
    /// 实例ID
    /// </summary>
    public string InstanceId { get; set; } = string.Empty;
    
    /// <summary>
    /// 主机地址
    /// </summary>
    public string Host { get; set; } = string.Empty;
    
    /// <summary>
    /// 端口
    /// </summary>
    public int Port { get; set; }
    
    /// <summary>
    /// 元数据
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
    
    /// <summary>
    /// 是否健康
    /// </summary>
    public bool IsHealthy { get; set; } = true;
    
    /// <summary>
    /// 权重
    /// </summary>
    public double Weight { get; set; } = 1.0;
}