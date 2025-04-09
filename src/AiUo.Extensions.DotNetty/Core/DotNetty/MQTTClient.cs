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
/// DotNetty实现的MQTT客户端
/// </summary>
public class MQTTClient : IDisposable
{
    private readonly string _clientId;
    private readonly string _server;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly bool _cleanSession;
    private readonly TimeSpan _keepAlivePeriod;
    private readonly bool _useTls;
    
    private Bootstrap _bootstrap;
    private IChannel _channel;
    private MQTTClientHandler _handler;
    private IEventLoopGroup _group;
    private Timer _keepAliveTimer;
    private readonly ConcurrentDictionary<string, Func<MQTTMessage, Task>> _topicHandlers;
    private readonly ConcurrentDictionary<ushort, TaskCompletionSource<bool>> _pendingMessages;
    private ushort _nextMessageId = 1;
    
    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnected => _handler?.IsConnected ?? false;
    
    /// <summary>
    /// 客户端ID
    /// </summary>
    public string ClientId => _clientId;
    
    /// <summary>
    /// 创建MQTT客户端
    /// </summary>
    public MQTTClient(string server, int port, string clientId = null, string username = null, string password = null, bool cleanSession = true, int keepAlivePeriod = 60, bool useTls = false)
    {
        _server = server;
        _port = port;
        _clientId = clientId ?? Guid.NewGuid().ToString();
        _username = username;
        _password = password;
        _cleanSession = cleanSession;
        _keepAlivePeriod = TimeSpan.FromSeconds(keepAlivePeriod);
        _useTls = useTls;
        _topicHandlers = new ConcurrentDictionary<string, Func<MQTTMessage, Task>>();
        _pendingMessages = new ConcurrentDictionary<ushort, TaskCompletionSource<bool>>();
    }
    
    /// <summary>
    /// 启动客户端
    /// </summary>
    public async Task StartAsync()
    {
        if (_channel != null && _channel.Active)
            return;
            
        _group = new MultithreadEventLoopGroup();
        _handler = new MQTTClientHandler(_clientId, _server, _port);
        
        _bootstrap = new Bootstrap();
        _bootstrap
            .Group(_group)
            .Channel<TcpSocketChannel>()
            .Option(ChannelOption.TcpNodelay, true)
            .Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(30))
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
            bool connected = await _handler.WaitForConnectAsync(TimeSpan.FromSeconds(30));
            if (!connected)
                throw new Exception($"连接MQTT服务器超时: {_server}:{_port}");
                
            // 启动保活定时器
            StartKeepAliveTimer();
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "连接MQTT服务器失败: {Server}:{Port}", _server, _port);
            throw;
        }
    }
    
    /// <summary>
    /// 停止客户端
    /// </summary>
    public async Task StopAsync()
    {
        StopKeepAliveTimer();
        
        if (_channel != null && _channel.Active)
        {
            // 发送DISCONNECT消息
            _handler.SendDisconnect(_channel.Pipeline.Context(_handler));
            
            // 关闭连接
            await _channel.CloseAsync();
        }
        
        if (_group != null)
        {
            await _group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        }
        
        _channel = null;
        _handler = null;
        _group = null;
    }
    
    /// <summary>
    /// 发布消息
    /// </summary>
    public async Task PublishAsync(string topic, byte[] payload, MQTTQualityOfServiceLevel qos = MQTTQualityOfServiceLevel.AtMostOnce, bool retain = false)
    {
        if (_channel == null || !_channel.Active)
            throw new InvalidOperationException("MQTT客户端未连接");
            
        ushort messageId = 0;
        TaskCompletionSource<bool> completionSource = null;
        
        // 对于QoS > 0的消息，需要生成消息ID并等待确认
        if (qos > MQTTQualityOfServiceLevel.AtMostOnce)
        {
            messageId = GetNextMessageId();
            completionSource = new TaskCompletionSource<bool>();
            _pendingMessages[messageId] = completionSource;
        }
        
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
                throw new TimeoutException("发布消息超时");
        }
    }
    
    /// <summary>
    /// 发布消息（泛型版本）
    /// </summary>
    public async Task PublishAsync<TMessage>(string topic, TMessage message, MQTTQualityOfServiceLevel qos = MQTTQualityOfServiceLevel.AtMostOnce, bool retain = false)
        where TMessage : new()
    {
        // 序列化消息
        var json = JsonSerializer.Serialize(message);
        var payload = Encoding.UTF8.GetBytes(json);
        
        await PublishAsync(topic, payload, qos, retain);
    }
    
    /// <summary>
    /// 订阅主题
    /// </summary>
    public async Task SubscribeAsync(string topic, Func<MQTTMessage, Task> handler, MQTTQualityOfServiceLevel qos = MQTTQualityOfServiceLevel.AtMostOnce)
    {
        if (_channel == null || !_channel.Active)
            throw new InvalidOperationException("MQTT客户端未连接");
            
        // 添加主题处理器
        _handler.AddTopicHandler(topic, handler);
        _topicHandlers[topic] = handler;
        
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
            throw new TimeoutException("订阅主题超时");
            
        LogUtil.Info("MQTT订阅主题: {Topic}, QoS={QoS}", topic, qos);
    }
    
    /// <summary>
    /// 订阅主题（泛型版本）
    /// </summary>
    public async Task SubscribeAsync<TMessage>(string topic, Func<TMessage, Task> handler, MQTTQualityOfServiceLevel qos = MQTTQualityOfServiceLevel.AtMostOnce)
        where TMessage : class, new()
    {
        await SubscribeAsync(topic, async message =>
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
    public async Task UnsubscribeAsync(string topic)
    {
        if (_channel == null || !_channel.Active)
            throw new InvalidOperationException("MQTT客户端未连接");
            
        // 移除主题处理器
        _handler.RemoveTopicHandlers(topic);
        _topicHandlers.TryRemove(topic, out _);
        
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
            throw new TimeoutException("取消订阅主题超时");
            
        LogUtil.Info("MQTT取消订阅主题: {Topic}", topic);
    }
    
    /// <summary>
    /// 入队消息（兼容MQTTnet.Extensions.ManagedClient.IManagedMqttClient接口）
    /// </summary>
    public async Task EnqueueAsync(MQTTApplicationMessage message)
    {
        await PublishAsync(message.Topic, message.Payload, message.QoS, message.Retain);
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
        if (_nextMessageId == 0)
            _nextMessageId = 1;
            
        return _nextMessageId++;
    }
    
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        StopAsync().Wait();
        StopKeepAliveTimer();
    }
}

/// <summary>
/// MQTT应用消息（兼容MQTTnet.MqttApplicationMessage）
/// </summary>
public class MQTTApplicationMessage
{
    /// <summary>
    /// 主题
    /// </summary>
    public string Topic { get; set; }
    
    /// <summary>
    /// 有效载荷
    /// </summary>
    public byte[] Payload { get; set; }
    
    /// <summary>
    /// 服务质量级别
    /// </summary>
    public MQTTQualityOfServiceLevel QoS { get; set; }
    
    /// <summary>
    /// 是否保留消息
    /// </summary>
    public bool Retain { get; set; }
}