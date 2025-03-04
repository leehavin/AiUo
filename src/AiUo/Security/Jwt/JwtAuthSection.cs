namespace AiUo.Configuration;

/// <summary>
/// Api采用jwt验证的设置
/// </summary>
public class JwtAuthSection : ConfigSection
{
    public override string SectionName => "JwtAuth";

    /// <summary>
    /// 是否起效
    /// </summary>
    public bool Enabled { get; set; } = true;
    /// <summary>
    /// 签名秘钥,32长度密码
    /// NoiA32QqU0elJ0FW5qgnILF7M3WpP7fS
    /// </summary>
    public string SigningKey { get; set; } = "NoiA32QqU0elJ0FW5qgnILF7M3WpP7fS";

    public string Audience { get; set; } = "AiUo.com";
    public string Issuer { get; set; } = "AiUo.com";
    /// <summary>
    /// 是否验证过期
    /// </summary>
    public bool ValidateLifetime { get; set; } = false;
    /// <summary>
    /// Jwt Token过期时间(分钟）
    /// </summary>
    public int ExpireMinutes { get; set; } = int.MaxValue;

    /// <summary>
    /// Debug时默认的JwtToken或者UserId（仅ConfigUtil.IsDebugEnvironment时有效）
    /// </summary>
    public string DebugToken { get; set; }
}