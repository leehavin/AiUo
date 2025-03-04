using AiUo.Configuration.Common;
using AiUo.Hosting;
using AiUo.Text;

namespace AiUo.Configuration;

/// <summary>
/// Host注册信息
///     ENV_HOST_IP => HostIp (HostPort根据配置选择)
///     ENV_HTTP_PORT => HttpPort(多个环境变量都可设置)
///     ENV_GRPC_PORT => GrpcPort 默认HttpPort+10000
/// </summary>
public class ServiceInfo
{
    #region Host
    public AiUoHostEndPoint HostEndPoint = new();
    /// <summary>
    /// 注册主机IP
    /// </summary>
    public string HostIp { get => HostEndPoint.Ip; set => HostEndPoint.Ip = value; }
    /// <summary>
    /// 注册主机端口
    /// </summary>
    public int HostPort { get => HostEndPoint.Port; set => HostEndPoint.Port = value; }
    public bool HostSecure { get => HostEndPoint.Secure; set => HostEndPoint.Secure = value; }
    public HostApiType HostApiType { get => HostEndPoint.ApiType; set => HostEndPoint.ApiType = value; }
    /// <summary>
    /// 服务外部访问地址
    /// </summary>
    public string HostUrl => HostEndPoint.ToString();
    #endregion

    public int HttpPort { get; set; }
    /// <summary>
    /// 默认HttpPort+10000
    /// </summary>
    public int GrpcPort { get; set; }
    /// <summary>
    /// 默认HttpPort+20000
    /// </summary>
    public int WebSocketPort { get; set; }

    /// <summary>
    /// 服务唯一ID
    /// </summary>
    public string SID { get; internal set; }
    /// <summary>
    /// 服务的唯一标识，默认: projectId:guid
    /// </summary>
    public string ServiceId { get; internal set; }

    public ServiceInfo(EnvironmentInfo env)
    {
        SID = ObjectId.NewId(env.StartUtcTime);
        HostIp = new HostIpGetter(env).Get();

        var portGetter = new HostPortGetter(env);
        HttpPort = portGetter.GetHttpPort();
        GrpcPort = portGetter.GetGrpcPort();
        WebSocketPort = portGetter.GetWebSocketPort();
    }
}