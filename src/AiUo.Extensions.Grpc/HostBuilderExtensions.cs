using AiUo.Configuration;
using AiUo.Extensions.Grpc;
using AiUo.Hosting;
using AiUo.Logging;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace AiUo;

/// <summary>
/// gRPC扩展
/// </summary>
public static class GrpcHostBuilderExtensions
{
    /// <summary>
    /// 添加gRPC扩展
    /// </summary>
    /// <param name="builder">主机构建器</param>
    /// <returns>主机构建器</returns>
    public static IHostBuilder AddGrpcEx(this IHostBuilder builder)
    {
        var section = ConfigUtil.GetSection<GrpcSection>();
        if (section == null || !section.Enabled)
            return builder;

        var watch = new Stopwatch();
        watch.Start();

        builder.ConfigureServices((context, services) =>
        {
            // 注册gRPC服务
            if (section.Server.Enabled)
            {
                services.AddGrpc(options =>
                {
                    // 配置拦截器
                    options.Interceptors.Add<GrpcInterceptor>();
                    // 配置最大消息大小
                    options.MaxReceiveMessageSize = section.Server.MaxReceiveMessageSize ?? 1024 * 1024 * 50; // 默认50MB
                    options.MaxSendMessageSize = section.Server.MaxSendMessageSize ?? 1024 * 1024 * 50; // 默认50MB
                    // 配置压缩选项
                    if (section.Server.EnableCompression)
                    {
                        options.CompressionProviders = new[]{
                            new Grpc.Net.Compression.GzipCompressionProvider(System.IO.Compression.CompressionLevel.Fastest)
                        };
                    }
                });
                
                // 注册拦截器
                services.AddScoped<GrpcInterceptor>();
            }

            // 注册gRPC客户端
            if (section.Clients.Count > 0)
            {
                var grpcClients = new GrpcClientContainer();
                services.AddSingleton(grpcClients);

                foreach (var client in section.Clients)
                {
                    grpcClients.RegisterClient(client.Key, () =>
                    {
                        var config = client.Value;
                        var channel = GrpcChannel.ForAddress(config.Address, new GrpcChannelOptions
                        {
                            Credentials = config.UseSSL ? ChannelCredentials.SecureSsl : ChannelCredentials.Insecure,
                            MaxReceiveMessageSize = 1024 * 1024 * 50, // 50MB
                        });
                        return channel;
                    });
                }

                // 在应用停止时释放资源
                HostingUtil.RegisterStopped(() =>
                {
                    grpcClients.Dispose();
                    LogUtil.Info("停止 => [gRPC]释放客户端连接");
                    return Task.CompletedTask;
                });
            }
        });

        watch.Stop();
        LogUtil.Info("配置 => [gRPC] [{ElapsedMilliseconds} 毫秒]", watch.ElapsedMilliseconds);

        return builder;
    }

    /// <summary>
    /// 使用gRPC服务
    /// </summary>
    /// <param name="app">应用构建器</param>
    /// <returns>应用构建器</returns>
    public static IApplicationBuilder UseGrpcEx(this IApplicationBuilder app)
    {
        var section = ConfigUtil.GetSection<GrpcSection>();
        if (section == null || !section.Enabled || !section.Server.Enabled)
            return app;

        // 配置gRPC服务
        app.UseRouting();
        
        // 如果启用了压缩
        if (section.Server.EnableCompression)
        {
            app.UseResponseCompression();
        }
        
        // 映射gRPC服务
        app.UseEndpoints(endpoints =>
        {
            // 自动映射当前程序集中的所有gRPC服务
            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            foreach (var type in assembly.GetTypes())
            {
                if (type.Name.EndsWith("Base") && !type.IsAbstract)
                {
                    var method = typeof(GrpcEndpointRouteBuilderExtensions)
                        .GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService))
                        .MakeGenericMethod(type);
                    
                    method.Invoke(null, new object[] { endpoints });
                    LogUtil.Info("映射gRPC服务: {ServiceType}", type.FullName);
                }
            }
        });

        return app;
    }
}