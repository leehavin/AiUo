using AiUo.Configuration;

namespace AiUo.Extensions.Grpc;

/// <summary>
/// gRPC配置节
/// </summary>
public class GrpcSection : ConfigSection
{
    /// <summary>
    /// 是否启用gRPC
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// 服务端配置
    /// </summary>
    public ServerConfig Server { get; set; } = new ServerConfig();

    /// <summary>
    /// 客户端配置列表
    /// </summary>
    public Dictionary<string, ClientConfig> Clients { get; set; } = new Dictionary<string, ClientConfig>();

    public override string SectionName => "Grpc";

    /// <summary>
    /// 服务端配置
    /// </summary>
    public class ServerConfig
    { 
        /// <summary>
        /// 是否启用服务端
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// 监听地址，默认为空表示使用默认地址
        /// </summary>
        public string ListenAddress { get; set; } = string.Empty;

        /// <summary>
        /// 最大接收消息大小（字节），默认50MB
        /// </summary>
        public int? MaxReceiveMessageSize { get; set; } = 1024 * 1024 * 50;

        /// <summary>
        /// 最大发送消息大小（字节），默认50MB
        /// </summary>
        public int? MaxSendMessageSize { get; set; } = 1024 * 1024 * 50;

        /// <summary>
        /// 是否启用压缩
        /// </summary>
        public bool EnableCompression { get; set; } = false;
    }

    /// <summary>
    /// 客户端配置
    /// </summary>
    public class ClientConfig
    {
        /// <summary>
        /// 服务地址
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 是否使用SSL
        /// </summary>
        public bool UseSSL { get; set; } = false;

        /// <summary>
        /// 超时时间(秒)
        /// </summary>
        public int Timeout { get; set; } = 30;
    }
}