namespace AiUo.Extensions.RabbitMQ;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
public class MQRequestMessageAttribute : Attribute
{
    public string ConnectionStringName { get; set; }
    public int ExpireSeconds { get; set; }
    public byte Priority { get; set; }
    public string QueueName { get; set; }
    public MQRequestMessageAttribute(string connectionStringName = null, int expireSeconds = 0, string queueName = null, byte priority = 0)
    {
        ExpireSeconds = expireSeconds;
        QueueName = queueName;
        Priority = priority;
        ConnectionStringName = connectionStringName;
    }
}