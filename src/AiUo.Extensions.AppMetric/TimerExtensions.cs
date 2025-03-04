using AiUo.Extensions.AppMetric.Formatting.ElasticSearch;
using AiUo.Extensions.AppMetric.Formatting.ElasticSearch.Client;
using App.Metrics;
using App.Metrics.Filtering;
using App.Metrics.Reporting.Abstractions;
using App.Metrics.Tagging;
using AspectCore.Configuration;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace AiUo.Extensions.AppMetric;

public static class TimerExtensions
{
    public static WebApplicationBuilder AddAppMetricEx(this WebApplicationBuilder builder, IConfiguration Configuration, string projectId)
    {
        ElasticSearchSettings elasticSearchSettings;
        string userName = Configuration["Serilog:WriteTo:ELKSink:Args:username"];
        string passwd = Configuration["Serilog:WriteTo:ELKSink:Args:password"];
        string esUrl = Configuration["Serilog:WriteTo:ELKSink:Args:nodeUris"]?.Split(";")?[0];
        if (esUrl == null)
        {
            return builder;
        }
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(passwd))
        {
            elasticSearchSettings = new ElasticSearchSettings(new Uri(esUrl), "appmetricss");
        }
        else
        {
            elasticSearchSettings = new ElasticSearchSettings(new Uri(esUrl), "appmetricss", userName, passwd);
        }
        builder.Services.AddTransient<TimerInterceptorAttribute>(provider => new TimerInterceptorAttribute());
        var predicatesDic = Configuration.GetSection("Metirc:AspNetMetrics:FilterRoute")
            .Get<Dictionary<string, string[]>>(); //
        if (predicatesDic != null)
        {
            builder.Services.ConfigureDynamicProxy(config =>
            {
                if (predicatesDic.ContainsKey("ForService"))
                {
                    foreach (string t in predicatesDic["ForService"])
                    {
                        config.Interceptors.AddTyped<TimerInterceptorAttribute>(Predicates.ForService(t));
                    }
                }
                if (predicatesDic.ContainsKey("ForMethod"))
                {
                    foreach (string t in predicatesDic["ForMethod"])
                    {
                        config.Interceptors.AddTyped<TimerInterceptorAttribute>(Predicates.ForMethod(t));
                    }
                }

                if (predicatesDic.ContainsKey("ForNameSpace"))
                {
                    foreach (string t in predicatesDic["ForNameSpace"])
                    {
                        config.Interceptors.AddTyped<TimerInterceptorAttribute>(Predicates.ForNameSpace(t));
                    }
                }

            });
        }
              
        builder.Host.UseServiceProviderFactory(new DynamicProxyServiceProviderFactory());

        bool isOpenMetrics = Convert.ToBoolean(Configuration["Metirc:AppMetrics:ReportingEnabled"]);
        //if (isOpenMetrics)
        {

            string environmentString = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            if (string.IsNullOrEmpty(environmentString))
                environmentString = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrEmpty(environmentString))
                environmentString = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            if (string.IsNullOrEmpty(environmentString))
            {
                environmentString = Configuration["AppMetrics:GlobalTags:MyEnv"];
            }

            Dictionary<string, string> tags = new Dictionary<string, string>();
            tags.Add("env", environmentString);
            tags.Add("app", projectId);

            string ElasticSearchIndex = elasticSearchSettings.Index;
            //Uri ElasticSearchUri = new Uri("https://10.0.128.11:9200");
            var reportFilter = new DefaultMetricsFilter();
            reportFilter.WithHealthChecks(false);
            //services.AddMetrics(builder.Configuration.GetSection("AppMetrics")).
            builder.Services.AddMetrics(o =>
                    {
                
                        o.MetricsEnabled = isOpenMetrics;
                        o.GlobalTags = new GlobalMetricTags(tags);
                        o.DefaultContextLabel = projectId;
                        o.ReportingEnabled = isOpenMetrics;
                    }
                ).
                AddJsonHealthSerialization().
                // AddJsonMetricsTextSerialization().
                AddElasticsearchMetricsTextSerialization(ElasticSearchIndex)
                .AddElasticsearchMetricsSerialization(ElasticSearchIndex).AddReporting(
                    factory =>
                    {
                        factory.AddElasticSearch(
                            new ElasticSearchReporterSettings
                            {
                                HttpPolicy = new HttpPolicy
                                {
                                    FailuresBeforeBackoff = 3,
                                    BackoffPeriod = TimeSpan.FromSeconds(5),
                                    Timeout = TimeSpan.FromSeconds(5)
                                },
                                ElasticSearchSettings = elasticSearchSettings,//new ElasticSearchSettings(ElasticSearchUri, ElasticSearchIndex, "elastic", "Z4jXF09ZWG6OcLvhX02I"),//
                                ReportInterval = TimeSpan.FromSeconds(5)
                            },
                            reportFilter);
                        //factory.AddElasticSearch(
                        //    ElasticSearchUri, ElasticSearchIndex,
                        //    reportFilter);
                    }).AddMetricsMiddleware(Configuration.GetSection("Metirc:AspNetMetrics"));
        }
       
        //builder.UseMiddleware<CurrentTimeMiddleware>();
        return builder;
    }

    public static void UseAppMetric(this WebApplication app)
    {
        //if (Convert.ToBoolean(app.Configuration["Metirc:AppMetrics:ReportingEnabled"]))
        {
            try
            {
                app.UseMiddleware<CurrentTimeMiddleware>();

                var reportFactory = app.Services.GetRequiredService<IReportFactory>();

                var metrics = app.Services.GetRequiredService<IMetrics>();
                var reporter = reportFactory.CreateReporter();

                app.Lifetime.ApplicationStarted.Register(() =>
                {
                    Task.Run(() => reporter.RunReports(metrics, app.Lifetime.ApplicationStopping),
                        app.Lifetime.ApplicationStopping);
                });
            }
            catch
            {
                    
            }
              
        }
        
    }
}