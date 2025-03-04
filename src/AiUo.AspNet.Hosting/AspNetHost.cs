using AiUo.AspNet;
using AiUo.Configuration;
using AiUo.Extensions.Nacos;
using AiUo.Extensions.Serilog;
using AiUo.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Diagnostics.NETCore.Client;
using System.Diagnostics;
using System.Reflection;
using System.Runtime; 

namespace AiUo;

public static class AspNetHost
{
    public static WebApplicationBuilder CreateBuilder(string envString = null, string[] args = null)
    {
        SerilogUtil.CreateBootstrapLogger();
        var builder = WebApplication.CreateBuilder(args);
        builder.Host
            .AddAiUoEx(envString)
            .AddSerilogEx()
            .AddAutoMapperEx()
            .AddRedisEx()
            .AddSqlSugarEx()
            .AddRabbitMQEx()
            .AddSnowflakeIdEx()
            .AddDbCachingEx()
            .AddIP2CountryEx()
            .AddAWSEx()
            .AddNacosEx()
            .AddOAuthEx()
            .AddHCaptchaEx();
        return builder;
    }
    internal static async Task<string> MapEnvPath()
    {
        var processInfos = DiagnosticsClient.GetPublishedProcesses()
            .Select(Process.GetProcessById)
            .Where(process => process != null)
            .Select(o => { return $"id:{o.Id} name:{o.ProcessName} threads:{o.Threads.Count}"; })
            .ToList();
        var lastBuildTime = File.GetLastWriteTimeUtc(Assembly.GetEntryAssembly()!.Location).AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");
        var startTime = ConfigUtil.Environment.StartUtcTime.UtcToCNString();
        string outputIp = null;
        try
        {
            outputIp = (await HttpClientExFactory.CreateClientEx().CreateAgent().AddUrl("http://api.ip.sb/ip").GetStringAsync()).ResultString.TrimEnd('\n');
        }
        catch { }
        var dict = new Dictionary<string, object>
        {
            { "服务启动时间", startTime },
            { "ConfigUtil.ServiceId", ConfigUtil.Service.ServiceId },
            { "ConfigUtil.ServiceUrl", ConfigUtil.Service.HostUrl },
            { "ConfigUtil.Environment.Name", ConfigUtil.Environment.Name },
            { "ConfigUtil.Environment.Type", ConfigUtil.Environment.Type.ToString() },
            { "header:Host", HttpContextEx.RequestHeaders.GetOrDefault("Host", null) },
            { "header:X-Forwarded-Proto",HttpContextEx.RequestHeaders.GetOrDefault("X-Forwarded-Proto", null)},
            { "header:Referer", HttpContextEx.RequestHeaders.GetOrDefault("Referer", null) },
            { "header:X-Real_IP", HttpContextEx.RequestHeaders.GetOrDefault("X-Real_IP", null) },
            { "header:X-Forwarded-For", HttpContextEx.RequestHeaders.GetOrDefault("X-Forwarded-For", null) },
            { "AspNetUtil.GetRequestBaseUrl()", AspNetUtil.GetRequestBaseUrl() },
            { "AspNetUtil.GetRefererUrl()", AspNetUtil.GetRefererUrl() },
            { "AspNetUtil.GetRemoteIpString()", AspNetUtil.GetRemoteIpString() },
            { "AppContext.BaseDirectory", AppContext.BaseDirectory },
            { "DiagnosticsClient.GetPublishedProcesses()", processInfos },
            { "分配的内存总量: GC.GetTotalMemory(false)-(gc-heap-size)", GC.GetTotalMemory(false) },
            { "GCSettings.IsServerGC", GCSettings.IsServerGC },
            { "服务器本机IP", NetUtil.GetLocalIPs() },
            { "服务器出口IP", outputIp },
            { "代码最后一次编译时间", lastBuildTime },
        };
        ThreadPool.GetAvailableThreads(out var worker, out var completion);
        dict.Add("ThreadPool.GetAvailableThreads()", $"workerThreads:{worker} completionPortThreads:{completion}");

        dict.Add("headers总量", HttpContextEx.Request.Headers.Count);
        foreach (var header in HttpContextEx.Request.Headers)
        {
            dict.Add($"headers.{header.Key}", header.Value);
        }

        return SerializerUtil.SerializeJson(dict);
    }

    /// <summary>
    /// 崩溃自动dump，环境变量
    /// COMPlus_DbgEnableMiniDump=1
    /// DOTNET_DbgMiniDumpType=2
    /// COMPlus_DbgMiniDumpName=./dumps/crash-%p-%e-%h-%t.dmp
    /// COMPlus_EnableCrashReport = 1
    /// </summary>
    /// <param name="dtype"></param>
    /// <returns></returns>
    internal static Task<string> MapDumpPath(DumpType dtype)
    {
        var processId = DiagnosticsClient.GetPublishedProcesses()
            .Select(Process.GetProcessById)
            .Where(process => process != null)
            .Select(x => x.Id).ToList().FirstOrDefault();
        if (processId == 0)
            return null;
        var section = ConfigUtil.GetSection<AspNetSection>();
        if (section == null || string.IsNullOrEmpty(section.DumpPath))
            return null;
        if (!Directory.Exists(section.DumpPath))
            Directory.CreateDirectory(section.DumpPath);
        var file = Path.Combine(section.DumpPath, $"{ConfigUtil.Project.ProjectId}.{dtype.ToString()}.{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.dmp");
        var client = new DiagnosticsClient(processId);
        client.WriteDumpAsync(dtype, file, false, CancellationToken.None);
        return Task.FromResult(file);
    }
}