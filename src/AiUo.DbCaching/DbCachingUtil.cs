using AiUo.Configuration;
using AiUo.Data.SqlSugar;
using AiUo.DbCaching.Caching;
using AiUo.Extensions.RabbitMQ;
using AiUo.Extensions.StackExchangeRedis;
using AiUo.Hosting.Services;
using AiUo.Logging;
using AiUo.Text;
using SqlSugar;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace AiUo.DbCaching;

/// <summary>
/// 支持通知更新的内存缓存辅助类
/// </summary>
public static class DbCachingUtil
{
    // key: typename|splitDbKey value: configId|tablename
    internal static ConcurrentDictionary<string, string> CachKeyDict = new();
    // key: configId|tableName ===> [eoTypeName, memory]
    internal static ConcurrentDictionary<string, ConcurrentDictionary<string, object>> CacheDict = new();

    #region GetSingle
    /// <summary>
    /// 获取单个缓存项
    /// Demo: DbCachingUtil.GetSingle(123);
    /// </summary>
    /// <typeparam name="TEntity">有SugarTableAttribute的数据库实体类</typeparam>
    /// <param name="id">主键值</param>
    /// <param name="splitDbKey">分库路由数据</param>
    /// <returns></returns>
    public static TEntity GetSingle<TEntity>(object id, object splitDbKey = null)
        where TEntity : class, new()
        => GetCache<TEntity>(splitDbKey).GetSingle(id);

    /// <summary>
    /// 获取单个缓存项
    /// Demo: DbCachingUtil.GetSingle(it=>it.Id, 123);
    /// </summary>
    /// <typeparam name="TEntity">有SugarTableAttribute的数据库实体类</typeparam>
    /// <param name="fieldsExpr">主键或者唯一索引值的列定义</param>
    /// <param name="valuesEntity">主键或者唯一索引值的值定义</param>
    /// <param name="splitDbKey">分库路由数据</param>
    /// <returns></returns>
    public static TEntity GetSingle<TEntity>(Expression<Func<TEntity, object>> fieldsExpr, object valuesEntity, object splitDbKey = null)
        where TEntity : class, new()
        => GetCache<TEntity>(splitDbKey).GetSingle(fieldsExpr, valuesEntity);
    /// <summary>
    /// 获取单个缓存项
    /// Demo: DbCachingUtil.GetSingle(it=> new User{Id=123,Sex=true});
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="expr"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public static TEntity GetSingle<TEntity>(Expression<Func<TEntity>> expr, object splitDbKey = null)
        where TEntity : class, new()
        => GetCache<TEntity>(splitDbKey).GetSingle(expr);
    public static TEntity GetSingleByKey<TEntity>(string fieldsKey, string valuesKey, object splitDbKey = null)
        where TEntity : class, new()
        => GetCache<TEntity>(splitDbKey).GetSingleByKey(fieldsKey, valuesKey);
    #endregion

    #region GetList
    /// <summary>
    /// 获取单个缓存项
    /// </summary>
    /// <typeparam name="TEntity">有SugarTableAttribute的数据库实体类</typeparam>
    /// <param name="fieldsExpr">索引值的列定义</param>
    /// <param name="valuesEntity">索引值的值定义</param>
    /// <param name="splitDbKey">分库路由数据</param>
    /// <returns></returns>
    public static List<TEntity> GetList<TEntity>(Expression<Func<TEntity, object>> fieldsExpr, object valuesEntity, object splitDbKey = null)
        where TEntity : class, new()
        => GetCache<TEntity>(splitDbKey).GetList(fieldsExpr, valuesEntity);
    public static List<TEntity> GetList<TEntity>(Expression<Func<TEntity>> expr, object splitDbKey = null)
        where TEntity : class, new()
        => GetCache<TEntity>(splitDbKey).GetList(expr);
    public static List<TEntity> GetListByKey<TEntity>(string fieldsKey, string valuesKey, object splitDbKey = null)
        where TEntity : class, new()
        => GetCache<TEntity>(splitDbKey).GetListByKey(fieldsKey, valuesKey);

    /// <summary>
    /// 获取所有缓存项
    /// </summary>
    /// <typeparam name="TEntity">有SugarTableAttribute的数据库实体类</typeparam>
    /// <param name="splitDbKey">分库路由数据</param>
    /// <returns></returns>
    public static List<TEntity> GetAllList<TEntity>(object splitDbKey = null)
        where TEntity : class, new()
        => GetCache<TEntity>(splitDbKey).GetAllList();
    #endregion

    #region GetOrAddCustom
    /// <summary>
    /// 自定义字典缓存，name唯一
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="name"></param>
    /// <param name="func"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public static Dictionary<string, TEntity> GetOrAddCustom<TEntity>(string name, Func<List<TEntity>, Dictionary<string, TEntity>> func, object splitDbKey = null)
        where TEntity : class, new()
        => GetCache<TEntity>(splitDbKey).GetOrAddCustom(name, func);
    /// <summary>
    /// 自定义列表缓存，name唯一
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="name"></param>
    /// <param name="func"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public static Dictionary<string, List<TEntity>> GetOrAddCustom<TEntity>(string name, Func<List<TEntity>, Dictionary<string, List<TEntity>>> func, object splitDbKey = null)
        where TEntity : class, new()
        => GetCache<TEntity>(splitDbKey).GetOrAddCustom(name, func);
    /// <summary>
    /// 自定义对象缓存，name唯一
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TCache"></typeparam>
    /// <param name="name"></param>
    /// <param name="func"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public static TCache GetOrAddCustom<TEntity, TCache>(string name, Func<List<TEntity>, TCache> func, object splitDbKey = null)
        where TEntity : class, new()
        => GetCache<TEntity>(splitDbKey).GetOrAddCustom(name, func);

    /// <summary>
    /// 获取缓存对象DbCacheMemory
    /// </summary>
    /// <typeparam name="TEntity">有SugarTableAttribute的数据库实体类</typeparam>
    /// <param name="splitDbKey">分库路由数据</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static DbCacheMemory<TEntity> GetCache<TEntity>(object splitDbKey = null)
        where TEntity : class, new()
    {
        var key = splitDbKey == null
            ? typeof(TEntity).FullName
            : $"{typeof(TEntity).FullName}|{splitDbKey}";

        // configId|tableName
        var cacheKey = CachKeyDict.GetOrAdd(key, k =>
        {
            var attr = typeof(TEntity).GetCustomAttribute<SugarTable>();
            if (attr == null)
                throw new Exception($"内存缓存类型仅支持有SugarTableAttribute的类。type: {typeof(TEntity).FullName}");
            var routingProvider = DIUtil.GetRequiredService<IDbSplitProvider>();
            var configId = routingProvider.SplitDb<TEntity>(splitDbKey);
            return GetCacheKey(configId, attr.TableName);
        });
        // configId|tableName => dict
        var dict = CacheDict.GetOrAdd(cacheKey, (k) => new ConcurrentDictionary<string, object>());
        // eoTypeName => memory 可能存在多个Entity类对应一个表
        var cacheName = typeof(TEntity).FullName;
        var cacheKeys = ParseCacheKey(cacheKey);
        var ret = dict.GetOrAdd(cacheName, (k) => new DbCacheMemory<TEntity>(cacheKeys.configId, cacheKeys.tableName));
        return (DbCacheMemory<TEntity>)ret;
    }
    internal static object PreloadCache(Type entityType, object splitDbKey = null)
    {
        var key = splitDbKey == null
            ? entityType.FullName
            : $"{entityType.FullName}|{splitDbKey}";

        // configId|tableName
        var cacheKey = CachKeyDict.GetOrAdd(key, k =>
        {
            var attr = entityType.GetCustomAttribute<SugarTable>();
            if (attr == null)
                throw new Exception($"内存缓存类型仅支持有SugarTableAttribute的类。type: {entityType.FullName}");
            var routingProvider = DIUtil.GetRequiredService<IDbSplitProvider>();
            var method = routingProvider.GetType().GetMethod("SplitDb").MakeGenericMethod(entityType);
            var paras = splitDbKey == null ? null : new object[] { splitDbKey };
            var configId = method.Invoke(routingProvider, paras) as string;
            return GetCacheKey(configId, attr.TableName);
        });
        // configId|tableName => dict
        var dict = CacheDict.GetOrAdd(cacheKey, (k) => new ConcurrentDictionary<string, object>());
        // eoTypeName => memory
        var cacheName = entityType.FullName;
        var cacheKeys = ParseCacheKey(cacheKey);
        var ret = dict.GetOrAdd(cacheName, (k) =>
        {
            var baseType = typeof(DbCacheMemory<>);
            var memoryType = baseType.MakeGenericType(entityType);
            return Activator.CreateInstance(memoryType, cacheKeys.configId, cacheKeys.tableName);
        });
        return ret;
    }
    #endregion

    #region 缓存更新--后台管理
    /// <summary>
    /// 数据表是否存在内存缓存对象
    /// </summary>
    /// <param name="configId"></param>
    /// <param name="tableName"></param>
    /// <param name="redisConnectionStringName"></param>
    /// <returns></returns>
    public static async Task<bool> ContainsCacheItem(string configId, string tableName, string redisConnectionStringName = null)
    {
        var listDCache = new DbCacheListDCache(redisConnectionStringName);
        var dataDCache = new DbCacheDataDCache(configId, tableName, redisConnectionStringName);
        var key = GetCacheKey(configId, tableName);
        return await listDCache.FieldExistsAsync(key) && await dataDCache.KeyExistsAsync();
    }
    /// <summary>
    /// 获取所有缓存项
    /// </summary>
    /// <param name="redisConnectionStringName"></param>
    /// <returns></returns>
    public static async Task<List<DbCacheItem>> GetAllCacheItem(string redisConnectionStringName = null)
    {
        var ret = new List<DbCacheItem>();
        var listDCache = new DbCacheListDCache(redisConnectionStringName);
        var fields = await listDCache.GetFieldsAsync();
        foreach (var field in fields)
        {
            var keys = ParseCacheKey(field);
            var dataDCache = new DbCacheDataDCache(keys.configId, keys.tableName, redisConnectionStringName);
            if (!await dataDCache.KeyExistsAsync())
                continue;
            ret.Add(new DbCacheItem
            {
                ConfigId = keys.configId,
                TableName = keys.tableName,
            });
        }
        return ret;
    }

    /// <summary>
    /// 发布更新通知
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static async Task PublishUpdate(DbCacheChangeMessage message)
    {
        if (message.Changed == null || message.Changed.Count == 0)
            throw new Exception("DbCachingUtil.PublishUpdate时message.Changed必须有值");
        foreach (var item in message.Changed)
        {
            if (!await ContainsCacheItem(item.ConfigId, item.TableName, message.RedisConnectionStringName))
                continue;
            var dataProvider = new PageDataProvider(item.ConfigId, item.TableName, message.RedisConnectionStringName);
            await dataProvider.SetRedisValues();
        }
        switch (message.PublishMode)
        {
            case DbCachingPublishMode.Redis:
                await RedisUtil.PublishAsync(message, message.RedisConnectionStringName);
                break;
            case DbCachingPublishMode.MQ:
                await MQUtil.PublishAsync(message, null, message.MQConnectionStringName);
                break;
        }
    }
    /// <summary>
    /// 发布更新全部通知
    /// </summary>
    /// <param name="redisConnectionStringName"></param>
    /// <returns></returns>
    public static async Task<bool> PublishUpdateAll(string redisConnectionStringName = null)
    {
        var msg = new DbCacheChangeMessage
        {
            RedisConnectionStringName = redisConnectionStringName,
            PublishMode = DbCachingPublishMode.Redis,
            Changed = await GetAllCacheItem(redisConnectionStringName)
        };

        if (msg.Changed?.Count > 0)
        {
            await PublishUpdate(msg);
            return true;
        }
        else
            return false;
    }

    internal const string DB_CACHING_CHECK_KEY = "DB_CACHING_CHECK_KEY";
    internal const string DB_CACHING_CHECK_DATA = "DB_CACHING_CHECK_DATA";

    /// <summary>
    /// 发送验证消息
    /// </summary>
    /// <param name="items">验证的缓存项，null为全部</param>
    /// <param name="redisConnectionStringName"></param>
    /// <param name="timeoutSeconds">单个host验证的timeout秒</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static async Task<DbCacheCheckResult> PublishCheck(List<DbCacheItem> items = null, string redisConnectionStringName = null, int timeoutSeconds = 10)
    {
        var ret = new DbCacheCheckResult();
        if (items == null || items.Count == 0)
            items = await GetAllCacheItem(redisConnectionStringName);

        // 1)验证数据库与redis是否一致
        var checkItems = new Dictionary<string, DbCacheCheckItem>();
        foreach (var item in items)
        {
            var dataProvider = new PageDataProvider(item.ConfigId, item.TableName, redisConnectionStringName);
            var dbData = await dataProvider.GetDbTableData();
            var redisData = await dataProvider.GetRedisValues();
            if (redisData.DataHash != dbData.DataHash)
            {
                ret.RedisAndDbDiffs.Add(item);
            }
            else
            {
                var key = GetCacheKey(item.ConfigId, item.TableName);
                checkItems.Add(key, new DbCacheCheckItem
                {
                    ConfigId = item.ConfigId,
                    TableName = item.TableName,
                    DbHash = dbData.DataHash,
                });
            }
        }
        if (ret.RedisAndDbDiffs.Count > 0)
        {
            ret.Error = "存在redis缓存与数据库数据不一致的项";
            return ret;
        }

        // 发送验证消息
        var msg = new DbCacheCheckMessage
        {
            TraceId = ObjectId.NewId(),
            RedisConnectionStringName = redisConnectionStringName,
            CheckDate = DateTime.UtcNow.UtcToCNString(),
            CheckItems = checkItems,
        };
        await RedisUtil.PublishAsync(msg, redisConnectionStringName);

        // 获取服务信息
        var regDataService = DIUtil.GetService<IAiUoHostDataService>();
        if (regDataService == null)
            throw new Exception("获取所有host的DbCaching缓存检查数据异常，IAiUoHostRegisterService不存在");

        var serviceIds = await regDataService.GetAllServiceIds(redisConnectionStringName);
        var idQueue = new Queue<(string serviceId, long waitTime)>();
        serviceIds.ForEach(x => idQueue.Enqueue((x, 0)));
        var maxTime = timeoutSeconds * 1000;
        while (idQueue.Count > 0)
        {
            var item = idQueue.Dequeue();
            var serviceId = item.serviceId;
            var waitTime = item.waitTime;
            waitTime += 1000;
            await Task.Delay(1000);

            if (waitTime > maxTime)
            {
                ret.RedisAndServiceDiffs.Add(new DbCacheCheckServiceCache
                {
                    ServiceId = serviceId,
                    Success = false,
                    Error = "服务未收到验证消息"
                });
                continue;
            }
            var traceId = await regDataService.GetHostData<string>(DB_CACHING_CHECK_KEY
                , serviceId, redisConnectionStringName);
            if (!traceId.HasValue || traceId.Value != msg.TraceId)
            {
                idQueue.Enqueue((serviceId, waitTime));
                continue;
            }
            var serviceItems = await regDataService.GetHostData<List<DbCacheCheckServiceItem>>(DB_CACHING_CHECK_DATA
                , serviceId, redisConnectionStringName);

            var result = new DbCacheCheckServiceCache();
            result.ServiceId = serviceId;
            if (serviceItems != null && serviceItems.HasValue)
                result.Items = serviceItems.Value;
            result.Success = result.Items == null || result.Items.Count == 0;
            if (!result.Success)
                result.Error = "服务存在缓存不一致";
            ret.RedisAndServiceDiffs.Add(result);
        }
        ret.Success = ret.RedisAndServiceDiffs.TrueForAll(x => x.Success);
        if (!ret.Success)
            ret.Error = "存在服务缓存和服务内存缓存不一致的项";
        return ret;
    }
    public static async Task LogPublishCheck(string redisConnectionStringName = null, int timeoutSeconds = 300)
    {
        var begin = DateTime.UtcNow;
        DbCacheCheckResult ret = null;
        while (true)
        {
            ret = await PublishCheck(null, redisConnectionStringName);
            if (ret.Success || (DateTime.UtcNow - begin).TotalSeconds > timeoutSeconds)
                break;
        }
        if (ret == null || ret.Success)
            return;

        var logger = LogUtil.GetContextLogger()
            .SetCategoryName("DbCaching.Check")
            .SetLevel(Microsoft.Extensions.Logging.LogLevel.Error)
            .AddMessage(ret.Error);
        foreach (var item in ret.RedisAndDbDiffs)
        {
            logger.AddMessage($"redis和数据库不一致：configId:{item.ConfigId} tableName:{item.TableName}");
        }
        foreach (var item in ret.RedisAndServiceDiffs)
        {
            if (item.Success)
                continue;
            foreach (var item2 in item.Items)
            {
                logger.AddMessage($"内存和数据库不一致：serviceId:{item.ServiceId} configId:{item2.ConfigId} tableName:{item2.TableName} cacheUpdate:{item2.CacheUpdate}");
            }
        }
        logger.Save();
    }
    #endregion

    #region Utils
    internal static string GetCacheKey(string configId, string tableName)
        => $"{configId ?? DbUtil.DefaultConfigId}|{tableName}";
    internal static (string configId, string tableName) ParseCacheKey(string key)
    {
        var keys = key.Split('|');
        return (keys[0], keys[1]);
    }

    private static ConcurrentDictionary<string, string> _redisConnDict = new();
    private const int REDIS_ASYNC_TIMEOUT = 20000;
    internal static string GetRedisConnectionString(string connectionStringName = null)
    {
        connectionStringName ??= string.Empty;
        if (!_redisConnDict.TryGetValue(connectionStringName, out var ret))
        {
            var redisSection = ConfigUtil.GetSection<RedisSection>();
            ret = string.IsNullOrEmpty(connectionStringName)
                ? redisSection.GetConnectionStringElement(ConfigUtil.GetSection<DbCachingSection>().RedisConnectionStringName).ConnectionString
                : redisSection.GetConnectionStringElement(connectionStringName).ConnectionString;
            var conn = ConfigurationOptions.Parse(ret);
            conn.ClientName = "DbCacheDataDCache";
            conn.AsyncTimeout = REDIS_ASYNC_TIMEOUT;
            conn.SyncTimeout = REDIS_ASYNC_TIMEOUT;
            ret = conn.ToString();

            _redisConnDict.TryAdd(connectionStringName, ret);
        }
        return ret;
    }
    #endregion
}