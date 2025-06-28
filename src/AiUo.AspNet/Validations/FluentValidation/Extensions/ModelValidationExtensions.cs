using AiUo.AspNet.Validations.FluentValidation.Services;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using AiUo.AspNet.Validations.FluentValidation.Models;
using Microsoft.AspNetCore.Http;

namespace AiUo.AspNet.Validations.FluentValidation.Extensions;

/// <summary>
/// 模型验证扩展方法
/// </summary>
public static class ModelValidationExtensions
{
    private static IServiceProvider _serviceProvider;
    private static IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 配置模型验证的服务提供者
    /// </summary>
    /// <param name="serviceProvider">服务提供者</param>
    public static void ConfigureServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
    }

    /// <summary>
    /// 使用FluentValidation异步验证模型
    /// </summary>
    /// <typeparam name="T">要验证的模型类型</typeparam>
    /// <param name="model">要验证的模型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>验证结果</returns>
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