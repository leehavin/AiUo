namespace AiUo.Extensions.StackExchangeRedis;

/// <summary>
/// RedisClient配置信息
/// </summary>
public class RedisClientOptions
{
    /// <summary>
    /// Redis连接字符串名称，配置文件：Redis:ConnectionStrings:Name
    /// </summary>
    public string ConnectionStringName { get; set; }
    /// <summary>
    /// Redis连接字符串。ConnectionStringName 和ConnectionString二选一，ConnectionString优先
    /// </summary>
    public string ConnectionString { get; set; }
    /// <summary>
    /// 使用的Redis Database Index
    /// </summary>
    public int DatabaseIndex { get; set; } = -1;
    /// <summary>
    /// 序列化方式
    /// </summary>
    public RedisSerializeMode SerializeMode { get; set; } = RedisSerializeMode.Json;

    /// <summary>
    /// 如果设置，每次访问，RedisKey的过期将自动延续
    /// </summary>
    public TimeSpan? SlidingExpiration { get; set; }

}