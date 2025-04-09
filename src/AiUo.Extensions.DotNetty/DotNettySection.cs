using AiUo.Configuration;
using Microsoft.Extensions.Configuration;

namespace AiUo.Extensions.DotNetty.Configuration
{
    /// <summary>
    /// DotNetty配置节
    /// </summary>
    public class DotNettySection : ConfigSection
    {
        public override string SectionName => "DotNetty";
        
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// 是否使用Libuv
        /// </summary>
        public bool UseLibuv { get; set; } = true;
        
        /// <summary>
        /// 协议类型：MQTT或WebSocket
        /// </summary>
        public string Protocol { get; set; } = "MQTT";
        
        /// <summary>
        /// 服务端口
        /// </summary>
        public int Port { get; set; } = 1883; // MQTT默认端口
        
        /// <summary>
        /// 读取空闲超时时间（秒），0表示服务器被动心跳
        /// </summary>
        public int ReadIdelTimeOut { get; set; } = 0;
        
        /// <summary>
        /// 连接超时时间（毫秒）
        /// </summary>
        public int ConnectTimeout { get; set; } = 5000;
        
        /// <summary>
        /// 排队数
        /// </summary>
        public int SoBacklog { get; set; } = 2048;
        
        /// <summary>
        /// 日志级别：TRACE,DEBUG,INFO,WARN,ERROR
        /// </summary>
        public string LogLevel { get; set; } = "INFO";
        
        /// <summary>
        /// 是否启用SSL
        /// </summary>
        public bool Ssl { get; set; } = false;
        
        /// <summary>
        /// SSL证书路径
        /// </summary>
        public string SslCer { get; set; } = "dotnetty.com.pfx";
        
        /// <summary>
        /// SSL证书密码
        /// </summary>
        public string SslPassword { get; set; } = "password";
        
        /// <summary>
        /// 是否启用接收事件
        /// </summary>
        public bool EnableReceiveEvent { get; set; } = false;
        
        /// <summary>
        /// 是否启用发送事件
        /// </summary>
        public bool EnableSendEvent { get; set; } = false;
        
        /// <summary>
        /// 是否启用关闭事件
        /// </summary>
        public bool EnableClosedEvent { get; set; } = false;
        
        /// <summary>
        /// 是否启用心跳事件
        /// </summary>
        public bool EnableHeartbeatEvent { get; set; } = true;
        
        /// <summary>
        /// 是否使用小端序
        /// </summary>
        public bool IsLittleEndian { get; set; } = false;
        
        /// <summary>
        /// 检查未登录Session的间隔时间（毫秒），小于等于0不检查
        /// </summary>
        public int CheckSessionInterval { get; set; } = 0;
        
        /// <summary>
        /// 未登录Session的超时时间（毫秒），防止空连接，小于等于0不检查
        /// </summary>
        public int CheckSessionTimeout { get; set; } = 5000;
        
        /// <summary>
        /// 是否自动加载
        /// </summary>
        public bool AutoLoad { get; set; } = true;
        
        /// <summary>
        /// 消息处理器所在的程序集
        /// </summary>
        public List<string> Assemblies { get; set; } = new List<string>();
        
        public override void Bind(IConfiguration configuration)
        {
            base.Bind(configuration);
            
            // 绑定程序集列表
            Assemblies = configuration?.GetSection("Assemblies").Get<List<string>>() ?? new List<string>();
        }
    }
}