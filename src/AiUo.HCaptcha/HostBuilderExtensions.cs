using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using AiUo.Configuration;
using AiUo.HCaptcha;
using AiUo.Logging;

namespace AiUo;

public static class HCaptchaHostBuilderExtensions
{
    public static IHostBuilder AddHCaptchaEx(this IHostBuilder builder)
    {
        var section = ConfigUtil.GetSection<HCaptchaSection>();
        if (section == null || !section.Enabled) 
            return builder;

        var watch = new Stopwatch();
        watch.Start();
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IHCaptchaService>(new HCaptchaService());
        });

        watch.Stop();
        LogUtil.Info("配置 => [HCaptcha] [{ElapsedMilliseconds} 毫秒]", watch.ElapsedMilliseconds);
        return builder;
    }
}