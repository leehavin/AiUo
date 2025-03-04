using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using AiUo.Configuration;
using AiUo.IP2Country;
using AiUo.Logging;

namespace AiUo;

public static class IP2CountryHostBuilderExtensions
{
    public static IHostBuilder AddIP2CountryEx(this IHostBuilder builder)
    {
        var section = ConfigUtil.GetSection<IP2CountrySection>();
        if (section == null || !section.Enabled)
            return builder;

        var watch = new Stopwatch();
        watch.Start();
        builder.ConfigureServices(services => 
        {
            var service = new IP2CountryService();
            service.Init();
            services.AddSingleton<IIP2CountryService>(service);
        });

        watch.Stop();
        LogUtil.Info("配置 => [IP2Country] [{ElapsedMilliseconds} 毫秒]", watch.ElapsedMilliseconds);
        return builder;
    }
}