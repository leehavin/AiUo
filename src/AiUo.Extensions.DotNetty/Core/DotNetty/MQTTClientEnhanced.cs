using AiUo.Logging;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace AiUo.Extensions.DotNetty.Core.DotNetty;

/// <summary>
/// 增强版MQTT客户端，提供更完善的连接管理和消息处理功能
/// </summary>
public class MQTTClientEnhanced : IDisposable
{
    private readonly string _clientId;
    private readonly string _server;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly bool _cleanSession;
    private readonly TimeSpan _keepAlivePeriod;
    private readonly bool _useTls;
    private readonly int _connectionTimeout;
    
    private Bootstrap _bootstrap;
    private IChannel _channel;
    private MQTTClientHandler _handler;
    private IEventLoopGroup _group;
    private Timer _keepAliveTimer;
    private readonly ConcurrentDictionary<string, Func<MQTTMessage, Task>> _topicHandlers;
    private readonly ConcurrentDictionary<ushort, TaskCompletionSource<bool>> _pendingMessages;
    private readonly ConcurrentDictionary<string, MQTTSubscriptionOptions> _subscriptions;
    private ushort _nextMessageId = 1;
    private readonly SemaphoreSlim _connectionSemaphore = new SemaphoreSlim(1, 1);
    private readonly MQTTConnectionHandler _connectionHandler;
    private readonly MQTTRetransmissionManager _retransmissionManager;
    
    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnected => _handler?.IsConnected ?? false;
    
    /// <summary>
    /// 连接状态
    /// </summary>
    public MQTTConnectionStatus ConnectionStatus => _connectionHandler.ConnectionStatus;
    
    /// <summary>
    /// 连接状态变更事件
    /// </summary>
    public event EventHandler<MQTTConnectionStatusEventArgs> ConnectionStatusChanged
    {
        add { _connectionHandler.ConnectionStatusChanged += value; }
        remove { _connectionHandler.ConnectionStatusChanged -= value; }
    }
    
    /// <summary>
    /// 消息接收事件
    /// </summary>
    public event EventHandler<MQTTMessageReceivedEventArgs> MessageReceived;
    
    /// <summary>
    /// 创建增强版MQTT客户端
    /// </summary>
    public MQTTClientEnhanced(
        string server, 
        int port, 
        string clientId = null, 
        string username = null, 
        string password = null, 
        bool cleanSession = true, 
        int keepAlivePeriod = 60, 
        bool useTls = false,
        int connectionTimeout = 30,
        bool autoReconnect = true,
        int maxReconnectAttempts = 10,
        int reconnectDelay = 5000,
        int reconnectMaxDelay = 120000,
        bool useExponentialBackoff = true)
    {
        _server = server;
        _port = port;
        _clientId = clientId ?? Guid.NewGuid().ToString();
        _username = username;
        _password = password;
        _cleanSession = cleanSession;
        _keepAlivePeriod = TimeSpan.FromSeconds(keepAlivePeriod);
        _useTls = useTls;
        _connectionTimeout = connectionTimeout;
        _topicHandlers = new ConcurrentDictionary<string, Func<MQTTMessage, Task>>();
        _pendingMessages = new ConcurrentDictionary<ushort, TaskCompletionSource<bool>>();
        _subscriptions = new ConcurrentDictionary<string, MQTTSubscriptionOptions>();
        
        // 创建重传管理器
        _retransmissionManager = new MQTTRetransmissionManager();
        
        // 创建连接处理器
        _connectionHandler = new MQTTConnectionHandler(
            this,
            _clientId,
            _server,
            _port,
            autoReconnect,
            maxReconnectAttempts,
            reconnectDelay,
            reconnectMaxDelay,
            useExponentialBackoff);
    }
    
    /// <summary>
    /// 启动客户端
    /// </summary>
    public async Task StartAsync()
    {
        // 使用信号量确保同一时间只有一个连接操作
        await _connectionSemaphore.WaitAsync();
        
        try
        {
            if (_channel != null && _channel.Active)
                return;
                
            // 释放现有资源
            await CleanupResourcesAsync();
            
            _group = new MultithreadEventLoopGroup();
            _handler = new MQTTClientHandler(_clientId, _server, _port);
            
            // 设置消息接收处理
            _handler.MessageReceived += OnMessageReceived;
            
            _bootstrap = new Bootstrap();
            _bootstrap
                .Group(_group)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(_connectionTimeout))
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    
                    if (_useTls)
                    {
                        pipeline.AddLast(TlsHandler.Client());
                    }
                    
                    pipeline.AddLast(new MQTTEncoder());
                    pipeline.AddLast(new MQTTDecoder());
                    pipeline.AddLast(_handler);
                }));
                
            try
            {
                _channel = await _bootstrap.ConnectAsync(_server, _port);
                
                // 等待连接完成
                bool connected = await _handler.WaitForConnectAsync(TimeSpan.FromSeconds(_connectionTimeout));
                if (!connected)
                    throw new Exception($"连接MQTT服务器超时: {_server}:{_port}");
                    
                // 通知连接处理器连接成功
                _connectionHandler.HandleConnected();
                
                // 启动保活定时器
                StartKeepAliveTimer();
                
                // 重新订阅之前的主题
                await ResubscribeTopicsAsync();
            }
            catch (Exception ex)
            {
                // 通知连接处理器连接失败
                _connectionHandler.HandleConnectionFailed(ex);
                LogUtil.Error(ex, "连接MQTT服务器失败: {Server}:{Port}", _server, _port);
                throw;
            }
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }
    
    /// <summary>
    /// 停止客户端
    /// </summary>
    public async Task StopAsync()
    {
        await _connectionSemaphore.WaitAsync();
        
        try
        {
            await CleanupResourcesAsync();
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }
    
    /// <summary>
    /// 清理资源
    /// </summary>
    private async Task CleanupResourcesAsync()
    {
        StopKeepAliveTimer();
        
        if (_channel != null && _channel.Active)
        {
            try
            {
                // 发送DISCONNECT消息
                _handler.SendDisconnect(_channel.Pipeline.Context(_handler));
                
                // 关闭连接
                await _channel.CloseAsync();
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "关闭MQTT连接失败: {Server}:{Port}", _server, _port);
            }
        }
        
        if (_group != null)
        {
            try
            {
                await _group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "关闭MQTT事件循环组失败: {Server}:{Port}", _server, _port);
            }
        }
        
        if (_handler != null)
        {
            _handler.MessageReceived -= OnMessageReceived;
        }
        
        _channel = null;
        _handler = null;
        _group = null;
    }
    
    /// <summary>
    /// 发布消息
    /// </summary>
    public async Task<bool> PublishAsync(string topic, byte[] payload, MQTTQualityOfServiceLevel qos = MQTTQualityOfServiceLevel.AtMostOnce, bool retain = false)
    {
        if (!IsConnected)
        {
            // 如果未连接，尝试重新连接
            try
            {
                await StartAsync();
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "发布消息前重连失败: {Server}:{Port}, Topic={Topic}", _server, _port, topic);
                return false;
            }
        }
            
        ushort messageId = 0;
        TaskCompletionSource<bool> completionSource = null;
        
        // 对于QoS > 0的消息，需要生成消息ID并等待确认
        if (qos > MQTTQualityOfServiceLevel.AtMostOnce)
        {
            messageId = GetNextMessageId();
            completionSource = new TaskCompletionSource<bool>();
            _pendingMessages[messageId] = completionSource;
            
            // 添加到重传管理器
            _retransmissionManager.AddPendingMessage(
                _clientId,
                messageId,
                new { Topic = topic, Payload = payload, QoS = qos, Retain = retain },
                MQTTMessageType.PUBLISH,
                qos,
                async (item) =>
                {
                    // 重传动作
                    if (IsConnected)
                    {
                        _handler.Publish(_channel.Pipeline.Context(_handler), topic, payload, qos, retain, messageId);
                        return;
                    }
                    throw new InvalidOperationException("MQTT客户端未连接，无法重传消息");
                });
        }
        
        try
        {
            // 发送PUBLISH消息
            _handler.Publish(_channel.Pipeline.Context(_handler), topic, payload, qos, retain, messageId);
            
            // 对于QoS > 0的消息，等待确认
            if (qos > MQTTQualityOfServiceLevel.AtMostOnce)
            {
                // 设置超时
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
                var completedTask = await Task.WhenAny(completionSource.Task, timeoutTask);
                
                // 移除挂起的消息
                _pendingMessages.TryRemove(messageId, out _);
                
                if (completedTask == timeoutTask)
                {
                    LogUtil.Warning("发布消息超时: Topic={Topic}, QoS={QoS}, MessageId={MessageId}", topic, qos, messageId);
                    return false;
                }
            }
            
            return true;
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "发布消息失败: Topic={Topic}, QoS={QoS}", topic, qos);
            return false;
        }
    }
    
    /// <summary>
    /// 发布消息（泛型版本）
    /// </summary>
    public async Task<bool> PublishAsync<TMessage>(string topic, TMessage message, MQTTQualityOfServiceLevel qos = MQTTQualityOfServiceLevel.AtMostOnce, bool retain = false)
        where TMessage : class, new()
    {
        // 序列化消息
        var json = JsonSerializer.Serialize(message);
        var payload = Encoding.UTF8.GetBytes(json);
        
        return await PublishAsync(topic, payload, qos, retain);
    }
    
    /// <summary>
    /// 订阅主题
    /// </summary>
    public async Task<bool> SubscribeAsync(string topic, Func<MQTTMessage, Task> handler, MQTTQualityOfServiceLevel qos = MQTTQualityOfServiceLevel.AtMostOnce)
    {
        if (!IsConnected)
        {
            // 如果未连接，尝试重新连接
            try
            {
                await StartAsync();
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "订阅主题前重连失败: {Server}:{Port}, Topic={Topic}", _server, _port, topic);
                return false;
            }
        }
        
        // 保存订阅信息，用于重连后重新订阅
        _subscriptions[topic] = new MQTTSubscriptionOptions
        {
            Topic = topic,
            QoS = qos,
            Handler = handler
        };
        
        // 添加主题处理器
        _handler.AddTopicHandler(topic, handler);
        _topicHandlers[topic] = handler;
        
        try
        {
            // 发送SUBSCRIBE消息
            ushort messageId = GetNextMessageId();
            var completionSource = new TaskCompletionSource<bool>();
            _pendingMessages[messageId] = completionSource;
            
            _handler.Subscribe(_channel.Pipeline.Context(_handler), topic, qos, messageId);
            
            // 等待确认
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
            var completedTask = await Task.WhenAny(completionSource.Task, timeoutTask);
            
            // 移除挂起的消息
            _pendingMessages.TryRemove(messageId, out _);
            
            if (completedTask == timeoutTask)
            {
                LogUtil.Warning("订阅主题超时: Topic={Topic}, QoS={QoS}", topic, qos);
                return false;
            }
            
            LogUtil.Info("MQTT订阅主题: {Topic}, QoS={QoS}", topic, qos);
            return true;
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "订阅主题失败: Topic={Topic}, QoS={QoS}", topic, qos);
            return false;
        }
    }
    
    /// <summary>
    /// 订阅主题（泛型版本）
    /// </summary>
    public async Task<bool> SubscribeAsync<TMessage>(string topic, Func<TMessage, Task> handler, MQTTQualityOfServiceLevel qos = MQTTQualityOfServiceLevel.AtMostOnce)
        where TMessage : class, new()
    {
        return await SubscribeAsync(topic, async message =>
        {
            try
            {
                var payload = message.Payload;
                if (payload != null && payload.Length > 0)
                {
                    var json = Encoding.UTF8.GetString(payload);
                    var msg = JsonSerializer.Deserialize<TMessage>(json);
                    await handler(msg);
                }
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "MQTT消息处理异常: {Topic}", topic);
            }
        }, qos);
    }
    
    /// <summary>
    /// 取消订阅主题
    /// </summary>
    public async Task<bool> UnsubscribeAsync(string topic)
    {
        if (!IsConnected)
            return false;
            
        // 移除主题处理器
        _handler.RemoveTopicHandlers(topic);
        _topicHandlers.TryRemove(topic, out _);
        _subscriptions.TryRemove(topic, out _);
        
        try
        {
            // 发送UNSUBSCRIBE消息
            ushort messageId = GetNextMessageId();
            var completionSource = new TaskCompletionSource<bool>();
            _pendingMessages[messageId] = completionSource;
            
            _handler.Unsubscribe(_channel.Pipeline.Context(_handler), topic, messageId);
            
            // 等待确认
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
            var completedTask = await Task.WhenAny(completionSource.Task, timeoutTask);
            
            // 移除挂起的消息
            _pendingMessages.TryRemove(messageId, out _);
            
            if (completedTask == timeoutTask)
            {
                LogUtil.Warning("取消订阅主题超时: Topic={Topic}", topic);
                return false;
            }
            
            LogUtil.Info("MQTT取消订阅主题: {Topic}", topic);
            return true;
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "取消订阅主题失败: Topic={Topic}", topic);
            return false;
        }
    }
    
    /// <summary>
    /// 重新订阅之前的主题
    /// </summary>
    private async Task ResubscribeTopicsAsync()
    {
        if (_subscriptions.IsEmpty)
            return;
            
        LogUtil.Info("重新订阅MQTT主题，共{Count}个", _subscriptions.Count);
        
        foreach (var subscription in _subscriptions.Values)
        {
            try
            {
                // 添加主题处理器
                _handler.AddTopicHandler(subscription.Topic, subscription.Handler);
                
                // 发送SUBSCRIBE消息
                ushort messageId = GetNextMessageId();
                var completionSource = new TaskCompletionSource<bool>();
                _pendingMessages[messageId] = completionSource;
                
                _handler.Subscribe(_channel.Pipeline.Context(_handler), subscription.Topic, subscription.QoS, messageId);
                
                // 等待确认，但不阻塞其他主题的重新订阅
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
                        var completedTask = await Task.WhenAny(completionSource.Task, timeoutTask);
                        
                        // 移除挂起的消息
                        _pendingMessages.TryRemove(messageId, out _);
                        
                        if (completedTask == timeoutTask)
                        {
                            LogUtil.Warning("重新订阅主题超时: Topic={Topic}, QoS={QoS}", subscription.Topic, subscription.QoS);
                        }
                        else
                        {
                            LogUtil.Info("重新订阅MQTT主题成功: {Topic}, QoS={QoS}", subscription.Topic, subscription.QoS);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogUtil.Error(ex, "重新订阅主题处理异常: Topic={Topic}", subscription.Topic);
                    }
                });
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "重新订阅主题失败: Topic={Topic}, QoS={QoS}", subscription.Topic, subscription.QoS);
            }
            
            // 添加短暂延迟，避免一次性发送大量订阅请求
            await Task.Delay(100);
        }
    }
    
    /// <summary>
    /// 处理消息接收事件
    /// </summary>
    private void OnMessageReceived(object sender, MQTTMessageReceivedEventArgs e)
    {
        // 触发消息接收事件
        MessageReceived?.Invoke(this, e);
        
        // 对于QoS 1和QoS 2的消息，确认已收到
        if (e.Message.MessageType == MQTTMessageType.PUBLISH && e.Message.QoS > MQTTQualityOfServiceLevel.AtMostOnce)
        {
            _retransmissionManager.AcknowledgeMessage(_clientId, e.Message.MessageId);
        }
    }
    
    /// <summary>
    /// 启动保活定时器
    /// </summary>
    private void StartKeepAliveTimer()
    {
        StopKeepAliveTimer();
        
        // 创建定时器，定期发送PINGREQ消息
        _keepAliveTimer = new Timer(state =>
        {
            try
            {
                if (_channel != null && _channel.Active && _handler != null)
                {
                    _handler.SendPingReq(_channel.Pipeline.Context(_handler));
                }
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "发送MQTT PINGREQ消息失败");
            }
        }, null, _keepAlivePeriod, _keepAlivePeriod);
    }
    
    /// <summary>
    /// 停止保活定时器
    /// </summary>
    private void StopKeepAliveTimer()
    {
        if (_keepAliveTimer != null)
        {
            _keepAliveTimer.Dispose();
            _keepAliveTimer = null;
        }
    }
    
    /// <summary>
    /// 获取下一个消息ID
    /// </summary>
    private ushort GetNextMessageId()
    {
        // 消息ID范围为1-65535
        if (_nextMessageId == 0 || _nextMessageId == 65535)
            _nextMessageId = 1;
        else
            _nextMessageId++;
            
        return _nextMessageId;
    }
    
    /// <summary>
    /// 入队消息（兼容MQTTnet.Extensions.ManagedClient.IManagedMqttClient接口）
    /// </summary>
    public async Task EnqueueAsync(MQTTApplicationMessage message)
    {
        await PublishAsync(message.Topic, message.Payload, message.QoS, message.Retain);
    }
    
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        StopAsync().Wait();
        StopKeepAliveTimer();
        _connectionSemaphore.Dispose();
        _retransmissionManager.Dispose();
    }
}

/// <summary>
/// MQTT连接状态
/// </summary>
public enum MQTTConnectionStatus
{
    /// <summary>
    /// 已断开连接
    /// </summary>
    Disconnected,
    
    /// <summary>
    /// 正在连接
    /// </summary>
    Connecting,
    
    /// <summary>
    /// 已连接
    /// </summary>
    Connected,
    
    /// <summary>
    /// 正在重新连接
    /// </summary>
    Reconnecting,
    
    /// <summary>
    /// 连接失败
    /// </summary>
    ConnectionFailed
}

/// <summary>
/// MQTT连接状态事件参数
/// </summary>
public class MQTTConnectionStatusEventArgs : EventArgs
{
    /// <summary>
    /// 连接状态
    /// </summary>
    public MQTTConnectionStatus Status { get; }
    
    /// <summary>
    /// 异常信息
    /// </summary>
    public Exception Exception { get; }
    
    /// <summary>
    /// 创建MQTT连接状态事件参数
    /// </summary>
    public MQTTConnectionStatusEventArgs(MQTTConnectionStatus status, Exception exception = null)
    {
        Status = status;
        Exception = exception;
    }
}

/// <summary>
/// MQTT消息接收事件参数
/// </summary>
public class MQTTMessageReceivedEventArgs : EventArgs
{
    /// <summary>
    /// 消息内容
    /// </summary>
    public MQTTMessage Message { get; }
    
    /// <summary>
    /// 创建MQTT消息接收事件参数
    /// </summary>
    public MQTTMessageReceivedEventArgs(MQTTMessage message)
    {
        Message = message;
    }
}

/// <summary>
/// MQTT订阅选项
/// </summary>
public class MQTTSubscriptionOptions
{
    /// <summary>
    /// 主题
    /// </summary>
    public string Topic { get; set; }
    
    /// <summary>
    /// 服务质量级别
    /// </summary>
    public MQTTQualityOfServiceLevel QoS { get; set; }
    
    /// <summary>
    /// 消息处理函数
    /// </summary>
    public Func<MQTTMessage, Task> Handler { get; set; }
}

/// <summary>
/// MQTT连接处理器
/// </summary>
public class MQTTConnectionHandler
{
    private readonly MQTTClientEnhanced _client;
    private readonly string _clientId;
    private readonly string _server;
    private readonly int _port;
    private readonly bool _autoReconnect;
    private readonly int _maxReconnectAttempts;
    private readonly int _reconnectDelay;
    private readonly int _reconnectMaxDelay;
    private readonly bool _useExponentialBackoff;
    
    private int _reconnectAttempts;
    private Timer _reconnectTimer;
    private MQTTConnectionStatus _connectionStatus = MQTTConnectionStatus.Disconnected;
    
    /// <summary>
    /// 连接状态
    /// </summary>
    public MQTTConnectionStatus ConnectionStatus => _connectionStatus;
    
    /// <summary>
    /// 连接状态变更事件
    /// </summary>
    public event EventHandler<MQTTConnectionStatusEventArgs> ConnectionStatusChanged;
    
    /// <summary>
    /// 创建MQTT连接处理器
    /// </summary>
    public MQTTConnectionHandler(
        MQTTClientEnhanced client,
        string clientId,
        string server,
        int port,
        bool autoReconnect = true,
        int maxReconnectAttempts = 10,
        int reconnectDelay = 5000,
        int reconnectMaxDelay = 120000,
        bool useExponentialBackoff = true)
    {
        _client = client;
        _clientId = clientId;
        _server = server;
        _port = port;
        _autoReconnect = autoReconnect;
        _maxReconnectAttempts = maxReconnectAttempts;
        _reconnectDelay = reconnectDelay;
        _reconnectMaxDelay = reconnectMaxDelay;
        _useExponentialBackoff = useExponentialBackoff;
    }
    
    /// <summary>
    /// 处理连接成功
    /// </summary>
    public void HandleConnected()
    {
        _reconnectAttempts = 0;
        SetConnectionStatus(MQTTConnectionStatus.Connected);
    }
    
    /// <summary>
    /// 处理连接断开
    /// </summary>
    public void HandleDisconnected()
    {
        SetConnectionStatus(MQTTConnectionStatus.Disconnected);
        
        // 如果启用了自动重连，则尝试重新连接
        if (_autoReconnect)
        {
            TryReconnect();
        }
    }
    
    /// <summary>
    /// 处理连接失败
    /// </summary>
    public void HandleConnectionFailed(Exception exception)
    {
        SetConnectionStatus(MQTTConnectionStatus.ConnectionFailed, exception);
        
        // 如果启用了自动重连，则尝试重新连接
        if (_autoReconnect)
        {
            TryReconnect();
        }
    }
    
    /// <summary>
    /// 尝试重新连接
    /// </summary>
    private void TryReconnect()
    {
        // 如果已达到最大重连次数，则停止重连
        if (_maxReconnectAttempts > 0 && _reconnectAttempts >= _maxReconnectAttempts)
        {
            LogUtil.Warning("MQTT客户端已达到最大重连次数({MaxReconnectAttempts})，停止重连: {ClientId}@{Server}:{Port}",
                _maxReconnectAttempts, _clientId, _server, _port);
            return;
        }
        
        // 增加重连次数
        _reconnectAttempts++;
        
        // 计算重连延迟
        int delay = _reconnectDelay;
        if (_useExponentialBackoff)
        {
            // 使用指数退避算法：初始延迟 * 2^(重连次数-1)，但不超过最大延迟
            delay = (int)Math.Min(_reconnectDelay * Math.Pow(2, _reconnectAttempts - 1), _reconnectMaxDelay);
        }
        
        LogUtil.Info("MQTT客户端将在{Delay}ms后尝试第{Attempt}次重连: {ClientId}@{Server}:{Port}",
            delay, _reconnectAttempts, _clientId, _server, _port);
        
        // 设置重连状态
        SetConnectionStatus(MQTTConnectionStatus.Reconnecting);
        
        // 创建重连定时器
        _reconnectTimer?.Dispose();
        _reconnectTimer = new Timer(async _ =>
        {
            try
            {
                await _client.StartAsync();
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "MQTT客户端第{Attempt}次重连失败: {ClientId}@{Server}:{Port}",
                    _reconnectAttempts, _clientId, _server, _port);
                
                // 继续尝试重连
                TryReconnect();
            }
        }, null, delay, Timeout.Infinite);
    }
    
    /// <summary>
    /// 设置连接状态
    /// </summary>
    private void SetConnectionStatus(MQTTConnectionStatus status, Exception exception = null)
    {
        if (_connectionStatus != status)
        {
            _connectionStatus = status;
            ConnectionStatusChanged?.Invoke(this, new MQTTConnectionStatusEventArgs(status, exception));
        }
    }
    
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _reconnectTimer?.Dispose();
        _reconnectTimer = null;
    }
}