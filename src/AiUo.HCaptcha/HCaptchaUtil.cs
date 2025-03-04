using AiUo.AspNet;

namespace AiUo.HCaptcha;

public static class HCaptchaUtil
{
    /// <summary>
    /// 验证HCaptcha返回的token
    /// </summary>
    /// <param name="token"></param>
    /// <param name="remoteIp"></param>
    /// <returns></returns>
    public static async Task<ApiResult<HCaptchaVerifyRsp>> Verify(string token, string remoteIp = null)
    {
        return await DIUtil.GetRequiredService<IHCaptchaService>().Verify(token,remoteIp);
    }
}