namespace AiUo.Extensions.RabbitMQ;

public interface IMQMessage
{
    MQMessageMeta MQMeta { get; set; }
}
public class MQMessageMeta
{
    /// <summary>
    /// 消息唯一ID（自动设置）
    /// </summary>
    public string MessageId { get; set; }
    /// <summary>
    /// 消息发送时间（自动设置）
    /// </summary>
    public long Timestamp { get; set; }

    #region Republish
    public string Topic { get; set; } // topic
    public bool Mandatory { get; set; }
    public EasyNetQ.MessageProperties Properties { get; set; }
    public string MessageBody { get; set; }
    #endregion

    /// <summary>
    /// 异常的Action（自动设置）
    /// </summary>
    public List<string> ErrorHandlers { get; set; } = new();
    public int RepublishCount { get; set; }
}

public class MQMessageData
{
}
internal class MQMessage : IMQMessage
{
    public MQMessageMeta MQMeta { get; set; }
}