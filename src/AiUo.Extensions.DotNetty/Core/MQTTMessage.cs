namespace AiUo.Extensions.DotNetty;

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
}

/// <summary>
/// MQTT消息接口
/// </summary>
public interface IMQTTMessage
{
    /// <summary>
    /// MQTT消息元数据
    /// </summary>
    MQTTMessageMeta MQTTMeta { get; set; }
}