using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AiUo.AspNet.Validations.FluentValidation.Services;

namespace AiUo.AspNet.Validations.FluentValidation.Extensions;

/// <summary>
/// ASP.NET Core的FluentValidation扩展
/// </summary>
public static class FluentValidationExtensions
{
    /// <summary>
    /// 添加支持基于特性配置的FluentValidation
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Optional configuration for FluentValidation options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddFluentValidationWithAttributes(
        this IServiceCollection services,
        Action<FluentValidationOptions>? configureOptions = null)
    {
        // Configure options
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<FluentValidationOptions>(_ => { });
        }
        
        // Validate configuration on startup
        services.AddSingleton<IStartupFilter, FluentValidationStartupFilter>();

        // Register core services
        services.TryAddSingleton<Services.IAttributeBasedValidatorFactory, AttributeBasedValidatorFactory>();
        services.TryAddScoped<IValidationService, ValidationService>();

        return services;
    }

    /// <summary>
    /// 添加支持基于特性配置和自定义工厂的FluentValidation
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Optional configuration for FluentValidation options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddFluentValidationWithAttributes<TFactory>(
        this IServiceCollection services,
        Action<FluentValidationOptions>? configureOptions = null)
        where TFactory : class, Services.IAttributeBasedValidatorFactory
    {
        // Configure options
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<FluentValidationOptions>(options => { });
        }

        // Register custom factory
        services.TryAddSingleton<Services.IAttributeBasedValidatorFactory, TFactory>();
        services.TryAddScoped<IValidationService, ValidationService>();

        return services;
    }
}