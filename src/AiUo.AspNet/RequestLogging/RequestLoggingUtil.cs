using Microsoft.AspNetCore.Http;
using AiUo.Logging;

namespace AiUo.AspNet.RequestLogging;

internal static class RequestLoggingUtil
{
    private const string REQUEST_LOGGING_RULE = "REQUEST_LOGGING_RULE";
    public static void AddRule(HttpContext context, RequestLoggingRule rule)
    {
        context.Items.TryAdd(REQUEST_LOGGING_RULE, rule);
    }
    public static bool TryGetRule(HttpContext context, out RequestLoggingRule rule)
    {
        var ret = context.Items.TryGetValue(REQUEST_LOGGING_RULE, out var value);
        rule = ret ? (RequestLoggingRule)value : null;
        return ret;
    }

    private const string REQUEST_RESPONSE_BODY = "REQUEST_RESPONSE_BODY";
    private const string REQUEST_RESPONSE_BODY_TYPE = "REQUEST_RESPONSE_BODY_TYPE";
    public static void AddResponseBody(HttpContext context, object response)
    {
        context.Items.TryAdd(REQUEST_RESPONSE_BODY, response);
        context.Items.TryAdd(REQUEST_RESPONSE_BODY_TYPE, false);
    }
    public static void AddResponseBodyJson(HttpContext context, string response)
    {
        context.Items.TryAdd(REQUEST_RESPONSE_BODY, response);
        context.Items.TryAdd(REQUEST_RESPONSE_BODY_TYPE, true);
    }
    public static string GetResponseBody(HttpContext context)
    {
        if (!context.Items.TryGetValue(REQUEST_RESPONSE_BODY, out var body))
            return null; //无值退出
        var hasType = context.Items.TryGetValue(REQUEST_RESPONSE_BODY_TYPE, out var json);
        try
        {
            return hasType && (bool)json
                ? Convert.ToString(body)
                : SerializerUtil.SerializeJsonNet(body);
        }
        catch (Exception ex)
        {
            LogUtil.GetContextLogger()
                .AddException(ex)
                .AddMessage("RequestLoggingUtil.GetResponseBody()异常，序列化失败，请检查对象是否支持!");
        }
        return null;
    }
}