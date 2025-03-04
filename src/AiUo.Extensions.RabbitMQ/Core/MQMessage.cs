namespace AiUo.Extensions.RabbitMQ;

public class MQMessage<T>
    where T : new()
{
    public T Message { get; set; }

    /// <summary>
    /// 消息唯一ID（自动设置）
    /// </summary>
    public string MessageId { get; set; }
    /// <summary>
    /// 消息发送时间（自动设置）
    /// </summary>
    public long Timestamp { get; set; }
    /// <summary>
    /// 当前异常的Action（自动设置）
    /// </summary>
    public List<string> ErrorActions { get; set; }
    /// <summary>
    /// 重试次数
    /// </summary>
    public int RepublishCount { get; set; }
}