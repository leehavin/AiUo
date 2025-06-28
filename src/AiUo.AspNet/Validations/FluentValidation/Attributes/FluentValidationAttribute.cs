using AiUo.Net;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace AiUo.AspNet.Validations.FluentValidation.Attributes;

/// <summary>
/// Base class for FluentValidation attributes
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public abstract class FluentValidationAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the error code
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the error message
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the validation order
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationAttribute"/> class
    /// </summary>
    /// <param name="code">The error code</param>
    /// <param name="message">The error message</param>
    protected FluentValidationAttribute(string code = null, string message = null)
    {
        Code = code ?? GResponseCodes.G_BAD_REQUEST;
        ErrorMessage = message;
    }

    /// <summary>
    /// Applies the validation rule to the rule builder
    /// </summary>
    /// <typeparam name="T">The model type</typeparam>
    /// <typeparam name="TProperty">The property type</typeparam>
    /// <param name="ruleBuilder">The rule builder</param>
    /// <param name="propertyName">The property name</param>
    /// <returns>The rule builder options</returns>
    public abstract IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName);
}

/// <summary>
/// Action filter for automatic FluentValidation model validation
/// </summary>
public sealed class FluentValidationActionFilter : IActionFilter
{
    private readonly ILogger<FluentValidationActionFilter> _logger;
    private readonly IAttributeBasedValidatorFactory _validatorFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationActionFilter"/> class
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="validatorFactory">The validator factory</param>
    public FluentValidationActionFilter(
        ILogger<FluentValidationActionFilter> logger,
        IAttributeBasedValidatorFactory validatorFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validatorFactory = validatorFactory ?? throw new ArgumentNullException(nameof(validatorFactory));
    }

    /// <inheritdoc />
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context?.ActionArguments == null)
        {
            return;
        }

        foreach (var (parameterName, argument) in context.ActionArguments)
        {
            if (argument == null)
            {
                continue;
            }

            try
            {
                var validationResult = ValidateModel(argument);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning(
                        "Model validation failed for parameter '{ParameterName}' of type '{ModelType}'. Errors: {Errors}",
                        parameterName,
                        argument.GetType().Name,
                        string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

                    context.Result = new BadRequestObjectResult(new
                    {
                        Message = "Validation failed",
                        Errors = validationResult.Errors.Select(e => new
                        {
                            PropertyName = e.PropertyName,
                            ErrorMessage = e.ErrorMessage,
                            ErrorCode = e.ErrorCode
                        })
                    });
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error occurred during validation of parameter '{ParameterName}' of type '{ModelType}'",
                    parameterName,
                    argument.GetType().Name);

                context.Result = new BadRequestObjectResult(new
                {
                    Message = "Validation error occurred",
                    Error = "Internal validation error"
                });
                return;
            }
        }
    }

    /// <inheritdoc />
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No implementation needed for post-action processing
    }

    /// <summary>
    /// Validates the specified model using attribute-based validation
    /// </summary>
    /// <param name="model">The model to validate</param>
    /// <returns>The validation result</returns>
    private global::FluentValidation.Results.ValidationResult ValidateModel(object model)
    {
        var validator = _validatorFactory.GetValidator(model.GetType());
        var context = new ValidationContext<object>(model);
        return validator.Validate(context);
    }
}

/// <summary>
/// Interface for creating attribute-based validators
/// </summary>
public interface IAttributeBasedValidatorFactory
{
    /// <summary>
    /// Gets a validator for the specified model type
    /// </summary>
    /// <param name="modelType">The model type</param>
    /// <returns>The validator instance</returns>
    IValidator GetValidator(Type modelType);

    /// <summary>
    /// Gets a validator for the specified model type
    /// </summary>
    /// <typeparam name="T">The model type</typeparam>
    /// <returns>The validator instance</returns>
    IValidator<T> GetValidator<T>();
}

/// <summary>
/// Default implementation of the attribute-based validator factory
/// </summary>
internal sealed class DefaultAttributeBasedValidatorFactory : IAttributeBasedValidatorFactory
{
    private static readonly ConcurrentDictionary<Type, IValidator> ValidatorCache = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DefaultAttributeBasedValidatorFactory> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultAttributeBasedValidatorFactory"/> class
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <param name="logger">The logger</param>
    public DefaultAttributeBasedValidatorFactory(
        IServiceProvider serviceProvider,
        ILogger<DefaultAttributeBasedValidatorFactory> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public IValidator GetValidator(Type modelType)
    {
        if (modelType == null)
        {
            throw new ArgumentNullException(nameof(modelType));
        }

        return ValidatorCache.GetOrAdd(modelType, CreateValidatorInternal);
    }

    /// <inheritdoc />
    public IValidator<T> GetValidator<T>()
    {
        return (IValidator<T>)GetValidator(typeof(T));
    }

    /// <summary>
    /// Creates a validator instance for the specified model type
    /// </summary>
    /// <param name="modelType">The model type</param>
    /// <returns>The validator instance</returns>
    private IValidator CreateValidatorInternal(Type modelType)
    {
        try
        {
            var validatorType = typeof(AttributeBasedValidator<>).MakeGenericType(modelType);
            var validator = (IValidator?)Activator.CreateInstance(validatorType, _serviceProvider);
            
            if (validator == null)
            {
                throw new InvalidOperationException(
                    $"Failed to create validator instance for type '{modelType.Name}'");
            }

            _logger.LogDebug("Created validator for type '{ModelType}'", modelType.Name);
            return validator;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create validator for type '{ModelType}'", modelType.Name);
            throw new InvalidOperationException(
                $"Failed to create validator for type '{modelType.Name}': {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Attribute-based validator that dynamically creates validation rules from attributes
/// </summary>
/// <typeparam name="T">The model type to validate</typeparam>
public sealed class AttributeBasedValidator<T> : AbstractValidator<T>
{
    private readonly IServiceProvider? _serviceProvider;
    private readonly ILogger<AttributeBasedValidator<T>>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AttributeBasedValidator{T}"/> class
    /// </summary>
    public AttributeBasedValidator()
    {
        ConfigureValidationRules();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AttributeBasedValidator{T}"/> class
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection</param>
    public AttributeBasedValidator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider?.GetService<ILogger<AttributeBasedValidator<T>>>();
        ConfigureValidationRules();
    }

    /// <summary>
    /// Configures validation rules based on attributes
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
    /// Configures class-level validation attributes
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

        // Class-level validation can be implemented here if needed
        // For now, we focus on property-level validation
    }

    /// <summary>
    /// Configures property-level validation attributes
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
    /// Configures validation for a specific property
    /// </summary>
    /// <param name="property">The property info</param>
    /// <param name="attributes">The validation attributes</param>
    private void ConfigurePropertyValidation(PropertyInfo property, FluentValidationAttribute[] attributes)
    {
        try
        {
            // Create property expression: x => x.PropertyName
            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.Property(parameter, property);
            var lambda = Expression.Lambda(propertyAccess, parameter);

            // Get the RuleFor method for this property type
            var ruleForMethod = typeof(AbstractValidator<T>)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .First(m => m.Name == "RuleFor" && 
                           m.GetParameters().Length == 1 && 
                           m.IsGenericMethodDefinition)
                .MakeGenericMethod(property.PropertyType);

            // Create the rule builder
            var ruleBuilder = ruleForMethod.Invoke(this, new object[] { lambda });

            if (ruleBuilder == null)
            {
                throw new InvalidOperationException(
                    $"Failed to create rule builder for property '{property.Name}'");
            }

            // Apply each validation attribute
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
    /// Applies a validation attribute to a rule builder
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