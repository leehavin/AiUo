using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Concurrent;
using System.Net;
using AiUo.Net;

namespace AiUo.AspNet.Filters;

public class ValidateModelFilter : Attribute, IActionFilter
{
    private static ConcurrentDictionary<string, IActionResult> _msgResultDic = new();

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
            return;
        var errMsg = context.ModelState
            ?.FirstOrDefault(m => m.Value.ValidationState == ModelValidationState.Invalid).Value
            ?.Errors
            ?.FirstOrDefault()
            ?.ErrorMessage;
        //
        if (!string.IsNullOrEmpty(errMsg) && _msgResultDic.TryGetValue(errMsg, out var result))
        {
            context.Result = result;
            if (IgnoreActionFilterAttribute.CheckIgnore(context.HttpContext))
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }
        //
        var code = GResponseCodes.G_BAD_REQUEST;
        var msg = errMsg;
        if (!string.IsNullOrEmpty(errMsg))
        {
            var idx = errMsg.LastIndexOf('|');
            if (idx > -1)
            {
                code = errMsg.Substring(idx + 1);
                msg = errMsg.Substring(0, idx);
            }
        }

        // 统一返回结构
        if (!IgnoreActionFilterAttribute.CheckIgnore(context.HttpContext))
        {
            result = BuildResult(code, msg);
        }
        else
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            result = new ObjectResult(msg);
        }
        if (!string.IsNullOrEmpty(errMsg))
            _msgResultDic.TryAdd(errMsg, result);
        context.Result = result;
    }
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    private IActionResult BuildResult(string code, string msg)
    {
        var ret = new ApiResult();
        ret.Success = false;
        ret.Status = 400;
        ret.Code = code;
        ret.Message = msg;
        ret.Result = null;
        return new ObjectResult(ret);
    }
}