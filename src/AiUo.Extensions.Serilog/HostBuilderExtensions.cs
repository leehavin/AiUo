using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Diagnostics;
using AiUo.Configuration;
using AiUo.Extensions.Serilog;
using AiUo.Logging;

namespace AiUo;

public static class SerilogHostBuilderExtensions
{
    /// <summary>
    /// 设置Serilog为logging provider,
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHostBuilder AddSerilogEx(this IHostBuilder builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));
        var section = ConfigUtil.Configuration.GetSection("Serilog");
        if (section == null)
            return builder;

        var watch = new Stopwatch();
        watch.Start();
        // 配置Log.Logger
        var idxName = SetELKSinkIndexFormat(ConfigUtil.Configuration);
        var config = new LoggerConfiguration()
            .ReadFrom.Configuration(ConfigUtil.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty(SerilogUtil.EnvironmentPropertyName, $"[{ConfigUtil.Environment.Type.ToString()}]{ConfigUtil.Environment.Name}")
            .Enrich.WithProperty(SerilogUtil.ProjectIdPropertyName, ConfigUtil.Project?.ProjectId ?? "未知程序")
            .Enrich.WithProperty(SerilogUtil.ServiceIdPropertyName, ConfigUtil.Service.ServiceId ?? "未知服务")
            .Enrich.WithProperty(SerilogUtil.HostPropertyName, $"[{ConfigUtil.Service.HostApiType.ToString()}] {ConfigUtil.Service.HostIp}:{ConfigUtil.Service.HostPort} [http]{ConfigUtil.Service.HttpPort} [grpc]{ConfigUtil.Service.GrpcPort} [websocket]{ConfigUtil.Service.WebSocketPort}]")
            //.Enrich.WithProperty(SerilogUtil.IndexNamePropertyName, idxName)
            .Enrich.WithTemplateHash();
        Log.Logger = config.CreateLogger();
        builder.UseSerilog(Log.Logger);
        LogUtil.Init();

        // 启动Serilog内部调试
        //Serilog.Debugging.SelfLog.Enable(msg => System.Diagnostics.Debug.WriteLine(msg));
        //Serilog.Debugging.SelfLog.Enable(Console.Error);
        watch.Stop();
        LogUtil.Info("配置 => [Serilog] [{ElapsedMilliseconds} 毫秒]", watch.ElapsedMilliseconds);
        return builder;
    }
    private static string SetELKSinkIndexFormat(IConfiguration config)
    {
        string ret = null;
        var elk = config["Serilog:WriteTo:ELKSink:Name"];
        if (!string.IsNullOrEmpty(elk))
        {
            ret = config["Serilog:WriteTo:ELKSink:Args:indexFormat"];
            if (string.IsNullOrEmpty(ret))
            {
                var projectId = ConfigUtil.Project.ProjectId
                    .Replace('.', '_').Replace('-', '_');
                var env = ConfigUtil.Environment.Name?.ToLower()
                    .Replace('.', '_').Replace('-', '_');
                if (!string.IsNullOrEmpty(env))
                    env += "-";
                ret = $"idx-{projectId}-{env}{{0:yyyyMMdd}}";
                config["Serilog:WriteTo:ELKSink:Args:indexFormat"] = ret;
            }
        }
        return ret;
    }
}