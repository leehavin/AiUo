using StackExchange.Redis;
using AiUo.Caching;

namespace AiUo.Extensions.StackExchangeRedis;

public class RedisHashBase<TField> : RedisClientBase
{
    public override RedisType RedisType => RedisType.Hash;
    /// <summary>
    /// GetOrLoad时，如值不存在是否保存到redis
    /// </summary>
    public virtual bool IsLoadValueNotExistsToRedis { get; } = false;

    public RedisHashBase() { }

    #region LoadValueWhenRedisNotExists
    public delegate Task<CacheValue<TField>> LoadValueDelegate(string field);
    public LoadValueDelegate LoadValueHandler;
    /// <summary>
    /// 在调用GetOrLoad时，当Redis不存在此field时，将调用此方法获取field对应的缓存值，返回并存储到Redis中，需要时子类实现override。
    ///     1) 返回true表示保存缓存项，false表示不保存缓存项
    ///     2) value可返回null，存入Redis的值为RedisValue.EmptyString
    ///     3) 可抛出异常CacheNotFound：表示field对应的值不存在
    /// </summary>
    /// <param name="field">key</param>
    /// <returns></returns>
    protected virtual async Task<CacheValue<TField>> LoadValueWhenRedisNotExistsAsync(string field)
    { 
        if(LoadValueHandler != null)
            return await LoadValueHandler(field);
        throw new NotImplementedException();
    }

    public delegate Task<CacheValue<Dictionary<string, TField>>> LoadAllValuesDelegate();
    public LoadAllValuesDelegate LoadAllValuesHandler;
    /// <summary>
    /// 调用GetAllOrLoad()时，当RedisKey不存在，则调用此方法返回并存储全部hash值到redis中，需要时子类实现override
    /// </summary>
    /// <returns></returns>
    protected virtual async Task<CacheValue<Dictionary<string, TField>>> LoadAllValuesWhenRedisNotExistsAsync()
    {
        if (LoadAllValuesHandler != null)
            return await LoadAllValuesHandler();
        throw new NotImplementedException();
    }
    #endregion

    #region Set
    /// <summary>
    /// 【创建或更新】设置hash结构中的field。如果key不存在创建，如果field存在则覆盖，不存在则添加
    /// </summary>
    /// <param name="values"></param>
    /// <param name="flags"></param>
    public async Task SetAsync(Dictionary<string, TField> values, CommandFlags flags = CommandFlags.None)
    {
        var entries = values.Select(kv => new HashEntry(kv.Key, Serialize(kv.Value)));
        await Database.HashSetAsync(RedisKey, entries.ToArray(), flags);
        await SetSlidingExpirationAsync();
    }
    #endregion

    #region Get
    /// <summary>
    /// 获取一组field对应的缓存值
    /// </summary>
    /// <param name="fields"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, CacheValue<TField>>> GetAsync(IEnumerable<string> fields, CommandFlags flags = CommandFlags.None)
    {
        var arrFields = fields.Select(key => (RedisValue)key).ToArray();
        var values = await Database.HashGetAsync(RedisKey, arrFields, flags);
        var ret = new Dictionary<string, CacheValue<TField>>();
        for (int i = 0; i < arrFields.Length; i++)
        { 
            var field = arrFields[i];
            var value = values[i];
            if (TryDeserialize(value, out TField v))
                ret.Add(field, new CacheValue<TField>(true, v));
            else
                ret.Add(field, new CacheValue<TField>(false));
        }
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 从Hash结构Get所有缓存项
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, TField>> GetAllAsync(CommandFlags flags = CommandFlags.None)
    {
        var values = await Database.HashGetAllAsync(RedisKey, flags);
        var ret = values.ToDictionary(
            x => x.Name.ToString(),
            x => Deserialize<TField>(x.Value),
            StringComparer.Ordinal);
        await SetSlidingExpirationAsync();
        return ret;
    }
    /// <summary>
    /// 获取所有hash缓存值，如果RedisKey不存在，则调用LoadAllValuesWhenRedisNotExists()返回并保存到redis中
    /// </summary>
    /// <param name="expire"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<CacheValue<Dictionary<string, TField>>> GetAllOrLoadAsync(TimeSpan? expire = null, CommandFlags flags = CommandFlags.None)
    {
        CacheValue<Dictionary<string, TField>> ret = null;
        if (!await KeyExistsAsync(flags))
        {
            ret = await LoadAllValuesWhenRedisNotExistsAsync();
            if (ret.HasValue)
                await SetAsync(ret.Value, flags);
        }
        else
        {
            ret = new CacheValue<Dictionary<string, TField>>(await GetAllAsync(flags));
        }
        if(ret.HasValue)
            await SetSlidingExpirationAsync(expire);
        return ret;
    }
    #endregion

    #region Delete
    /// <summary>
    /// 从Hash结构移除kefield。时间复杂度：O(1)
    /// </summary>
    /// <param name="field"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<bool> DeleteAsync(string field, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.HashDeleteAsync(RedisKey, field, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 从Hash结构移除fields。时间复杂度：O(1)
    /// </summary>
    /// <param name="fields"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> DeleteAsync(IEnumerable<string> fields, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.HashDeleteAsync(RedisKey, fields.Select(x => (RedisValue)x).ToArray(), flags);
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion

    #region Exists/GetFields/GetValues/GetLength/Scan
    /// <summary>
    /// Hash是否存在指定field
    /// </summary>
    /// <param name="field"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<bool> FieldExistsAsync(string field, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.HashExistsAsync(RedisKey, field, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 获取hash中所有的fields
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<IEnumerable<string>> GetFieldsAsync(CommandFlags flags = CommandFlags.None)
    {
        var ret = (await Database.HashKeysAsync(RedisKey, flags)).Select(x => x.ToString());
        await SetSlidingExpirationAsync();
        return ret;
    }


    /// <summary>
    /// 返回hash中所有的values
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<IEnumerable<TField>> GetValuesAsync(CommandFlags flags = CommandFlags.None)
    {
        var ret = (await Database.HashValuesAsync(RedisKey, flags))
            .Select(x => Deserialize<TField>(x));
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 获取hash内缓存项数量
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> GetLengthAsync(CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.HashLengthAsync(RedisKey, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// HSCAN游标查询命令
    /// </summary>
    /// <param name="pattern">查询表达式，如：t*</param>
    /// <param name="pageSize">要迭代的页面大小</param>
    /// <param name="cursor">HSCAN 命令每次被调用之后， 都会向用户返回一个新的游标， 用户在下次迭代时需要使用这个新游标作为 HSCAN 命令的游标参数， 以此来延续之前的迭代过程。返回0表示结束</param>
    /// <param name="pageOffset"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, TField>> ScanAsync(RedisValue pattern, int pageSize, long cursor = 0, int pageOffset = 0, CommandFlags flags = CommandFlags.None)
    {
        var ret = new Dictionary<string, TField>();
        var items = Database.HashScanAsync(RedisKey, pattern, pageSize, cursor, pageOffset, flags);
        await foreach (var item in items)
        {
            ret.Add(item.Name.ToString(), Deserialize<TField>(item.Value));
        }
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion

    #region Increment
    /// <summary>
    /// Hash结构存储增量数字。如果field不存在则设置为0。支持long
    /// </summary>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> IncerementAsync(string field, long value = 1, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.HashIncrementAsync(RedisKey, field, value, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// Hash结构存储增量数字。如果field不存在则设置为0。支持long
    /// </summary>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<double> IncerementAsync(string field, double value, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.HashIncrementAsync(RedisKey, field, value, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }


    /// <summary>
    /// 减量数字-value,如不存在key则创建，返回减量后值
    /// </summary>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> DecrementAsync(string field, long value = 1, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.HashDecrementAsync(RedisKey, field, value, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 减量数字-value,如不存在key则创建，返回减量后值
    /// </summary>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<double> DecrementAsync(string field, double value, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.HashDecrementAsync(RedisKey, field, value, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion

    #region Common
    protected async Task<bool> SetBaseAsync<T>(string field, T value, bool always = true, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.HashSetAsync(RedisKey, field, Serialize(value), always ? When.Always : When.NotExists, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }
    protected async Task<CacheValue<T>> GetBaseAsync<T>(string field, CommandFlags flags = CommandFlags.None)
    {
        var value = await Database.HashGetAsync(RedisKey, field, flags);
        var ret = TryDeserialize(value, out T v)
            ? new CacheValue<T>(true, v)
            : new CacheValue<T>(false);
        await SetSlidingExpirationAsync();
        return ret;
    }
    protected async Task<T> GetOrExceptionBaseAsync<T>(string field, CommandFlags flags = CommandFlags.None)
    {
        var redisValue = await Database.HashGetAsync(RedisKey, field, flags);
        if (!TryDeserialize(redisValue, out T ret))
            throw new CacheNotFound($"[Redis Hash]field不存在。RedisKey: {RedisKey} field: {field} type:{GetType().FullName}");
        await SetSlidingExpirationAsync();
        return ret;
    }
    protected async Task<T> GetOrDefaultBaseAsync<T>(string field, T defaultValue, CommandFlags flags = CommandFlags.None)
    {
        var redisValue = await Database.HashGetAsync(RedisKey, field, flags);
        if (!TryDeserialize(redisValue, out T ret))
            ret = defaultValue;
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion
}