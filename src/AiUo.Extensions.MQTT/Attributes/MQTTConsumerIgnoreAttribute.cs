namespace AiUo.Extensions.MQTT;

/// <summary>
/// 标记不需要自动注册的MQTT消费者类
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class MQTTConsumerIgnoreAttribute : Attribute
{
}