using AiUo.Extensions.Grpc;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcDemo.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GrpcDemo.Client;

public class Program
{
    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("gRPC客户端演示程序启动...");
        
        // 创建服务集合
        var services = new ServiceCollection();
        
        // 添加日志
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        // 配置gRPC客户端工厂
        services.AddGrpcClientFactory(options =>
        {
            options.EnableSsl = false;
            options.DefaultConnectionTimeoutSeconds = 10;
            options.MaxRetries = 3;
            options.RetryDelayMilliseconds = 200;
            options.EnableCompression = true;
        }, interceptorOptions =>
        {
            interceptorOptions.DefaultTimeoutSeconds = 30;
            interceptorOptions.MaxRetries = 3;
            interceptorOptions.LogPayloads = true;
        });
        
        // 添加gRPC客户端
        string serverAddress = "http://localhost:5000";
        services.AddGrpcClient<Greeter.GreeterClient>(serverAddress);
        
        // 构建服务提供者
        var serviceProvider = services.BuildServiceProvider();
        
        // 获取gRPC客户端
        var clientFactory = serviceProvider.GetRequiredService<GrpcClientFactory>();
        var client = clientFactory.CreateClient<Greeter.GreeterClient>(serverAddress);
        
        try
        {
            // 显示菜单
            while (true)
            {
                Console.WriteLine("\n请选择要测试的gRPC调用类型：");
                Console.WriteLine("1. 一元调用 (Unary)");
                Console.WriteLine("2. 服务端流式 (Server Streaming)");
                Console.WriteLine("3. 客户端流式 (Client Streaming)");
                Console.WriteLine("4. 双向流式 (Bidirectional Streaming)");
                Console.WriteLine("0. 退出");
                
                Console.Write("请输入选项: ");
                var input = Console.ReadLine();
                
                switch (input)
                {
                    case "1":
                        await UnaryCallExample(client);
                        break;
                    case "2":
                        await ServerStreamingExample(client);
                        break;
                    case "3":
                        await ClientStreamingExample(client);
                        break;
                    case "4":
                        await BidirectionalStreamingExample(client);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("无效的选项，请重新输入");
                        break;
                }
            }
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"RPC错误: {ex.Status.StatusCode} - {ex.Status.Detail}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发生错误: {ex.Message}");
        }
    }
    
    // 一元调用示例
    private static async Task UnaryCallExample(Greeter.GreeterClient client)
    {
        Console.WriteLine("\n===== 一元调用示例 =====");
        Console.Write("请输入您的名字: ");
        string name = Console.ReadLine() ?? "Guest";
        
        Console.Write("请输入语言代码 (zh, en, fr, de, es, it, ja): ");
        string language = Console.ReadLine() ?? "zh";
        
        Console.WriteLine("发送请求...");
        var reply = await client.SayHelloAsync(new HelloRequest { Name = name, Language = language });
        
        Console.WriteLine($"收到回复: {reply.Message}");
        Console.WriteLine($"时间戳: {reply.Timestamp}");
    }
    
    // 服务端流式示例
    private static async Task ServerStreamingExample(Greeter.GreeterClient client)
    {
        Console.WriteLine("\n===== 服务端流式示例 =====");
        Console.Write("请输入您的名字: ");
        string name = Console.ReadLine() ?? "Guest";
        
        Console.Write("请输入语言代码 (zh, en, fr, de, es, it, ja): ");
        string language = Console.ReadLine() ?? "zh";
        
        Console.WriteLine("发送请求，准备接收流式响应...");
        
        // 创建取消令牌，5秒后自动取消
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        
        try
        {
            // 调用服务端流式方法
            using var call = client.SayHellosServerStream(new HelloRequest { Name = name, Language = language }, cancellationToken: cts.Token);
            
            // 读取流式响应
            await foreach (var response in call.ResponseStream.ReadAllAsync(cts.Token))
            {
                Console.WriteLine($"收到流式回复: {response.Message}");
                Console.WriteLine($"时间戳: {response.Timestamp}");
            }
            
            Console.WriteLine("服务端流式调用完成");
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            Console.WriteLine("调用已取消");
        }
    }
    
    // 客户端流式示例
    private static async Task ClientStreamingExample(Greeter.GreeterClient client)
    {
        Console.WriteLine("\n===== 客户端流式示例 =====");
        Console.WriteLine("将发送多个问候请求...");
        
        // 创建客户端流式调用
        using var call = client.SayHellosClientStream();
        
        // 发送多个请求
        var languages = new[] { "zh", "en", "fr", "de", "es" };
        var names = new[] { "张三", "John", "Marie", "Hans", "Carlos" };
        
        for (int i = 0; i < 5; i++)
        {
            var request = new HelloRequest
            {
                Name = names[i],
                Language = languages[i]
            };
            
            await call.RequestStream.WriteAsync(request);
            Console.WriteLine($"已发送请求 {i+1}: 名称={request.Name}, 语言={request.Language}");
            await Task.Delay(500); // 短暂延迟
        }
        
        // 完成请求流
        await call.RequestStream.CompleteAsync();
        
        // 获取响应
        var summary = await call;
        Console.WriteLine($"收到汇总回复: {summary.Message}");
        Console.WriteLine($"请求数量: {summary.RequestCount}");
    }
    
    // 双向流式示例
    private static async Task BidirectionalStreamingExample(Greeter.GreeterClient client)
    {
        Console.WriteLine("\n===== 双向流式示例 =====");
        Console.WriteLine("将发送多个问候请求并接收实时响应...");
        
        // 创建双向流式调用
        using var call = client.SayHellosBidirectional();
        
        // 创建取消令牌
        using var cts = new CancellationTokenSource();
        
        // 启动一个任务来读取响应
        var responseTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var response in call.ResponseStream.ReadAllAsync(cts.Token))
                {
                    Console.WriteLine($"收到响应: {response.Message}");
                    Console.WriteLine($"时间戳: {response.Timestamp}");
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine("响应流已取消");
            }
        });
        
        // 发送多个请求
        var languages = new[] { "zh", "en", "fr", "de", "es" };
        
        for (int i = 0; i < 5; i++)
        {
            Console.Write($"请输入第 {i+1} 个名字 (或直接回车使用默认名称): ");
            string name = Console.ReadLine();
            if (string.IsNullOrEmpty(name))
            {
                name = $"用户{i+1}";
            }
            
            var request = new HelloRequest
            {
                Name = name,
                Language = languages[i]
            };
            
            await call.RequestStream.WriteAsync(request);
            Console.WriteLine($"已发送请求: 名称={request.Name}, 语言={request.Language}");
            await Task.Delay(1000); // 等待1秒
        }
        
        // 完成请求流
        await call.RequestStream.CompleteAsync();
        
        // 等待响应流完成
        await responseTask;
        
        Console.WriteLine("双向流式调用完成");
    }
}