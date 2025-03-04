using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using AiUo.AspNet.Common;
using AiUo.Configuration;
using AiUo.Security;
using AiUo.Serialization;

namespace AiUo.AspNet;

/// <summary>
/// 当前上下文 (尽量使用生命周期中的HttpContext!)
/// </summary>
public static class HttpContextEx
{
    #region Base
    /// <summary>
    /// 当前Current
    /// </summary>
    public static HttpContext Current
    {
        get
        {
            var contextAccessor = DIUtil.GetRequiredService<IHttpContextAccessor>();
            return contextAccessor.HttpContext;
        }
    }

    public static HttpRequest Request => Current?.Request;
    public static HttpResponse Response => Current?.Response;
    public static ClaimsPrincipal User => Current?.User;
    /// <summary>
    /// 当前授权用户编码
    /// </summary>
    public static string IdentityUserId => User?.Identity?.Name;
    public static ConnectionInfo Connection => Current?.Connection;

    /// <summary>
    /// Current?.Items访问封装
    /// </summary>
    public static readonly HttpContextItemCollection Items = new HttpContextItemCollection();
    public static readonly HttpContextHeaderCollection RequestHeaders = new HttpContextHeaderCollection(true);
    public static readonly HttpContextHeaderCollection ResponseHeaders = new HttpContextHeaderCollection(false);
    #endregion

    #region Context
    private const string CONTEXT_KEY = "CONTEXT_KEY";

    /// <summary>
    /// 设置自定义上下文
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    public static void SetContext<T>(T context)
        where T : class
    {
        Items.AddOrUpdate(CONTEXT_KEY, context);
    }
    /// <summary>
    /// 获取自定义上下文
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static T GetContext<T>()
        where T : class
    {
        if (!Items.TryGet(CONTEXT_KEY, out T ret))
            throw new Exception($"获取自定义上下文Contexnt失败，没有初始化或类型错误! type:{typeof(T).GetType().FullName}");
        return ret;
    }
    #endregion

    public static string GetTraceId(this HttpContext context)
        => context?.TraceIdentifier?.ToString();

    #region Session
    public static ISession Session => Current?.Session;

    private static ISerializer _serializer = new TinyJsonSerializer();
    public static void SetSession(string key, object value, HttpContext context = null)
    {
        context ??= Current;
        var data = _serializer.Serialize(value);
        context.Session.Set(key, data);
    }
    public static T GetSessionOrDefault<T>(string key, T defaultValue, HttpContext context = null)
    {
        context ??= Current;
        if (!context.Session.TryGetValue(key, out var buffer))
            return defaultValue;
        return _serializer.Deserialize<T>(buffer);
    }
    #endregion

    #region Cookie Identity
    /// <summary>
    /// 使用cookie登录验证（需要配置SessionOrCookie）
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="enforceUse">是否强制使用,true:没有配置抛出异常</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task SignInUseCookie(string userId, bool enforceUse = false, HttpContext context = null)
    {
        context ??= Current;
        var section = ConfigUtil.GetSection<SessionAndCookieSection>();
        if (section == null || !section.UseCookieIdentity)
        {
            if (enforceUse)
                throw new Exception("使用cookie登录时，必须设置SessionOrCookieSection");
            return;
        }
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userId)
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.MaxValue,//cookie不过期
            IsPersistent = true,
        };

        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }

    /// <summary>
    /// cookie验证登出
    /// </summary>
    /// <returns></returns>
    public static async Task SignOutUseCookie(HttpContext context = null)
    {
        context ??= Current;
        var section = ConfigUtil.GetSection<SessionAndCookieSection>();
        if (section == null || !section.UseCookieIdentity)
        {
            return;
        }
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
    #endregion

    #region JWT
    private const string JWT_CONTEXT_KEY = "JWT_CONTEXT_KEY";
    public static JwtTokenInfo GetJwtToken(HttpContext context = null)
    {
        context ??= Current;
        if (context.Items.TryGetValue(JWT_CONTEXT_KEY, out var value))
            return (JwtTokenInfo)value;

        JwtTokenInfo ret = null;
        if (context.Request.Headers.TryGetValue("Authorization", out var auth))
        {
            var token = Convert.ToString(auth);
            if (!string.IsNullOrEmpty(token) && token.StartsWith("Bearer "))
            {
                token = token.Substring(7).Trim();
                if (!string.IsNullOrEmpty(token))
                    ret = JwtUtil.ReadJwtToken(token);
            }
        }
        context.Items.TryAdd(JWT_CONTEXT_KEY, ret);
        return ret;
    }
    internal static void SetJwtToken(JwtTokenInfo info, HttpContext context)
    {
        if (context.Items.ContainsKey(JWT_CONTEXT_KEY))
            context.Items[JWT_CONTEXT_KEY] = info;
        else
            context.Items.Add(JWT_CONTEXT_KEY, info);
    }
    #endregion
}