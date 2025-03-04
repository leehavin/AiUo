namespace AiUo.Extensions.RabbitMQ;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
public class MQPublishMessageAttribute : Attribute
{
    public string ConnectionStringName { get; set; }
    public string Topic { get; set; }
    public int ExpireSeconds { get; set; }
    public byte Priority { get; set; }
    public MQPublishMessageAttribute(string connectionStringName = null, string topic = null, int expireSeconds = 0, byte priority = 0)
    {
        Topic = topic;
        ExpireSeconds = expireSeconds;
        Priority = priority;
        ConnectionStringName = connectionStringName;
    }
}