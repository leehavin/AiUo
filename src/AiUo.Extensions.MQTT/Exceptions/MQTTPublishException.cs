namespace AiUo.Extensions.MQTT.Exceptions;

/// <summary>
/// MQTT消息发布异常
/// </summary>
public class MQTTPublishException : MQTTException
{
    /// <summary>
    /// 主题
    /// </summary>
    public string Topic { get; }

    /// <summary>
    /// 消息ID
    /// </summary>
    public string MessageId { get; }

    /// <summary>
    /// 创建一个新的MQTT消息发布异常
    /// </summary>
    /// <param name="topic">主题</param>
    /// <param name="messageId">消息ID</param>
    public MQTTPublishException(string topic, string messageId)
        : base($"MQTT消息发布失败: 主题={topic}, 消息ID={messageId}")
    {
        Topic = topic;
        MessageId = messageId;
    }

    /// <summary>
    /// 使用指定错误消息创建一个新的MQTT消息发布异常
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="topic">主题</param>
    /// <param name="messageId">消息ID</param>
    /// <param name="innerException">内部异常</param>
    public MQTTPublishException(string message, string topic, string messageId, Exception innerException = null)
        : base(message, innerException)
    {
        Topic = topic;
        MessageId = messageId;
    }
}