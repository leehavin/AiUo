namespace AiUo.Extensions.DotNetty;

/// <summary>
/// MQTT服务质量级别
/// </summary>
public enum MQTTQualityOfServiceLevel
{
    /// <summary>
    /// 最多一次传递 (QoS 0)
    /// </summary>
    AtMostOnce = 0,

    /// <summary>
    /// 至少一次传递 (QoS 1)
    /// </summary>
    AtLeastOnce = 1,

    /// <summary>
    /// 精确一次传递 (QoS 2)
    /// </summary>
    ExactlyOnce = 2
}