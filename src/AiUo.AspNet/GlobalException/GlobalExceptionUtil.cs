using Microsoft.AspNetCore.Http;
using AiUo.Configuration;
using AiUo.Logging;
using AiUo.Net;

namespace AiUo.AspNet;

internal static class GlobalExceptionUtil
{
    /// <summary>
    /// 保存返回客户端的错误码：HttpContext.Items添加的Key
    /// </summary>
    public const string ERROR_CODE_KEY = "ERROR_CODE";
    public const string ERROR_MESSAGE_KEY = "ERROR_MESSAGE";

    static GlobalExceptionUtil()
    {
    }
    public static ApiResult BuildApiResult(Exception ex, ILogBuilder logger, HttpContext context)
    {
        ApiResult ret;
        logger.AddException(ex);

        // 获取异常链中的 CustomException
        CustomException exc = null;
        // TODO: 微软异常临时处理
        if (ex != null && ex is BadHttpRequestException
                       && (ex.Message == "Unexpected end of request content."
                           || ex.Message == "Reading the request body timed out due to data arriving too slowly. See MinRequestBodyDataRate."))
        {
            exc = new EndRequestContentException($"客户端请求中断:{ex.Message}");
            logger.SetCategoryName(nameof(EndRequestContentException))
                .SetLevel(Microsoft.Extensions.Logging.LogLevel.Warning);
        }
        else
        {
            exc = ExceptionUtil.GetException<CustomException>(ex);
        }

        if (exc != null)
        {
            ret = new ApiResult()
            {
                Success = false,
                Status = context.Response.StatusCode,
                Code = exc.Code,
                Result = exc.Result,
                TraceId = context.GetTraceId(),
                Exception = null
            };
            if (string.IsNullOrEmpty(ret.Code))
            {
                ret.Code = AspNetUtil.TryGetUnhandledExceptionCode(context, out var code)
                    ? code : GResponseCodes.G_BAD_REQUEST;
            }

            if (ConfigUtil.Project?.ResponseErrorMessage ?? false)
                ret.Message = exc.Message;
        }
        else
        {
            ret = new ApiResult()
            {
                Success = false,
                Status = context.Response.StatusCode,
                Code = AspNetUtil.TryGetUnhandledExceptionCode(context, out var code)
                    ? code : GResponseCodes.G_INTERNAL_SERVER_ERROR,
                Message = (ConfigUtil.Project?.ResponseErrorMessage ?? false)
                    ? ex.Message : null,
                Result = null,
                TraceId = context.GetTraceId(),
                Exception = AspNetUtil.GetResponseExceptionDetail(context) ? ex : null
            };
        }
        return ret;
    }
}