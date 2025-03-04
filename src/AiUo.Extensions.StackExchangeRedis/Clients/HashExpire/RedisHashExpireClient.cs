using StackExchange.Redis;
using AiUo.Caching;

namespace AiUo.Extensions.StackExchangeRedis;

/// <summary>
/// Redis Hash表（key-value结构）
///     可以在field上设置过期值(注意：缓存项过期时不会自动删除)
///     其他参考RedisHashClient
/// </summary>
public class RedisHashExpireClient<TField> : RedisHashExpireBase<TField>
{
    public RedisHashExpireClient() { }

    #region Common
    /// <summary>
    /// 【创建或更新】设置hash结构中的field对应的缓存值
    /// </summary>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <param name="expireAt">过期时间</param>
    /// <param name="always">true:无论是否存在总是添加，false：不存在时才添加</param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<bool> SetAsync(string field, TField value, DateTime? expireAt = null, bool always = true, CommandFlags flags = CommandFlags.None)
        => await SetBaseAsync<TField>(field, value, expireAt, always, flags);

    public async Task<bool> SetAsync(string field, TField value, TimeSpan expire, bool always = true, CommandFlags flags = CommandFlags.None)
        => await SetBaseAsync<TField>(field, value, expire, always, flags);
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
        => await GetOrDefaultBaseAsync<TField>(field, defaultValue, flags);
    #endregion

    #region Set
    public Task<bool> SetAndExpireSecondsAsync(string field, TField value, int seconds, CommandFlags flags = CommandFlags.None)
        => SetAsync(field, value, new TimeSpan(0, 0, seconds), true, flags);
    public Task<bool> SetAndExpireMinutesAsync(string field, TField value, int minutes, CommandFlags flags = CommandFlags.None)
        => SetAsync(field, value, new TimeSpan(0, minutes, 0), true, flags);
    public Task<bool> SetAndExpireHoursAsync(string field, TField value, int hours, CommandFlags flags = CommandFlags.None)
        => SetAsync(field, value, new TimeSpan(hours, 0, 0), true, flags);
    public Task<bool> SetAndExpireDaysAsync(string field, TField value, int days, CommandFlags flags = CommandFlags.None)
        => SetAsync(field, value, new TimeSpan(days, 0, 0, 0), true, flags);
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
        if (enforce || !TryDeserializeExpire(field, await Database.HashGetAsync(RedisKey, field, flags), out CacheItem<TField> value))
        {
            var loadValue = await LoadValueWhenRedisNotExistsAsync(field);
            if (loadValue.HasValue && !loadValue.Value.IsExpired)
            {
                await Database.HashSetAsync(RedisKey, field, SerializeExpire(loadValue.Value), When.Always, flags);
                ret = new CacheValue<TField>(true, loadValue.Value.Value);
            }
            else
            {
                ret = new CacheValue<TField>(false, default);
            }
        }
        else
        {
            ret = new CacheValue<TField>(value.Value);
        }
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion
}