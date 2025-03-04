using AiUo.Caching;
using AiUo.Configuration;
using AiUo.Extensions.StackExchangeRedis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AiUo.Hosting.Services;

/// <summary>
/// service服务数据
/// </summary>
internal class AiUoHostDataDCache : RedisHashClient
{
    public string ServiceId { get; }
    private TimeSpan _expireSpan;
    public bool IsDeregistering { get; set; }
    public AiUoHostDataDCache(string serviceId, string connectionStringName = null)
    {
        ServiceId = serviceId;
        Options.ConnectionStringName = connectionStringName;
        RedisKey = $"{RedisPrefixConst.HOSTS}:Data:{serviceId}";
        _expireSpan = ConfigUtil.Environment.IsDebug
            ? TimeSpan.FromMinutes(20) // 没有设置或Debug时20分钟
            : TimeSpan.FromMilliseconds(ConfigUtil.Host.HeathInterval * 3);
    }

    public async Task RegisterData()
    {
        var dict = new Dictionary<string, object>();
        dict.Add("ServiceId", ServiceId);
        dict.Add("HostIp", ConfigUtil.Service.HostIp);
        dict.Add("HostPort", ConfigUtil.Service.HostPort);
        dict.Add("HostApiType", ConfigUtil.Service.HostApiType.ToString());
        dict.Add("HostUrl", ConfigUtil.Service.HostUrl);
        dict.Add("RegisterDate", DateTime.UtcNow.UtcToCNString());
        await SetAsync(dict);
        await KeyExpireAsync(TimeSpan.FromSeconds(30));
    }
    /// <summary>
    /// 激活服务
    /// </summary>
    /// <returns></returns>
    public async Task ActiveData()
    {
        if (IsDeregistering) return;
        await SetAsync("ActiveDate", DateTime.UtcNow.ToTimestamp());
        await KeyExpireAsync(_expireSpan);
    }
    public async Task<AiUoHostDataStatus> GetStatus()
    {
        if (!await KeyExistsAsync())
            return AiUoHostDataStatus.None;
        var last = await GetOrDefaultAsync<long>("ActiveDate", 0);
        if (last == -1)
            return AiUoHostDataStatus.Deregistering;
        if (last == 0)
        {
            var regDate = await GetOrDefaultAsync<string>("RegisterDate", null);
            return !string.IsNullOrEmpty(regDate)
                ? AiUoHostDataStatus.Registering
                : AiUoHostDataStatus.None;
        }
        var lastTime = DateTimeUtil.ParseTimestamp(last);
        var ret = DateTime.UtcNow - lastTime < _expireSpan;
        return ret ? AiUoHostDataStatus.Actived : AiUoHostDataStatus.None;
    }
    public async Task DeleteData()
    {
        await KeyDeleteAsync();
    }

    /// <summary>
    /// 设置服务数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetData<T>(string field, T value)
    {
        await SetAsync(field, value);
    }

    /// <summary>
    /// 获取服务数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="field"></param>
    /// <returns></returns>
    public async Task<CacheValue<T>> GetData<T>(string field)
    {
        return await GetAsync<T>(field);
    }
}
internal enum AiUoHostDataStatus
{
    /// <summary>
    /// 不存在
    /// </summary>
    None = 0,
    /// <summary>
    /// 注册中
    /// </summary>
    Registering = 1,
    /// <summary>
    /// 激活中
    /// </summary>
    Actived = 2,
    /// <summary>
    /// 注销中
    /// </summary>
    Deregistering = 3
}