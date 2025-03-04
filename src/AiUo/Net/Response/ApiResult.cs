using System;
using AiUo.Net;

namespace AiUo.AspNet;

public class ApiResultBase : IResponseBase
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
    /// <summary>
    /// 自定义异常码
    /// </summary>
    public string Code { get; set; }
    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; }
    /// <summary>
    /// 异常
    /// </summary>
    public Exception Exception { get; set; }

    /// <summary>
    /// 状态码
    /// </summary>
    public int Status { get; set; }
    /// <summary>
    /// 跟踪ID
    /// </summary>
    public string TraceId { get; set; }
}
/// <summary>
/// Api返回的统一结构
/// </summary>
public class ApiResult : ApiResultBase, IResponseResult
{
    /// <summary>
    /// 返回给客户端的数据
    /// </summary>
    public object Result { get; set; }
    public ApiResult()
    {
        Success = true;
        Status = 200;
        Result = null;
    }
    /// <summary>
    /// 构造函数-返回200
    /// </summary>
    /// <param name="result"></param>
    public ApiResult(object result)
    {
        Success = true;
        Status = 200;
        Result = result;
    }
    public ApiResult(string code, string message, Exception ex = null, object result = null)
        : this(code, message, 400, ex, result)
    {
    }
    public ApiResult(string code, string message, int status, Exception ex = null, object result = null)
    {
        if (string.IsNullOrEmpty(code))
            throw new ArgumentNullException("ApiResult构造错误时Code不能为null", "code");
        Success = false;
        Status = status;
        Code = code;
        Message = message;
        Exception = ex;
        Result = result;
    }
}
/// <summary>
/// Api返回的统一结构
/// </summary>
/// <typeparam name="T"></typeparam>
public class ApiResult<T> : ApiResultBase, IResponseResult<T>
{
    /// <summary>
    /// 返回的数据
    /// </summary>
    public T Result { get; set; }
    public ApiResult()
    {
        Success = true;
        Status = 200;
        Result = default;
    }
    /// <summary>
    /// 构造函数-返回200
    /// </summary>
    /// <param name="result"></param>
    public ApiResult(T result)
    {
        Success = true;
        Status = 200;
        Result = result;
    }
    /// <summary>
    /// 构造函数-返回400
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="ex"></param>
    public ApiResult(string code, string message, Exception ex = null)
    {
        if (string.IsNullOrEmpty(code))
            throw new ArgumentNullException("ApiResult构造错误时Code不能为null", "code");
        Success = false;
        Status = 400;
        Code = code;
        Message = message;
        Exception = ex;
    }
    /// <summary>
    /// 隐式转换
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator ApiResult<T>(T value)
        => new ApiResult<T>(value);
    public static implicit operator ApiResult<T>(ApiResult value)
    {
        var ret = new ApiResult<T>()
        {
            Success = value.Success,
            Status = value.Status,
            Code = value.Code,
            Message = value.Message,
            Exception = value.Exception,
            TraceId = value.TraceId,
            Result = (T)value.Result
        };
        return ret;
    }
}