using Microsoft.Extensions.Configuration;
using AiUo.Hosting;

namespace AiUo.Configuration;

public class HostSection : ConfigSection
{
    public override string SectionName => "Host";

    /// <summary>
    /// 是否注册Redis Host服务(RedisHostRegisterProvider,RedisHostDataService,RedisHostMicroService)
    /// </summary>
    public bool RegisterEnabled { get; set; }
    public string RegisterApiType { get; set; }
    private HostApiType? _apiType;
    /// <summary>
    /// 注册主机时API类型: Http,Grpc,WebSocket
    /// </summary>
    public HostApiType ApiType
    {
        get
        {
            if (!_apiType.HasValue)
                _apiType = RegisterApiType.ToEnum(HostApiType.Http);
            return _apiType.Value;
        }
    }
    /// <summary>
    /// 主机注册心跳间隔，默认5秒最小1秒
    /// </summary>
    public int HeartbeatInterval { get; set; } = 5000;
    /// <summary>
    /// 主机检查间隔，默认1分钟最小10秒
    /// </summary>
    public int HeathInterval { get; set; } = 60000;

    /// <summary>
    /// 主机Timer最小Delay时间, 默认200最小100
    /// </summary>
    public int TimerMinDelay { get; set; } = 200;
    /// <summary>
    /// 主机Timer关闭等待超时，默认20秒最小5秒
    /// </summary>
    public int TimerWaitTimeout { get; set; } = 20000;

    /// <summary>
    /// 关机timeout时间(秒)
    /// </summary>
    public int ShutdownTimeout { get; set; } = 30;

    public override void Bind(IConfiguration configuration)
    {
        base.Bind(configuration);

        if (HeartbeatInterval <= 1000)
            HeartbeatInterval = 1000;
        if (HeathInterval <= 10000)
            HeathInterval = 10000;
        if (TimerMinDelay <= 100)
            TimerMinDelay = 100;
        if (TimerWaitTimeout <= 5000)
            TimerWaitTimeout = 5000;
        if (ShutdownTimeout <= 30)
            ShutdownTimeout = 30;
    }
}