namespace AiUo.Extensions.RabbitMQ;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
public class MQSendMessageAttribute : Attribute
{
    public string ConnectionStringName { get; set; }
    public string QueueName { get; set; }
    public byte Priority { get; set; }
    public MQSendMessageAttribute(string connectionStringName = null, string queueName = null, byte priority = 0)
    {
        QueueName = queueName;
        Priority = priority;
        ConnectionStringName = connectionStringName;
    }
}