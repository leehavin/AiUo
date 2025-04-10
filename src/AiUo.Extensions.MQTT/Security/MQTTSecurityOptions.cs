namespace AiUo.Extensions.MQTT.Security;

/// <summary>
/// MQTT安全选项
/// </summary>
public class MQTTSecurityOptions
{
    /// <summary>
    /// 是否使用TLS/SSL连接
    /// </summary>
    public bool UseTls { get; set; }

    /// <summary>
    /// 是否允许不受信任的证书
    /// </summary>
    public bool AllowUntrustedCertificates { get; set; }

    /// <summary>
    /// 是否忽略证书链错误
    /// </summary>
    public bool IgnoreCertificateChainErrors { get; set; }

    /// <summary>
    /// 是否忽略证书吊销错误
    /// </summary>
    public bool IgnoreCertificateRevocationErrors { get; set; }

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
}