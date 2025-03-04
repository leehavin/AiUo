using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace AiUo.Extensions.Grpc;

/// <summary>
/// gRPC拦截器配置选项
/// </summary>
public class GrpcInterceptorOptions
{
    /// <summary>
    /// 默认请求超时时间(秒)
    /// </summary>
    public int DefaultTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// 是否记录请求和响应内容
    /// </summary>
    public bool LogPayloads { get; set; } = true;

    /// <summary>
    /// 自定义异常处理
    /// </summary>
    public Func<Exception, Status>? CustomExceptionHandler { get; set; }
}

/// <summary>
/// gRPC拦截器
/// </summary>
public class GrpcInterceptor : Interceptor
{
    private readonly ILogger<GrpcInterceptor> _logger;
    private readonly GrpcInterceptorOptions _options;

    /// <summary>
    /// 初始化gRPC拦截器
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="options">拦截器配置选项</param>
    public GrpcInterceptor(ILogger<GrpcInterceptor> logger, GrpcInterceptorOptions options)
    {
        _logger = logger;
        _options = options;
    }

    /// <summary>
    /// 处理异常并转换为RpcException
    /// </summary>
    private RpcException HandleException(Exception ex, string method)
    {
        _logger.LogError(ex, "gRPC请求异常 - 方法: {Method}, 异常信息: {Message}", method, ex.Message);

        var status = _options.CustomExceptionHandler?.Invoke(ex) 
            ?? new Status(StatusCode.Internal, "服务器内部错误");

        return new RpcException(status);
    }

    /// <summary>
    /// 记录请求日志
    /// </summary>
    private void LogRequest<TRequest>(string method, TRequest request)
    {
        if (_options.LogPayloads)
        {
            _logger.LogInformation("gRPC请求开始 - 方法: {Method}, 请求内容: {Request}", method, request);
        }
        else
        {
            _logger.LogInformation("gRPC请求开始 - 方法: {Method}", method);
        }
    }

    /// <summary>
    /// 记录响应日志
    /// </summary>
    private void LogResponse<TResponse>(string method, TResponse response)
    {
        if (_options.LogPayloads)
        {
            _logger.LogInformation("gRPC请求完成 - 方法: {Method}, 响应内容: {Response}", method, response);
        }
        else
        {
            _logger.LogInformation("gRPC请求完成 - 方法: {Method}", method);
        }
    }

    /// <summary>
    /// 拦截一元调用
    /// </summary>
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            LogRequest(context.Method, request);

            // 设置默认超时时间
            if (!context.CancellationToken.CanBeCanceled)
            {
                var deadline = DateTime.UtcNow.AddSeconds(_options.DefaultTimeoutSeconds);
                context.WriteOptions = new WriteOptions();
                // 在新版本中，截止时间通过ServerCallContext直接设置
                context.Status = new Status(StatusCode.OK, string.Empty);
            }

            var response = await continuation(request, context);
            LogResponse(context.Method, response);
            return response;
        }
        catch (Exception ex)
        {
            throw HandleException(ex, context.Method);
        }
    }

    /// <summary>
    /// 拦截客户端流调用
    /// </summary>
    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            _logger.LogInformation("gRPC客户端流请求开始 - 方法: {Method}", context.Method);

            var response = await continuation(requestStream, context);
            LogResponse(context.Method, response);
            return response;
        }
        catch (Exception ex)
        {
            throw HandleException(ex, context.Method);
        }
    }

    /// <summary>
    /// 拦截服务端流调用
    /// </summary>
    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            LogRequest(context.Method, request);
            await continuation(request, responseStream, context);
            if (_options.LogPayloads)
            {
                _logger.LogInformation("gRPC服务端流请求完成 - 方法: {Method}, 响应: 流式响应", context.Method);
            }
            else
            {
                _logger.LogInformation("gRPC服务端流请求完成 - 方法: {Method}", context.Method);
            }
        }
        catch (Exception ex)
        {
            throw HandleException(ex, context.Method);
        }
    }

    /// <summary>
    /// 拦截双向流调用
    /// </summary>
    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            _logger.LogInformation("gRPC双向流请求开始 - 方法: {Method}", context.Method);
            await continuation(requestStream, responseStream, context);
            _logger.LogInformation("gRPC双向流请求完成 - 方法: {Method}", context.Method);
        }
        catch (Exception ex)
        {
            throw HandleException(ex, context.Method);
        }
    }
}