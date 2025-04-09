namespace AiUo.Extensions.DotNetty;

/// <summary>
/// MQTT消费者接口
/// </summary>
public interface IMQTTConsumer
{
    /// <summary>
    /// 注册消费者
    /// </summary>
    Task Register();

    /// <summary>
    /// 注销消费者
    /// </summary>
    void Unregister();
}

/// <summary>
/// MQTT消费者忽略特性
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class MQTTConsumerIgnoreAttribute : Attribute
{
}

/// <summary>
/// MQTT订阅消费者基类
/// </summary>
/// <typeparam name="TMessage">消息类型</typeparam>
public abstract class MQTTSubscribeConsumer<TMessage> : IMQTTConsumer
    where TMessage : class, new()
{
    /// <summary>
    /// 获取订阅主题
    /// </summary>
    protected abstract string GetTopic();

    /// <summary>
    /// 获取服务质量级别
    /// </summary>
    protected virtual MQTTQualityOfServiceLevel GetQoS() => MQTTQualityOfServiceLevel.AtMostOnce;

    /// <summary>
    /// 获取连接字符串名称
    /// </summary>
    protected virtual string GetConnectionStringName() => null;

    /// <summary>
    /// 处理消息
    /// </summary>
    /// <param name="message">消息内容</param>
    protected abstract Task HandleMessageAsync(TMessage message);

    /// <summary>
    /// 注册消费者
    /// </summary>
    public virtual async Task Register()
    {
        var topic = GetTopic();
        var qos = GetQoS();
        var connectionStringName = GetConnectionStringName();

        await MQTTUtil.SubscribeAsync<TMessage>(
            topic,
            HandleMessageAsync,
            qos,
            connectionStringName);
    }

    /// <summary>
    /// 注销消费者
    /// </summary>
    public virtual void Unregister()
    {
        var topic = GetTopic();
        var connectionStringName = GetConnectionStringName();

        MQTTUtil.UnsubscribeAsync(topic, connectionStringName).Wait();
    }
}