using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Collections.Concurrent;
using System.Reflection;
using AiUo.AspNet.RequestLogging;
using AiUo.Net;

namespace AiUo.AspNet;

public class ApiActionResultFilter : Attribute, IActionFilter
{
    private static ConcurrentDictionary<Type, bool> _ignoreControllerDic = new ConcurrentDictionary<Type, bool>();
    private static ConcurrentDictionary<string, bool> _ignoreActionDic = new ConcurrentDictionary<string, bool>();
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // IgnoreActionFilterAttribute
        var controllerType = context.Controller.GetType();
        var isIgnore = _ignoreControllerDic.GetOrAdd(controllerType, (t) =>
        {
            return t.GetCustomAttributes<IgnoreActionFilterAttribute>().FirstOrDefault() != null;
        });
        if (!isIgnore)
        {
            var actionId = context.ActionDescriptor.Id;
            isIgnore = _ignoreActionDic.GetOrAdd(actionId, (_) =>
            {
                var method = (context.ActionDescriptor as ControllerActionDescriptor).MethodInfo;
                return method.GetCustomAttributes<IgnoreActionFilterAttribute>().FirstOrDefault() != null;
            });
        }
        if (isIgnore)
            context.HttpContext.Items.Add(IgnoreActionFilterAttribute.ITEM_NAME, true);
    }
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // 忽略不封装
        if (IgnoreActionFilterAttribute.CheckIgnore(context.HttpContext))
        {
            var rspBody = context.Result is ObjectResult obj
                ? obj.Value : null;
            RequestLoggingUtil.AddResponseBody(context.HttpContext, rspBody);
            return;
        }

        // 异常统一处理 GlobalExceptionMiddleware
        if (context.Exception != null)
            return;

        // 封装
        var statusCode = context.Result is IStatusCodeActionResult scar && scar.StatusCode.HasValue
            ? scar.StatusCode.Value
            : context.HttpContext.Response.StatusCode;
        object result = null;
        if (context.Result is ObjectResult objr)
        {
            // 已包装不处理
            if (objr.Value is ApiResultBase)
            {
                RequestLoggingUtil.AddResponseBody(context.HttpContext, objr.Value);
                return;
            }
            result = objr.Value;
        }

        //只处理2xx
        if (NetUtil.IsSuccessStatusCode(statusCode))
            context.Result = BuildResult(statusCode, result, context.HttpContext);
    }
    private IActionResult BuildResult(int statusCode, object value, HttpContext context)
    {
        var ret = new ApiResult();
        ret.Success = true;
        ret.Status = statusCode;
        ret.Result = value;
        if (AspNetUtil.TryGetSuccessCode(context, out var code))
            ret.Code = code;
        ret.TraceId = context.GetTraceId();

        RequestLoggingUtil.AddResponseBody(context, ret);
        return new ObjectResult(ret);
    }
}