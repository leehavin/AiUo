using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using AiUo.Configuration;
using AiUo.Logging;

namespace AiUo.AspNet.RequestLogging;

/// <summary>
/// 记录配置文件RequestLogging:Urls中的请求日志
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    private bool _enabled;
    private RequestLoggingRule _defaultRule;
    private RequestLoggingRule _matchRule;
    private static HashSet<string> _innerUrl = new HashSet<string>
    {
        "/healthz","/env","/dump",
        "/swagger/index.html","/swagger/v1/swagger.json"
    };
    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
        Init();
        ConfigUtil.RegisterChangedCallback(() => Init());
    }
    private void Init()
    {
        var section = ConfigUtil.GetSection<RequestLoggingSection>();
        if (section?.Enabled ?? false)
        {
            _enabled = true;
            _defaultRule = section.DefaultRule;
            _matchRule = section.MatchRule;
        }
    }
    public async Task Invoke(HttpContext context, ILogBuilder logger)
    {
        if (!_enabled)
        {
            await _next(context); // 继续执行
            if (logger.Exception != null)
            {
                await LogRequestBody(context, logger);
                LogResponseBody(context, logger);
            }
            SaveLog(logger);
            return;
        }

        var requestUrl = context.Request.Path.ToString().ToLower();
        if (_innerUrl.Contains(requestUrl))
        {
            await _next(context);// 继续执行
            return;
        }

        var r = _matchRule.IsMatch(requestUrl) ? _matchRule : _defaultRule;
        RequestLoggingUtil.AddRule(context, r);
        logger.SetLevel(r.Level);
        logger.CustomeExceptionLevel = r.CustomeExceptionLevel;

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        await _next(context); // 继续执行
        stopwatch.Stop();

        if (!RequestLoggingUtil.TryGetRule(context, out var rule))
            return;
        logger.AddMessage($"[{context.Request.Method}] {requestUrl}");
        logger.AddField("Request.Elasped", stopwatch.ElapsedMilliseconds);

        var property = rule.GetProperty();
        if (property.Referer)
            logger.AddField("Request.Referer", AspNetUtil.GetRefererUrl(context));
        if (property.RemoteIp)
            logger.AddField("Request.RemoteIp", AspNetUtil.GetRemoteIpString(context));
        if (property.UserId)
            logger.AddField("Request.UserId", context?.User?.Identity?.Name);
        if (property.RequestHeaders)
            logger.AddField("Request.Headers", context.Request.Headers.ToDictionary(x => x.Key, v => string.Join(";", v.Value.ToList())));
        if (property.ResponseHeaders)
            logger.AddField("Response.Headers", context.Response.Headers.ToDictionary(x => x.Key, v => string.Join(";", v.Value.ToList())));

        if (property.RequestBody || logger.Exception != null)
            await LogRequestBody(context, logger);
        if (property.ResponseBody || logger.Exception != null)
            LogResponseBody(context, logger);

        SaveLog(logger);
    }
    private const string FIELD_REQUEST_BODY = "Request.Body";
    private static async Task LogRequestBody(HttpContext context, ILogBuilder logger)
    {
        // 获取请求body内容
        if (context.Request.Method == "POST")
        {
            try
            {
                logger.AddField(FIELD_REQUEST_BODY, await AspNetUtil.GetRequestBodyAsync(context));
            }
            catch (Exception ex)
            {
                logger.AddException(ex);
                logger.AddField(FIELD_REQUEST_BODY, "获取Request.Body出现异常:AspNetUtil.GetRequestBodyAsync()");
            }
        }
        else if (context.Request.Method == "GET")
            logger.AddField(FIELD_REQUEST_BODY, context.Request.QueryString.Value);
    }
    private const string FIELD_RESPONSE_BODY = "Response.Body";
    private static void LogResponseBody(HttpContext context, ILogBuilder logger)
    {
        var rspBody = RequestLoggingUtil.GetResponseBody(context);
        logger.AddField(FIELD_RESPONSE_BODY, rspBody);
    }
    private void SaveLog(ILogBuilder logger)
    {
        var myLogger = logger as LogBuilder;
        if (myLogger != null)
            myLogger.Save(true);
        else
            logger.Save();
    }


    //private static async Task<string> GetResponse(HttpResponse response)
    //{
    //    response.Body.Seek(0, SeekOrigin.Begin);
    //    var reader = new StreamReader(response.Body, Encoding.UTF8, false, 1024, true);
    //    var text = await reader.ReadToEndAsync();
    //    response.Body.Seek(0, SeekOrigin.Begin);
    //    return text;
    //}
}