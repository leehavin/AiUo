namespace AiUo.Extensions.StackExchangeRedis;

public static class RedisPrefixConst
{
    /// <summary>
    /// 全局缓存前缀
    /// </summary>
    public const string GLOBAL = "Global";//GLOBAL

    /// <summary>
    /// Session前缀
    /// </summary>
    public const string SESSION = "_SESSION";

    /// <summary>
    /// 主机服务注册前缀
    /// </summary>
    public const string HOSTS = "_AiUo:Host";

    /// <summary>
    /// 分布式锁
    /// </summary>
    public const string REDIS_LOCK = "_AiUo:RedisLock";
    /// <summary>
    /// 布隆过滤器
    /// </summary>
    public const string BLOOM_FILTER = "_AiUo:BloomFilter";

    /// <summary>
    /// RabbitMQ 的 PubSub使用
    /// </summary>
    public const string MQ_SUB_QUEUE = "_AiUo:MQSubQueue";
    /// <summary>
    /// AiUo.SnowflakeId使用
    /// </summary>
    public const string SNOWFLAKE_ID = "_AiUo:SnowflakeId";
    /// <summary>
    /// AiUo.DbCaching.DbCacheDataDCache使用
    /// </summary>
    public const string DB_CACHING = "_AiUo:DbCaching";

    /// <summary>
    /// AiUo.AspNet.SyncNotifyAttribute使用
    /// </summary>
    public const string SYNC_NOTIFY = "_AiUo:SyncNotify";

    /// <summary>
    /// AiUo.Extensions.AWS.AwsGlobalDCache
    /// </summary>
    public const string AWS = "_AiUo:AWS";
}