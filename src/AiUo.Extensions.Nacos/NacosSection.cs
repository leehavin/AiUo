using Microsoft.Extensions.Configuration;
using Nacos.AspNetCore.V2;
using AiUo.Hosting;

namespace AiUo.Configuration;

public class NacosSection : NacosAspNetOptions
{
    public bool Enabled { get; set; }
    public string RegisterApiType { get; set; }
    private HostApiType? _apiType;
    /// <summary>
    /// 注册主机时API类型: Http,Grpc,WebSocket
    /// </summary>
    public HostApiType ApiType
    {
        get
        {
            if(!_apiType.HasValue)
                _apiType = RegisterApiType.ToEnum(HostApiType.Http);
            return _apiType.Value;
        }
    }

    /// <summary>
    /// 负载均衡策略 true: 选第一次成功 false: 轮询
    /// </summary>
    public bool LBPickFirst { get; set; } = true;
    /// <summary>
    /// 负载均衡刷新时间,默认30秒
    /// </summary>
    public int LBRefreshInterval { get; set; } = 30000;

    public void Bind(IConfiguration configuration)
    {
        configuration?.Bind(this);
        if (LBRefreshInterval <= 3000)
            LBRefreshInterval = 30000;
    }
}