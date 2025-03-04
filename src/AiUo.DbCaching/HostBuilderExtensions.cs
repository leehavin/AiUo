using AiUo.Configuration;
using AiUo.DbCaching;
using AiUo.DbCaching.ChangeConsumers;
using AiUo.Hosting;
using AiUo.Hosting.Services;
using AiUo.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace AiUo;

public static class DbCachingHostBuilderExtensions
{
    public static IHostBuilder AddDbCachingEx(this IHostBuilder builder)
    {
        var section = ConfigUtil.GetSection<DbCachingSection>();
        if (section == null || !section.Enabled)
            return builder;

        var watch = new Stopwatch();
        watch.Start();
        builder.ConfigureServices(async (context, services) =>
        {
            var checkConsumer = new RedisDbCacheCheckConsumer();
            IDbCacheChangeConsumer changeConsumer = null;
            DbCacheUpdator updator = null;
            switch (section.PublishMode)
            {
                case DbCachingPublishMode.Redis:
                    changeConsumer = new RedisDbCacheChangeConsumer(section.RedisConnectionStringName);
                    updator = new DbCacheUpdator(DbCachingPublishMode.Redis);
                    break;
                case DbCachingPublishMode.MQ:
                    changeConsumer = new MQDbCacheChangeConsumer(section.MQConnectionStringName);
                    updator = new DbCacheUpdator(DbCachingPublishMode.MQ);
                    break;
                default:
                    throw new Exception("未知的DbCachingPublishMode");
            }
            HostingUtil.RegisterStarting(async () =>
            {
                await changeConsumer!.RegisterConsumer();
                checkConsumer.Register();
                // preload
                if (section.PreloadProviders != null && section.PreloadProviders.Count > 0)
                {
                    foreach (var providerType in section.PreloadProviders)
                    {
                        var type = Type.GetType(providerType);
                        var provider = Activator.CreateInstance(type) as IDbCachePreloadProvider;
                        foreach (var preload in provider.GetPreloadList())
                        {
                            DbCachingUtil.PreloadCache(preload.EntityType, preload.SplitDbKey);
                        }
                    }
                }
                LogUtil.Info("启动 => 内存缓存[DbCaching]");
            });
            HostingUtil.RegisterStopping(() =>
            {
                LogUtil.Info("停止 => 内存缓存[DbCaching]");
                return Task.CompletedTask;
            });
            await DetalRefleshTables(section, updator);

            services.AddSingleton(checkConsumer);
            services.AddSingleton(changeConsumer!);
            services.AddSingleton(updator);
        });


        watch.Stop();
        LogUtil.Info("配置 => [DbCaching] [{ElapsedMilliseconds} 毫秒]"
            , watch.ElapsedMilliseconds);
        return builder;
    }

    private static Task DetalRefleshTables(DbCachingSection section, DbCacheUpdator updator)
    {
        if (section.RefleshTables != null && section.RefleshTables.Count > 0)
        {
            foreach (var table in section.RefleshTables)
            {
                var key = DbCachingUtil.GetCacheKey(table.ConfigId, table.TableName);
                HostingUtil.RegisterTimer(new AiUoHostTimerItem
                {
                    Id = $"RefleshTables:{key}",
                    Title = "自动刷新内存缓存",
                    Interval = (int)TimeSpan.FromMinutes(table.Interval).TotalMilliseconds,
                    Callback = async (_) =>
                    {
                        var dataProvider = new PageDataProvider(table.ConfigId, table.TableName, section.RedisConnectionStringName);
                        await dataProvider.SetRedisValues();
                        // update
                        await updator.Execute(new DbCacheChangeMessage
                        {
                            PublishMode = section.PublishMode,
                            RedisConnectionStringName = section.RedisConnectionStringName,
                            MQConnectionStringName = section.MQConnectionStringName,
                            Changed = new List<DbCacheItem>
                            {
                                new DbCacheItem
                                {
                                    ConfigId = table.ConfigId,
                                    TableName = table.TableName,
                                }
                            }
                        });
                    }
                });
            }
        }
        return Task.CompletedTask;
    }
}