using static StackExchange.Redis.RedisChannel;

namespace AiUo.Extensions.StackExchangeRedis;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
public class RedisPublishMessageAttribute : Attribute
{
    public string ConnectionStringName { get; }
    public PatternMode PatternMode { get; }
    public RedisPublishMessageAttribute(string connectionStringName = null, PatternMode mode = PatternMode.Auto)
    {
        ConnectionStringName = connectionStringName;
        PatternMode = mode;
    }
}