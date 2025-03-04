using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using AiUo.Configuration;
using AiUo.Logging;
using AiUo.Net;
using AiUo.Security;

namespace AiUo.AspNet;

public class TinyJwtBearerEvents : JwtBearerEvents
{
    protected ILogger<TinyJwtBearerEvents> Logger;
    public TinyJwtBearerEvents()
    {
        Logger = LogUtil.CreateLogger<TinyJwtBearerEvents>();
    }
    /// <summary>
    /// 在第一次收到协议消息时调用
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public override Task MessageReceived(MessageReceivedContext context)
    {
        var section = ConfigUtil.GetSection<JwtAuthSection>();
        if (string.IsNullOrEmpty(context.Token)
            && !string.IsNullOrEmpty(section.DebugToken)
            && ConfigUtil.Environment.IsDebug
            && !context.Request.Headers.ContainsKey("Authorization")
           )
        {
            if (section.DebugToken.Length < 100)
                context.Token = JwtUtil.CreateJwtToken(section.DebugToken);
            else if (JwtUtil.ReadJwtToken(section.DebugToken).Status == JwtTokenStatus.Success)
                context.Token = section.DebugToken;
        }
        return base.MessageReceived(context);
    }
    /// <summary>
    /// 在将质询发送回调用方之前调用
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public override Task Challenge(JwtBearerChallengeContext context)
    {
        if (context.AuthenticateFailure is SecurityTokenExpiredException)
        {
            context.HttpContext.Items.Add(GlobalExceptionUtil.ERROR_CODE_KEY, GResponseCodes.G_JWT_TOKEN_EXPIRED);
            context.HttpContext.Items.Add(GlobalExceptionUtil.ERROR_MESSAGE_KEY, "jwt token过期");
        }
        else
        {
            var token = context.Request.Headers["Authorization"];
            context.HttpContext.Items.Add(GlobalExceptionUtil.ERROR_CODE_KEY, GResponseCodes.G_JWT_TOKEN_INVALID);
            context.HttpContext.Items.Add(GlobalExceptionUtil.ERROR_MESSAGE_KEY, $"jwt token无效: {Convert.ToString(token)}");
        }
        return base.Challenge(context);
    }
    /// <summary>
    /// 在安全令牌通过验证并生成 ClaimsIdentity 后调用
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public override Task TokenValidated(TokenValidatedContext context)
    {
        var token = context.SecurityToken as JwtSecurityToken;
        if (token != null)
        {
            var jwt = JwtUtil.ReadJwtToken(context.Principal);
            HttpContextEx.SetJwtToken(jwt, context.HttpContext);
        }
        return base.TokenValidated(context);
    }
    public override Task AuthenticationFailed(AuthenticationFailedContext context)
    {
        return base.AuthenticationFailed(context);
    }
    public override Task Forbidden(ForbiddenContext context)
    {
        return base.Forbidden(context);
    }
}