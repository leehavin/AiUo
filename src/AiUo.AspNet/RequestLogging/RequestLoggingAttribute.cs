using Microsoft.AspNetCore.Mvc.Filters;
using AiUo.Configuration;
using AiUo.Logging;

namespace AiUo.AspNet.RequestLogging;

public class RequestLoggingAttribute : Attribute, IAsyncActionFilter
{
    private RequestLoggingRule _rule;
    public RequestLoggingAttribute(string ruleName)
    {
        var section = ConfigUtil.GetSection<RequestLoggingSection>();
        if (section?.NamingRules?.TryGetValue(ruleName, out var r) == true)
        {
            _rule = r;
        }
        else
        {
            LogUtil.Warning($"配置文件RequestLogging:NamingRules中没有找到匹配的name: {ruleName}");
        }
    }
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (_rule != null)
        {
            if (!RequestLoggingUtil.TryGetRule(context.HttpContext, out var r) || r.Type != RequestLoggingRuleType.Match)
            {
                RequestLoggingUtil.AddRule(context.HttpContext, _rule);
                var logger = LogUtil.GetContextLogger();
                logger.SetLevel(_rule.Level);
                logger.CustomeExceptionLevel = _rule.CustomeExceptionLevel;
            }
        }
        await next.Invoke();
    }
}