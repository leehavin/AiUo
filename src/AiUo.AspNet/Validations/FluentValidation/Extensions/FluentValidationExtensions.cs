using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AiUo.AspNet.Validations.FluentValidation.Services;

namespace AiUo.AspNet.Validations.FluentValidation.Extensions;

/// <summary>
/// FluentValidation extensions for ASP.NET Core
/// </summary>
public static class FluentValidationExtensions
{
    /// <summary>
    /// Adds FluentValidation with attribute-based configuration support
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
    /// Adds FluentValidation with attribute-based configuration support and custom factory
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