namespace AiUo.Extensions.MQTT;

/// <summary>
/// 标记MQTT发布消息类型
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class MQTTPublishMessageAttribute : Attribute
{
    /// <summary>
    /// 消息主题
    /// </summary>
    public string Topic { get; set; }

    /// <summary>
    /// 服务质量等级(0,1,2)
    /// </summary>
    public int QosLevel { get; set; }

    /// <summary>
    /// 是否保留消息
    /// </summary>
    public bool Retain { get; set; }

    /// <summary>
    /// 创建一个新的MQTT发布消息特性
    /// </summary>
    /// <param name="topic">消息主题</param>
    public MQTTPublishMessageAttribute(string topic)
    {
        Topic = topic;
        QosLevel = 0; // 默认QoS级别为0
        Retain = false; // 默认不保留消息
    }

    /// <summary>
    /// 创建一个新的MQTT发布消息特性
    /// </summary>
    /// <param name="topic">消息主题</param>
    /// <param name="qosLevel">服务质量等级(0,1,2)</param>
    public MQTTPublishMessageAttribute(string topic, int qosLevel) : this(topic)
    {
        QosLevel = qosLevel;
    }

    /// <summary>
    /// 创建一个新的MQTT发布消息特性
    /// </summary>
    /// <param name="topic">消息主题</param>
    /// <param name="qosLevel">服务质量等级(0,1,2)</param>
    /// <param name="retain">是否保留消息</param>
    public MQTTPublishMessageAttribute(string topic, int qosLevel, bool retain) : this(topic, qosLevel)
    {
        Retain = retain;
    }
}