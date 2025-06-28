using AiUo.AspNet.Validations.FluentValidation.Services;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using AiUo.AspNet.Validations.FluentValidation.Models;
using Microsoft.AspNetCore.Http;

namespace AiUo.AspNet.Validations.FluentValidation.Extensions;

/// <summary>
/// Model validation extension methods
/// </summary>
public static class ModelValidationExtensions
{
    private static IServiceProvider _serviceProvider;
    private static IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Configure the service provider for model validation
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    public static void ConfigureServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
    }

    /// <summary>
    /// Validates a model asynchronously using FluentValidation
    /// </summary>
    /// <typeparam name="T">The type of the model to validate</typeparam>
    /// <param name="model">The model instance to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    public static async Task<ValidationResult<T>> ValidateModelAsync<T>(
        this T model, 
        CancellationToken cancellationToken = default) where T : class
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        var validationService = GetValidationService();
        
        if (validationService == null)
            throw new InvalidOperationException("IValidationService not found. Make sure FluentValidation is properly configured and ConfigureServiceProvider is called.");

        return await validationService.ValidateAsync(model, cancellationToken);
    }

    private static IValidationService GetValidationService()
    {
        // Try to get from current HTTP context first
        if (_httpContextAccessor?.HttpContext != null)
        {
            return _httpContextAccessor.HttpContext.RequestServices.GetService<IValidationService>();
        }

        // Fallback to configured service provider
        if (_serviceProvider != null)
        {
            return _serviceProvider.GetService<IValidationService>();
        }

        return null;
    }
}