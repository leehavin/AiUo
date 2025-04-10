namespace AiUo.Extensions.MQTT;

/// <summary>
/// MQTT消息接口
/// </summary>
 public interface IMQTTMessage
{
    /// <summary>
    /// 消息元数据
    /// </summary>
    MQTTMessageMeta MQTTMeta { get; set; }
}

/// <summary>
/// MQTT消息元数据
/// </summary>
public class MQTTMessageMeta
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public string MessageId { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    /// 主题
    /// </summary>
    public string Topic { get; set; }

    /// <summary>
    /// 服务质量等级
    /// </summary>
    public int QosLevel { get; set; }

    /// <summary>
    /// 是否保留消息
    /// </summary>
    public bool Retain { get; set; }
}