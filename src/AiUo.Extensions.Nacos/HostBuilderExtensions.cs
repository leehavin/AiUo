using Grpc.Core;
using Grpc.Net.Client.Balancer;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nacos.AspNetCore.V2;
using System;
using System.Diagnostics;
using System.Net.Http;
using AiUo.Configuration;
using AiUo.Logging;

namespace AiUo.Extensions.Nacos;

public static class NacosHostBuilderExtensions
{
    public static IHostBuilder AddNacosEx(this IHostBuilder builder)
    {
        var watch = new Stopwatch();
        watch.Start();
        builder.ConfigureServices(services =>
        {
            var section = DIUtil.GetService<NacosSection>();
            if (section?.Enabled ?? false)
            {
                services.AddHttpClient(NacosOpenApiService.HTTP_CLIENT_NAME)
                    .ConfigurePrimaryHttpMessageHandler(() =>
                        new HttpClientHandler()
                        {
                            UseProxy = false,
                            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
                        }
                    );
                services.AddSingleton<ResolverFactory>(sp => new NacosGrpcResolverFactory(TimeSpan.FromMilliseconds(section.LBRefreshInterval)));
                services.AddSingleton<ServiceConfig>(sp =>
                {
                    var defaultMethodConfig = new MethodConfig
                    {
                        Names = { MethodName.Default },
                        RetryPolicy = new RetryPolicy
                        {
                            MaxAttempts = 5, // 尝试次数
                            InitialBackoff = TimeSpan.FromSeconds(1),
                            MaxBackoff = TimeSpan.FromSeconds(5),
                            BackoffMultiplier = 1.5,
                            RetryableStatusCodes = { StatusCode.Unavailable }
                        }
                    };
                    var ret = new ServiceConfig
                    {
                        MethodConfigs = { defaultMethodConfig }
                    };
                    // PickFirstConfig => （默认）尝试连接到地址，直到成功建立连接。 gRPC 调用都是针对第一次成功连接进行的
                    // RoundRobinConfig => 尝试连接到所有地址。 gRPC 调用是使用轮循机制逻辑分布在所有成功的连接上的
                    if (section.LBPickFirst)
                        ret.LoadBalancingConfigs.Add(new PickFirstConfig());
                    else
                        ret.LoadBalancingConfigs.Add(new RoundRobinConfig());
                    return ret;
                });

                //
                if (string.IsNullOrEmpty(section.ServiceName))
                    throw new Exception("配置Nacos:ServiceName不能为空且必须与ProjectId相同");
                if (section.ServiceName != ConfigUtil.Project.ProjectId)
                    LogUtil.Warning($"Nacose ServiceName 和 ProjectId 不相同。ServiceName: {section.ServiceName} ProjectId: {ConfigUtil.Project.ProjectId}");

                ConfigUtil.Configuration["Nacos:Metadata:aiuo.service_id"] = ConfigUtil.Service.ServiceId;
                ConfigUtil.Configuration["Nacos:Metadata:aiuo.register_date"] = DateTime.UtcNow.UtcToCNString();
                services.AddNacosAspNet(ConfigUtil.Configuration, "Nacos");
            }
        });

        watch.Stop();
        LogUtil.Info("配置 => [Nacos] [{ElapsedMilliseconds} 毫秒]", watch.ElapsedMilliseconds);
        return builder;
    }
}