using AiUo.Extensions.RabbitMQ;

namespace MQDemoLib;

public class SubMsg: IMQMessage
{
    public string Message { get; set; }
    public int Id { get; set; }
    public string MessageId { get; set; }
    public long Timestamp { get; set; }
    public MQMessageMeta MQMeta { get; set; }
}

public class SubMsg1 : IMQMessage
{
    public string Message { get; set; }
    public string MessageId { get; set; }
    public long Timestamp { get; set; }
    public MQMessageMeta MQMeta { get; set; }
}

public class SubMsg2 : IMQMessage
{
    public string Message { get; set; }
    public string MessageId { get; set; }
    public long Timestamp { get; set; }
    public MQMessageMeta MQMeta { get; set; }
}