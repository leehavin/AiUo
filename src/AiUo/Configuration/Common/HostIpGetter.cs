using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AiUo.Net;

namespace AiUo.Configuration.Common;

internal class HostIpGetter
{
    private EnvironmentInfo _info;
    private List<string> _envs = new List<string>()
    {
        "ENV_HOST_IP"
    };
    public HostIpGetter(EnvironmentInfo info) 
    {
        _info = info;
    }
    public string Get()
    {
        string ret = null;
        // 1.env
        foreach (var env in _envs)
        {
            ret = Environment.GetEnvironmentVariable(env);
            if (!string.IsNullOrEmpty(ret))
                return ret;
        }

        // 2.aws ecs
        ret = GetECSHostIp().GetTaskResult();
        if (!string.IsNullOrEmpty(ret))
            return ret;

        // 4.本机
        if (!_info.IsRunningDocker)
        {
            // 3.ASPNETCORE_URLS
            if (!string.IsNullOrEmpty(_info.UrlsEndPoint?.Ip))
                return _info.UrlsEndPoint.Ip;
            ret = NetUtil.GetLocalIP();
        }
        return ret;
    }

    #region GetECSHostIp
    private async Task<string> GetECSHostIp()
    {
        string ret = null;
        try
        {
            var url = Environment.GetEnvironmentVariable("ECS_CONTAINER_METADATA_URI");
            if (!string.IsNullOrEmpty(url))
            {
                var client = new HttpClient();
                var rsp = await client.GetStringAsync($"{url}/task");
                if (!string.IsNullOrEmpty(rsp))
                {
                    var json = SerializerUtil.DeserializeJson<ECSMetaData>(rsp);
                    if (json?.Containers?.Count > 0)
                    {
                        var container = json.Containers.OrderByDescending(x => x.StartedAt).First();
                        ret = container?.Networks?.FirstOrDefault()?.IPv4Addresses?.FirstOrDefault();
                    }
                }
            }
        }
        catch
        {
        }
        return ret;
    }

    #region ECSMetaData
    class ECSMetaData
    {
        public List<ECSContainer> Containers { get; set; }
    }
    class ECSContainer
    {
        public string StartedAt { get; set; }
        public List<ECSNetworks> Networks { get; set; }
    }
    class ECSNetworks
    {
        public List<string> IPv4Addresses { get; set; }
    }
    #endregion
    #endregion
}