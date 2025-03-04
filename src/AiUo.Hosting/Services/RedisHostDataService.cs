using AiUo.Caching;
using AiUo.Configuration;
using AiUo.Extensions.StackExchangeRedis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiUo.Hosting.Services;

public class RedisHostDataService : IAiUoHostDataService
{
    public async Task<List<string>> GetAllServiceIds(string connectionStringName = null)
    {
        var ret = new List<string>();
        var namesDCache = RedisUtil.CreateSetClient<string>(RedisHostRegisterProvider.HOST_NAMES_KEY, connectionStringName, true);
        var serviceNames = (await namesDCache.GetAllAsync()).ToList();
        foreach (var serviceName in serviceNames)
        {
            var idsDCache = RedisUtil.CreateSetClient<string>($"{RedisHostRegisterProvider.HOST_IDS_KEY}:{serviceName}", connectionStringName, true);
            var serviceIds = (await idsDCache.GetAllAsync()).ToList();
            foreach (var serviceId in serviceIds)
            {
                var status = await new AiUoHostDataDCache(serviceId, connectionStringName).GetStatus();
                if (status != AiUoHostDataStatus.Actived)
                    await idsDCache.RemoveAsync(serviceId);
                else
                    ret.Add(serviceId);
            }
        }
        return ret;
    }
    public async Task<List<string>> GetServiceIds(string serviceName = null, string connectionStringName = null)
    {
        serviceName ??= ConfigUtil.Project.ProjectId;
        if (string.IsNullOrEmpty(serviceName))
            throw new Exception($"RedisHostDataService.GetServiceIds时serviceName不能为空。serviceName:{serviceName}");
        var idsDCache = RedisUtil.CreateSetClient<string>($"{RedisHostRegisterProvider.HOST_IDS_KEY}:{serviceName}", connectionStringName, true);
        return (await idsDCache.GetAllAsync()).ToList();
    }

    public async Task SetHostData<T>(string key, T value, string serviceId = null, string connectionStringName = null)
    {
        serviceId ??= ConfigUtil.Service.ServiceId;
        if (string.IsNullOrEmpty(serviceId) || string.IsNullOrEmpty(key))
            throw new Exception($"RedisHostDataService.SetHostData时ServiceId和Key不能为空。serviceId:{serviceId} key:{key}");
        var dcache = new AiUoHostDataDCache(serviceId, connectionStringName);
        await dcache.SetData(key, value);
    }
    public async Task<CacheValue<T>> GetHostData<T>(string key, string serviceId = null, string connectionStringName = null)
    {
        serviceId ??= ConfigUtil.Service.ServiceId;
        if (string.IsNullOrEmpty(serviceId) || string.IsNullOrEmpty(key))
            throw new Exception($"RedisHostDataService.GetHostData时ServiceId和Key不能为空。serviceId:{serviceId} key:{key}");
        var dcache = new AiUoHostDataDCache(serviceId, connectionStringName);
        return await dcache.GetData<T>(key);
    }
}