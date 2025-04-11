using AiUo.Configuration;
using AiUo.Extensions.MQTT;
using AiUo.Extensions.MQTT.Server;
using AiUo.Hosting;
using AiUo.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace AiUo;

public static class MQTTHostBuilderExtensions
{
    /// <summary>
    /// 添加MQTT客户端支持
    /// </summary>
    /// <param name="builder">主机构建器</param>
    /// <returns>主机构建器</returns>
    public static IHostBuilder AddMQTTEx(this IHostBuilder builder)
    {
        var section = ConfigUtil.GetSection<MQTTSection>();
        if (section == null || !section.Enabled || section.ConnectionStrings == null || section.ConnectionStrings.Count == 0)
            return builder;

        var watch = new Stopwatch();
        watch.Start();

        builder.ConfigureServices((context, services) =>
        {
            // 初始化客户端容器
            var container = new MQTTContainer();
            services.AddSingleton(container);
            
            // 初始化服务器容器
            var serverContainer = new MQTTServerContainer();
            services.AddSingleton(serverContainer);
            
            HostingUtil.RegisterStarting(async () =>
            { 
                // 初始化服务器
                await serverContainer.InitAsync();

                // 初始化客户端
                await container.InitAsync();
                if (container.ConsumerAssemblies.Count > 0)
                {
                    var asms = string.Join('|', container.ConsumerAssemblies.Select(x => x.GetName().Name));
                    LogUtil.Info("启动 => [MQTT]加载ConsumerAssemblies: {ConsumerAssemblies}" , asms);
                }
                
            });
            
            HostingUtil.RegisterStopping(async () =>
            {
                // 停止服务器
                await serverContainer.StopAsync();
                LogUtil.Info("停止 => [MQTT]停止服务器");
                
                // 释放消费者
                container.ReleaseConsumers();
                LogUtil.Info("停止 => [MQTT]释放 Consumer");
                return;
            });
            
            HostingUtil.RegisterStopped(() =>
            {
                // 释放服务器容器
                serverContainer.Dispose();
                
                // 释放客户端容器
                container.Dispose();
                LogUtil.Info("停止 => [MQTT]释放 Client 和 Server");
                return Task.CompletedTask;
            });
        });
        watch.Stop();
        LogUtil.Info("配置 => [MQTT] [{ElapsedMilliseconds} 毫秒]" , watch.ElapsedMilliseconds);
        return builder;
    }
}