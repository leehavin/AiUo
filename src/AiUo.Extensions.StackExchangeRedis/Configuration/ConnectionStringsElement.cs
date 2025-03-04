namespace AiUo.Extensions.StackExchangeRedis;

public class ConnectionStringElement
{
    public string Name { get; internal set; }
    public string ConnectionString { get; set; }
    public RedisSerializeMode SerializeMode { get; set; } = RedisSerializeMode.Json;
    public string NamespaceMap { get; set; }
}