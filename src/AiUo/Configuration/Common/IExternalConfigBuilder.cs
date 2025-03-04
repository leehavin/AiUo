using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace AiUo.Configuration;

/// <summary>
/// 外部配置源提供者
/// </summary>
public interface IExternalConfigBuilder
{
    IConfiguration Build(IConfiguration fileConfig, IHostBuilder hostBuilder);
}