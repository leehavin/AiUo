using System;
using System.Threading.Tasks;
using AiUo.Configuration;
using AiUo.Logging;
using AiUo.Net;
using AiUo.OAuth.Common;

namespace AiUo.OAuth.Providers;

internal interface IOAuthProvider
{
    OAuthProviders Provider { get; }
    Task<string> GetOAuthUrl(string redirectUri, string uuid);
    Task<ResponseResult<OAuthUserDto>> GetUserInfo(OAuthUserIpo ipo);
}

internal abstract class BaseOAuthProvider : IOAuthProvider
{
    public abstract OAuthProviders Provider { get; }
    protected abstract string OAuthUrl { get; }
    protected abstract string TokenUrl { get; }
    protected abstract string UserInfoUrl { get; }

    public IOAuthProviderElement Config { get; }
    private OAuthDCache _dcache;
    private HttpClientEx _client;

    public BaseOAuthProvider()
    {
        var section = ConfigUtil.GetSection<OAuthSection>();
        Config = section.GetProviderElement(Provider);
        _dcache = new();
        _client = HttpClientExFactory.CreateClientEx(GetType().FullName);
    }

    protected abstract string AppendOAuthUrl();
    public async Task<string> GetOAuthUrl(string redirectUri, string uuid)
    {
        var state = StringUtil.GetGuidString();
        await _dcache.SetState(state, uuid);
        var ret = $"{OAuthUrl}?response_type=code%20token&client_id={Config.ClientId}&redirect_uri={redirectUri}&state={state}";
        var append = AppendOAuthUrl();
        if (!string.IsNullOrEmpty(append))
            ret += $"&{append.TrimStart('&')}";
        return ret;
    }

    protected abstract string AppendUserUrl();

    protected async Task<ResponseResult<OAuthUserDto>> RequestUser<TSuccess, TError>(OAuthUserIpo ipo, Func<TSuccess, OAuthUserDto> convertFunc)
    {
        var ret = new ResponseResult<OAuthUserDto>();

        var log = LogUtil.GetContextLogger();
        log.AddField("oauth.ipo", SerializerUtil.SerializeJson(ipo));
        var value = await _dcache.GetState(ipo.State);
        log.AddField("oauth.uuid", value);
        if (string.IsNullOrEmpty(value) || (!string.IsNullOrEmpty(ipo.Uuid) && value != "default" && ipo.Uuid != value))
        {
            ret.Success = false;
            ret.Message = "OAuth请求异常";
            return ret;
        }

        var rsp = await _client.CreateAgent()
            .AddUrl($"{UserInfoUrl}?access_token={ipo.AccessToken}")
            .AppendUrl(AppendUserUrl())
            .GetAsync<TSuccess, TError>();
        log.AddField("oauth.rsp", SerializerUtil.SerializeJson(rsp));
        if (rsp.Success)
        {
            ret.Success = true;
            ret.Result = convertFunc(rsp.SuccessResult);
            log.AddField("oauth.result", SerializerUtil.SerializeJson(ret.Result));
        }
        else
        {
            ret.Success = false;
            ret.Exception = rsp.Exception;
            ret.Message = rsp.ResultString;
            log.AddField("oauth.resultString", rsp.ResultString);
            log.AddException(rsp.Exception);
        }
        if (!log.IsContext)
            log.SetCategoryName("OAUTH").Save();
        return ret;
    }

    public abstract Task<ResponseResult<OAuthUserDto>> GetUserInfo(OAuthUserIpo ipo);
}