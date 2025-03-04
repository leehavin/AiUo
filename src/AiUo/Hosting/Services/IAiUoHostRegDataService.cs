using System.Collections.Generic;
using System.Threading.Tasks;
using AiUo.Caching;

namespace AiUo.Hosting.Services;

public interface IAiUoHostDataService
{
    Task<List<string>> GetAllServiceIds(string connectionStringName = null);
    Task<List<string>> GetServiceIds(string serviceName = null, string connectionStringName = null);
    Task SetHostData<T>(string key, T value, string serviceId = null, string connectionStringName = null);
    Task<CacheValue<T>> GetHostData<T>(string key, string serviceId = null, string connectionStringName = null);
}