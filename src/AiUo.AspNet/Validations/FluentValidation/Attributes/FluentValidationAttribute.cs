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
using AiUo.AspNet.Validations.FluentValidation.Services;

namespace AiUo.AspNet.Validations.FluentValidation.Attributes;

/// <summary>
/// FluentValidation特性的基类
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public abstract class FluentValidationAttribute : Attribute
{
    /// <summary>
    /// 获取或设置错误代码
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 获取或设置错误消息
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// 获取或设置验证顺序
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 初始化<see cref="FluentValidationAttribute"/>类的新实例
    /// </summary>
    /// <param name="code">The error code</param>
    /// <param name="message">The error message</param>
    protected FluentValidationAttribute(string code = null, string message = null)
    {
        Code = code ?? GResponseCodes.G_BAD_REQUEST;
        ErrorMessage = message;
    }

    /// <summary>
    /// 将验证规则应用到规则构建器
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
/// 自动FluentValidation模型验证的操作过滤器
/// </summary>
public sealed class FluentValidationActionFilter : IActionFilter
{
    private readonly ILogger<FluentValidationActionFilter> _logger;
    private readonly IAttributeBasedValidatorFactory _validatorFactory;

    /// <summary>
    /// 初始化<see cref="FluentValidationActionFilter"/>类的新实例
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
    /// 使用基于特性的验证来验证指定的模型
    /// </summary>
    /// <param name="model">The model to validate</param>
    /// <returns>The validation result</returns>
    private global::FluentValidation.Results.ValidationResult ValidateModel(object model)
    {
        var validator = _validatorFactory.CreateValidator(model.GetType());
        if (validator == null)
        {
            return new global::FluentValidation.Results.ValidationResult();
        }
        var context = new ValidationContext<object>(model);
        return validator.Validate(context);
    }
}