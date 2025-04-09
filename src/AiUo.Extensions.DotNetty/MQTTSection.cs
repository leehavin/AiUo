using AiUo.Collections;
using AiUo.Configuration;
using Microsoft.Extensions.Configuration;
using AiUo.Extensions.MQTT;

namespace AiUo.Extensions.DotNetty.Configuration
{
    public class MQTTSection : ConfigSection
    {
        public override string SectionName => "MQTT";
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// 是否开启消息消费日志
        /// </summary>
        public bool MessageLogEnabled { get; set; }
        /// <summary>
        /// 是否开启调试日志
        /// </summary>
        public bool DebugLogEnabled { get; set; }
        /// <summary>
        /// 是否开启Consumer消费
        /// </summary>
        public bool ConsumerEnabled { get; set; } = true;
        public string DefaultConnectionStringName { get; set; }
        public Dictionary<string, MQTTConnectionStringElement> ConnectionStrings = new();

        public bool AutoLoad { get; set; }
        /// <summary>
        /// 消息处理器所在的程序集,用于消费注册
        /// </summary>
        public List<string> ConsumerAssemblies { get; set; } = new List<string>();
        public override void Bind(IConfiguration configuration)
        {
            base.Bind(configuration);
            ConnectionStrings = configuration.GetSection("ConnectionStrings")
                .Get<Dictionary<string, MQTTConnectionStringElement>>() ?? new();
            ConnectionStrings.ForEach(x =>
            {
                x.Value.Name = x.Key;
            });

            if (string.IsNullOrEmpty(DefaultConnectionStringName) && ConnectionStrings.Count == 1)
                DefaultConnectionStringName = ConnectionStrings.First().Key;
            // Assemblies
            ConsumerAssemblies = configuration?.GetSection("ConsumerAssemblies").Get<List<string>>()
                ?? new List<string>();
        }
    }
}

namespace AiUo.Extensions.DotNetty
{
    public class MQTTConnectionStringElement
    {
        public string Name { get; internal set; }
        /// <summary>
        /// 服务器地址
        /// </summary>
        public string Server { get; set; }
        /// <summary>
        /// 服务器端口
        /// </summary>
        public int Port { get; set; } = 1883;
        /// <summary>
        /// 客户端ID
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 是否使用TLS/SSL
        /// </summary>
        public bool UseTls { get; set; }
        /// <summary>
        /// 是否清除会话
        /// </summary>
        public bool CleanSession { get; set; } = true;
        /// <summary>
        /// 保持连接的时间间隔（秒）
        /// </summary>
        public int KeepAlivePeriod { get; set; } = 60;
        /// <summary>
        /// 连接超时时间（秒）
        /// </summary>
        public int ConnectionTimeout { get; set; } = 30;
    }
}