namespace AiUo.Configuration;

public class HCaptchaSection : ConfigSection
{
    public override string SectionName => "HCaptcha";
    public bool Enabled { get; set; } = true;
    /// <summary>
    /// hCaptcha Site Key
    /// </summary>
    public string SiteKey { get; set; } = "9459ee20-43d9-4777-af5d-b1fbe89b403e";

    /// <summary>
    /// hCaptcha Site Secret
    /// </summary>
    public string Secret { get; set; } = "ES_01c1e11250f54abc8d90248733f370db";

    /// <summary>
    /// hCaptcha 基本 URL
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://api.hcaptcha.com/";

    /// <summary>
    /// 是否验证客户端IP
    /// </summary>
    public bool VerifyRemoteIp { get; set; } = true;
}