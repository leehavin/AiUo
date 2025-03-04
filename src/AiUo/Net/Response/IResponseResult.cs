using System;

namespace AiUo.Net;

public interface IResponseBase
{
    bool Success { get; set; }
    /// <summary>
    /// 消息码
    /// </summary>
    string Code { get; set; }
    /// <summary>
    /// 消息
    /// </summary>
    string Message { get; set; }
    /// <summary>
    /// 异常
    /// </summary>
    Exception Exception { get; set; }
}
public interface IResponseResult:IResponseBase
{
    object Result { get;set; }
}
public interface IResponseResult<T> : IResponseBase
{ 
    T Result { get; set; }
}
public class ResponseBase: IResponseBase
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
}
public class ResponseResult : ResponseBase,IResponseResult
{
    /// <summary>
    /// 返回给客户端的数据
    /// </summary>
    public object Result { get; set; }
    public ResponseResult() { }
    /// <summary>
    /// 构造函数-返回200
    /// </summary>
    /// <param name="result"></param>
    public ResponseResult(object result)
    {
        Success = true;
        Result = result;
    }
    /// <summary>
    /// 构造函数-返回400
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="ex"></param>
    /// <param name="result"></param>
    public ResponseResult(string code, string message, Exception ex = null, object result = null)
    {
        if (string.IsNullOrEmpty(code))
            throw new ArgumentNullException("ResponseResult构造错误时Code不能为null", "code");
        Success = false;
        Code = code;
        Message = message;
        Exception = ex;
        Result = result;
    }
}
public class ResponseResult<T> : ResponseBase, IResponseResult<T>
{
    /// <summary>
    /// 返回的数据
    /// </summary>
    public T Result { get; set; }
    public ResponseResult() { }
    /// <summary>
    /// 构造函数-返回200
    /// </summary>
    /// <param name="result"></param>
    public ResponseResult(T result)
    {
        Success = true;
        Result = result;
    }
    /// <summary>
    /// 构造函数-返回400
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="ex"></param>
    public ResponseResult(string code, string message, Exception ex = null)
    {
        if (string.IsNullOrEmpty(code))
            throw new ArgumentNullException("ApiResult构造错误时Code不能为null", "code");
        Success = false;
        Code = code;
        Message = message;
        Exception = ex;
    }
    /// <summary>
    /// 隐式转换
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator ResponseResult<T>(T value)
        => new ResponseResult<T>(value);
    public static implicit operator ResponseResult<T>(ResponseResult value)
    {
        var ret = new ResponseResult<T>()
        {
            Success = value.Success,
            Code = value.Code,
            Message = value.Message,
            Exception = value.Exception,
            Result = (T)value.Result
        };
        return ret;
    }
}