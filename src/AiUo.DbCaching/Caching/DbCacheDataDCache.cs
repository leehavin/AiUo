using AiUo.Data.SqlSugar;
using AiUo.Extensions.StackExchangeRedis;

namespace AiUo.DbCaching.Caching;

internal class DbCacheDataDCache : RedisHashClient<string>
{
    public DbCacheDataDCache(string configId, string tableName, string connectionStringName = null)
    {
        Options.ConnectionString = DbCachingUtil.GetRedisConnectionString(connectionStringName);
        RedisKey = $"{RedisPrefixConst.DB_CACHING}:Data:{configId ?? DbUtil.DefaultConfigId}:{tableName}";
    }
}