using AiUo.Configuration;
using AiUo.Extensions.MQTT;
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
            var container = new MQTTContainer();
            services.AddSingleton(container);
            HostingUtil.RegisterStarting(async () =>
            {
                await container.InitAsync();
                if (container.ConsumerAssemblies.Count > 0)
                {
                    var asms = string.Join('|', container.ConsumerAssemblies.Select(x => x.GetName().Name));
                    LogUtil.Info("启动 => [MQTT]加载ConsumerAssemblies: {ConsumerAssemblies}" , asms);
                }
            });
            HostingUtil.RegisterStopping(() =>
            {
                container.ReleaseConsumers();
                LogUtil.Info("停止 => [MQTT]释放 Consumer");
                return Task.CompletedTask;
            });
            HostingUtil.RegisterStopped(() =>
            {
                container.Dispose();
                LogUtil.Info("停止 => [MQTT]释放 Client");
                return Task.CompletedTask;
            });
        });
        watch.Stop();
        LogUtil.Info("配置 => [MQTT] [{ElapsedMilliseconds} 毫秒]" , watch.ElapsedMilliseconds);
        return builder;
    }
}