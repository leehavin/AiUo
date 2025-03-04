namespace AiUo.Extensions.RabbitMQ;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
public class MQConsumerIgnoreAttribute: Attribute
{
    public MQConsumerIgnoreAttribute() { }
}