using AiUo.Collections;
using AiUo.Configuration;
using AiUo.Extensions.StackExchangeRedis;
using AiUo.Hosting;
using AiUo.Logging;

namespace AiUo.DbCaching.ChangeConsumers;

[RedisConsumerRegisterIgnore]
internal class RedisDbCacheCheckConsumer : RedisSubscribeConsumer<DbCacheCheckMessage>
{
    protected override async Task OnMessage(DbCacheCheckMessage message)
    {
        if (!ConfigUtil.Host.RegisterEnabled)
            return;
        var list = new List<DbCacheCheckServiceItem>();
        //var listDCache = new DbCacheListDCache(message.RedisConnectionStringName);
        var values = DbCachingUtil.CacheDict.Values.ToArray();
        foreach (var dict in values)
        {
            dict.Values.ForEach((x) =>
            {
                var cacheData = ((IDbCacheMemory)x).RedisData;
                var key = DbCachingUtil.GetCacheKey(cacheData.ConfigId, cacheData.TableName);
                if (!message.CheckItems.ContainsKey(key))
                    return;
                var dbHash = message.CheckItems[key].DbHash;
                if (cacheData.DataHash != dbHash)
                {
                    list.Add(new DbCacheCheckServiceItem
                    {
                        ConfigId = cacheData.ConfigId,
                        TableName = cacheData.TableName,
                        CacheHash = cacheData.DataHash,
                        CacheUpdate = cacheData.UpdateDate,
                        DbHash = dbHash
                    });
                }
            });
        }
        await HostingUtil.SetHostData(DbCachingUtil.DB_CACHING_CHECK_DATA, list);
        await HostingUtil.SetHostData(DbCachingUtil.DB_CACHING_CHECK_KEY, message.TraceId);
    }
    protected override Task OnError(DbCacheCheckMessage message, Exception ex)
    {
        LogUtil.CreateLogBuilder(nameof(RedisDbCacheCheckConsumer))
            .AddException(ex)
            .AddMessage("DbCaching检查内存缓存时出现异常")
            .AddField("DbCaching-Message", message)
            .Save();
        return Task.CompletedTask;
    }
}