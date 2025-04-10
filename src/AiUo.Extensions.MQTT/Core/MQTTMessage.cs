using AiUo.Text;

namespace AiUo.Extensions.MQTT;

/// <summary>
/// MQTT消息基类
/// </summary>
public class MQTTMessage : IMQTTMessage
{
    /// <summary>
    /// 消息元数据
    /// </summary>
    public MQTTMessageMeta MQTTMeta { get; set; }

    /// <summary>
    /// 创建一个新的MQTT消息
    /// </summary>
    public MQTTMessage()
    {
    }

    /// <summary>
    /// 使用指定主题创建一个新的MQTT消息
    /// </summary>
    /// <param name="topic">消息主题</param>
    public MQTTMessage(string topic)
    {
        MQTTMeta = new MQTTMessageMeta
        {
            Topic = topic,
            MessageId = ObjectId.NewId(),
            Timestamp = DateTime.UtcNow.ToTimestamp()
        };
    }

    /// <summary>
    /// 使用指定主题和QoS级别创建一个新的MQTT消息
    /// </summary>
    /// <param name="topic">消息主题</param>
    /// <param name="qosLevel">服务质量等级(0,1,2)</param>
    public MQTTMessage(string topic, int qosLevel) : this(topic)
    {
        MQTTMeta.QosLevel = qosLevel;
    }

    /// <summary>
    /// 使用指定主题、QoS级别和保留标志创建一个新的MQTT消息
    /// </summary>
    /// <param name="topic">消息主题</param>
    /// <param name="qosLevel">服务质量等级(0,1,2)</param>
    /// <param name="retain">是否保留消息</param>
    public MQTTMessage(string topic, int qosLevel, bool retain) : this(topic, qosLevel)
    {
        MQTTMeta.Retain = retain;
    }
}