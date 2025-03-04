using StackExchange.Redis;
using AiUo.Caching;

namespace AiUo.Extensions.StackExchangeRedis;

/// <summary>
/// Redis Zset集合（排序的不重复集合）
/// </summary>
/// <typeparam name="T"></typeparam>
public class RedisSortedSetClient<T> : RedisClientBase
{
    public override RedisType RedisType => RedisType.SortedSet;

    #region Constructors
    public RedisSortedSetClient()
    {
    }
    #endregion

    #region LoadAllValuesWhenRedisNotExists
    public delegate Task<CacheValue<IEnumerable<T>>> LoadAllValuesDelegate();
    public LoadAllValuesDelegate LoadAllValuesHandler;
    /// <summary>
    /// 调用GetAllOrLoad()时，当RedisKey不存在，则调用此方法返回并存储全部zset值到redis中，需要时子类实现override
    /// </summary>
    /// <returns></returns>
    protected virtual async Task<CacheValue<IEnumerable<T>>> LoadAllValuesWhenRedisNotExists()
    {
        if (LoadAllValuesHandler != null)
            return await LoadAllValuesHandler();
        throw new NotImplementedException();
    }
    #endregion

    #region Add
    /// <summary>
    /// 将指定分数的成员添加到zset中。
    /// 如果已存在，则将更新分数并在正确的位置重新插入元素
    /// </summary>
    /// <param name="value"></param>
    /// <param name="score">成员要添加到排序集中的分数</param>
    /// <param name="when"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(T value, double score, When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.SortedSetAddAsync(RedisKey, Serialize(value), score, when, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 将一组指定分数的成员添加到zset中。
    /// 如果已存在，则将更新分数并在正确的位置重新插入元素
    /// </summary>
    /// <param name="when"></param>
    /// <param name="values"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> AddAsync(IEnumerable<(T value, double score)> values, When when = When.Always, CommandFlags flags = CommandFlags.None)
    {
        var entries = values.Select(item => new SortedSetEntry(Serialize(item.value), item.score));
        var ret = await Database.SortedSetAddAsync(RedisKey, entries.ToArray(), when, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion

    #region Remove & RemoveRangeByRank & RemoveRangeByScore & RemoveRangeByValue
    /// <summary>
    /// 从存储在key的排序集中删除指定的成员。 不存在的成员将被忽略
    /// </summary>
    /// <param name="members"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> RemoveAsync(IEnumerable<T> members, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.SortedSetRemoveAsync(RedisKey, members.Select(member => Serialize(member)).ToArray(), flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 从存储在key的排序集中删除指定的成员。 不存在的成员将被忽略
    /// </summary>
    /// <param name="member"></param>
    /// <param name="flags"></param>
    /// <returns>如果成员存在于已排序集中并已被删除，则为true；否则为true。 否则为假</returns>
    public async Task<bool> RemoveAsync(T member, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.SortedSetRemoveAsync(RedisKey, Serialize(member), flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 删除指定排名区间的元素。顺序从0开始，倒序-1开始
    /// </summary>
    /// <param name="start"></param>
    /// <param name="stop"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> RemoveRangeByRankAsync(long start, long stop, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.SortedSetRemoveRangeByRankAsync(RedisKey, start, stop, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 删除指定score区间的元素，默认包含start和stop
    /// </summary>
    /// <param name="start"></param>
    /// <param name="stop"></param>
    /// <param name="exclude"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long> RemoveRangeByScoreAsync(double start, double stop, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.SortedSetRemoveRangeByScoreAsync(RedisKey, start, stop, exclude, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    public async Task<long> RemoveRangeByValueAsync(T min, T max, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.SortedSetRemoveRangeByValueAsync(RedisKey, Serialize(min), Serialize(max), exclude, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion

    #region Pop & Scan & Score & Rank & GetLength & LengthByValue
    /// <summary>
    /// 从键存储的排序集中删除并返回第一个元素，默认情况下，分数从低到高排序。
    /// </summary>
    /// <param name="order">排序依据（默认为升序）</param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<(T Element, double Score)?> PopAsync(Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
    {
        (T Element, double Score)? ret = null;
        var entry = await Database.SortedSetPopAsync(RedisKey, order, flags);
        if (entry.HasValue)
        {
            var value = Deserialize<T>(entry.Value.Element);
            ret = (Element: value, Score: entry.Value.Score);
        }
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 从键存储的排序集中删除并返回指定数量的第一元素，默认情况下，分数从低到高排序
    /// </summary>
    /// <param name="count">要返回的元素数。</param>
    /// <param name="order">排序依据（默认为升序）</param>
    /// <param name="flags"></param>
    /// <returns>一个元素数组，或者当键不存在时为空数组。</returns>
    public async Task<IEnumerable<(T Element, double Score)>> PopAsync(long count, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
    {
        var ret = (await Database.SortedSetPopAsync(RedisKey, count, order, flags))
            .Select(entry => (Element: Deserialize<T>(entry.Element), Score: entry.Score));
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// ZSCAN命令用于对迭代集进行增量迭代
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="pageSize"></param>
    /// <param name="cursor"></param>
    /// <param name="pageOffset"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<IEnumerable<(T Element, double Score)>> ScanAsync(string pattern, int pageSize, long cursor = 0, int pageOffset = 0, CommandFlags flags = CommandFlags.None)
    {
        var ret = new List<(T Element, double Score)>();
        var entries = Database.SortedSetScanAsync(RedisKey, pattern, pageSize, cursor, pageOffset, flags);
        await foreach (var entry in entries)
        {
            ret.Add((Element: Deserialize<T>(entry.Element), Score: entry.Score));
        }
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 返回key排序集中的成员分数； 如果成员不存在于排序集中，或者键不存在，则返回null
    /// </summary>
    /// <param name="member"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<double?> ScoreAsync(T member, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.SortedSetScoreAsync(RedisKey, Serialize(member), flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 返回ZSet中的成员的排名，默认情况下，分数从低到高排序。 等级（或索引）从0开始
    /// </summary>
    /// <param name="member"></param>
    /// <param name="order"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<long?> RankAsync(T member, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.SortedSetRankAsync(RedisKey, Serialize(member), order, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 返回区间值之间的元素数
    /// </summary>
    /// <param name="min">要过滤的最大分数（默认为正无穷小）。</param>
    /// <param name="max">要过滤的最大分数（默认为正无穷大）。</param>
    /// <param name="exclude">是否从范围检查中排除最小值和最大值（默认值包括两者）</param>
    /// <param name="flags"></param>
    /// <returns>排序集的基数（元素数）；如果键不存在，则为0。</returns>
    public async Task<long> GetLengthAsync(double min = double.NegativeInfinity, double max = double.PositiveInfinity, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.SortedSetLengthAsync(RedisKey, min, max, exclude, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 当以相同的分数插入排序集中的所有元素时，为了强制按字典顺序排序，此命令将返回键中排序集中的元素数，其值介于min和max之间
    /// </summary>
    /// <param name="min">过滤的最小值。</param>
    /// <param name="max">要过滤的最大值。</param>
    /// <param name="exclude"></param>
    /// <param name="flags"></param>
    /// <returns>指定分数范围内的元素数。</returns>
    public async Task<long> LengthByValueAsync(T min, T max, Exclude exclude = Exclude.None, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.SortedSetLengthByValueAsync(RedisKey, Serialize(min), Serialize(max), exclude, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion

    #region RangeByRank & RangeByRankWithScores & RangeByScore & RangeByScoreWithScores & RangeByValue
    /// <summary>
    /// 返回指定范围的元素，默认从低到高。
    /// 索引从零开始，负数表示末尾偏移量，-1最后一个，-2倒数第二个
    /// </summary>
    /// <param name="start">筛选所依据的最低Score</param>
    /// <param name="stop">筛选所依据的最高Score</param>
    /// <param name="order">排序依据（默认为升序）</param>
    /// <param name="flags"></param>
    /// <returns>指定范围内的元素列表</returns>
    public async Task<IEnumerable<T>> RangeByRankAsync(long start = 0, long stop = -1, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
    {
        var ret = (await Database.SortedSetRangeByRankAsync(RedisKey, start, stop, order, flags))
            .Select(value => Deserialize<T>(value));
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 返回指定范围的元素，默认从低到高。
    /// 索引从零开始，负数表示末尾偏移量，-1最后一个，-2倒数第二个
    /// </summary>
    /// <param name="start">筛选所依据的最低Score</param>
    /// <param name="stop">筛选所依据的最高Score</param>
    /// <param name="order">排序依据（默认为升序）</param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<IEnumerable<(T Element, double Score)>> RangeByRankWithScoresAsync(long start = 0, long stop = -1, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
    {
        var ret = (await Database.SortedSetRangeByRankWithScoresAsync(RedisKey, start, stop, order, flags))
            .Select(entry => (Element: Deserialize<T>(entry.Element), Score: entry.Score));
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 返回指定Score范围的元素，默认从低到高。
    /// </summary>
    /// <param name="start">筛选所依据的最低Score</param>
    /// <param name="stop">筛选所依据的最高Score</param>
    /// <param name="exclude">设置是否包含start和stop</param>
    /// <param name="order">排序依据（默认为升序）</param>
    /// <param name="skip">跳过多少个项目</param>
    /// <param name="take">取多少个元素</param>
    /// <param name="flags"></param>
    /// <returns>指定分数范围内的元素列表</returns>
    public async Task<IEnumerable<T>> RangeByScoreAsync(double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
    {
        var ret = (await Database.SortedSetRangeByScoreAsync(RedisKey, start, stop, exclude, order, skip, take, flags))
            .Select(value => Deserialize<T>(value));
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 返回指定Score范围的元素，默认从低到高。
    /// </summary>
    /// <param name="start">筛选所依据的最低Score</param>
    /// <param name="stop">筛选所依据的最高Score</param>
    /// <param name="exclude">设置是否包含start和stop</param>
    /// <param name="order">排序依据（默认为升序）</param>
    /// <param name="skip">跳过多少个项目</param>
    /// <param name="take">取多少个元素</param>
    /// <param name="flags"></param>
    /// <returns>指定分数范围内的元素列表</returns>
    public async Task<IEnumerable<(T Element, double Score)>> RangeByScoreWithScoresAsync(double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
    {
        var ret = (await Database.SortedSetRangeByScoreWithScoresAsync(RedisKey, start, stop, exclude, order, skip, take, flags))
            .Select(entry => (Element: Deserialize<T>(entry.Element), Score: entry.Score));
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 当以相同的分数插入排序集中的所有元素时，为了强制按字典顺序排序，
    /// 此命令将返回ZSet中的所有元素，且其值介于min和max之间
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="exclude"></param>
    /// <param name="order"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<IEnumerable<T>> RangeByValueAsync(T min, T max, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
    {
        var ret = (await Database.SortedSetRangeByValueAsync(RedisKey, Serialize(min), Serialize(max), exclude, order, skip, take, flags))
            .Select(value => Deserialize<T>(value));
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion

    #region Increment & Decrement
    /// <summary>
    /// 递减存储在key上的排序集中的成员分数。
    /// 如果该成员不存在于排序集中，则将其添加-decrement作为其分数（好像其先前分数为0.0）
    /// </summary>
    /// <param name="member"></param>
    /// <param name="value"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<double> DecrementAsync(T member, double value, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.SortedSetDecrementAsync(RedisKey, Serialize(member), value, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }

    /// <summary>
    /// 按增量递增存储在键中的排序集中成员的分数。 
    /// 如果成员不存在于排序集中，则将其添加为分数作为增量（好像其先前分数为0.0）
    /// </summary>
    /// <param name="member"></param>
    /// <param name="value"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    public async Task<double> IncrementAsync(T member, double value, CommandFlags flags = CommandFlags.None)
    {
        var ret = await Database.SortedSetIncrementAsync(RedisKey, Serialize(member), value, flags);
        await SetSlidingExpirationAsync();
        return ret;
    }
    #endregion
}