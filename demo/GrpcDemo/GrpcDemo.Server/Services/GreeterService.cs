using Grpc.Core;
using GrpcDemo.Shared;

namespace GrpcDemo.Server.Services;

/// <summary>
/// 问候服务实现
/// </summary>
public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;

    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 发送问候（一元调用）
    /// </summary>
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        _logger.LogInformation("收到来自 {Name} 的问候请求，语言：{Language}", request.Name, request.Language);
        
        string greeting = GetGreeting(request.Language, request.Name);
        
        return Task.FromResult(new HelloReply
        {
            Message = greeting,
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }

    /// <summary>
    /// 发送多次问候（服务端流式）
    /// </summary>
    public override async Task SayHellosServerStream(HelloRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
    {
        _logger.LogInformation("开始服务端流式问候，客户端：{Name}，语言：{Language}", request.Name, request.Language);
        
        // 发送5次问候
        for (int i = 0; i < 5; i++)
        {
            // 检查客户端是否取消请求
            if (context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("客户端取消了请求");
                break;
            }
            
            string greeting = GetGreeting(request.Language, request.Name) + $" ({i + 1}/5)";
            
            await responseStream.WriteAsync(new HelloReply
            {
                Message = greeting,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
            
            // 等待1秒再发送下一条消息
            await Task.Delay(1000);
        }
        
        _logger.LogInformation("服务端流式问候完成");
    }

    /// <summary>
    /// 发送多次问候（客户端流式）
    /// </summary>
    public override async Task<HelloSummary> SayHellosClientStream(IAsyncStreamReader<HelloRequest> requestStream, ServerCallContext context)
    {
        _logger.LogInformation("开始客户端流式问候");
        
        int requestCount = 0;
        var names = new List<string>();
        
        // 读取客户端发送的所有请求
        await foreach (var request in requestStream.ReadAllAsync())
        {
            requestCount++;
            names.Add(request.Name);
            _logger.LogInformation("收到来自 {Name} 的问候请求，语言：{Language}", request.Name, request.Language);
        }
        
        _logger.LogInformation("客户端流式问候完成，共收到 {Count} 个请求", requestCount);
        
        // 返回汇总信息
        return new HelloSummary
        {
            RequestCount = requestCount,
            Message = $"收到来自 {string.Join("、", names)} 的问候，共 {requestCount} 条"
        };
    }

    /// <summary>
    /// 双向流式问候
    /// </summary>
    public override async Task SayHellosBidirectional(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
    {
        _logger.LogInformation("开始双向流式问候");
        
        // 读取客户端发送的所有请求并立即回复
        await foreach (var request in requestStream.ReadAllAsync())
        {
            _logger.LogInformation("收到来自 {Name} 的问候请求，语言：{Language}", request.Name, request.Language);
            
            string greeting = GetGreeting(request.Language, request.Name);
            
            await responseStream.WriteAsync(new HelloReply
            {
                Message = greeting,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
        
        _logger.LogInformation("双向流式问候完成");
    }
    
    /// <summary>
    /// 根据语言获取问候语
    /// </summary>
    private string GetGreeting(string language, string name)
    {
        return language?.ToLower() switch
        {
            "en" => $"Hello, {name}!",
            "fr" => $"Bonjour, {name}!",
            "de" => $"Hallo, {name}!",
            "es" => $"¡Hola, {name}!",
            "it" => $"Ciao, {name}!",
            "ja" => $"こんにちは, {name}さん!",
            "zh" => $"你好, {name}!",
            _ => $"你好, {name}!"
        };
    }
}