namespace AiUo.DbCaching;

public class DbCacheChangeMessage
{
    public DbCachingPublishMode PublishMode { get; set; } = DbCachingPublishMode.Redis;
    public string RedisConnectionStringName { get; set; }
    public string MQConnectionStringName { get; set; }
    public List<DbCacheItem> Changed { get; set; } = new();
}
public class DbCacheItem
{
    public string ConfigId { get; set; }
    public string TableName { get; set; }
}