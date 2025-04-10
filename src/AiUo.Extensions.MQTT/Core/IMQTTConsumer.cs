namespace AiUo.Extensions.MQTT;

/// <summary>
/// MQTT消费者接口
/// </summary>
public interface IMQTTConsumer : IDisposable
{
    /// <summary>
    /// 注册消费者
    /// </summary>
    /// <returns></returns>
    Task Register();
}