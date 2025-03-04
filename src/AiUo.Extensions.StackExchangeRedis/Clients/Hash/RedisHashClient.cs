using StackExchange.Redis;
using AiUo.Caching;

namespace AiUo.Extensions.StackExchangeRedis;

/// <summary>
/// Redis Hash表（key-value结构）value值的类型统一是T类型
///     可以被继承，也可以直接构建
///     RedisKey => Field => RedisValue
///     可存入null值
/// </summary>
/// <typeparam name="TField"></typeparam>
public class RedisHashClient<TField> : RedisHashBase<TField>
{
    public RedisHashClient() { }

    #region Common
    /// <summary>
    /// 【创建或更新】设置hash结构中的field对应的缓存值
    /// </summary>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <param name="always">true:无论是否存在总是添加，false：不存在时才添加</param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<bool> SetAsync(string field, TField value, bool always = true, CommandFlags flags = CommandFlags.None)
        => await SetBaseAsync(field, value, always, flags);
        
    public async Task<CacheValue<TField>> GetAsync(string field, CommandFlags flags = CommandFlags.None)
        => await GetBaseAsync<TField>(field, flags);
    /// <summary>
    /// 从Hash结构根据field获取缓存项，如果不存在则抛出异常CacheNotFound
    /// </summary>
    /// <param name="field"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<TField> GetOrExceptionAsync(string field, CommandFlags flags = CommandFlags.None)
        => await GetOrExceptionBaseAsync<TField>(field, flags);
    /// <summary>
    /// 从Hash结构根据field获取缓存项，如果不存在，则返回默认值。
    /// </summary>
    /// <param name="field"></param>
    /// <param name="defaultValue"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<TField> GetOrDefaultAsync(string field, TField defaultValue, CommandFlags flags = CommandFlags.None)
        => await GetOrDefaultBaseAsync(field, defaultValue, flags);
    #endregion


    #region GetOrLoad
    /// <summary>
    /// 从Hash结构根据field获取缓存项，如果不存在则调用LoadValueWhenRedisNotExists()放入redis并返回
    /// </summary>
    /// <param name="field"></param>
    /// <param name="enforce">是否强制Load</param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<CacheValue<TField>> GetOrLoadAsync(string field, bool enforce = false, CommandFlags flags = CommandFlags.None)
    {
        CacheValue<TField> ret;
        if (enforce || !TryDeserialize(await Database.HashGetAsync(RedisKey, field, flags), out TField value))
        {
            ret = await LoadValueWhenRedisNotExistsAsync(field);
            if (ret.HasValue)
            {
                await Database.HashSetAsync(RedisKey, field, Serialize(ret.Value), When.Always, flags);
            }
            else
            {
                if (IsLoadValueNotExistsToRedis)
                {
                    await Database.HashSetAsync(RedisKey, field, Serialize(null), When.Always, flags);
                }
            }
        }
        else
        {
            ret = new CacheValue<TField>(true, value);
        }
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion
}