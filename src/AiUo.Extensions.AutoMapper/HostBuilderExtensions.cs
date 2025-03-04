using AiUo.Configuration;
using AiUo.Extensions.AutoMapper;
using AiUo.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace AiUo;

public static class AutoMapperHostBuilderExtensions
{
    public static IHostBuilder AddAutoMapperEx(this IHostBuilder builder)
    {
        var section = ConfigUtil.GetSection<AutoMapperSection>();
        if (section == null || ((section.Assemblies == null || section.Assemblies.Count == 0) && !section.AutoLoad))
            return builder;

        var watch = new Stopwatch();
        watch.Start();
        builder.ConfigureServices((context, services) =>
        {
            var registered = AutoMapperUtil.Register();
            if (registered)
            {
                services.TryAddSingleton(AutoMapperUtil.Expression);
                services.TryAddSingleton(AutoMapperUtil.Configuration);
                services.TryAddSingleton(AutoMapperUtil.Mapper);
            }
        });

        watch.Stop();
        var asm = section.Assemblies?.Count > 0
            ? string.Join('|', section.Assemblies)
            : "NULL";
        LogUtil.Info("配置 => [AutoMapper] Assemblies: {Assemblies} [{ElapsedMilliseconds} 毫秒]"
            , asm, watch.ElapsedMilliseconds);
        return builder;
    }
}