using AiUo.Logging;
using System.Collections.Concurrent;

namespace AiUo.Extensions.DotNetty.Core.DotNetty;

/// <summary>
/// MQTT客户端适配器，将DotNetty实现的MQTT客户端适配为MQTTnet的IManagedMqttClient接口
/// </summary>
public class MQTTClientAdapter : IManagedMqttClient
{
    private readonly MQTTClient _client;
    private readonly ConcurrentDictionary<string, MqttTopicFilter> _subscriptions = new();
    private bool _isStarted = false;
    private bool _isDisposed = false;

    /// <summary>
    /// 创建MQTT客户端适配器
    /// </summary>
    /// <param name="client">DotNetty实现的MQTT客户端</param>
    public MQTTClientAdapter(MQTTClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    #region IManagedMqttClient接口实现
    /// <summary>
    /// 是否已启动
    /// </summary>
    public bool IsStarted => _isStarted;

    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnected => _client.IsConnected;

    /// <summary>
    /// 待处理的消息数量
    /// </summary>
    public int PendingApplicationMessagesCount => 0;

    /// <summary>
    /// 连接状态变更事件
    /// </summary>
    public event Func<MqttClientConnectedEventArgs, Task> ConnectedAsync;

    /// <summary>
    /// 连接断开事件
    /// </summary>
    public event Func<MqttClientDisconnectedEventArgs, Task> DisconnectedAsync;

    /// <summary>
    /// 连接状态变更事件
    /// </summary>
    public event Func<ManagedProcessFailedEventArgs, Task> ConnectingFailedAsync;

    /// <summary>
    /// 同步状态变更事件
    /// </summary>
    public event Func<ManagedProcessFailedEventArgs, Task> SynchronizingSubscriptionsFailedAsync;

    /// <summary>
    /// 消息接收事件
    /// </summary>
    public event Func<MqttApplicationMessageReceivedEventArgs, Task> ApplicationMessageReceivedAsync;

    /// <summary>
    /// 消息处理完成事件
    /// </summary>
    public event EventHandler<ApplicationMessageProcessedEventArgs> ApplicationMessageProcessed;

    /// <summary>
    /// 入队消息
    /// </summary>
    public Task<MqttClientPublishResult> EnqueueAsync(MqttApplicationMessage applicationMessage)
    {
        if (applicationMessage == null) throw new ArgumentNullException(nameof(applicationMessage));

        try
        {
            // 将MQTTnet的消息转换为DotNetty的消息
            var message = new MQTTApplicationMessage
            {
                Topic = applicationMessage.Topic,
                Payload = applicationMessage.PayloadSegment.ToArray(),
                QoS = (MQTTQualityOfServiceLevel)(int)applicationMessage.QualityOfServiceLevel,
                Retain = applicationMessage.Retain
            };

            // 发送消息
            _client.EnqueueAsync(message).Wait();

            // 返回成功结果
            return Task.FromResult(new MqttClientPublishResult
            {
                ReasonCode = MqttClientPublishReasonCode.Success,
                PacketIdentifier = 0
            });
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "MQTT消息发送失败: {Topic}", applicationMessage.Topic);
            throw;
        }
    }

    /// <summary>
    /// 启动客户端
    /// </summary>
    public async Task StartAsync(ManagedMqttClientOptions options)
    {
        if (_isStarted) return;

        try
        {
            await _client.StartAsync();
            _isStarted = true;

            // 触发连接成功事件
            if (ConnectedAsync != null)
            {
                var args = new MqttClientConnectedEventArgs(new MqttClientConnectResult
                {
                    ResultCode = MqttClientConnectResultCode.Success,
                    IsSessionPresent = false
                });
                await ConnectedAsync.Invoke(args);
            }
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "启动MQTT客户端失败");

            // 触发连接失败事件
            if (ConnectingFailedAsync != null)
            {
                var args = new ManagedProcessFailedEventArgs(ex, "启动MQTT客户端失败");
                await ConnectingFailedAsync.Invoke(args);
            }

            throw;
        }
    }

    /// <summary>
    /// 停止客户端
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isStarted) return;

        try
        {
            await _client.StopAsync();
            _isStarted = false;

            // 触发断开连接事件
            if (DisconnectedAsync != null)
            {
                var args = new MqttClientDisconnectedEventArgs
                {
                    ReasonString = "客户端主动断开连接",
                    ClientWasConnected = true
                };
                await DisconnectedAsync.Invoke(args);
            }
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "停止MQTT客户端失败");
            throw;
        }
    }

    /// <summary>
    /// 订阅主题
    /// </summary>
    public async Task SubscribeAsync(MqttTopicFilter topicFilter)
    {
        if (topicFilter == null) throw new ArgumentNullException(nameof(topicFilter));

        try
        {
            // 将MQTTnet的QoS转换为DotNetty的QoS
            var qos = (MQTTQualityOfServiceLevel)(int)topicFilter.QualityOfServiceLevel;

            // 订阅主题
            await _client.SubscribeAsync(topicFilter.Topic, async message =>
            {
                // 触发消息接收事件
                if (ApplicationMessageReceivedAsync != null)
                {
                    var appMessage = new MqttApplicationMessage
                    {
                        Topic = message.Topic,
                        PayloadSegment = new ArraySegment<byte>(message.Payload),
                        QualityOfServiceLevel = (MqttQualityOfServiceLevel)(int)message.QoS,
                        Retain = message.Retain
                    };

                    var args = new MqttApplicationMessageReceivedEventArgs(
                        "client-id",
                        message.Topic,
                        appMessage,
                        null,
                        null,
                        null);

                    await ApplicationMessageReceivedAsync.Invoke(args);

                    // 触发消息处理完成事件
                    ApplicationMessageProcessed?.Invoke(this, new ApplicationMessageProcessedEventArgs(appMessage));
                }
            }, qos);

            // 保存订阅信息
            _subscriptions[topicFilter.Topic] = topicFilter;
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "订阅MQTT主题失败: {Topic}", topicFilter.Topic);

            // 触发同步订阅失败事件
            if (SynchronizingSubscriptionsFailedAsync != null)
            {
                var args = new ManagedProcessFailedEventArgs(ex, $"订阅MQTT主题失败: {topicFilter.Topic}");
                await SynchronizingSubscriptionsFailedAsync.Invoke(args);
            }

            throw;
        }
    }

    /// <summary>
    /// 批量订阅主题
    /// </summary>
    public async Task SubscribeAsync(ICollection<MqttTopicFilter> topicFilters)
    {
        if (topicFilters == null) throw new ArgumentNullException(nameof(topicFilters));

        foreach (var filter in topicFilters)
        {
            await SubscribeAsync(filter);
        }
    }

    /// <summary>
    /// 取消订阅主题
    /// </summary>
    public async Task UnsubscribeAsync(string topic)
    {
        if (string.IsNullOrEmpty(topic)) throw new ArgumentNullException(nameof(topic));

        try
        {
            await _client.UnsubscribeAsync(topic);
            _subscriptions.TryRemove(topic, out _);
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "取消订阅MQTT主题失败: {Topic}", topic);
            throw;
        }
    }

    /// <summary>
    /// 批量取消订阅主题
    /// </summary>
    public async Task UnsubscribeAsync(ICollection<string> topics)
    {
        if (topics == null) throw new ArgumentNullException(nameof(topics));

        foreach (var topic in topics)
        {
            await UnsubscribeAsync(topic);
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;

        try
        {
            if (_isStarted)
            {
                StopAsync().Wait();
            }

            _client.Dispose();
            _isDisposed = true;
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "释放MQTT客户端资源失败");
        }
    }
    #endregion
}