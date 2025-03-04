namespace AiUo.HCaptcha;

/// <summary>
/// hCaptcha验证请求参数
/// </summary>
public class HCaptchaVerifyReq
{
    public string Secret { get; set; }
    public string Response { get; set; }
    public string RemoteIp { get; set; }
}