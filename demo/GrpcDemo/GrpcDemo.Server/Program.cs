using AiUo;
using AiUo.Extensions.Grpc;
using GrpcDemo.Server.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// 添加gRPC服务
builder.Services.AddGrpc(options =>
{
    options.MaxReceiveMessageSize = 50 * 1024 * 1024; // 50MB
    options.MaxSendMessageSize = 50 * 1024 * 1024; // 50MB
});

// 添加gRPC拦截器
builder.Services.AddSingleton(new GrpcInterceptorOptions
{
    DefaultTimeoutSeconds = 30,
    MaxRetries = 3,
    LogPayloads = true
});
builder.Services.AddScoped<GrpcInterceptor>();

// 配置Kestrel以支持gRPC
builder.WebHost.ConfigureKestrel(options =>
{
    // 设置HTTP/2协议，不使用TLS
    options.ListenLocalhost(5000, o => o.Protocols = HttpProtocols.Http2);
});

var app = builder.Build();

// 配置HTTP请求管道
app.UseRouting();

// 映射gRPC服务
app.MapGrpcService<GreeterService>();

app.MapGet("/", () => "与gRPC服务通信需要使用gRPC客户端。要了解如何创建客户端，请访问: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();