using AiUo.Extensions.StackExchangeRedis;

namespace AiUo.DbCaching.Caching;

internal class DbCacheListDCache : RedisHashClient<DbCacheListDO>
{
    public DbCacheListDCache(string connectionStringName = null)
    {
        Options.ConnectionString = DbCachingUtil.GetRedisConnectionString(connectionStringName);
        RedisKey = $"{RedisPrefixConst.DB_CACHING}:List";
    }
}
internal class DbCacheListDO
{
    public string ConfigId { get; set; }
    public string TableName { get; set; }
    public int PageCount { get; set; }
    public int PageSize { get; set; }
    public string DataHash { get; set; }
    public string UpdateDate { get; set; }
}