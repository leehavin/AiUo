using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AiUo.AspNet.Validations.FluentValidation.Attributes;
using AiUo.AspNet.Validations.FluentValidation.Services;

namespace AiUo.AspNet.Validations.FluentValidation.Validators;

/// <summary>
/// 基于特性的验证器，自动从特性配置验证规则
/// </summary>
/// <typeparam name="T">The type being validated</typeparam>
public sealed class AttributeBasedValidator<T> : AbstractValidator<T>
{
    private readonly IServiceProvider? _serviceProvider;
    private readonly ILogger<AttributeBasedValidator<T>>? _logger;
    private readonly FluentValidationOptions _options;

    /// <summary>
    /// 初始化<see cref="AttributeBasedValidator{T}"/>类的新实例
    /// </summary>
    /// <param name="options">The validation options</param>
    /// <param name="serviceProvider">Optional service provider for dependency injection</param>
    public AttributeBasedValidator(
        FluentValidationOptions options,
        IServiceProvider? serviceProvider = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _serviceProvider = serviceProvider;
        _logger = serviceProvider?.GetService<ILogger<AttributeBasedValidator<T>>>();
        
        // Validate options
        _options.Validate();
        
        ConfigureValidationRules();
    }

    /// <summary>
    /// 使用默认选项初始化<see cref="AttributeBasedValidator{T}"/>类的新实例
    /// </summary>
    public AttributeBasedValidator() : this(new FluentValidationOptions())
    {
    }

    /// <summary>
    /// 基于特性配置验证规则
    /// </summary>
    private void ConfigureValidationRules()
    {
        try
        {
            var modelType = typeof(T);
            _logger?.LogDebug("Configuring validation rules for type '{ModelType}'", modelType.Name);

            ConfigureClassLevelValidation(modelType);
            ConfigurePropertyLevelValidation(modelType);

            _logger?.LogDebug("Successfully configured validation rules for type '{ModelType}'", modelType.Name);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to configure validation rules for type '{ModelType}'", typeof(T).Name);
            throw new InvalidOperationException(
                $"Failed to configure validation rules for type '{typeof(T).Name}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 配置类级别的验证特性
    /// </summary>
    /// <param name="modelType">The model type</param>
    private void ConfigureClassLevelValidation(Type modelType)
    {
        var classAttributes = modelType
            .GetCustomAttributes<FluentValidationAttribute>(inherit: true)
            .OrderBy(attr => attr.Order)
            .ToArray();

        if (classAttributes.Length == 0)
        {
            return;
        }

        _logger?.LogDebug("Found {Count} class-level validation attributes for type '{ModelType}'",
            classAttributes.Length, modelType.Name);
    }

    /// <summary>
    /// 配置属性级别的验证特性
    /// </summary>
    /// <param name="modelType">The model type</param>
    private void ConfigurePropertyLevelValidation(Type modelType)
    {
        var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var attributes = property
                .GetCustomAttributes<FluentValidationAttribute>(inherit: true)
                .OrderBy(attr => attr.Order)
                .ToArray();

            if (attributes.Length == 0)
            {
                continue;
            }

            _logger?.LogDebug("Configuring {Count} validation attributes for property '{PropertyName}' of type '{ModelType}'",
                attributes.Length, property.Name, modelType.Name);

            ConfigurePropertyValidation(property, attributes);
        }
    }

    /// <summary>
    /// 为特定属性配置验证
    /// </summary>
    /// <param name="property">The property info</param>
    /// <param name="attributes">The validation attributes</param>
    private void ConfigurePropertyValidation(PropertyInfo property, FluentValidationAttribute[] attributes)
    {
        try
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.Property(parameter, property);
            var lambda = Expression.Lambda(propertyAccess, parameter);

            var ruleForMethod = typeof(AbstractValidator<T>)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .First(m => m.Name == "RuleFor" && 
                           m.GetParameters().Length == 1 && 
                           m.IsGenericMethodDefinition)
                .MakeGenericMethod(property.PropertyType);

            var ruleBuilder = ruleForMethod.Invoke(this, new object[] { lambda });

            if (ruleBuilder == null)
            {
                throw new InvalidOperationException(
                    $"Failed to create rule builder for property '{property.Name}'");
            }

            foreach (var attribute in attributes)
            {
                ApplyValidationAttribute(attribute, ruleBuilder, property);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to configure validation for property '{PropertyName}'", property.Name);
            throw new InvalidOperationException(
                $"Failed to configure validation for property '{property.Name}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 将验证特性应用到规则构建器
    /// </summary>
    /// <param name="attribute">The validation attribute</param>
    /// <param name="ruleBuilder">The rule builder</param>
    /// <param name="property">The property info</param>
    private void ApplyValidationAttribute(
        FluentValidationAttribute attribute,
        object ruleBuilder,
        PropertyInfo property)
    {
        try
        {
            // Get the ApplyRule method for this property type
            var applyRuleMethod = attribute.GetType()
                .GetMethod(nameof(FluentValidationAttribute.ApplyRule))
                ?.MakeGenericMethod(typeof(T), property.PropertyType);

            if (applyRuleMethod == null)
            {
                throw new InvalidOperationException(
                    $"ApplyRule method not found on attribute type '{attribute.GetType().Name}'");
            }

            // Apply the validation rule
            applyRuleMethod.Invoke(attribute, new[] { ruleBuilder, property.Name });

            _logger?.LogDebug("Applied validation attribute '{AttributeType}' to property '{PropertyName}'",
                attribute.GetType().Name, property.Name);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex,
                "Failed to apply validation attribute '{AttributeType}' to property '{PropertyName}'",
                attribute.GetType().Name, property.Name);
            throw;
        }
    }
}