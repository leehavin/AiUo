using Grpc.Net.Client;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AiUo.Extensions.Grpc;

/// <summary>
/// gRPC客户端容器
/// </summary>
/// <remarks>
/// 用于管理和复用gRPC通道，避免重复创建连接
/// </remarks>
public class GrpcClientContainer : IDisposable
{
    private readonly ConcurrentDictionary<string, Lazy<GrpcChannel>> _channels;
    private readonly ConcurrentDictionary<string, object> _clients;
    private bool _disposed;
    private const string DefaultClientName = "__default__";

    /// <summary>
    /// 初始化gRPC客户端容器
    /// </summary>
    public GrpcClientContainer()
    {
        _channels = new ConcurrentDictionary<string, Lazy<GrpcChannel>>();
        _clients = new ConcurrentDictionary<string, object>();
    }

    /// <summary>
    /// 注册gRPC通道
    /// </summary>
    /// <param name="name">通道名称</param>
    /// <param name="channelFactory">通道工厂方法</param>
    /// <returns>当前容器实例，支持链式调用</returns>
    public GrpcClientContainer RegisterClient(string name, Func<GrpcChannel> channelFactory)
    {
        _channels.TryAdd(name, new Lazy<GrpcChannel>(channelFactory));
        return this;
    }

    /// <summary>
    /// 注册默认gRPC通道
    /// </summary>
    /// <param name="channelFactory">通道工厂方法</param>
    /// <returns>当前容器实例，支持链式调用</returns>
    public GrpcClientContainer RegisterDefaultClient(Func<GrpcChannel> channelFactory)
    {
        return RegisterClient(DefaultClientName, channelFactory);
    }

    /// <summary>
    /// 获取gRPC客户端
    /// </summary>
    /// <typeparam name="TClient">客户端类型</typeparam>
    /// <param name="name">通道名称</param>
    /// <param name="clientFactory">客户端工厂方法</param>
    /// <returns>gRPC客户端实例</returns>
    public TClient GetClient<TClient>(string name, Func<GrpcChannel, TClient> clientFactory)
        where TClient : class
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(GrpcClientContainer));

        if (!_channels.TryGetValue(name, out var channelLazy))
            throw new ArgumentException($"未找到名为'{name}'的gRPC通道", nameof(name));

        var clientKey = $"{name}_{typeof(TClient).FullName}";
        return (TClient)_clients.GetOrAdd(clientKey, _ =>
        {
            var channel = channelLazy.Value;
            return clientFactory(channel);
        });
    }

    /// <summary>
    /// 获取默认gRPC客户端
    /// </summary>
    /// <typeparam name="TClient">客户端类型</typeparam>
    /// <param name="clientFactory">客户端工厂方法</param>
    /// <returns>gRPC客户端实例</returns>
    public TClient GetDefaultClient<TClient>(Func<GrpcChannel, TClient> clientFactory)
        where TClient : class
    {
        return GetClient<TClient>(DefaultClientName, clientFactory);
    }

    /// <summary>
    /// 尝试获取gRPC客户端，如果不存在则返回false
    /// </summary>
    /// <typeparam name="TClient">客户端类型</typeparam>
    /// <param name="name">通道名称</param>
    /// <param name="clientFactory">客户端工厂方法</param>
    /// <param name="client">输出的客户端实例</param>
    /// <returns>是否成功获取客户端</returns>
    public bool TryGetClient<TClient>(string name, Func<GrpcChannel, TClient> clientFactory, out TClient client)
        where TClient : class
    {
        client = default;
        
        if (_disposed || !_channels.TryGetValue(name, out var channelLazy))
            return false;

        var clientKey = $"{name}_{typeof(TClient).FullName}";
        client = (TClient)_clients.GetOrAdd(clientKey, _ =>
        {
            var channel = channelLazy.Value;
            return clientFactory(channel);
        });
        
        return true;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _clients.Clear();

        foreach (var channel in _channels.Values)
        {
            if (channel.IsValueCreated)
            {
                channel.Value.Dispose();
            }
        }
        _channels.Clear();

        GC.SuppressFinalize(this);
    }
}