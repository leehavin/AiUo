using AiUo.Extensions.DotNetty.Core.DotNetty;
using AiUo.Logging;
using System.Collections.Concurrent;

namespace AiUo.Extensions.DotNetty;

/// <summary>
/// MQTT连接管理器
/// </summary>
public class MQTTConnectionManager : IDisposable
{
    private readonly MQTTSessionManager _sessionManager;
    private readonly ConcurrentDictionary<string, MQTTClient> _connections = new();
    private readonly ConcurrentDictionary<string, DateTime> _lastActivityTimes = new();
    private readonly ConcurrentDictionary<string, ConnectionInfo> _connectionInfos = new();
    private readonly Timer _connectionMonitorTimer;
    private readonly TimeSpan _connectionTimeout;
    private readonly TimeSpan _connectionMonitorInterval;
    private readonly int _maxReconnectAttempts;
    private readonly TimeSpan _reconnectDelay;
    private readonly bool _autoReconnect;
    
    /// <summary>
    /// 创建MQTT连接管理器
    /// </summary>
    /// <param name="sessionManager">会话管理器</param>
    /// <param name="connectionTimeout">连接超时时间，默认为5分钟</param>
    /// <param name="connectionMonitorInterval">连接监控间隔，默认为1分钟</param>
    /// <param name="maxReconnectAttempts">最大重连尝试次数，默认为5次</param>
    /// <param name="reconnectDelay">重连延迟，默认为10秒</param>
    /// <param name="autoReconnect">是否自动重连，默认为true</param>
    public MQTTConnectionManager(
        MQTTSessionManager sessionManager,
        TimeSpan? connectionTimeout = null,
        TimeSpan? connectionMonitorInterval = null,
        int maxReconnectAttempts = 5,
        TimeSpan? reconnectDelay = null,
        bool autoReconnect = true)
    {
        _sessionManager = sessionManager;
        _connectionTimeout = connectionTimeout ?? TimeSpan.FromMinutes(5);
        _connectionMonitorInterval = connectionMonitorInterval ?? TimeSpan.FromMinutes(1);
        _maxReconnectAttempts = maxReconnectAttempts;
        _reconnectDelay = reconnectDelay ?? TimeSpan.FromSeconds(10);
        _autoReconnect = autoReconnect;
        
        // 创建连接监控定时器
        _connectionMonitorTimer = new Timer(MonitorConnections, null, _connectionMonitorInterval, _connectionMonitorInterval);
    }
    
    /// <summary>
    /// 注册连接
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="client">MQTT客户端</param>
    /// <param name="clientId">客户端ID</param>
    /// <param name="remoteAddress">远程地址</param>
    /// <param name="remotePort">远程端口</param>
    /// <param name="protocolVersion">协议版本</param>
    /// <param name="keepAlive">保持连接时间（秒）</param>
    /// <param name="useTls">是否使用TLS</param>
    public void RegisterConnection(string connectionId, MQTTClient client, string clientId, string remoteAddress = null, int remotePort = 0, string protocolVersion = "MQTT 3.1.1", int keepAlive = 60, bool useTls = false)
    {
        _connections[connectionId] = client;
        _lastActivityTimes[connectionId] = DateTime.UtcNow;
        
        // 创建连接信息
        var connectTime = DateTime.UtcNow;
        var connectionInfo = new ConnectionInfo
        {
            ConnectionId = connectionId,
            ClientId = clientId,
            RemoteAddress = remoteAddress,
            RemotePort = remotePort,
            ConnectTime = connectTime,
            LastActivityTime = connectTime,
            IsConnected = true,
            ProtocolVersion = protocolVersion,
            KeepAlive = keepAlive,
            UseTls = useTls
        };
        
        _connectionInfos[connectionId] = connectionInfo;
        
        // 更新会话信息
        var session = _sessionManager.GetSession(clientId);
        if (session != null)
        {
            session.ConnectTime = connectTime;
            session.ClientAddress = remoteAddress;
            session.SetState(MQTTSessionState.Connected);
        }
        
        LogUtil.Debug("已注册MQTT连接: ConnectionId={ConnectionId}, ClientId={ClientId}, RemoteAddress={RemoteAddress}:{RemotePort}", 
            connectionId, clientId, remoteAddress, remotePort);
    }
    
    /// <summary>
    /// 更新连接活动时间
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="bytesReceived">接收的字节数</param>
    /// <param name="bytesSent">发送的字节数</param>
    public void UpdateConnectionActivity(string connectionId, long bytesReceived = 0, long bytesSent = 0)
    {
        var now = DateTime.UtcNow;
        
        if (_connections.ContainsKey(connectionId))
        {
            _lastActivityTimes[connectionId] = now;
            
            // 更新连接信息
            if (_connectionInfos.TryGetValue(connectionId, out var connectionInfo))
            {
                connectionInfo.LastActivityTime = now;
                connectionInfo.BytesReceived += bytesReceived;
                connectionInfo.BytesSent += bytesSent;
                
                // 更新会话信息
                var session = _sessionManager.GetSession(connectionInfo.ClientId);
                if (session != null)
                {
                    session.UpdateActivity();
                }
            }
        }
    }
    
    /// <summary>
    /// 获取连接
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <returns>MQTT客户端</returns>
    public MQTTClient GetConnection(string connectionId)
    {
        if (_connections.TryGetValue(connectionId, out var client))
        {
            return client;
        }
        return null;
    }
    
    /// <summary>
    /// 移除连接
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <returns>是否成功移除</returns>
    public bool RemoveConnection(string connectionId)
    {
        if (_connections.TryRemove(connectionId, out _))
        {
            _lastActivityTimes.TryRemove(connectionId, out _);
            LogUtil.Debug("已移除MQTT连接: {ConnectionId}", connectionId);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 监控连接状态
    /// </summary>
    private void MonitorConnections(object state)
    {
        var now = DateTime.UtcNow;
        var inactiveConnections = new List<string>();
        
        // 检查所有连接的活动状态
        foreach (var kvp in _lastActivityTimes)
        {
            if ((now - kvp.Value) > _connectionTimeout)
            {
                inactiveConnections.Add(kvp.Key);
            }
        }
        
        // 处理不活跃的连接
        foreach (var connectionId in inactiveConnections)
        {
            if (_connections.TryGetValue(connectionId, out var client))
            {
                try
                {
                    // 尝试重新连接
                    if (!client.IsConnected)
                    {
                        LogUtil.Warning("MQTT连接不活跃，尝试重新连接: {ConnectionId}", connectionId);
                        client.StartAsync().Wait();
                        _lastActivityTimes[connectionId] = DateTime.UtcNow;
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.Error(ex, "MQTT连接重连失败: {ConnectionId}", connectionId);
                    // 移除连接
                    RemoveConnection(connectionId);
                }
            }
        }
    }
    
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _connectionMonitorTimer?.Dispose();
        
        // 关闭所有连接
        foreach (var client in _connections.Values)
        {
            try
            {
                client.StopAsync().Wait();
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "关闭MQTT连接失败");
            }
        }
        
        _connections.Clear();
        _lastActivityTimes.Clear();
    }
}