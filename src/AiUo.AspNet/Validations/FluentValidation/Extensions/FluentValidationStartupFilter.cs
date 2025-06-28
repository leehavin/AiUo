using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AiUo.AspNet.Validations.FluentValidation.Services;

namespace AiUo.AspNet.Validations.FluentValidation.Extensions;

/// <summary>
/// 启动过滤器，用于在应用程序启动时验证FluentValidation配置
/// </summary>
internal sealed class FluentValidationStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            // Validate configuration during startup
            var serviceProvider = app.ApplicationServices;
            var logger = serviceProvider.GetRequiredService<ILogger<FluentValidationStartupFilter>>();
            
            try
            {
                var options = serviceProvider.GetRequiredService<IOptions<FluentValidationOptions>>();
                options.Value.Validate();
                
                logger.LogInformation("FluentValidation configuration validated successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "FluentValidation configuration validation failed");
                throw;
            }
            
            next(app);
        };
    }
}