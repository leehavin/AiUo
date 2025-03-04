using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text;
using AiUo.AspNet.RequestLogging;
using AiUo.Configuration;
using AiUo.Logging;

namespace AiUo.AspNet;

/// <summary>
/// 全局异常处理中间件
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private bool _useGlobalException = true;
    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
        _useGlobalException = ConfigUtil.GetSection<AspNetSection>()
            ?.UseApiActionResultFilter ?? true;
    }
    public async Task Invoke(HttpContext context, ILogBuilder logger)
    {
        bool handled = false;
        try
        {
            await _next(context);   //调用管道执行下一个中间件
        }
        catch (Exception ex)
        {
            handled = true;
            if (_useGlobalException)
            {
                ApiResult result = GlobalExceptionUtil.BuildApiResult(ex, logger, context);
                await ResponseApiResult(context, result, logger);
            }
            else
            {
                await ResponseInternalServerError(context, ex, logger);
            }
        }
        finally
        {
            if (!handled && context.Items.ContainsKey(GlobalExceptionUtil.ERROR_CODE_KEY))
            {
                string errorCode = Convert.ToString(context.Items[GlobalExceptionUtil.ERROR_CODE_KEY]);
                string errorMessage = context.Items.ContainsKey(GlobalExceptionUtil.ERROR_MESSAGE_KEY)
                    ? Convert.ToString(context.Items[GlobalExceptionUtil.ERROR_MESSAGE_KEY]) : null;
                if (_useGlobalException)
                {
                    ApiResult result = new ApiResult(errorCode, errorMessage);
                    logger.AddException(new CustomException(errorCode, errorMessage));
                    await ResponseApiResult(context, result, logger);
                }
                else
                {
                    var msg = $"没有使用ApiActionFilter时，不支持context.Items自定义错误！{GlobalExceptionUtil.ERROR_CODE_KEY}: {errorCode} {GlobalExceptionUtil.ERROR_MESSAGE_KEY}: {errorMessage}";
                    await ResponseInternalServerError(context, new Exception(msg), logger);
                }
            }
        }
    }

    #region Utils
    private async Task ResponseApiResult(HttpContext context, ApiResult result, ILogBuilder logger)
    {
        string json = null;
        try
        {
            json = SerializerUtil.SerializeJsonNet(result);
            RequestLoggingUtil.AddResponseBodyJson(context, json);
        }
        catch (Exception ex)
        {
            logger.AddMessage("GlobalExceptionMiddleware.ResponseApiResult序列化异常，必须处理!");
            logger.AddException(ex);
        }
        context.Response.Clear();
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsync(json, Encoding.UTF8);
    }
    private async Task ResponseInternalServerError(HttpContext context, Exception ex, ILogBuilder logger)
    {
        logger.AddException(ex);
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync("发生意外错误: InternalServerError");
    }
    #endregion
}