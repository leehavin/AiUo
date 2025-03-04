using StackExchange.Redis;
using AiUo.Caching;

namespace AiUo.Extensions.StackExchangeRedis;

/// <summary>
/// Redis List双向链表结构（左右两边操作）,可以被继承，也可以直接构建
/// </summary>
/// <typeparam name="T"></typeparam>
public class RedisListClient<T> : RedisClientBase
{
    public override RedisType RedisType => RedisType.List;

    #region Constructors
    /// <summary>
    /// 继承调用
    /// </summary>
    public RedisListClient() { }
    #endregion

    #region LoadAllValuesWhenRedisNotExists
    public delegate Task<CacheValue<IEnumerable<T>>> LoadAllValuesDelegate();
    public LoadAllValuesDelegate LoadAllValuesHandler;
    /// <summary>
    /// 调用GetAllOrLoad()时，当RedisKey不存在，则调用此方法返回并存储全部list值到redis中，需要时子类实现override
    /// </summary>
    /// <returns></returns>
    protected virtual async Task<CacheValue<IEnumerable<T>>> LoadAllValuesWhenRedisNotExistsAsync()
    {
        if (LoadAllValuesHandler != null)
            return await LoadAllValuesHandler();
        throw new NotImplementedException();
    }
    #endregion

    #region LeftPop & LeftPush & RightPop & RightPush & RightPopLeftPush
    /// <summary>
    /// 删除并返回存储在key处的列表的第一个元素
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<T> LeftPopAsync(CommandFlags flags = CommandFlags.None)
    {
        var redisValue = await Database.ListLeftPopAsync(RedisKey, flags);
        var ret = Deserialize<T>(redisValue);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 删除并返回列表的多个元素
    /// </summary>
    /// <param name="count">元素数量</param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<List<T>> LeftPopAsync(long count, CommandFlags flags = CommandFlags.None)
    {
        var values = await Database.ListLeftPopAsync(RedisKey, count);
        var ret = values.Select(value => Deserialize<T>(value)).ToList();
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 将所有指定的值插入存储在key的列表的开头(左边第一个）。 如果键不存在，则在执行推入操作之前将其创建为空列表。 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="when"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> LeftPushAsync(T value, When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.ListLeftPushAsync(RedisKey, Serialize(value), when, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 将所有指定的值插入存储在key的列表的开头(左边第一个）。 如果键不存在，则在执行推入操作之前将其创建为空列表。 
    /// 元素从最左边的元素到最右边的元素一个接一个地插入列表的开头。 
    /// 因此，例如，命令LPUSH mylist a b c将导致一个包含c作为第一元素，b作为第二元素和a作为第三元素的列表
    /// </summary>
    /// <param name="values"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> LeftPushAsync(IEnumerable<T> values, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.ListLeftPushAsync(RedisKey, values.Select(x => Serialize(x)).ToArray(), flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 删除并返回键处存储的列表的右边最后一个元素
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<T> RightPopAsync(CommandFlags flags = CommandFlags.None)
    {
        var redisValue = await Database.ListRightPopAsync(RedisKey, flags);
        var ret = Deserialize<T>(redisValue);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 将指定的值插入存储在key的列表的末尾。 如果键不存在，则在执行推入操作之前将其创建为空列表
    /// </summary>
    /// <param name="value"></param>
    /// <param name="when"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> RightPushAsync(T value, When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.ListRightPushAsync(RedisKey, Serialize(value), when, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 将所有指定的值插入存储在key的列表的末尾。 如果键不存在，则在执行推入操作之前将其创建为空列表。 
    /// 元素从最左边的元素到最右边的元素一个接一个插入到列表的末尾。 
    /// 因此，例如命令RPUSH mylist a b c将导致一个列表，其中包含a作为第一元素，b作为第二元素，c作为第三元素。
    /// </summary>
    /// <param name="values"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> RightPushAsync(IEnumerable<T> values, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.ListRightPushAsync(RedisKey, values.Select(x => Serialize(x)).ToArray(), flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 以原子方式返回并删除源中存储的列表的最后一个元素（尾部），并将该元素推入存储在目标位置的列表的第一个元素（头）
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<T> RightPopLeftPushAsync(string destination, CommandFlags flags = CommandFlags.None)
    {
        var redisValue = await Database.ListRightPopLeftPushAsync(RedisKey, destination, flags);
        var ret = Deserialize<T>(redisValue);
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion

    #region SetByIndex & InsertAfter & InsertBefore
    /// <summary>
    /// 将索引处的列表元素设置为value。 有关index参数的更多信息，请参见List GetByIndex。 超出范围的索引将返回错误
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    /// <param name="flags"></param>
    public async Task SetByIndexAsync(long index, T value, CommandFlags flags = CommandFlags.None)
    {
        await Database.ListSetByIndexAsync(RedisKey, index, Serialize(value), flags);
        await SetSlidingExpirationAsync();
    }

    /// <summary>
    /// 在指定列表缓存项后插入缓存项，如果键不存在，则将其视为空列表，并且不执行任何操作。
    /// </summary>
    /// <param name="pivot"></param>
    /// <param name="value"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> InsertAfterAsync(T pivot, T value, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.ListInsertAfterAsync(RedisKey, Serialize(pivot), Serialize(value), flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 在指定列表缓存项前插入缓存项，如果键不存在，则将其视为空列表，并且不执行任何操作。
    /// </summary>
    /// <param name="pivot"></param>
    /// <param name="value"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> InsertBeforeAsync(T pivot, T value, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.ListInsertBeforeAsync(RedisKey, Serialize(pivot), Serialize(value), flags);
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion

    #region GetByIndex & GetByIndexOrDefault & GetByIndexOrException & GetAll & GetAllOrLoad & Range
    /// <summary>
    /// 返回键存储在列表中索引index处的元素。 
    /// </summary>
    /// <param name="index">索引从零开始，0表示第一个元素，1表示第二个元素。 
    /// 负索引可用于指定从列表末尾开始的元素。 -1表示最后一个元素，-2表示倒数第二个。</param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<T> GetByIndexAsync(long index, CommandFlags flags = CommandFlags.None)
    {
        var redisValue = await Database.ListGetByIndexAsync(RedisKey, index, flags);
        var ret = Deserialize<T>(redisValue);
        await SetSlidingExpirationAsync();
        return ret;
    }

    public async Task<T> GetByIndexOrDefaultAsync(long index, T defaultValue, CommandFlags flags = CommandFlags.None)
    {
        var redisValue = await Database.ListGetByIndexAsync(RedisKey, index, flags);
        if (!TryDeserialize(redisValue, out T ret))
            ret = defaultValue;
        await SetSlidingExpirationAsync();
        return ret;
    }

    public async Task<T> GetByIndexOrExceptionAsync(long index, CommandFlags flags = CommandFlags.None)
    {
        var redisValue = await Database.ListGetByIndexAsync(RedisKey, index, flags);
        if (!TryDeserialize(redisValue, out T ret))
            throw new CacheNotFound($"[Redis List]index不存在。Key:{RedisKey}, Index:{index} type:{GetType().FullName}");
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 获取全部list缓存值
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<IEnumerable<T>> GetAllAsync(CommandFlags flags = CommandFlags.None)
    {
        var ret = await RangeAsync(0, -1, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 获取所有list缓存值，如果RedisKey不存在，则调用LoadAllValuesWhenRedisNotExists()返回并保存到redis中
    /// </summary>
    /// <param name="enforce">是否强制加载</param>
    /// <param name="expire"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<CacheValue<IEnumerable<T>>> GetAllOrLoadAsync(bool enforce = false, TimeSpan? expire = null, CommandFlags flags = CommandFlags.None)
    {
        CacheValue<IEnumerable<T>> ret = default;
        if (enforce || !await KeyExistsAsync(flags))
        {
            ret = await LoadAllValuesWhenRedisNotExistsAsync();
            if (ret.HasValue)
            {
                await RightPushAsync(ret.Value, flags);
            }
        }
        else
        {
            ret = new CacheValue<IEnumerable<T>>(await RangeAsync(0, -1, flags));
        }
        if (ret.HasValue)
            await SetSlidingExpirationAsync(expire);
        return ret;
    }

    public async Task<IEnumerable<T>> GetAllOrDefaultAsync(IEnumerable<T> defaultValue, CommandFlags flags = CommandFlags.None)
    {
        if (await KeyExistsAsync(flags))
        {
            var ret = await RangeAsync(0, -1, flags);
            await SetSlidingExpirationAsync();
            return ret;
        }
        return defaultValue;
    }

    public async Task<IEnumerable<T>> GetAllOrExceptionAsync(CommandFlags flags = CommandFlags.None)
    {
        if (!await KeyExistsAsync(flags))
            throw new CacheNotFound($"[Redis List]不存在。RedisKey: {RedisKey} type:{GetType().FullName}");
        var ret = await RangeAsync(0, -1, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 返回存储在key处的列表的指定元素。 
    /// 偏移量start和stop是从零开始的索引，0是列表的第一个元素（列表的开头），1是下一个元素，依此类推。 
    /// 这些偏移量也可以是负数，表示从列表末尾开始的偏移量，例如-1是列表的最后一个元素，-2是倒数第二个，依此类推。 
    /// 请注意，如果您有一个从0到100的数字列表，则LRANGE list 0 10将返回11个元素，即，其中包括最右边的项目。
    /// </summary>
    /// <param name="start">0表示第一个元素，-1表示最后一个元素</param>
    /// <param name="stop">0表示第一个元素，-1表示最后一个元素</param>
    /// <param name="flags"></param>
    /// <returns>返回包含最右边的项</returns>
    public async Task<IEnumerable<T>> RangeAsync(long start = 0, long stop = -1, CommandFlags flags = CommandFlags.None)
    {
        var ret = (await Database.ListRangeAsync(RedisKey, start, stop, flags))
            .Select(x => Deserialize<T>(x));
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion

    #region Remove & Trim & GetLength
    /// <summary>
    /// 从存储的列表中删除等于value的元素的第一个计数出现。 
    /// count参数通过以下方式影响操作：
    ///     count > 0：删除等于从头到尾移动的值的元素。 
    ///     count 小于 0：删除等于从尾到头的值的元素。 
    ///     count  0：删除所有等于value的元素
    /// </summary>
    /// <param name="value"></param>
    /// <param name="count">count大于0 从头删除count个元素; count=0 删除全部; count小于0 从后删除count个元素</param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> RemoveAsync(T value, long count = 0, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.ListRemoveAsync(RedisKey, Serialize(value), count, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 修剪现有列表，使其仅包含指定范围的指定元素。 
    /// 开始和停止都是从零开始的索引，其中0是列表的第一个元素（头），1是下一个元素，依此类推。 
    /// 例如：LTRIM foobar 0 2将修改存储在foobar的列表，以便仅保留列表的前三个元素。 
    /// start和end也可以是负数，指示与列表末尾的偏移量，其中-1是列表的最后一个元素，-2是倒数第二个元素，依此类推。
    /// </summary>
    /// <param name="start"></param>
    /// <param name="stop"></param>
    /// <param name="flags"></param>
    public async Task TrimAsync(long start, long stop, CommandFlags flags = CommandFlags.None)
    {
        await Database.ListTrimAsync(RedisKey, start, stop, flags);
        await SetSlidingExpirationAsync();
    }

    /// <summary>
    /// 返回键处存储的列表的长度。 如果key不存在，则将其解释为空列表并返回0。
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> GetLengthAsync(CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.ListLengthAsync(RedisKey, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion
}