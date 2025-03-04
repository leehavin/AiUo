using StackExchange.Redis;
using AiUo.Caching;

namespace AiUo.Extensions.StackExchangeRedis;

/// <summary>
/// Redis string结构,可以被继承，也可以直接构建
/// 可存入null值，不存在抛出异常CacheNotFound
/// </summary>
/// <typeparam name="T"></typeparam>
public class RedisStringClient<T> : RedisClientBase
{
    public override RedisType RedisType => RedisType.String;
    /// <summary>
    /// GetOrLoad时，如值不存在是否保存到redis
    /// </summary>
    public virtual bool IsLoadValueNotExistsToRedis { get; } = false;

    #region Constructors
    public RedisStringClient() { }
    #endregion

    #region LoadValueWhenRedisNotExists
    public delegate Task<CacheValue<T>> LoadValueDelegate();
    public LoadValueDelegate LoadValueHeadler;
    /// <summary>
    /// 在调用GetOrLoad时，当RedisKey不存时，将调用此方法获取缓存值，返回并存储到Redis中，需要时子类实现override。
    /// 子类实现时注意：
    /// 1)value可等于null，存入Redis的值为RedisValue.EmptyString
    /// 2)可抛出异常CacheNotFound：表示缓存值不存在
    /// </summary>
    /// <returns></returns>
    protected virtual async Task<CacheValue<T>> LoadValueWhenRedisNotExistsAsync()
    {
        if (LoadValueHeadler != null)
            return await LoadValueHeadler();
        throw new NotImplementedException();
    }
    #endregion

    #region Set
    public Task<bool> SetAndExpireSecondsAsync(T value, int seconds, When when = When.Always, CommandFlags flags = CommandFlags.None)
        => Database.StringSetAsync(RedisKey, Serialize(value), new TimeSpan(0, 0, seconds), when, flags);

    public Task<bool> SetAndExpireMinutesAsync(T value, int minutes, When when = When.Always, CommandFlags flags = CommandFlags.None)
        => Database.StringSetAsync(RedisKey, Serialize(value), new TimeSpan(0, minutes, 0), when, flags);

    public Task<bool> SetAndExpireHoursAsync(T value, int hours, When when = When.Always, CommandFlags flags = CommandFlags.None)
        => Database.StringSetAsync(RedisKey, Serialize(value), new TimeSpan(hours, 0, 0), when, flags);

    public Task<bool> SetAndExpireDaysAsync(T value, int days, When when = When.Always, CommandFlags flags = CommandFlags.None)
        => Database.StringSetAsync(RedisKey, Serialize(value), new TimeSpan(days, 0, 0, 0), when, flags);

    public async Task<bool> SetAsync(T value, TimeSpan? expire = null, When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.StringSetAsync(RedisKey, Serialize(value), expire, when, flags);
        if (!expire.HasValue)
            await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion

    #region Get & GetOrLoad & GetOrException & GetOrDefault
    /// <summary>
    /// 获取此Hash中field对应的缓存值，不存在返回default(T)
    /// </summary>
    /// <param name="expire"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<T> GetAsync(TimeSpan? expire = null, CommandFlags flags = CommandFlags.None)
    {
        var redisValue = await Database.StringGetAsync(RedisKey, flags);
        var ret = Deserialize<T>(redisValue);
        await SetSlidingExpirationAsync(expire);
        return ret;
    }
    /// <summary>
    /// 获取缓存项，如果不存在则调用LoadValueWhenRedisNotExists()放入redis并返回
    /// </summary>
    /// <param name="enforce">是否强制Load</param>
    /// <param name="expire"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<CacheValue<T>> GetOrLoadAsync(bool enforce = false, TimeSpan? expire = null, CommandFlags flags = CommandFlags.None)
    {
        CacheValue<T> ret;
        var isExpired = false;
        if (enforce || !TryDeserialize(await Database.StringGetAsync(RedisKey, flags), out T value))
        {
            ret = await LoadValueWhenRedisNotExistsAsync();
            if (ret.HasValue)
            {
                isExpired = expire.HasValue;
                await Database.StringSetAsync(RedisKey, Serialize(ret.Value), expire, When.Always, flags);
            }
            else
            {
                if (IsLoadValueNotExistsToRedis)
                {
                    isExpired = expire.HasValue;
                    await Database.StringSetAsync(RedisKey, Serialize(null), expire, When.Always, flags);
                }
            }
        }
        else
        {
            ret = new CacheValue<T>(value);
        }
        if (!isExpired)
            await SetSlidingExpirationAsync(expire);
        return ret;
    }

    /// <summary>
    /// 获取缓存，如果不存在则抛出异常CacheNotFound
    /// </summary>
    /// <param name="expire"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<T> GetOrExceptionAsync(TimeSpan? expire = null, CommandFlags flags = CommandFlags.None)
    {
        var redisValue = await Database.StringGetAsync(RedisKey, flags);
        if (!TryDeserialize(redisValue, out T ret))
            throw new CacheNotFound($"[Redis String]缓存项不存在。key:{RedisKey} type:{GetType().FullName}");
        await SetSlidingExpirationAsync(expire);
        return ret;
    }

    /// <summary>
    /// 获取缓存,如果不存在，则返回默认值
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <param name="expire"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<T> GetOrDefaultAsync(T defaultValue, TimeSpan? expire = null, CommandFlags flags = CommandFlags.None)
    {
        var redisValue = await Database.StringGetAsync(RedisKey, flags);
        if (!TryDeserialize(redisValue, out T ret))
            ret = defaultValue;
        await SetSlidingExpirationAsync(expire);
        return ret;
    }
    #endregion

    #region GetLength & Append & BitCount & GetRange &SetRange
    /// <summary>
    /// 返回存储在键处的字符串值的长度
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> GetLengthAsync(CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.StringLengthAsync(RedisKey, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 如果key已经存在并且是字符串，则此命令将值附加在字符串的末尾。 
    /// 如果key不存在，则会创建key并将其设置为空字符串，因此APPEND在这种特殊情况下将类似于SET
    /// </summary>
    /// <param name="value"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> AppendAsync(string value, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.StringAppendAsync(RedisKey, value, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 计算字符串中的设置位数（填充计数）。 
    /// 默认情况下，将检查字符串中包含的所有字节，也可以仅在传递附加参数start和end的间隔中指定计数操作。 
    /// 像GETRANGE命令一样，开始和结束可以包含负值，以便从字符串的末尾开始索引字节，其中-1是最后一个字节，-2是倒数第二个，依此类推
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> BitCountAsync(long start = 0, long end = -1, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.StringBitCountAsync(RedisKey, start, end, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 返回存储在key处的字符串值的子字符串，该字符串由偏移量start和end（包括两端）确定。 
    /// 可以使用负偏移量来提供从字符串末尾开始的偏移量。 因此-1表示最后一个字符，-2表示倒数第二个，依此类推。
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<string> GetRangeAsync(long start, long end, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.StringGetRangeAsync(RedisKey, start, end, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 从指定的偏移量开始，覆盖整个值范围内从key存储的字符串的一部分。 
    /// 如果偏移量大于key处字符串的当前长度，则该字符串将填充零字节以使偏移量适合。 
    /// 不存在的键被视为空字符串，因此此命令将确保它包含足够大的字符串以能够将值设置为offset
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="value"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<string> SetRangeAsync(long offset, string value, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.StringSetRangeAsync(RedisKey, offset, value, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion

    #region Increment
    /// <summary>
    /// Hash结构存储增量数字。如果field不存在则设置为0。支持long
    /// </summary>
    /// <param name="value"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> IncrementAsync(long value = 1, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.StringIncrementAsync(RedisKey, value, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// Hash结构存储增量数字。如果field不存在则设置为0。支持long
    /// </summary>
    /// <param name="value"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<double> IncrementAsync(double value, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.StringIncrementAsync(RedisKey, value, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 减量数字-value,如不存在key则创建，返回减量后值
    /// </summary>
    /// <param name="value"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> DecrementAsync(long value = 1, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.StringDecrementAsync(RedisKey, value, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 减量数字-value,如不存在key则创建，返回减量后值
    /// </summary>
    /// <param name="value"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<double> DecrementAsync(double value, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.StringDecrementAsync(RedisKey, value, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion

    #region Bit操作
    public async Task<bool> GetBitAsync(long offset, CommandFlags flags = CommandFlags.None)
        => await Database.StringGetBitAsync(RedisKey, offset, flags);
    public async Task SetBitAsync(long offset, bool bit, CommandFlags flags = CommandFlags.None)
        => await Database.StringSetBitAsync(RedisKey, offset, bit, flags);
    #endregion
}