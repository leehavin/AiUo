using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace AiUo.HCaptcha;

/// <summary>
/// hCaptcha验证返回
/// </summary>
/// <remarks>https://docs.hcaptcha.com/#server</remarks>
public class HCaptchaVerifyRsp
{
    /// <summary>
    /// 是否成功
    /// </summary>
    [JsonPropertyName("success")]
    [JsonProperty("success")]
    public bool Success { get; set; }

    /// <summary>
    /// 验证码的时间戳（ISO 格式 yyyy-MM-dd'T'HH:mm:ssZZ）
    /// </summary>
    [JsonPropertyName("challenge_ts")]
    [JsonProperty("challenge_ts")]
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// 通过验证码的站点的主机名
    /// the hostname of the site where the captcha was solved
    /// </summary>
    [JsonPropertyName("hostname")]
    [JsonProperty("hostname")]
    public string Hostname { get; set; }

    /// <summary>
    /// 可选：是否将响应记入贷方(credited)
    /// </summary>
    [JsonPropertyName("credit")]
    [JsonProperty("credit")]
    public bool Credit { get; set; }

    /// <summary>
    /// 基于字符串的错误代码数组
    /// </summary>
    [JsonPropertyName("error-codes")]
    [JsonProperty("error-codes")]
    public string[] ErrorCodes { get; set; }

    /// <summary>
    /// ENTERPRISE 特征：表示恶意活动的分数
    /// </summary>
    [JsonPropertyName("score")]
    [JsonProperty("score")]
    public double? Score { get; set; }

    /// <summary>
    /// ENTERPRISE 特征：得分原因
    /// </summary>
    [JsonPropertyName("score_reason")]
    [JsonProperty("score_reason")]
    public string[] ScoreReason { get; set; }
}