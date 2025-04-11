using AiUo.Collections;
using AiUo.Extensions.MQTT;
using AiUo.Extensions.MQTT.Server;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace AiUo.Configuration
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
        
        /// <summary>
        /// MQTT服务器配置
        /// </summary>
        public MQTTServerSection Server { get; set; } = new MQTTServerSection();

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
                
            // 绑定服务器配置
            Server = configuration?.GetSection("Server").Get<MQTTServerSection>() ?? new MQTTServerSection();
        }
    }
}

namespace AiUo.Extensions.MQTT
{
    public class MQTTConnectionStringElement
    {
        public string Name { get; internal set; }
        /// <summary>
        /// MQTT服务器地址
        /// </summary>
        public string Server { get; set; }
        /// <summary>
        /// MQTT服务器端口
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
        /// 是否使用TLS/SSL连接
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
        /// 通信超时时间（秒）
        /// </summary>
        public int CommunicationTimeout { get; set; } = 10;
        /// <summary>
        /// 重连延迟（秒）
        /// </summary>
        public int ReconnectDelay { get; set; } = 5;
        /// <summary>
        /// 是否允许不受信任的证书
        /// </summary>
        public bool AllowUntrustedCertificates { get; set; } = true;
        /// <summary>
        /// 是否忽略证书链错误
        /// </summary>
        public bool IgnoreCertificateChainErrors { get; set; } = true;
        /// <summary>
        /// 是否忽略证书吊销错误
        /// </summary>
        public bool IgnoreCertificateRevocationErrors { get; set; } = true;
        /// <summary>
        /// 客户端证书文件路径
        /// </summary>
        public string ClientCertificatePath { get; set; }
        /// <summary>
        /// 客户端证书密码
        /// </summary>
        public string ClientCertificatePassword { get; set; }
        /// <summary>
        /// 服务器证书指纹
        /// </summary>
        public string ServerCertificateFingerprint { get; set; }
        /// <summary>
        /// 是否启用消息持久化
        /// </summary>
        public bool EnablePersistence { get; set; } = false;
        /// <summary>
        /// 持久化存储目录
        /// </summary>
        public string PersistenceDirectory { get; set; }
    }
}