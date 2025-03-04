using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Serilog;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using AiUo.Extensions.Nacos;
using AiUo.Extensions.Serilog;
using AiUo.Hosting;
using AiUo.Hosting.Common;
using AiUo.Hosting.Services;
using AiUo.Configuration;
using AiUo.Reflection;
using AiUo.Logging;
using AiUo.Net;

namespace AiUo;

public static class AiUoHostBuilderExtensions
{
    /// <summary>
    /// 应用程序中配置AiUo，优先使用应用程序的配置文件，其次使用AiUo.json
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="envString"></param>
    /// <returns></returns>
    public static IHostBuilder AddAiUoEx(this IHostBuilder builder, string envString = null)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        // 注册中文字符编码
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Logger
        if (Serilog.Log.Logger == null)
            SerilogUtil.CreateBootstrapLogger();
        builder.ConfigureLogging(logger => logger.ClearProviders());
        builder.ConfigureServices((context, services) =>
        {
            // DI
            DIUtil.InitServices(services);
            services.AddSingleton<IAssemblyContainer>(new AssemblyContainer());
            services.AddOptions();
            services.AddSingleton(new LoggerFactory().AddSerilog(Log.Logger)); // ILoggerFactory
            services.AddScoped<ILogBuilder>((sp) => // ILogBuilder
            {
                var ret = new LogBuilder("AiUo_CONTEXT");
                ret.IsContext = true;
                return ret;
            });
            services.AddHttpClient();

            // DistributedMemoryCache
            services.AddDistributedMemoryCache();
        });

        // Configuration
        var fileConfig = ConfigUtil.BuildConfiguration(envString);
        var configHelper = new ConfigSourceBuilder(builder, fileConfig);
        var configuration = configHelper.Build();
        ConfigUtil.InitConfiguration(configuration);
        builder.ConfigureAppConfiguration((context, builder) =>
        {
            context.Configuration = ConfigUtil.Configuration;
        });

        // ThreadPool
        if (ConfigUtil.Project.MinThreads > 0)
            ThreadPool.SetMinThreads(ConfigUtil.Project.MinThreads, ConfigUtil.Project.MinThreads);

        // Hosting
        builder.ConfigureServices((context, services) =>
        {
            var hostSection = ConfigUtil.GetSection<HostSection>();
            if (hostSection != null && hostSection.ShutdownTimeout > 0)
            {
                services.Configure<HostOptions>(opts =>
                {
                    opts.ShutdownTimeout = TimeSpan.FromSeconds(hostSection.ShutdownTimeout);
                });
            }

            services.AddSingleton<IAiUoHostLifetimeService>(HostingUtil.LifetimeService);
            services.AddSingleton<IAiUoHostTimerService>(HostingUtil.TimerService);
            services.AddSingleton<IAiUoHostRegisterService>(HostingUtil.RegisterService);

            if (hostSection != null && hostSection.RegisterEnabled)
            {
                HostingUtil.RegisterService.AddProvider(new RedisHostRegisterProvider());
                services.AddSingleton<IAiUoHostDataService>(new RedisHostDataService());
                if (configHelper.From == ConfigSourceFrom.File)
                    services.AddSingleton<IAiUoHostMicroService>(new RedisHostMicroService());
            }
            if (configHelper.From == ConfigSourceFrom.Nacos)
                services.AddSingleton<IAiUoHostMicroService>(new NacosHostMicroService());
            services.AddHostedService<AiUoHostedService>();
        });

        // HttpClient
        builder.ConfigureServices(services =>
        {
            var clientSection = ConfigUtil.GetSection<JsonHttpClientSection>();
            if (clientSection == null || clientSection.Clients.Count == 0)
                return;
            foreach (var client in clientSection.Clients)
            {
                var builder = services.AddHttpClient(client.Key, c =>
                {
                    if (!string.IsNullOrEmpty(client.Value.BaseAddress))
                        c.BaseAddress = new Uri(client.Value.BaseAddress);
                    if (client.Value.RequestHeaders.Count > 0)
                    {
                        foreach (var header in client.Value.RequestHeaders)
                        {
                            c.DefaultRequestHeaders.Add(header.Key, header.Value);
                        }
                    }
                    if (client.Value.Timeout > 0)
                        c.Timeout = TimeSpan.FromSeconds(client.Value.Timeout);
                }).SetHandlerLifetime(TimeSpan.FromMinutes(5));

                if (client.Value.Retry > 0)
                {
                    builder.AddPolicyHandler(HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .Or<TimeoutRejectedException>() // 若超时则抛出此异常
                        .WaitAndRetryAsync(client.Value.Retry, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
                    builder.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(10));

                }
            }
        });

        LogUtil.Info("配置 => [AiUo]");
        return builder;
    }
}