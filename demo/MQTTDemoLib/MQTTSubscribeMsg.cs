using AiUo.Extensions.MQTT;

namespace MQTTDemoLib;

/// <summary>
/// MQTT订阅消息示例
/// </summary>
public class MQTTSubscribeMsg : MQTTMessage
{
    /// <summary>
    /// 消息内容
    /// </summary>
    public string Content { get; set; }
    
    /// <summary>
    /// 数值
    /// </summary>
    public int Value { get; set; }
}