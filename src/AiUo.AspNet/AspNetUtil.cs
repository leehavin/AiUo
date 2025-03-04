using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text;
using AiUo.Configuration;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;

namespace AiUo.AspNet;

public static class AspNetUtil
{
    #region RemoteIpAddress
    /// <summary>
    /// 获取客户端IP需要 services.AddSingleton(HttpContextAccessor);
    /// </summary>
    /// <returns></returns>
    public static string GetRemoteIpString(HttpContext context = null)
    {
        return GetRemoteIpAddress(context)?.ToString();
    }

    /// <summary>
    /// 获取客户端IP需要 services.AddSingleton(HttpContextAccessor);
    /// </summary>
    /// <returns></returns>
    public static IPAddress GetRemoteIpAddress(HttpContext context = null)
    {
        context ??= HttpContextEx.Current;
        var headers = context.Request.Headers;
        if (headers.ContainsKey("X-Forwarded-For"))
        {
            var ips = Convert.ToString(headers["X-Forwarded-For"]);
            if (!string.IsNullOrEmpty(ips))
            {
                var ip = ips.Split(',', StringSplitOptions.RemoveEmptyEntries)[0];
                if (IPAddress.TryParse(ip, out var ret))
                    return ret;
            }
        }
        return context.Connection.RemoteIpAddress?.MapToIPv4();
    }
    #endregion

    #region Referer Url
    /// <summary>
    /// 获取Referer路径
    /// </summary>
    /// <returns></returns>
    public static string GetRefererUrl(HttpContext context = null)
    {
        context ??= HttpContextEx.Current;
        return context.Request.Headers.TryGetValue("Referer", out var ret)
            ? ret.ToString() : null;
    }
    public static Uri GetRefererUri(HttpContext context = null)
    {
        var url = GetRefererUrl(context);
        return url != null ? new Uri(url) : null;
    }
    /// <summary>
    /// 获取Referer基础路径，如: https://www.abc.com
    /// </summary>
    /// <param name="hasPort"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetRefererBaseUrl(bool hasPort = false, HttpContext context = null)
        => GetBaseUrl(GetRefererUrl(context), hasPort);
    /// <summary>
    /// 获取Referer一级域名，如: abc.com
    /// </summary>
    /// <returns></returns>
    public static string GetRefererTopDomain(HttpContext context = null)
    {
        var uri = GetRefererUri(context);
        return uri != null
            ? string.Join('.', uri.Host.Split('.').TakeLast(2))
            : null;
    }
    /// <summary>
    /// 获取相对Referer域名的二级域名,如：https://xxx.abc.com
    /// </summary>
    /// <param name="secondDomain"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetRefererSecondDomainUrl(string secondDomain, HttpContext context = null)
    {
        var uri = GetRefererUri(context);
        if (uri == null) return null;
        var topDomain = string.Join('.', uri.Host.Split('.').TakeLast(2));
        return $"{uri.Scheme}://{secondDomain}.{topDomain}";
    }

    /// <summary>
    /// 获取基础url，如:https://www.abc.com
    /// </summary>
    /// <param name="url"></param>
    /// <param name="hasPort"></param>
    /// <returns></returns>
    public static string GetBaseUrl(string url, bool hasPort = false)
    {
        if (string.IsNullOrEmpty(url)) return null;
        var uri = new Uri(url);
        var domain = hasPort
            ? $"{uri.Host}:{uri.Port}"
            : uri.Host;
        return $"{uri.Scheme}://{domain}";
    }
    #endregion

    #region Request Url
    /// <summary>
    /// 获得当前请求的主机URL（只包含Scheme,Host和端口）
    /// </summary>
    /// <param name="hasPort">是否包含端口</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetRequestBaseUrl(bool hasPort = false, HttpContext context = null)
    {
        context ??= HttpContextEx.Current;
        var schema = GetRequestSchema(context);
        var domain = hasPort
            ? context.Request.Host.Value
            : context.Request.Host.Host;
        return $"{schema}://{domain}";
    }

    /// <summary>
    /// 获取当前请求的顶级域名（有可能带端口），如：exemple.com
    /// </summary>
    /// <param name="hasPort">是否包含端口</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetRequestTopDomain(bool hasPort = false, HttpContext context = null)
    {
        context ??= HttpContextEx.Current;
        var host = hasPort
            ? context.Request.Host.Value
            : context.Request.Host.Host;
        return string.Join('.', host.Split('.').TakeLast(2));
    }

    /// <summary>
    /// 获取当前请求指定二级域名URL，如：http://xxx.exemple.com
    /// </summary>
    /// <param name="secondDomain"></param>
    /// <param name="hasPort">是否包含端口</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetRequestSecondDomainUrl(string secondDomain, bool hasPort = false, HttpContext context = null)
    {
        var schema = GetRequestSchema(context);
        return $"{schema}://{secondDomain}.{GetRequestTopDomain(hasPort, context)}";
    }
    private static string GetRequestSchema(HttpContext context = null)
    {
        context ??= HttpContextEx.Current;
        var ret = context.Request.Headers.ContainsKey("X-Forwarded-Proto")
            ? context.Request.Headers["X-Forwarded-Proto"].FirstOrDefault() : null;
        if (ret == null)
            ret = context.Request.Scheme;
        return ret;
    }
    #endregion

    #region CONTEXT_ITEM_SUCCESS_CODE
    internal const string CONTEXT_ITEM_SUCCESS_CODE = "CONTEXT_ITEM_SUCCESS_CODE";
    /// <summary>
    /// 设置api成功的code码
    /// </summary>
    /// <param name="code"></param>
    /// <param name="context"></param>
    public static void SetSuccessCode(string code, HttpContext context = null)
    {
        context ??= HttpContextEx.Current;
        context.Items.TryAdd(CONTEXT_ITEM_SUCCESS_CODE, code);
    }
    internal static bool TryGetSuccessCode(HttpContext context, out string code)
    {
        var ret = context.Items.TryGetValue(CONTEXT_ITEM_SUCCESS_CODE, out var value);
        code = ret ? Convert.ToString(value) : null;
        return ret;
    }
    #endregion

    #region CONTEXT_ITEM_UNHANDLED_EXCEPTION
    internal const string CONTEXT_ITEM_UNHANDLED_EXCEPTION = "CONTEXT_ITEM_UNHANDLED_EXCEPTION";
    /// <summary>
    /// 设置api未处理异常的统一code码
    /// </summary>
    /// <param name="code"></param>
    /// <param name="context"></param>
    public static void SetUnhandledExceptionCode(string code, HttpContext context = null)
    {
        context ??= HttpContextEx.Current;
        context.Items.TryAdd(CONTEXT_ITEM_UNHANDLED_EXCEPTION, code);
    }
    internal static bool TryGetUnhandledExceptionCode(HttpContext context, out string code)
    {
        var ret = context.Items.TryGetValue(CONTEXT_ITEM_UNHANDLED_EXCEPTION, out var value);
        code = ret ? Convert.ToString(value) : null;
        return ret;
    }
    #endregion

    #region CONTEXT_ITEM_RESPONSE_EXCEPTION_DETAIL
    /// <summary>
    /// 设置api是否返回异常详细信息
    /// </summary>
    internal const string CONTEXT_ITEM_RESPONSE_EXCEPTION_DETAIL = "CONTEXT_ITEM_RESPONSE_EXCEPTION_DETAIL";
    public static void SetResponseExceptionDetail(bool detail = false, HttpContext context = null)
    {
        context ??= HttpContextEx.Current;
        context.Items.TryAdd(CONTEXT_ITEM_RESPONSE_EXCEPTION_DETAIL, detail);
    }
    internal static bool GetResponseExceptionDetail(HttpContext context)
    {
        return context.Items.ContainsKey(CONTEXT_ITEM_RESPONSE_EXCEPTION_DETAIL)
            ? (bool)context.Items[CONTEXT_ITEM_RESPONSE_EXCEPTION_DETAIL]
            : ConfigUtil.Project.ResponseErrorDetail;
    }
    #endregion

    #region CONTEXT_REQUEST_BODY_STRING
    internal const string CONTEXT_REQUEST_BODY_STRING = "CONTEXT_REQUEST_BODY_STRING";
    /// <summary>
    /// 获取原始请求正文并转换成字符串
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task<string> GetRequestBodyAsync(HttpContext context)
    {
        if (context.Items.TryGetValue(CONTEXT_REQUEST_BODY_STRING, out var value))
            return Convert.ToString(value);

        context.Request.Body.Seek(0, SeekOrigin.Begin);
        // 方案1
        //var ret = await IOUtil.GetStringFromPipe(context.Request.BodyReader);
        // 方案2
        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var ret = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;
        //context.Request.Body.Seek(0, SeekOrigin.Begin);

        context.Items.TryAdd(CONTEXT_REQUEST_BODY_STRING, ret);
        return ret;
    }
    #endregion

    #region CORS
    public static Action<CorsPolicyBuilder> GetPolicyBuilder(CorsPolicyElement element)
    {
        return new Action<CorsPolicyBuilder>(builder =>
        {
            builder.SetIsOriginAllowedToAllowWildcardSubdomains();
            // Origins
            if (!string.IsNullOrEmpty(element.Origins) && element.Origins.Trim() != "*")
                builder.WithOrigins(element.Origins.Split(';'));
            else
                builder.AllowAnyOrigin();
            // Methods
            if (!string.IsNullOrEmpty(element.Methods) && element.Origins.Trim() != "*")
                builder.WithMethods(element.Methods.Split(';'));
            else
                builder.AllowAnyMethod();
            // Headers
            if (!string.IsNullOrEmpty(element.Headers) && element.Origins.Trim() != "*")
                builder.WithHeaders(element.Headers.Split(';'));
            else
                builder.AllowAnyHeader();
            // MaxAge
            if (element.MaxAge > 0)
                builder.SetPreflightMaxAge(TimeSpan.FromSeconds(element.MaxAge));
        });
    }
    #endregion

    #region ISyncNotifyService
    /// <summary>
    /// 设置客户端同步通知项Id集合
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="notifyIds"></param>
    /// <returns></returns>
    public static async Task SetSyncNotify(string userId, params int[] notifyIds)
        => await DIUtil.GetService<IClientSyncNotifyProvider>()?.AddNotify(userId, notifyIds);
    /// <summary>
    /// 解析同步值获取通知项Id集合
    /// </summary>
    /// <param name="notifyValue"></param>
    /// <returns></returns>
    public static List<int> ParseSyncNotify(long notifyValue)
    {
        return EnumUtil.ParseFlagsValue(notifyValue);
    }
    #endregion

    #region Base64Url
    public static string Base64UrlEncode(byte[] bytes)
        => WebEncoders.Base64UrlEncode(bytes);
    public static string Base64UrlEncode(string str, Encoding encode = null)
        => WebEncoders.Base64UrlEncode((encode ?? Encoding.UTF8).GetBytes(str));
    public static byte[] Base64UrlDecode(string base64Str)
        => WebEncoders.Base64UrlDecode(base64Str);
    public static string Base64UrlDecode(string base64Str, Encoding encode)
        => (encode ?? Encoding.UTF8).GetString(WebEncoders.Base64UrlDecode(base64Str));
    #endregion
}