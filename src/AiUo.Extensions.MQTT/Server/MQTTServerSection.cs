using System.Collections.Generic;

namespace AiUo.Extensions.MQTT.Server;

/// <summary>
/// MQTT服务器配置节
/// </summary>
public class MQTTServerSection
{
    /// <summary>
    /// 是否启用MQTT服务器
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// 服务器监听端口
    /// </summary>
    public int Port { get; set; } = 1883;

    /// <summary>
    /// 是否使用TLS/SSL
    /// </summary>
    public bool UseTls { get; set; } = false;

    /// <summary>
    /// TLS/SSL端口
    /// </summary>
    public int TlsPort { get; set; } = 8883;

    /// <summary>
    /// 证书文件路径
    /// </summary>
    public string CertificatePath { get; set; }

    /// <summary>
    /// 证书密码
    /// </summary>
    public string CertificatePassword { get; set; }

    /// <summary>
    /// 是否启用认证
    /// </summary>
    public bool EnableAuthentication { get; set; } = false;

    /// <summary>
    /// 是否启用订阅授权
    /// </summary>
    public bool EnableSubscriptionAuthorization { get; set; } = false;

    /// <summary>
    /// 是否启用发布授权
    /// </summary>
    public bool EnablePublishAuthorization { get; set; } = false;

    /// <summary>
    /// 未授权订阅时是否关闭连接
    /// </summary>
    public bool CloseConnectionOnUnauthorizedSubscription { get; set; } = false;

    /// <summary>
    /// 未授权发布时是否关闭连接
    /// </summary>
    public bool CloseConnectionOnUnauthorizedPublish { get; set; } = false;

    /// <summary>
    /// 最大并发客户端连接数
    /// </summary>
    public int MaximumConnections { get; set; } = 100;

    /// <summary>
    /// 用户凭据列表，格式为 "用户名:密码"
    /// </summary>
    public List<string> Credentials { get; set; } = new List<string>();

    /// <summary>
    /// 主题访问控制列表，格式为 "客户端ID:主题1,主题2,..."
    /// </summary>
    public List<string> TopicAccessControl { get; set; } = new List<string>();
}