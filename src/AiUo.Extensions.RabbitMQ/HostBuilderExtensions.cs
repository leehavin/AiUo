using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using AiUo.Configuration;
using AiUo.Extensions.RabbitMQ;
using AiUo.Hosting;
using AiUo.Logging;

namespace AiUo;

public static class RabbitMQHostBuilderExtensions
{
    public static IHostBuilder AddRabbitMQEx(this IHostBuilder builder)
    {
        var section = ConfigUtil.GetSection<RabbitMQSection>();
        if (section == null || !section.Enabled || section.ConnectionStrings == null || section.ConnectionStrings.Count == 0)
            return builder;

        var watch = new Stopwatch();
        watch.Start();

        builder.ConfigureServices((context, services) =>
        {
            var container = new MQContainer();
            services.AddSingleton(container);
            HostingUtil.RegisterStarting(async () =>
            {
                await container.InitAsync();
                if (container.ConsumerAssemblies.Count > 0)
                {
                    var asms = string.Join('|', container.ConsumerAssemblies.Select(x => x.GetName().Name));
                    LogUtil.Info("启动 => [RabbitMQ]加载ConsumerAssemblies: {ConsumerAssemblies}"
                        , asms);
                }
            });
            HostingUtil.RegisterStopping(() =>
            {
                container.ReleaseConsumers();
                LogUtil.Info("停止 => [RabbitMQ]释放 Consumer");
                return Task.CompletedTask;
            });
            HostingUtil.RegisterStopped(() =>
            {
                container.Dispose();
                LogUtil.Info("停止 => [RabbitMQ]释放 IBus");
                return Task.CompletedTask;
            });
        });
        watch.Stop();
        LogUtil.Info("配置 => [RabbitMQ] [{ElapsedMilliseconds} 毫秒]"
            , watch.ElapsedMilliseconds);
        return builder;
    }
}