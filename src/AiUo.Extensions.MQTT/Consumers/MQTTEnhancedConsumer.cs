using AiUo.Extensions.MQTT.Filters;
using AiUo.Extensions.MQTT.Pipeline;
using AiUo.Extensions.MQTT.Transformers;
using AiUo.Logging;
using MQTTnet;
using System.Text;
using System.Text.Json;

namespace AiUo.Extensions.MQTT.Consumers;

/// <summary>
/// 增强的MQTT订阅消费者基类，支持消息过滤和转换
/// </summary>
/// <typeparam name="TMessage">消息类型</typeparam>
public abstract class MQTTEnhancedConsumer<TMessage> : IMQTTConsumer
    where TMessage : class, new()
{
    private IMqttClient _client;
    private string _topic;
    private int _qosLevel;
    private bool _isRegistered;
    private readonly MQTTMessagePipeline<TMessage> _pipeline;

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
    /// 创建一个新的增强MQTT订阅消费者
    /// </summary>
    /// <param name="topic">订阅主题</param>
    /// <param name="qosLevel">服务质量等级(0,1,2)</param>
    protected MQTTEnhancedConsumer(string topic, int qosLevel = 0)
    {
        _topic = topic;
        _qosLevel = qosLevel;
        _pipeline = new MQTTMessagePipeline<TMessage>(ProcessMessageAsync);
    }

    /// <summary>
    /// 添加消息过滤器
    /// </summary>
    /// <param name="filter">消息过滤器</param>
    /// <returns>当前消费者</returns>
    protected MQTTEnhancedConsumer<TMessage> AddFilter(IMQTTMessageFilter<TMessage> filter)
    {
        _pipeline.AddFilter(filter);
        return this;
    }

    /// <summary>
    /// 添加消息转换器
    /// </summary>
    /// <typeparam name="TOutput">输出消息类型</typeparam>
    /// <param name="transformer">消息转换器</param>
    /// <param name="handler">转换后消息的处理函数</param>
    /// <returns>当前消费者</returns>
    protected MQTTEnhancedConsumer<TMessage> AddTransformer<TOutput>(
        IMQTTMessageTransformer<TMessage, TOutput> transformer,
        Func<TOutput, Task> handler)
        where TOutput : class
    {
        _pipeline.AddTransformer(transformer).AddFilter(new DelegateFilter<TOutput>(msg => true));
        return this;
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

            // 通过管道处理消息
            await _pipeline.ProcessAsync(message);
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

    /// <summary>
    /// 委托过滤器实现
    /// </summary>
    private class DelegateFilter<T> : IMQTTMessageFilter<T> where T : class
    {
        private readonly Func<T, bool> _filterFunc;

        public DelegateFilter(Func<T, bool> filterFunc)
        {
            _filterFunc = filterFunc ?? throw new ArgumentNullException(nameof(filterFunc));
        }

        public bool Filter(T message) => _filterFunc(message);
    }
}