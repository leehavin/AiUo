using AiUo.Collections;
using AiUo.Logging;

namespace AiUo.DbCaching.ChangeConsumers;

internal class DbCacheUpdator
{
    private DbCachingPublishMode _mode;
    public DbCacheUpdator(DbCachingPublishMode mode)
    {
        _mode = mode;
    }
    public async Task Execute(DbCacheChangeMessage message)
    {
        var logger = new LogBuilder(Microsoft.Extensions.Logging.LogLevel.Warning, "内存缓存更新通知消费")
            .AddField("DbCaching.PublishMode", _mode.ToString())
            .AddField("DbCaching.Message", SerializerUtil.SerializeJson(message));
        var list = new List<(IDbCacheMemory cache, DbTableRedisData data)>();
        try
        {
            foreach (var item in message.Changed)
            {
                logger.AddMessage($"开始更新: {item.ConfigId}.{item.TableName}");
                var key = DbCachingUtil.GetCacheKey(item.ConfigId, item.TableName);
                if (!DbCachingUtil.CacheDict.ContainsKey(key))
                    continue;
                var redisValues = await new PageDataProvider(item.ConfigId, item.TableName)
                    .GetRedisValues();
                if (DbCachingUtil.CacheDict.TryGetValue(key, out var dict))
                {
                    dict.Values.ForEach(x =>
                    {
                        list.Add(((IDbCacheMemory)x, redisValues));
                    });
                }
            }
            list.ForEach(x => x.cache.BeginUpdate(x.data));
            list.ForEach(x =>
            {
                x.cache.EndUpdate();
            });
            list.ForEach(x =>
            {
                x.cache.Updated();
                logger.AddMessage($"更新成功: {x.cache.ConfigId}.{x.cache.TableName} count:{x.cache.RowCount}");
            });
        }
        catch (Exception ex)
        {
            logger.AddMessage("内存缓存更新通知消费异常")
                .AddException(ex);
        }
        logger.Save();
    }
}