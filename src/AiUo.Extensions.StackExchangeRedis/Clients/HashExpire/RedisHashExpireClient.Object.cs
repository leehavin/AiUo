using StackExchange.Redis;
using AiUo.Caching;

namespace AiUo.Extensions.StackExchangeRedis;

public class RedisHashExpireClient : RedisHashExpireBase<object>
{
    public RedisHashExpireClient() { }

    #region Common
    /// <summary>
    /// 【创建或更新】设置hash结构中的field
    /// </summary>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <param name="expireAt">过期时间</param>
    /// <param name="always">true:无论是否存在总是添加，false：不存在时才添加</param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<bool> SetAsync<T>(string field, T value, DateTime? expireAt = null, bool always = true, CommandFlags flags = CommandFlags.None)
        => await SetBaseAsync<T>(field, value, expireAt, always, flags);
    public async Task<bool> SetAsync<T>(string field, T value, TimeSpan expire, bool always = true, CommandFlags flags = CommandFlags.None)
        => await SetBaseAsync<T>(field, value, expire, always, flags);
    public async Task<CacheValue<T>> GetAsync<T>(string field, CommandFlags flags = CommandFlags.None)
        => await GetBaseAsync<T>(field, flags);
    /// <summary>
    /// 从Hash结构根据field获取缓存项，如果不存在则抛出异常CacheNotFound
    /// </summary>
    /// <param name="field"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<T> GetOrExceptionAsync<T>(string field, CommandFlags flags = CommandFlags.None)
        => await GetOrExceptionBaseAsync<T>(field, flags);
    /// <summary>
    /// 从Hash结构根据field获取缓存项，如果不存在，则返回默认值。
    /// </summary>
    /// <param name="field"></param>
    /// <param name="defaultValue"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<T> GetOrDefaultAsync<T>(string field, T defaultValue, CommandFlags flags = CommandFlags.None)
        => await GetOrDefaultBaseAsync<T>(field, defaultValue, flags);
    #endregion

    #region SetAndExpire

    public Task<bool> SetAndExpireSecondsAsync<T>(string field, T value, int seconds, CommandFlags flags = CommandFlags.None)
        => SetAsync(field, value, new TimeSpan(0, 0, seconds), true, flags);

    public Task<bool> SetAndExpireMinutesAsync<T>(string field, T value, int minutes, CommandFlags flags = CommandFlags.None)
        => SetAsync(field, value, new TimeSpan(0, minutes, 0), true, flags);

    public Task<bool> SetAndExpireHoursAsync<T>(string field, T value, int hours, CommandFlags flags = CommandFlags.None)
        => SetAsync(field, value, new TimeSpan(hours, 0, 0), true, flags);

    public Task<bool> SetAndExpireDaysAsync<T>(string field, T value, int days, CommandFlags flags = CommandFlags.None)
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
    public async Task<CacheValue<T>> GetOrLoadAsync<T>(string field, bool enforce = false, CommandFlags flags = CommandFlags.None)
    {
        CacheValue<T> ret;
        if (enforce || !TryDeserializeExpire(field, await Database.HashGetAsync(RedisKey, field, flags), out T value))
        {
            var loadValue = await LoadValueWhenRedisNotExistsAsync(field);
            if (loadValue.HasValue && !loadValue.Value.IsExpired)
            {
                await Database.HashSetAsync(RedisKey, field, SerializeExpire(loadValue.Value), When.Always, flags);
                ret = new CacheValue<T>(true, (T)loadValue.Value.Value);
            }
            else
            {
                ret = new CacheValue<T>(false, default);
            }
        }
        else
        {
            ret = new CacheValue<T>(value);
        }
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion
}