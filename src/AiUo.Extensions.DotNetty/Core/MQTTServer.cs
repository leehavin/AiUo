using AiUo.Extensions.DotNetty.Core.DotNetty;
using AiUo.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System.Collections.Concurrent;
using System.Net;

namespace AiUo.Extensions.DotNetty;

/// <summary>
/// MQTT服务器
/// </summary>
public class MQTTServer : IDisposable
{
    private readonly string _host;
    private readonly int _port;
    private readonly bool _useTls;
    private readonly MQTTSessionManager _sessionManager;
    private readonly MQTTTopicRouter _topicRouter;
    private readonly MQTTRetransmissionManager _retransmissionManager;
    private readonly ConcurrentDictionary<string, IChannel> _clientChannels = new();
    
    private IEventLoopGroup _bossGroup;
    private IEventLoopGroup _workerGroup;
    private IChannel _serverChannel;
    private bool _isRunning;
    
    /// <summary>
    /// 创建MQTT服务器
    /// </summary>
    /// <param name="host">主机地址</param>
    /// <param name="port">端口</param>
    /// <param name="useTls">是否使用TLS</param>
    public MQTTServer(string host = "0.0.0.0", int port = 1883, bool useTls = false)
    {
        _host = host;
        _port = port;
        _useTls = useTls;
        _sessionManager = new MQTTSessionManager();
        _topicRouter = new MQTTTopicRouter();
        _retransmissionManager = new MQTTRetransmissionManager();
    }
    
    /// <summary>
    /// 启动服务器
    /// </summary>
    public async Task StartAsync()
    {
        if (_isRunning)
            return;
            
        _bossGroup = new MultithreadEventLoopGroup(1);
        _workerGroup = new MultithreadEventLoopGroup();
        
        try
        {
            var bootstrap = new ServerBootstrap();
            bootstrap
                .Group(_bossGroup, _workerGroup)
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.SoBacklog, 100)
                .ChildOption(ChannelOption.TcpNodelay, true)
                .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    
                    if (_useTls)
                    {
                        // 添加TLS处理器
                        // 实际使用时需要配置证书
                        // pipeline.AddLast(TlsHandler.Server(...));
                    }
                    
                    // 添加MQTT编解码器
                    pipeline.AddLast(new MQTTEncoder());
                    pipeline.AddLast(new MQTTDecoder());
                    
                    // 添加MQTT服务器处理器
                    pipeline.AddLast(new MQTTServerHandler(this, _sessionManager, _topicRouter, _retransmissionManager));
                }));
                
            _serverChannel = await bootstrap.BindAsync(IPAddress.Parse(_host), _port);
            _isRunning = true;
            
            LogUtil.Info("MQTT服务器已启动: {Host}:{Port}", _host, _port);
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "启动MQTT服务器失败: {Host}:{Port}", _host, _port);
            await StopAsync();
            throw;
        }
    }
    
    /// <summary>
    /// 停止服务器
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isRunning)
            return;
            
        try
        {
            // 关闭所有客户端连接
            foreach (var channel in _clientChannels.Values)
            {
                try
                {
                    await channel.CloseAsync();
                }
                catch (Exception ex)
                {
                    LogUtil.Error(ex, "关闭MQTT客户端连接失败");
                }
            }
            
            // 关闭服务器通道
            if (_serverChannel != null)
            {
                await _serverChannel.CloseAsync();
                _serverChannel = null;
            }
            
            // 关闭事件循环组
            var bossGroupTask = _bossGroup?.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            var workerGroupTask = _workerGroup?.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            
            if (bossGroupTask != null && workerGroupTask != null)
                await Task.WhenAll(bossGroupTask, workerGroupTask);
                
            _bossGroup = null;
            _workerGroup = null;
            
            // 清理资源
            _clientChannels.Clear();
            _topicRouter.ClearAllSubscriptions();
            _retransmissionManager.ClearAllPendingMessages();
            _sessionManager.ClearSessions();
            
            _isRunning = false;
            
            LogUtil.Info("MQTT服务器已停止: {Host}:{Port}", _host, _port);
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "停止MQTT服务器失败: {Host}:{Port}", _host, _port);
            throw;
        }
    }
    
    /// <summary>
    /// 注册客户端通道
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <param name="channel">通道</param>
    internal void RegisterClientChannel(string clientId, IChannel channel)
    {
        _clientChannels[clientId] = channel;
    }
    
    /// <summary>
    /// 移除客户端通道
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    internal bool RemoveClientChannel(string clientId)
    {
        return _clientChannels.TryRemove(clientId, out _);
    }
    
    /// <summary>
    /// 获取客户端通道
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <returns>通道</returns>
    internal IChannel GetClientChannel(string clientId)
    {
        _clientChannels.TryGetValue(clientId, out var channel);
        return channel;
    }
    
    /// <summary>
    /// 发布消息
    /// </summary>
    /// <param name="topic">主题</param>
    /// <param name="payload">有效载荷</param>
    /// <param name="qos">服务质量级别</param>
    /// <param name="retain">是否保留消息</param>
    /// <param name="sourceClientId">源客户端ID</param>
    public async Task PublishAsync(string topic, byte[] payload, MQTTQualityOfServiceLevel qos = MQTTQualityOfServiceLevel.AtMostOnce, bool retain = false, string sourceClientId = null)
    {
        // 验证主题
        if (!MQTTTopicMatcher.IsValidTopic(topic))
        {
            LogUtil.Warning("无效的MQTT主题: {Topic}", topic);
            return;
        }
        
        // 查找匹配的订阅
        var matchingSubscriptions = _topicRouter.FindMatchingSubscriptions(topic);
        
        // 发送消息到匹配的订阅者
        foreach (var subscription in matchingSubscriptions)
        {
            // 跳过源客户端
            if (subscription.ClientId == sourceClientId)
                continue;
                
            // 获取客户端通道
            var channel = GetClientChannel(subscription.ClientId);
            if (channel == null || !channel.Active)
                continue;
                
            // 获取会话
            var session = _sessionManager.GetSession(subscription.ClientId);
            if (session == null)
                continue;
                
            try
            {
                // 确定实际使用的QoS级别（取订阅QoS和发布QoS的较小值）
                var effectiveQoS = qos < subscription.QoS ? qos : subscription.QoS;
                
                // 创建PUBLISH消息
                var publishMessage = new MQTTMessage
                {
                    MessageType = MQTTMessageType.PUBLISH,
                    Topic = topic,
                    Payload = payload,
                    QoS = effectiveQoS,
                    IsRetain = false // 转发给订阅者时不设置保留标志
                };
                
                // 对于QoS > 0的消息，需要设置消息ID并等待确认
                if (effectiveQoS > MQTTQualityOfServiceLevel.AtMostOnce)
                {
                    // 生成消息ID
                    ushort messageId = GenerateMessageId(session);
                    publishMessage.MessageId = messageId;
                    
                    // 添加待确认消息
                    var pendingMessage = new PendingMessage
                    {
                        MessageId = messageId,
                        Topic = topic,
                        Payload = payload,
                        QoS = effectiveQoS,
                        Retain = false,
                        CreatedTime = DateTime.UtcNow
                    };
                    
                    session.AddPendingMessage(messageId, pendingMessage);
                    
                    // 添加到重传管理器
                    _retransmissionManager.AddPendingMessage(
                        subscription.ClientId,
                        messageId,
                        publishMessage,
                        MQTTMessageType.PUBLISH,
                        effectiveQoS,
                        async (item) =>
                        {
                            // 重传逻辑
                            var clientChannel = GetClientChannel(item.ClientId);
                            if (clientChannel != null && clientChannel.Active)
                            {
                                await clientChannel.WriteAndFlushAsync(item.Message);
                            }
                        });
                }
                
                // 发送消息
                await channel.WriteAndFlushAsync(publishMessage);
                
                LogUtil.Debug("MQTT消息已发送到客户端: ClientId={ClientId}, Topic={Topic}, QoS={QoS}", 
                    subscription.ClientId, topic, effectiveQoS);
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "发送MQTT消息到客户端失败: ClientId={ClientId}, Topic={Topic}", 
                    subscription.ClientId, topic);
            }
        }
        
        // 处理保留消息
        if (retain)
        {
            // 实现保留消息的存储逻辑
            // ...
        }
    }
    
    /// <summary>
    /// 生成消息ID
    /// </summary>
    /// <param name="session">MQTT会话</param>
    /// <returns>消息ID</returns>
    private ushort GenerateMessageId(MQTTSession session)
    {
        // 简单实现：从1开始递增，到65535后回到1
        ushort messageId = 1;
        while (session.PendingMessages.ContainsKey(messageId))
        {
            messageId++;
            if (messageId == 0) // 0是无效的消息ID
                messageId = 1;
        }
        return messageId;
    }
    
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        StopAsync().Wait();
        _retransmissionManager.Dispose();
    }
}