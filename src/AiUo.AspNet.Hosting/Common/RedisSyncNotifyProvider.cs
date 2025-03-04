using AiUo.Collections;
using AiUo.Extensions.StackExchangeRedis;

namespace AiUo.AspNet.Hosting;

public class RedisSyncNotifyProvider : IClientSyncNotifyProvider
{
    private string _connectionStringName;
    public RedisSyncNotifyProvider(string connectionStringName = null)
    {
        _connectionStringName = connectionStringName;
    }
    private RedisStringClient<long> GetRedis(string userId)
    {
        return RedisUtil.CreateStringClient<long>($"{RedisPrefixConst.SYNC_NOTIFY}:{userId}", _connectionStringName, true);
    }

    /// <summary>
    /// 设置需要通知的ID集合
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="notifyIds"></param>
    /// <returns></returns>
    public async Task AddNotify(string userId, params int[] notifyIds)
    {
        if (notifyIds == null || notifyIds.Length == 0)
            return;
        var redis = GetRedis(userId);
        var oldv = await redis.GetAsync();
        var list = EnumUtil.ParseFlagsValue(oldv);
        notifyIds.ForEach(x =>
        {
            if (!list.Contains(x))
                list.Add(x);
        });
        var value = EnumUtil.GetFlagsValue(list.ToArray());
        await GetRedis(userId).SetAsync(value, TimeSpan.FromDays(1));
    }

    /// <summary>
    /// 获取
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<string> GetNotifyValue(string userId)
    {
        var redis = GetRedis(userId);
        var value = await redis.GetOrDefaultAsync(-1);
        if (value < 0)
        {
            return null;
        }
        else
        {
            await redis.SetAsync(-1);
            return Convert.ToString(value);
        }
    }

    public Task<List<int>> ParseNotifyIds(string notifyValue)
    {
        var ret = new List<int>();
        if (!string.IsNullOrEmpty(notifyValue))
        {
            var value = notifyValue.ToInt64();
            ret = EnumUtil.ParseFlagsValue(value);
        }
        return Task.FromResult(ret);
    }
}