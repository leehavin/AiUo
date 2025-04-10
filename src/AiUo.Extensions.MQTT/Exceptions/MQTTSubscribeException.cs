namespace AiUo.Extensions.MQTT.Exceptions;

/// <summary>
/// MQTT订阅异常
/// </summary>
public class MQTTSubscribeException : MQTTException
{
    /// <summary>
    /// 主题
    /// </summary>
    public string Topic { get; }

    /// <summary>
    /// 服务质量等级
    /// </summary>
    public int QosLevel { get; }

    /// <summary>
    /// 创建一个新的MQTT订阅异常
    /// </summary>
    /// <param name="topic">主题</param>
    /// <param name="qosLevel">服务质量等级</param>
    public MQTTSubscribeException(string topic, int qosLevel)
        : base($"MQTT订阅失败: 主题={topic}, QoS={qosLevel}")
    {
        Topic = topic;
        QosLevel = qosLevel;
    }

    /// <summary>
    /// 使用指定错误消息创建一个新的MQTT订阅异常
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="topic">主题</param>
    /// <param name="qosLevel">服务质量等级</param>
    /// <param name="innerException">内部异常</param>
    public MQTTSubscribeException(string message, string topic, int qosLevel, Exception innerException = null)
        : base(message, innerException)
    {
        Topic = topic;
        QosLevel = qosLevel;
    }
}