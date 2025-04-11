using AiUo.Extensions.MQTT;

namespace MQTTDemoLib;

/// <summary>
/// MQTT发布消息示例
/// </summary>
[MQTTPublishMessage("mqtt/demo/publish", 1, false)]
public class MQTTPublishMsg : MQTTMessage
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