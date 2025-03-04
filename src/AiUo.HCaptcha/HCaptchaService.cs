using AiUo.AspNet;
using AiUo.Configuration;
using AiUo.Logging;
using AiUo.Net;

namespace AiUo.HCaptcha;

public interface IHCaptchaService
{
    Task<ApiResult<HCaptchaVerifyRsp>> Verify(string token, string remoteIp = null);
}

internal class HCaptchaService : IHCaptchaService
{
    private HCaptchaSection _section;
    private HttpClientEx _client;
    public HCaptchaService()
    {
        _section = ConfigUtil.GetSection<HCaptchaSection>();
        _client = HttpClientExFactory.CreateClientEx(new HttpClientConfig
        {
            BaseAddress = _section.ApiBaseUrl,
        });
    }

    public async Task<ApiResult<HCaptchaVerifyRsp>> Verify(string token, string remoteIp = null)
    {
        var ret = new ApiResult<HCaptchaVerifyRsp>();
        var logger = LogUtil.GetContextLogger();
        try
        {
            var req = new HCaptchaVerifyReq
            {
                Secret = _section.Secret,
                Response = token,
                RemoteIp = _section.VerifyRemoteIp ? remoteIp : null
            };
            logger.AddField("HCaptchaVerify.req", req);
            var rsp = await _client.CreateAgent()
                .AddUrl("/siteverify")
                .BuildJsonContent(req)
                .PostAsync<HCaptchaVerifyRsp, object>();
            logger.AddField("HCaptchaVerify.rsp", rsp);
            if (!rsp.Success)
            {
                ret.Success = false;
                ret.Exception = rsp.Exception;
            }
            else
            {
                ret.Success = rsp.Success && rsp.SuccessResult.Success;
                ret.Result = rsp.SuccessResult;
            }
        }
        catch (Exception ex)
        {
            ret.Success = false;
            ret.Exception = ex;
        }
        if (!ret.Success)
        {
            logger.AddException(ret.Exception);
            logger.Save();
        }
        return ret;
    }
}