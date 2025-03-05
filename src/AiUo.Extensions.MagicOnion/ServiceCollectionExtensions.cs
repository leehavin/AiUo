using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AiUo.Extensions.MagicOnion;

/// <summary>
/// MagicOnion服务集合扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加MagicOnion客户端
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="options">配置选项</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddMagicOnionClient(this IServiceCollection services, Action<MagicOnionOptions> options)
    {
        services.Configure(options);
        services.AddSingleton<MagicOnionClientFactory>();
        return services;
    }
    /// <summary>
    /// 添加MagicOnion服务端
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="options">配置选项</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddMagicOnionServer(this IServiceCollection services, Action<MagicOnionOptions> options)
    {
        services.Configure(options);
        services.AddGrpc();
        services.AddMagicOnion();
        return services;
    }
}