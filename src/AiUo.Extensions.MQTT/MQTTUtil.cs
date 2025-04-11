using AiUo.Configuration;
using AiUo.Logging;
using AiUo.Reflection;
using AiUo.Text;
using MQTTnet;
using MQTTnet.Protocol;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace AiUo.Extensions.MQTT;

/// <summary>
/// MQTT工具类，提供消息发布和订阅功能
/// </summary>
public static class MQTTUtil
{
    private static ConcurrentDictionary<Type, Attribute> _messageAttrDict = new();

    #region Helper Methods
    internal static MQTTConnectionStringElement GetConnectionStringElement(string connectionStringName = null)
    {
        var section = ConfigUtil.GetSection<MQTTSection>();
        connectionStringName ??= section?.DefaultConnectionStringName;
        if (!section.ConnectionStrings.TryGetValue(connectionStringName, out var ret))
            throw new Exception($"配置MQTT:ConnectionStrings中没有此Name: {connectionStringName}");
        return ret;
    }

    public static IMqttClient GetClient(string connectionStringName = null)
        => DIUtil.GetRequiredService<MQTTContainer>().GetPublishClient(connectionStringName);

    internal static T GetMessageAttribute<T>(object message)
        where T : Attribute
        => GetMessageAttribute<T>(message.GetType());

    internal static T GetMessageAttribute<T>(Type messageType)
        where T : Attribute
    {
        return (T)_messageAttrDict.GetOrAdd(messageType, messageType.GetCustomAttribute<T>());
    }

    private static MQTTMessageMeta SetMessageMeta<TMessage>(TMessage message, string topic = null, int qosLevel = 0, bool retain = false)
    {
        MQTTMessageMeta ret = null;
        if (message is IMQTTMessage msg)
        {
            if (msg.MQTTMeta != null)
                throw new Exception("MQTTUtil.Publish时,Message属性MQTTMeta必须为null");
            ret = msg.MQTTMeta = GetMessageMeta(topic, qosLevel, retain);
        }
        else
        {
            var prop = message.GetType().GetProperty("MQTTMeta");
            if (prop != null && (prop.PropertyType == typeof(object) || prop.PropertyType == typeof(MQTTMessageMeta)))
            {
                if (ReflectionUtil.GetPropertyValue(message, prop) != null)
                    throw new Exception($"MQTTUtil.Publish时,Message的MQTTMeta属性必须为null");
                ret = GetMessageMeta(topic, qosLevel, retain);
                ReflectionUtil.SetPropertyValue(message, prop, ret);
            }
        }
        return ret;

        MQTTMessageMeta GetMessageMeta(string topicName, int qos, bool isRetain)
        {
            var now = DateTime.UtcNow;
            return new MQTTMessageMeta
            {
                MessageId = ObjectId.NewId(now),
                Timestamp = now.ToTimestamp(),
                Topic = topicName,
                QosLevel = qos,
                Retain = isRetain
            };
        }
    }
    #endregion

    #region Publish Methods
    /// <summary>
    /// 发布MQTT消息
    /// </summary>
    /// <typeparam name="TMessage">消息类型</typeparam>
    /// <param name="message">消息对象</param>
    /// <param name="topic">消息主题，如果为null则使用消息类型上的MQTTPublishMessageAttribute中的Topic</param>
    /// <param name="qosLevel">服务质量等级(0,1,2)，如果为-1则使用消息类型上的MQTTPublishMessageAttribute中的QosLevel</param>
    /// <param name="retain">是否保留消息，如果为null则使用消息类型上的MQTTPublishMessageAttribute中的Retain</param>
    /// <param name="connectionStringName">连接字符串名称</param>
    public static void Publish<TMessage>(TMessage message, string topic = null, int qosLevel = -1, bool? retain = null, string connectionStringName = null)
        where TMessage : class, new()
    {
        var data = GetPublishData(message, topic, qosLevel, retain, connectionStringName);
        var client = GetClient(data.ConnectionStringName);

        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic(data.Topic)
            .WithPayload(data.Payload)
            .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)data.QosLevel)
            .WithRetainFlag(data.Retain)
            .Build();

        client.PublishAsync(applicationMessage, CancellationToken.None).Wait();

        if (ConfigUtil.GetSection<MQTTSection>().MessageLogEnabled)
        {
            LogUtil.Info("[MQTT] 已发布消息: {Topic}, QoS: {QosLevel}, Retain: {Retain}",
                data.Topic, data.QosLevel, data.Retain);
        }
    }

    /// <summary>
    /// 异步发布MQTT消息
    /// </summary>
    /// <typeparam name="TMessage">消息类型</typeparam>
    /// <param name="message">消息对象</param>
    /// <param name="topic">消息主题，如果为null则使用消息类型上的MQTTPublishMessageAttribute中的Topic</param>
    /// <param name="qosLevel">服务质量等级(0,1,2)，如果为-1则使用消息类型上的MQTTPublishMessageAttribute中的QosLevel</param>
    /// <param name="retain">是否保留消息，如果为null则使用消息类型上的MQTTPublishMessageAttribute中的Retain</param>
    /// <param name="connectionStringName">连接字符串名称</param>
    public static async Task PublishAsync<TMessage>(TMessage message, string topic = null, int qosLevel = -1, bool? retain = null, string connectionStringName = null)
        where TMessage : class, new()
    {
        var data = GetPublishData(message, topic, qosLevel, retain, connectionStringName);
        var client = GetClient(data.ConnectionStringName);

        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic(data.Topic)
            .WithPayload(data.Payload)
            .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)data.QosLevel)
            .WithRetainFlag(data.Retain)
            .Build();

        await client.PublishAsync(applicationMessage, CancellationToken.None);

        if (ConfigUtil.GetSection<MQTTSection>().MessageLogEnabled)
        {
            LogUtil.Info("[MQTT] 已发布消息: {Topic}, QoS: {QosLevel}, Retain: {Retain}",
                data.Topic, data.QosLevel, data.Retain);
        }
    }

    private static (string Topic, byte[] Payload, int QosLevel, bool Retain, string ConnectionStringName) GetPublishData<TMessage>(TMessage message, string topic, int qosLevel, bool? retain, string connectionStringName)
        where TMessage : class, new()
    {
        // 获取消息特性
        var attr = GetMessageAttribute<MQTTPublishMessageAttribute>(message);

        // 确定主题
        string finalTopic = topic ?? attr?.Topic;
        if (string.IsNullOrEmpty(finalTopic))
            throw new Exception($"发布消息时未指定Topic，且消息类型{message.GetType().Name}未标记MQTTPublishMessageAttribute或未指定Topic");

        // 确定QoS级别
        int finalQosLevel = qosLevel >= 0 ? qosLevel : (attr?.QosLevel ?? 0);
        if (finalQosLevel < 0 || finalQosLevel > 2)
            finalQosLevel = 0; // 默认使用QoS 0

        // 确定是否保留消息
        bool finalRetain = retain ?? attr?.Retain ?? false;

        // 设置消息元数据
        SetMessageMeta(message, finalTopic, finalQosLevel, finalRetain);

        // 序列化消息
        var payload = JsonSerializer.SerializeToUtf8Bytes(message);

        return (finalTopic, payload, finalQosLevel, finalRetain, connectionStringName);
    }
    #endregion

    #region Subscribe Methods
    /// <summary>
    /// 订阅MQTT主题
    /// </summary>
    /// <param name="topic">主题，支持通配符(+和#)</param>
    /// <param name="handler">消息处理函数</param>
    /// <param name="qosLevel">服务质量等级(0,1,2)</param>
    /// <param name="connectionStringName">连接字符串名称</param>
    /// <param name="enablePersistence">是否启用消息持久化</param>
    /// <returns>订阅标识符，可用于取消订阅</returns>
    public static Guid Subscribe(string topic, Action<MqttApplicationMessageReceivedEventArgs> handler, int qosLevel = 0, string connectionStringName = null, bool? enablePersistence = null)
    {
        var client = GetClient(connectionStringName);

        // 生成唯一标识符
        var subscriptionId = Guid.NewGuid();

        // 订阅主题
        var mqttTopicFilter = new MqttTopicFilterBuilder()
            .WithTopic(topic)
            .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qosLevel)
            .Build();

        client.SubscribeAsync(mqttTopicFilter, CancellationToken.None).Wait();

        // 注册消息处理程序
        client.ApplicationMessageReceivedAsync += async e =>
        {
            if (e.ApplicationMessage.Topic == topic)
            {
                handler(e);
            }
            await Task.CompletedTask;
        };

        LogUtil.Info("[MQTT] 已订阅主题: {Topic}, QoS: {QosLevel}", topic, qosLevel);

        return subscriptionId;
    }

    /// <summary>
    /// 异步订阅MQTT主题
    /// </summary>
    /// <param name="topic">主题</param>
    /// <param name="handler">消息处理函数</param>
    /// <param name="qosLevel">服务质量等级(0,1,2)</param>
    /// <param name="connectionStringName">连接字符串名称</param>
    /// <returns>订阅标识符，可用于取消订阅</returns>
    public static async Task<Guid> SubscribeAsync(string topic, Action<MqttApplicationMessageReceivedEventArgs> handler, int qosLevel = 0, string connectionStringName = null)
    {
        var client = GetClient(connectionStringName);

        // 生成唯一标识符
        var subscriptionId = Guid.NewGuid();

        // 订阅主题
        var mqttTopicFilter = new MqttTopicFilterBuilder()
            .WithTopic(topic)
            .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qosLevel)
            .Build();

        await client.SubscribeAsync(mqttTopicFilter, CancellationToken.None);

        // 注册消息处理程序
        client.ApplicationMessageReceivedAsync += async e =>
        {
            if (e.ApplicationMessage.Topic == topic)
            {
                handler(e);
            }
            await Task.CompletedTask;
        };

        LogUtil.Info("[MQTT] 已订阅主题: {Topic}, QoS: {QosLevel}", topic, qosLevel);

        return subscriptionId;
    }

    /// <summary>
    /// 取消订阅MQTT主题
    /// </summary>
    /// <param name="topic">主题</param>
    /// <param name="connectionStringName">连接字符串名称</param>
    public static void Unsubscribe(string topic, string connectionStringName = null)
    {
        var client = GetClient(connectionStringName);
        client.UnsubscribeAsync(topic, CancellationToken.None).Wait();
        LogUtil.Info("[MQTT] 已取消订阅主题: {Topic}", topic);
    }

    /// <summary>
    /// 异步取消订阅MQTT主题
    /// </summary>
    /// <param name="topic">主题</param>
    /// <param name="connectionStringName">连接字符串名称</param>
    public static async Task UnsubscribeAsync(string topic, string connectionStringName = null)
    {
        var client = GetClient(connectionStringName);
        await client.UnsubscribeAsync(topic, CancellationToken.None);
        LogUtil.Info("[MQTT] 已取消订阅主题: {Topic}", topic);
    }
    #endregion
}