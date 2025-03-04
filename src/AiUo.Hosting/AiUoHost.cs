using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using AiUo.Extensions.Nacos;
using AiUo.Extensions.Serilog;

namespace AiUo;

public static class AiUoHost
{
    #region Host启动
    /// <summary>
    /// 创建默认Host并UseAiUo
    /// </summary>
    /// <param name="envString"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IHostBuilder CreateBuilder(string envString = null, string[] args = null)
    {
        SerilogUtil.CreateBootstrapLogger();
        var builder = Host.CreateDefaultBuilder(args)
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
            .AddNacosEx();
        return builder;
    }

    public static IHost CreateHost(string envString = null, string[] args = null)
    {
        return CreateBuilder(envString, args)
            .Build()
            .UseAiUo();
    }

    /// <summary>
    /// 非阻塞运行
    /// </summary>
    /// <param name="envString"></param>
    /// <param name="args"></param>
    public static void Start(string envString = null, string[] args = null)
        => CreateHost(envString, args).Start();

    /// <summary>
    /// 非阻塞运行
    /// </summary>
    /// <param name="envString"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task StartAsync(string envString = null, string[] args = null)
        => CreateHost(envString, args).StartAsync();

    /// <summary>
    /// 阻塞运行
    /// </summary>
    /// <param name="envString"></param>
    /// <param name="args"></param>
    public static void Run(string envString = null, string[] args = null)
        => CreateHost(envString, args).Run();

    /// <summary>
    /// 阻塞运行
    /// </summary>
    /// <param name="envString"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Task RunAsync(string envString = null, string[] args = null)
        => CreateHost(envString, args).RunAsync();
    #endregion
}