using AiUo.Logging;
using MQTTnet;
using System.Text;
using System.Text.Json;

namespace AiUo.Extensions.MQTT;

/// <summary>
/// MQTT订阅消费者基类
/// </summary>
/// <typeparam name="TMessage">消息类型</typeparam>
public abstract class MQTTSubscribeConsumer<TMessage> : IMQTTConsumer
    where TMessage : class, new()
{
    private IMqttClient _client;
    private string _topic;
    private int _qosLevel;
    private bool _isRegistered;

    /// <summary>
    /// 连接字符串名称
    /// </summary>
    protected virtual string ConnectionStringName { get; set; }

    /// <summary>
    /// 订阅主题
    /// </summary>
    protected virtual string Topic => _topic;

    /// <summary>
    /// 服务质量等级(0,1,2)
    /// </summary>
    protected virtual int QosLevel => _qosLevel;

    /// <summary>
    /// 创建一个新的MQTT订阅消费者
    /// </summary>
    /// <param name="topic">订阅主题</param>
    /// <param name="qosLevel">服务质量等级(0,1,2)</param>
    protected MQTTSubscribeConsumer(string topic, int qosLevel = 0)
    {
        _topic = topic;
        _qosLevel = qosLevel;
    }

    /// <summary>
    /// 注册消费者
    /// </summary>
    public virtual async Task Register()
    {
        if (_isRegistered)
            return;

        _client = DIUtil.GetRequiredService<MQTTContainer>().GetSubscribeClient(ConnectionStringName);

        // 订阅主题
        var mqttSubscribeOptions = new MqttTopicFilterBuilder()
            .WithTopic(_topic)
            .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)_qosLevel)
            .Build();

        await _client.SubscribeAsync(mqttSubscribeOptions);

        // 注册消息处理程序
        _client.ApplicationMessageReceivedAsync += HandleMessageAsync;

        _isRegistered = true;

        LogUtil.Info("[MQTT] 已订阅主题: {Topic}, QoS: {QosLevel}", _topic, _qosLevel);
    }

    /// <summary>
    /// 处理接收到的消息
    /// </summary>
    /// <param name="e">消息事件参数</param>
    private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        try
        {
            if (e.ApplicationMessage.Topic != _topic)
                return;

            var payload = e.ApplicationMessage.PayloadSegment.ToArray();
            if (payload == null || payload.Length == 0)
                return;

            // 反序列化消息
            var messageJson = Encoding.UTF8.GetString(payload);
            var message = JsonSerializer.Deserialize<TMessage>(messageJson);

            if (message == null)
                return;

            // 设置消息元数据
            if (message is IMQTTMessage mqttMessage)
            {
                mqttMessage.MQTTMeta = new MQTTMessageMeta
                {
                    Topic = e.ApplicationMessage.Topic,
                    QosLevel = (int)e.ApplicationMessage.QualityOfServiceLevel,
                    Retain = e.ApplicationMessage.Retain,
                    MessageId = e.ApplicationMessage.Id ?? Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow.Ticks
                };
            }

            // 处理消息
            await ProcessMessageAsync(message);
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "[MQTT] 处理消息失败: {Topic}", e.ApplicationMessage.Topic);
        }
    }

    /// <summary>
    /// 处理消息的抽象方法，由子类实现
    /// </summary>
    /// <param name="message">接收到的消息</param>
    protected abstract Task ProcessMessageAsync(TMessage message);

    /// <summary>
    /// 释放资源
    /// </summary>
    public virtual void Dispose()
    {
        if (_isRegistered && _client != null)
        {
            _client.ApplicationMessageReceivedAsync -= HandleMessageAsync;
            _client.UnsubscribeAsync(_topic).Wait();
            _isRegistered = false;
        }
    }
}