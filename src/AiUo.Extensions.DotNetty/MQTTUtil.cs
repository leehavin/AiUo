using AiUo.Configuration;
using AiUo.Extensions.DotNetty.Configuration;
using AiUo.Extensions.DotNetty.Core.DotNetty;
using AiUo.Extensions.MQTT;
using AiUo.Logging;
using AiUo.Reflection;
using AiUo.Text;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AiUo.Extensions.DotNetty;

/// <summary>
/// MQTT辅助类，需要UseMQTTEx()注册
/// </summary>
public static class MQTTUtil
{
    private static ConcurrentDictionary<Type, Attribute> _messageAttrDict = new();

    #region Methods
    internal static MQTTConnectionStringElement GetConnectionStringElement(string connectionStringName = null)
    {
        var section = ConfigUtil.GetSection<MQTTSection>();
        connectionStringName ??= section?.DefaultConnectionStringName;
        if (!section.ConnectionStrings.TryGetValue(connectionStringName, out var ret))
            throw new Exception($"配置MQTT:ConnectionStrings中没有此Name: {connectionStringName}");
        return ret;
    }

    public static MQTTClient GetClient(string connectionStringName = null)
        => DIUtil.GetRequiredService<MQTTContainer>().GetClient(connectionStringName);

    internal static T GetMessageAttribute<T>(object message)
        where T : Attribute
        => GetMessageAttribute<T>(message.GetType());

    internal static T GetMessageAttribute<T>(Type messageType)
        where T : Attribute
    {
        return (T)_messageAttrDict.GetOrAdd(messageType, messageType.GetCustomAttribute<T>());
    }

    private static MQTTMessageMeta SetMessageMeta<TMessage>(TMessage message)
    {
        MQTTMessageMeta ret = null;
        if (message is IMQTTMessage msg)
        {
            if (msg.MQTTMeta != null)
                throw new Exception("MQTTUtil.Publish时,Message属性MQTTMeta必须为null");
            ret = msg.MQTTMeta = GetMessageMeta();
        }
        else
        {
            var prop = message.GetType().GetProperty("MQTTMeta");
            if (prop != null && (prop.PropertyType == typeof(object) || prop.PropertyType == typeof(MQTTMessageMeta)))
            {
                if (ReflectionUtil.GetPropertyValue(message, prop) != null)
                    throw new Exception($"MQTTUtil.Publish时,Message的MQTTMeta属性必须为null");
                ret = GetMessageMeta();
                ReflectionUtil.SetPropertyValue(message, prop, ret);
            }
        }
        return ret;

        MQTTMessageMeta GetMessageMeta()
        {
            var now = DateTime.UtcNow;
            return new MQTTMessageMeta
            {
                MessageId = ObjectId.NewId(now),
                Timestamp = now.ToTimestamp()
            };
        }
    }
    #endregion

    #region Publish
    /// <summary>
    /// 向MQTT服务器发布消息
    /// </summary>
    /// <typeparam name="TMessage">消息类型</typeparam>
    /// <param name="topic">主题</param>
    /// <param name="message">消息内容</param>
    /// <param name="qos">服务质量</param>
    /// <param name="retain">是否保留消息</param>
    /// <param name="connectionStringName">连接字符串名称</param>
    public static void Publish<TMessage>(string topic, TMessage message, MQTTQualityOfServiceLevel qos = MQTTQualityOfServiceLevel.AtMostOnce, bool retain = false, string connectionStringName = null)
        where TMessage : new()
    {
        var client = GetClient(connectionStringName);
        var payload = SerializeMessage(message);
        var mqttMessage = new MQTTApplicationMessage
        {
            Topic = topic,
            Payload = payload,
            QoS = qos,
            Retain = retain
        };

        client.EnqueueAsync(mqttMessage).Wait();

        if (ConfigUtil.GetSection<MQTTSection>().MessageLogEnabled)
        {
            LogUtil.Info("MQTT发布消息: Topic={Topic}, QoS={QoS}, Retain={Retain}, Size={Size}bytes",
                topic, qos, retain, payload.Length);
        }
    }

    /// <summary>
    /// 向MQTT服务器发布消息（异步）
    /// </summary>
    /// <typeparam name="TMessage">消息类型</typeparam>
    /// <param name="topic">主题</param>
    /// <param name="message">消息内容</param>
    /// <param name="qos">服务质量</param>
    /// <param name="retain">是否保留消息</param>
    /// <param name="connectionStringName">连接字符串名称</param>
    public static async Task PublishAsync<TMessage>(string topic, TMessage message, MQTTQualityOfServiceLevel qos = MQTTQualityOfServiceLevel.AtMostOnce, bool retain = false, string connectionStringName = null)
        where TMessage : new()
    {
        var client = GetClient(connectionStringName);
        var payload = SerializeMessage(message);
        var mqttMessage = new MQTTApplicationMessage
        {
            Topic = topic,
            Payload = payload,
            QoS = qos,
            Retain = retain
        };

        await client.EnqueueAsync(mqttMessage);

        if (ConfigUtil.GetSection<MQTTSection>().MessageLogEnabled)
        {
            LogUtil.Info("MQTT发布消息: Topic={Topic}, QoS={QoS}, Retain={Retain}, Size={Size}bytes",
                topic, qos, retain, payload.Length);
        }
    }
    #endregion

    #region Subscribe
    /// <summary>
    /// 订阅MQTT主题
    /// </summary>
    /// <param name="topic">主题</param>
    /// <param name="handler">消息处理函数</param>
    /// <param name="qos">服务质量</param>
    /// <param name="connectionStringName">连接字符串名称</param>
    public static async Task SubscribeAsync(string topic, Func<MQTTApplicationMessage, Task> handler, MQTTQualityOfServiceLevel qos = MQTTQualityOfServiceLevel.AtMostOnce, string connectionStringName = null)
    {
        var client = GetClient(connectionStringName);

        // 订阅主题
        await client.SubscribeAsync(topic, async message =>
        {
            var appMessage = new MQTTApplicationMessage
            {
                Topic = message.Topic,
                Payload = message.Payload,
                QoS = message.QoS,
                Retain = message.IsRetain
            };
            
            await handler(appMessage);
        }, qos);

        LogUtil.Info("MQTT订阅主题: {Topic}, QoS={QoS}", topic, qos);
    }

    /// <summary>
    /// 订阅MQTT主题并自动反序列化消息
    /// </summary>
    /// <typeparam name="TMessage">消息类型</typeparam>
    /// <param name="topic">主题</param>
    /// <param name="handler">消息处理函数</param>
    /// <param name="qos">服务质量</param>
    /// <param name="connectionStringName">连接字符串名称</param>
    public static async Task SubscribeAsync<TMessage>(string topic, Func<TMessage, Task> handler, MQTTQualityOfServiceLevel qos = MQTTQualityOfServiceLevel.AtMostOnce, string connectionStringName = null)
        where TMessage : class, new()
    {
        var client = GetClient(connectionStringName);
        
        // 直接使用客户端的泛型订阅方法
        await client.SubscribeAsync<TMessage>(topic, async message =>
        {
            try
            {
                await handler(message);
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "MQTT消息处理异常: {Topic}", topic);
            }
        }, qos);
        
        LogUtil.Info("MQTT订阅主题: {Topic}, QoS={QoS}", topic, qos);
    }

    /// <summary>
    /// 取消订阅MQTT主题
    /// </summary>
    /// <param name="topic">主题</param>
    /// <param name="connectionStringName">连接字符串名称</param>
    public static async Task UnsubscribeAsync(string topic, string connectionStringName = null)
    {
        var client = GetClient(connectionStringName);
        await client.UnsubscribeAsync(topic);
        LogUtil.Info("MQTT取消订阅主题: {Topic}", topic);
    }
    #endregion

    #region Serialization
    private static byte[] SerializeMessage<TMessage>(TMessage message)
    {
        SetMessageMeta(message);
        var json = JsonSerializer.Serialize(message);
        return Encoding.UTF8.GetBytes(json);
    }

    private static TMessage DeserializeMessage<TMessage>(byte[] payload)
        where TMessage : class, new()
    {
        var json = Encoding.UTF8.GetString(payload);
        return JsonSerializer.Deserialize<TMessage>(json);
    }
    #endregion
}