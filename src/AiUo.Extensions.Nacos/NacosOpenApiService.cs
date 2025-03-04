using Nacos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AiUo.Configuration;
using AiUo.Net;

namespace AiUo.Extensions.Nacos;

public class NacosOpenApiService
{
    #region Properties
    private INacosNamingService _namingService;
    public const string HTTP_CLIENT_NAME = "NacosOpenApi";
    private NacosSection _section;
    private string _serverUrl;
    public NacosOpenApiService()
    {
        _namingService = DIUtil.GetService<INacosNamingService>();
        _section = DIUtil.GetService<NacosSection>();
        _serverUrl = _section.ServerAddresses.First();
    }

    private JsonHttpClient GetClient()
    {
        var client = DIUtil.GetRequiredService<IHttpClientFactory>()
            .CreateClient(HTTP_CLIENT_NAME);
        var ret = new JsonHttpClient(client);
        ret.SetBaseAddress(_serverUrl);
        return ret;
    }
    #endregion

    #region 修改实例
    //public async Task SetInstanceEnabled()
    //{
    //    var rsp = await GetInstanceClient()
    //        .AddQuery("enabled", "true")
    //        .PutAsync<string>();
    //    if (!rsp.Success)
    //        throw new Exception("NacosOpenApiService.SetInstanceEnabled失败");
    //}
    public async Task SetInstanceMetadata(string key, string value)
    {
        var rsp = await GetInstanceClient()
            .AddQuery("metadata", $"{key}={value}")
            .PutAsync<string>();
        if (!rsp.Success)
            throw new Exception("NacosOpenApiService.SetInstanceMetadata失败");
    }
    private JsonHttpClient GetInstanceClient()
    {
        return GetClient()
            .AddUrl("/nacos/v1/ns/instance")
            .AddQuery("serviceName", _section.ServiceName)
            .AddQuery("groupName", _section.GroupName)
            .AddQuery("ip", _section.Ip)
            .AddQuery("port", _section.Port)
            .AddQuery("clusterName", _section.ClusterName)
            .AddQuery("namespaceId", _section.Namespace);
    }
    #endregion

    #region 查询实例
    /// <summary>
    /// 查询实例详情
    /// </summary>
    /// <returns></returns>
    public async Task<NacosInstanceInfo> GetInstanceInfo()
    {
        var rsp = await GetClient()
            .AddUrl("/nacos/v1/ns/instance")
            .AddQuery("serviceName", _section.ServiceName)
            .AddQuery("groupName", _section.GroupName)
            .AddQuery("ip", _section.Ip)
            .AddQuery("port", _section.Port.ToString())
            .AddQuery("namespaceId", _section.Namespace)
            .AddQuery("cluster", _section.ClusterName)
            .GetAsync<NacosInstanceInfo>();
        if (!rsp.Success)
            throw new Exception("NacosOpenApiService.GetInstanceInfo失败");
        return rsp.Result;
    }
    public async Task<string> GetInstanceMetadata(string key)
    {
        var result = await GetInstanceInfo();
        return result.Metadata?.TryGetValue(key, out var value) ?? false ? value : null;
    }
    #endregion

    #region 服务
    public async Task<List<string>> GetServices()
    {
        var ret = new List<string>();
        var idx = 0;
        var pageSize = 20;
        var pageCount = 0;
        while (true)
        {
            idx++;
            var rsp = await GetClient()
                .AddUrl("/nacos/v1/ns/service/list")
                .AddQuery("pageNo", idx)
                .AddQuery("pageSize", pageSize)
                .AddQuery("groupName", _section.GroupName)
                .AddQuery("namespaceId", _section.Namespace)
                .GetAsync<ServiceListInfo>();
            if (!rsp.Success)
                throw new Exception("NacosOpenApiService.GetServices失败");
            if (rsp.Result?.Doms?.Count > 0)
            {
                if (pageCount == 0)
                    pageCount = AiUoUtil.GetPageCount(rsp.Result.Count, pageSize);
                ret.AddRange(rsp.Result.Doms);
            }
            else
                break;
            if (idx == pageCount)
                break;
        }
        return ret;
    }
    class ServiceListInfo
    {
        public int Count { get; set; }
        public List<string> Doms { get; set; }
    }
    #endregion
}
public class NacosInstanceInfo
{
    public string InstanceId { get; set; }
    public string Ip { get; set; }
    public int Port { get; set; }
    public string ClusterName { get; set; }
    public string Service { get; set; }
    public double Weight { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
    public bool Healthy { get; set; }
}

public class NacosServiceInfo
{

}