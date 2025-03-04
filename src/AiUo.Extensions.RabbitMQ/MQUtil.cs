using EasyNetQ;
using EasyNetQ.Topology;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using AiUo.Configuration;
using AiUo.Text;
using AiUo.Reflection;
using AiUo.Logging;

namespace AiUo.Extensions.RabbitMQ;

/// <summary>
/// MQ辅助类，需要UseRabbitMQEx()注册
///     MQUtil发布 => IConsumer子类消费
/// </summary>
public static class MQUtil
{
    private static ConcurrentDictionary<Type, Attribute> _messageAttrDict = new();

    #region Methods
    internal static MQConnectionStringElement GetConnectionStringElement(string connectionStringName = null)
    {
        var section = ConfigUtil.GetSection<RabbitMQSection>();
        connectionStringName ??= section?.DefaultConnectionStringName;
        if (!section.ConnectionStrings.TryGetValue(connectionStringName, out var ret))
            throw new Exception($"配置RabbitMQ:ConnectionStrings中没有此Name: {connectionStringName}");
        return ret;
    }
    public static IBus GetBus(string connectionStringName = null)
        => DIUtil.GetRequiredService<MQContainer>().GetPubBus(connectionStringName);
    internal static T GetMessageAttribute<T>(object message)
        where T : Attribute
        => GetMessageAttribute<T>(message.GetType());
    internal static T GetMessageAttribute<T>(Type messageType)
        where T : Attribute
    {
        return (T)_messageAttrDict.GetOrAdd(messageType, messageType.GetCustomAttribute<T>());
    }
    private static MQMessageMeta SetMessageMeta<TMessage>(TMessage message)
    {
        MQMessageMeta ret = null;
        if (message is IMQMessage msg)
        {
            if (msg.MQMeta != null)
                throw new Exception("MQUtil.Publish时,Message属性MQMeta必须为null");
            ret = msg.MQMeta = GetMessageMeta();
        }
        else
        {
            var prop = message.GetType().GetProperty("MQMeta");
            if (prop != null && (prop.PropertyType == typeof(object) || prop.PropertyType == typeof(MQMessageMeta)))
            {
                if (ReflectionUtil.GetPropertyValue(message, prop) != null)
                    throw new Exception($"MQUtil.Publish时,Message的MQMeta属性必须为null");
                ret = GetMessageMeta();
                ReflectionUtil.SetPropertyValue(message, prop, ret);
            }
        }
        return ret;
        MQMessageMeta GetMessageMeta()
        {
            var now = DateTime.UtcNow;
            return new MQMessageMeta
            {
                MessageId = ObjectId.NewId(now),
                Timestamp = now.ToTimestamp()
            };
        }
    }
    #endregion

    #region Publish => Subscribe （消费类继承SubscribeConsumer）
    /// <summary>
    /// 向MQ发布Publish命令，消费类需要继承SubscribeConsumer进行消费
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message"></param>
    /// <param name="configAction"></param>
    /// <param name="connectionStringName"></param>
    public static void Publish<TMessage>(TMessage message, Action<IPublishConfiguration> configAction = null, string connectionStringName = null)
        where TMessage : new()
    {
        var data = GetPubSubData(message, configAction, connectionStringName);
        GetBus(data.ConnStrName)
            .PubSub.Publish(data.Message, data.Configure);
    }

    /// <summary>
    /// 向MQ发布Publish命令，消费类需要继承SubscribeConsumer进行消费
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message"></param>
    /// <param name="configAction"></param>
    /// <param name="connectionStringName"></param>
    /// <returns></returns>
    public static async Task PublishAsync<TMessage>(TMessage message, Action<IPublishConfiguration> configAction = null, string connectionStringName = null)
        where TMessage : new()
    {
        var data = GetPubSubData(message, configAction, connectionStringName);
        await GetBus(data.ConnStrName)
            .PubSub.PublishAsync(data.Message, data.Configure);

    }
    private static (string ConnStrName, TMessage Message, Action<IPublishConfiguration> Configure) GetPubSubData<TMessage>(TMessage message, Action<IPublishConfiguration> customConfigure, string connectionStringName = null)
    {
        // action
        var attr = GetMessageAttribute<MQPublishMessageAttribute>(message);
        Action<IPublishConfiguration> configure = (config) =>
        {
            if (attr != null)
            {
                if (attr.ExpireSeconds != 0)
                    config.WithExpires(TimeSpan.FromMilliseconds(attr.ExpireSeconds));
                if (attr.Priority != 0)
                    config.WithPriority(attr.Priority);
                if (!string.IsNullOrEmpty(attr.Topic))
                    config.WithTopic(attr.Topic);
            }
            customConfigure?.Invoke(config);
        };

        // connectionStringName
        var connStrName = connectionStringName ?? attr?.ConnectionStringName;
        return (connStrName, message, configure);
    }
    #endregion

    #region FuturePublish
    public static void FuturePublish<TMessage>(TMessage message, TimeSpan delay, Action<IFuturePublishConfiguration> configAction = null, string connectionStringName = null)
        where TMessage : new()
    {
        var data = GetFutureData(message, delay, configAction, connectionStringName);
        GetBus(data.ConnStrName)
            .Scheduler.FuturePublish(data.Message, message.GetType(), delay, data.Configure);
    }
    /// <summary>
    /// 发布延迟执行任务
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message"></param>
    /// <param name="delay"></param>
    /// <param name="configAction"></param>
    /// <param name="connectionStringName"></param>
    /// <returns></returns>
    public static Task FuturePublishAsync<TMessage>(TMessage message, TimeSpan delay, Action<IFuturePublishConfiguration> configAction = null, string connectionStringName = null)
        where TMessage : new()
    {
        var data = GetFutureData(message, delay, configAction, connectionStringName);
        return GetBus(data.ConnStrName)
            .Scheduler.FuturePublishAsync(data.Message, delay, data.Configure);
    }
    private static (string ConnStrName, TMessage Message, Action<IFuturePublishConfiguration> Configure) GetFutureData<TMessage>(TMessage message, TimeSpan delay, Action<IFuturePublishConfiguration> customConfigure, string connectionStringName = null)
        where TMessage : new()
    {
        var attr = GetMessageAttribute<MQPublishMessageAttribute>(message);
        Action<IFuturePublishConfiguration> configure = (config) =>
        {
            if (attr != null)
            {
                if (attr.Priority != 0)
                    config.WithPriority(attr.Priority);
                if (!string.IsNullOrEmpty(attr.Topic))
                    config.WithTopic(attr.Topic);
            }
            customConfigure?.Invoke(config);
        };
        var connStrName = connectionStringName ?? attr?.ConnectionStringName;
        return (connStrName, message, configure);
    }
    #endregion

    #region Request => Respond（响应类继承RespondConsumer）
    public static MQResponseResult<TResponse> Request<TMessage, TResponse>(TMessage message, Action<IRequestConfiguration> configAction = null, string connectionStringName = null)
        where TMessage : new()
    {
        var data = GetRpcData(message, configAction, connectionStringName);
        return GetBus(data.ConnStrName)
            .Rpc.Request<TMessage, MQResponseResult<TResponse>>(data.Message, data.Action);
    }
    /// <summary>
    /// 向MQ发布Request请求，响应类需要继承RespondConsumer进行响应
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="message"></param>
    /// <param name="configAction"></param>
    /// <param name="connectionStringName"></param>
    /// <returns></returns>
    public static Task<MQResponseResult<TResponse>> RequestAsync<TMessage, TResponse>(TMessage message, Action<IRequestConfiguration> configAction = null, string connectionStringName = null)
        where TMessage : new()
    {
        var data = GetRpcData(message, configAction, connectionStringName);
        return GetBus(data.ConnStrName)
            .Rpc.RequestAsync<TMessage, MQResponseResult<TResponse>>(data.Message, data.Action);
    }
    private static (TMessage Message, Action<IRequestConfiguration> Action, string ConnStrName) GetRpcData<TMessage>(TMessage message, Action<IRequestConfiguration> configAction, string connectionStringName = null)
        where TMessage : new()
    {
        SetMessageMeta(message);
        var attr = GetMessageAttribute<MQRequestMessageAttribute>(message);
        Action<IRequestConfiguration> action = (config) =>
        {
            if (attr != null)
            {
                if (attr.ExpireSeconds != 0)
                    config.WithExpiration(TimeSpan.FromMilliseconds(attr.ExpireSeconds));
                if (attr.Priority != 0)
                    config.WithPriority(attr.Priority);
                if (!string.IsNullOrEmpty(attr.QueueName))
                    config.WithQueueName(attr.QueueName);
            }
            configAction?.Invoke(config);
        };
        var connStrName = connectionStringName ?? attr?.ConnectionStringName;
        return (message, action, connStrName);
    }
    #endregion

    #region Send => Receive（消费类继承ReceiveConsumer）
    /// <summary>
    /// 向MQ发送Send消息，响应类需要继承ReceiveConsumer进行响应
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message"></param>
    /// <param name="queueName"></param>
    /// <param name="configAction"></param>
    /// <param name="connectionStringName"></param>
    public static void Send<TMessage>(TMessage message, string queueName = null, Action<ISendConfiguration> configAction = null, string connectionStringName = null)
        where TMessage : new()
    {
        var data = GetSendReceiveData(message, queueName, configAction, connectionStringName);
        GetBus(data.ConnStrName)
            .SendReceive.Send(data.Queue, data.Message, data.Action);
    }

    /// <summary>
    /// 向MQ发送Send消息，响应类需要继承ReceiveConsumer进行响应
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message"></param>
    /// <param name="queueName"></param>
    /// <param name="configAction"></param>
    /// <param name="connectionStringName"></param>
    /// <returns></returns>
    public static Task SendAsync<TMessage>(TMessage message, string queueName = null, Action<ISendConfiguration> configAction = null, string connectionStringName = null)
        where TMessage : new()
    {
        var data = GetSendReceiveData(message, queueName, configAction, connectionStringName);
        return GetBus(data.ConnStrName)
            .SendReceive.SendAsync(data.Queue, data.Message, data.Action);
    }
    private static (TMessage Message, Action<ISendConfiguration> Action, string ConnStrName, string Queue) GetSendReceiveData<TMessage>(TMessage message, string queueName = null, Action<ISendConfiguration> configAction = null, string connectionStringName = null)
        where TMessage : new()
    {
        SetMessageMeta(message);
        var attr = GetMessageAttribute<MQSendMessageAttribute>(message);
        Action<ISendConfiguration> action = (config) =>
        {
            if (attr != null)
            {
                if (attr.Priority != 0)
                    config.WithPriority(attr.Priority);
            }
            configAction?.Invoke(config);
        };
        var connStrName = connectionStringName ?? attr?.ConnectionStringName;
        var queue = queueName ?? attr.QueueName;
        if (string.IsNullOrEmpty(queue))
            throw new Exception($"MQUtil.Send时，queueName不能为空。messageType:{message.GetType().FullName}");
        return (message, action, connStrName, queue);
    }
    #endregion

    #region ErrorQueue
    /// <summary>
    /// 获取错误队列中的消息
    /// </summary>
    /// <param name="count"></param>
    /// <param name="connectionStringName"></param>
    /// <returns></returns>
    public static async Task<List<MQErrorQueueMessage>> GetErrorMessages(int count, string connectionStringName = null)
    {
        var ret = new List<MQErrorQueueMessage>();
        using var consumer = await GetErrorQueueConsumer(connectionStringName);
        var result = await consumer.PullBatchAsync(count);
        foreach (var msg in result.Messages)
        {
            if (msg.IsAvailable)
            {
                var item = GetMQErrorQueueMessage(msg);
                ret.Add(item);
            }
        }
        return ret;
    }
    /// <summary>
    /// 重新发送并移除错误队列中的消息
    /// </summary>
    /// <param name="messageId">如果为null，则会发送第一个没有MessageId的消息</param>
    /// <param name="connectionStringName"></param>
    /// <returns></returns>
    public static async Task<bool> RepublishErrorMessage(string messageId, string connectionStringName = null)
    {
        var bus = GetBus(connectionStringName).Advanced;
        using var consumer = await GetErrorQueueConsumer(connectionStringName);
        while (true)
        {
            var result = await consumer.PullAsync();
            if (!result.IsAvailable)
                return false;
            var item = GetMQErrorQueueMessage(result);
            // 相同message
            if (item.MQMessage?.MQMeta?.MessageId == messageId)
            {
                await bus.ExchangeDeclarePassiveAsync(item.Exchange);
                var exchange = new Exchange(item.Exchange);
                var msgBody = Encoding.UTF8.GetBytes(item.Message);
                await bus.PublishAsync(exchange, item.RoutingKey, true, item.BasicProperties, msgBody);
                await consumer.AckAsync(result.ReceivedInfo.DeliveryTag);
                LogUtil.Info("MQUtil.RepublishErrorMessage()重新发送并移除错误队列中的消息。{MessageId}", messageId);
                return true;
            }
        }
    }
    private static async Task<IPullingConsumer<PullResult>> GetErrorQueueConsumer(string connectionStringName = null)
    {
        var bus = GetBus(connectionStringName).Advanced;
        var errQueue = bus.Container.Resolve<IConventions>().ErrorQueueNamingConvention.Invoke(null);
        await bus.QueueDeclarePassiveAsync(errQueue);
        var queue = new EasyNetQ.Topology.Queue(errQueue);
        var consumer = bus.CreatePullingConsumer(queue, false);
        return consumer;
    }
    private static MQErrorQueueMessage GetMQErrorQueueMessage(PullResult result)
    {
        var body = Encoding.UTF8.GetString(result.Body.ToArray());
        var ret = SerializerUtil.DeserializeJson<MQErrorQueueMessage>(body);
        ret.MQMessage = SerializerUtil.DeserializeJson<MQMessage>(ret.Message.Replace("\\", ""));
        ret.ReceivedInfo = result.ReceivedInfo;
        return ret;
    }
    #endregion
}