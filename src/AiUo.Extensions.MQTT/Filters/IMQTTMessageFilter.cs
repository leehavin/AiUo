namespace AiUo.Extensions.MQTT.Filters;

/// <summary>
/// MQTT消息过滤器接口
/// </summary>
/// <typeparam name="TMessage">消息类型</typeparam>
public interface IMQTTMessageFilter<TMessage>
    where TMessage : class
{
    /// <summary>
    /// 过滤消息
    /// </summary>
    /// <param name="message">原始消息</param>
    /// <returns>如果返回true则继续处理，返回false则丢弃消息</returns>
    bool Filter(TMessage message);
}