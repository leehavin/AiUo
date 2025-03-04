using StackExchange.Redis;
using AiUo.Caching;

namespace AiUo.Extensions.StackExchangeRedis;

/// <summary>
/// Redis Set集合（不重复集合）
/// </summary>
/// <typeparam name="T"></typeparam>
public class RedisSetClient<T> : RedisClientBase
{
    public override RedisType RedisType => RedisType.Set;

    #region Constructors
    public RedisSetClient() { }
    #endregion

    #region LoadAllValuesWhenRedisNotExists
    public delegate Task<CacheValue<IEnumerable<T>>> LoadAllValuesDelegate();
    public LoadAllValuesDelegate LoadAllValuesHandler;
    /// <summary>
    /// 调用GetAllOrLoad()时，当RedisKey不存在，则调用此方法返回并存储全部set值到redis中，需要时子类实现override
    /// </summary>
    /// <returns></returns>
    protected virtual async Task<CacheValue<IEnumerable<T>>> LoadAllValuesWhenRedisNotExistsAsync()
    {
        if (LoadAllValuesHandler != null)
            return await LoadAllValuesHandler();
        throw new NotImplementedException();
    }
    #endregion

    #region GetAll & GetAllOrLoad
    /// <summary>
    /// 获取SET所有成员
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<IEnumerable<T>> MembersAsync(CommandFlags flags = CommandFlags.None)
    {
        var ret = (await Database.SetMembersAsync(RedisKey, flags))
            .Select(value => Deserialize<T>(value));
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 获取SET所有成员
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public Task<IEnumerable<T>> GetAllAsync(CommandFlags flags = CommandFlags.None)
        => MembersAsync(flags);

    /// <summary>
    /// 获取所有SET缓存值，如果RedisKey不存在，则调用LoadAllValuesWhenRedisNotExists()返回并保存到redis中
    /// </summary>
    /// <param name="expire"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<CacheValue<IEnumerable<T>>> GetAllOrLoadAsync(TimeSpan? expire = null, CommandFlags flags = CommandFlags.None)
    {
        CacheValue<IEnumerable<T>> ret = default;
        if (!await KeyExistsAsync(flags))
        {
            ret = await LoadAllValuesWhenRedisNotExistsAsync();
            if (ret.HasValue)
                await AddAsync(ret.Value, flags);
        }
        else
        {
            ret = new CacheValue<IEnumerable<T>>(await MembersAsync(flags));
        }
        if(ret.HasValue)
            await SetSlidingExpirationAsync(expire);
        return ret;
    }
    #endregion

    #region Add & Remove & Move & Contains & GetLength
    /// <summary>
    /// 将指定的成员添加到存储在key的集合中。 存在则忽略，不存在则添加，RedisKey不存在则创建
    /// </summary>
    /// <param name="value"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(T value, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.SetAddAsync(RedisKey, Serialize(value), flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    public async Task<long> AddAsync(IEnumerable<T> values, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.SetAddAsync(RedisKey, values.Select(value => Serialize(value)).ToArray(), flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 从SET中移除指定元素
    /// </summary>
    /// <param name="value"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<bool> RemoveAsync(T value, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.SetRemoveAsync(RedisKey, Serialize(value), flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 从SET中移除多个指定元素
    /// </summary>
    /// <param name="values"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> RemoveAsync(IEnumerable<T> values, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.SetRemoveAsync(RedisKey, values.Select(value => Serialize(value)).ToArray(), flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 将成员从源集合移到目标集合。 此操作是原子的。 
    /// 在每个给定的时刻，该元素将似乎是其他客户端的源或目标的成员。 如果指定的元素已存在于目标集中，则仅将其从源集中删除
    /// </summary>
    /// <param name="destinationKey">目标集合键</param>
    /// <param name="value"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<bool> MoveAsync(string destinationKey, T value, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.SetMoveAsync(RedisKey, destinationKey, Serialize(value), flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// SET是否存在此值
    /// </summary>
    /// <param name="value"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<bool> ContainsAsync(T value, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.SetContainsAsync(RedisKey, Serialize(value), flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// SET缓存个数
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> GetLengthAsync(CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.SetLengthAsync(RedisKey, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion

    #region Pop & Random & Scan

    /// <summary>
    /// 随机从SET中删除并返回一个元素
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<T> PopAsync(CommandFlags flags = CommandFlags.None)
    {
        var ret = Deserialize<T>(await Database.SetPopAsync(RedisKey, flags));
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 随机从SET中删除并返回指定数量的元素
    /// </summary>
    /// <param name="count"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<IEnumerable<T>> PopAsync(long count, CommandFlags flags = CommandFlags.None)
    {
        var ret = (await Database.SetPopAsync(RedisKey, count, flags))
            .Select(value => Deserialize<T>(value));
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 随机从SET中取出一个元素
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<T> RandomAsync(CommandFlags flags = CommandFlags.None)
    {
        var ret = Deserialize<T>(await Database.SetRandomMemberAsync(RedisKey, flags));
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 随机从SET中取出指定数量的元素.
    ///     如果count为正数，返回由count个不同元素组成的数组。
    ///     如果count为负数，则可以返回包含重复元素的数组，返回数量是count的绝对值
    /// </summary>
    /// <param name="count">取出的随机数量。正数：返回不同元素的数组。负数：返回可重复元素的数组，数组长度是此值的绝对值</param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<IEnumerable<T>> RandomAsync(long count, CommandFlags flags = CommandFlags.None)
    {
        var ret = (await Database.SetRandomMembersAsync(RedisKey, count, flags))
            .Select(value => Deserialize<T>(value));
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 使用SSCAN命令遍历集合
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="pageSize"></param>
    /// <param name="cursor"></param>
    /// <param name="pageOffset"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<IEnumerable<T>> ScanAsync(string pattern = default, int pageSize = 10, long cursor = 0, int pageOffset = 0, CommandFlags flags = CommandFlags.None)
    {
        var ret = new List<T>();
        var entries = Database.SetScanAsync(RedisKey, pattern, pageSize, cursor, pageOffset, flags);
        await foreach (var entry in entries)
        {
            ret.Add(Deserialize<T>(entry));
        }
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion
}