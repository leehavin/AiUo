using Nacos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AiUo.Configuration;
using AiUo.Hosting.Services;

namespace AiUo.Extensions.Nacos;

public class NacosHostMicroService : IAiUoHostMicroService
{
    public const string HOST_API_TYPE_KEY = "aiuo.host_api_type";
    public const string HOST_URL = "aiuo.host_url";
    /// <summary>
    /// 获取所有服务名
    /// </summary>
    /// <returns></returns>
    public async Task<List<string>> GetAllServiceNames()
        => await new NacosOpenApiService().GetServices();

    public async Task<string> SelectOneServiceUrl(string serviceName)
    {
        var section = DIUtil.GetService<NacosSection>();
        var instance = await DIUtil.GetRequiredService<INacosNamingService>()
            .SelectOneHealthyInstance(serviceName, section.GroupName, true);
        if (instance == null)
            throw new Exception($"NacosHostMicroService.SelectOneServiceUrl时没有有效实例。serviceName:{serviceName}");
        //var secure = instance.Metadata.TryGetValue("secure", out var value)
        //    ? value.ToBoolean(false) : false;
        var url = instance.Metadata.TryGetValue(HOST_URL, out var v)
            ? v : null;
        return url;
        //return new AiUoHostEndPoint(apiType, instance.Ip, instance.Port, secure);
    }
}