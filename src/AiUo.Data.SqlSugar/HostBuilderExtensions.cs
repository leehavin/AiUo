using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlSugar;
using System.Diagnostics;
using AiUo.Configuration;
using AiUo.Data.SqlSugar;
using AiUo.Logging;
using AiUo.Reflection;

namespace AiUo;

public static class SqlSugarHostBuilderExtensions
{
    public static IHostBuilder AddSqlSugarEx(this IHostBuilder builder)
    {
        var section = ConfigUtil.GetSection<SqlSugarSection>();
        if (section == null || !section.Enabled)
            return builder;


        var watch = new Stopwatch();
        watch.Start();
        builder.ConfigureServices((context, services) =>
        {
            // IDbConfigProvider
            var configProvider = !string.IsNullOrEmpty(section.DbConfigProvider)
                ? (IDbConfigProvider)ReflectionUtil.CreateInstance(section.DbConfigProvider)
                : new DefaultDbConfigProvider();
            services.AddSingleton(configProvider);

            // IDbSplitProvider
            var splitProvider = !string.IsNullOrEmpty(section.DbSplitProvider)
                ? (IDbSplitProvider)ReflectionUtil.CreateInstance(section.DbSplitProvider)
                : new DefaultSplitProvider();
            services.AddSingleton(splitProvider);

            services.AddSingleton<ISqlSugarClient>(sp =>
            {
                var provider = sp.GetRequiredService<IDbConfigProvider>();
                var config = provider.GetConfig(section.DefaultConnectionStringName);
                if (config == null)
                    throw new Exception($"配置SqlSugar:ConnectionStrings没有找到默认连接。name:{section.DefaultConnectionStringName} type:{provider.GetType().FullName}");
                var ret = new SqlSugarScope(config, db =>
                {
                    DbUtil.InitDb(db, config);
                });
                // 分库分表
                ret.CurrentConnectionConfig.ConfigureExternalServices.SplitTableService
                    = sp.GetRequiredService<IDbSplitProvider>().SplitTable();
                return ret;
            });
        });
        watch.Stop();
        LogUtil.Info("配置 => [SqlSugar] [{ElapsedMilliseconds} 毫秒]", watch.ElapsedMilliseconds);
        return builder;
    }
}