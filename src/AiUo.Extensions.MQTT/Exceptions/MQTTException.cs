namespace AiUo.Extensions.MQTT.Exceptions;

/// <summary>
/// MQTT异常基类
/// </summary>
public class MQTTException : Exception
{
    /// <summary>
    /// 创建一个新的MQTT异常
    /// </summary>
    public MQTTException() : base("MQTT操作发生异常")
    {
    }

    /// <summary>
    /// 使用指定错误消息创建一个新的MQTT异常
    /// </summary>
    /// <param name="message">错误消息</param>
    public MQTTException(string message) : base(message)
    {
    }

    /// <summary>
    /// 使用指定错误消息和内部异常创建一个新的MQTT异常
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="innerException">内部异常</param>
    public MQTTException(string message, Exception innerException) : base(message, innerException)
    {
    }
}