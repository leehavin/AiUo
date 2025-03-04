using AiUo.Configuration;
using AiUo.Extensions.StackExchangeRedis;
using AiUo.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AiUo.Hosting.Services;

public class RedisHostRegisterProvider : IAiUoHostRegisterProvider
{
    public const string HOST_NAMES_KEY = $"{RedisPrefixConst.HOSTS}:Names";
    public const string HOST_IDS_KEY = $"{RedisPrefixConst.HOSTS}:ServiceIds";
    public const string HOST_HEALTH_KEY = $"{RedisPrefixConst.HOSTS}:Health";

    private string _connectionStringName;
    public string ServiceName { get; }
    public string ServiceId { get; }

    private int HEALTH_INTERVAL;

    private RedisSetClient<string> _namesDCache;
    private RedisSetClient<string> _idsDCache;
    private RedisStringClient<long> _healthDCache;
    private AiUoHostDataDCache _dataDCache;
    public RedisHostRegisterProvider(string connectionStringName = null)
    {
        _connectionStringName = connectionStringName;

        ServiceName = ConfigUtil.Project.ProjectId;
        ServiceId = ConfigUtil.Service.ServiceId;
        HEALTH_INTERVAL = ConfigUtil.Host.HeathInterval;

        _namesDCache = RedisUtil.CreateSetClient<string>(HOST_NAMES_KEY, _connectionStringName, true);
        _idsDCache = RedisUtil.CreateSetClient<string>($"{HOST_IDS_KEY}:{ServiceName}", _connectionStringName, true);
        _healthDCache = RedisUtil.CreateStringClient<long>(HOST_HEALTH_KEY, _connectionStringName, true);
        _dataDCache = new AiUoHostDataDCache(ServiceId, _connectionStringName);
    }

    #region 注册
    public async Task Register()
    {
        using (var redLock = await GetRedLock())
        {
            await _dataDCache.RegisterData();
            await _idsDCache.AddAsync(ServiceId);
            await _namesDCache.AddAsync(ServiceName);
        }
        LogUtil.Info($"注册Host => Redis[{GetType().Name}] ServerId:{ServiceId}");
    }
    public async Task Deregistering()
    {
        using (var redLock = await GetRedLock())
        {
            _dataDCache.IsDeregistering = true;
            await _dataDCache.SetAsync<long>("ActiveDate", -1);
        }
    }
    public async Task Deregistered()
    {
        using (var redLock = await GetRedLock())
        {
            await _dataDCache.DeleteData();
            await _idsDCache.RemoveAsync(ServiceId);
            var count = await _idsDCache.GetLengthAsync();
            if (count == 0)
            {
                await _idsDCache.KeyDeleteAsync();
                await _namesDCache.RemoveAsync(ServiceName);
            }
        }
        LogUtil.Info($"停止 => 注销Host[{GetType().Name}] ServerId:{ServiceId}");
    }

    public async Task Heartbeat()
    {
        await _dataDCache.ActiveData();
    }

    public async Task Health()
    {
        using (var redLock = await RedisUtil.LockAsync(HOST_HEALTH_KEY, 20, connectionStringName: _connectionStringName))
        {
            if (!redLock.IsLocked)
                throw new Exception($"RedisHostRegisterService获取缓存锁超时。key: __HostRegister:_HEALTH");

            var lastTs = await _healthDCache.GetOrDefaultAsync(0);
            var utcTs = DateTime.UtcNow.ToTimestamp(true);
            if (utcTs - lastTs < HEALTH_INTERVAL)
                return;
            await _healthDCache.SetAsync(utcTs);
        }

        var serviceNames = (await _namesDCache.GetAllAsync()).ToList();
        foreach (var serviceName in serviceNames)
        {
            var idsDCache = RedisUtil.CreateSetClient<string>($"{HOST_IDS_KEY}:{serviceName}", _connectionStringName, true);
            var serviceIds = (await idsDCache.GetAllAsync()).ToList();
            foreach (var serviceId in serviceIds)
            {
                var status = await new AiUoHostDataDCache(serviceId, _connectionStringName).GetStatus();
                if (status != AiUoHostDataStatus.Registering && status != AiUoHostDataStatus.Actived)
                    await idsDCache.RemoveAsync(serviceId);
            }
            var count = await idsDCache.GetLengthAsync();
            if (count == 0)
            {
                await idsDCache.KeyDeleteAsync();
                await _namesDCache.RemoveAsync(serviceName);
            }
        }
    }
    #endregion

    private async Task<RedLock> GetRedLock()
    {
        var ret = await RedisUtil.LockAsync($"_HostRegister:{ServiceName}", 20, connectionStringName: _connectionStringName);
        if (!ret.IsLocked)
            throw new Exception($"RedisHostRegisterService获取缓存锁超时。key: __HostRegister:{ServiceName}");
        return ret;
    }
}