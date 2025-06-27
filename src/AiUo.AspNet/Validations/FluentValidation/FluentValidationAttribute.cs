using AiUo.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq.Expressions;
using System.Reflection;

namespace AiUo.AspNet.Validations;

/// <summary>
/// FluentValidation特性基类，用于基于attribute的验证实现
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public abstract class FluentValidationAttribute : Attribute
{
    public string Code { get; set; }
    public string ErrorMessage { get; set; }
    public int Order { get; set; } = 0;

    protected FluentValidationAttribute(string code = null, string message = null)
    {
        Code = code ?? GResponseCodes.G_BAD_REQUEST;
        ErrorMessage = message;
    }

    /// <summary>
    /// 应用验证规则到FluentValidation规则构建器
    /// </summary>
    /// <typeparam name="T">验证对象类型</typeparam>
    /// <typeparam name="TProperty">属性类型</typeparam>
    /// <param name="ruleBuilder">规则构建器</param>
    /// <param name="propertyName">属性名称</param>
    /// <returns></returns>
    public abstract IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName);
}

/// <summary>
/// FluentValidation动作过滤器，用于自动验证模型
/// </summary>
public class FluentValidationActionFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var parameter in context.ActionArguments)
        {
            if (parameter.Value != null)
            {
                var validator = FluentValidatorFactory.CreateValidator(parameter.Value.GetType());
                if (validator != null)
                {
                    var validationResult = validator.Validate(new ValidationContext<object>(parameter.Value));
                    if (!validationResult.IsValid)
                    {
                        var errors = validationResult.Errors.Select(e => new
                        {
                            Field = e.PropertyName,
                            Message = e.ErrorMessage,
                            Code = e.ErrorCode
                        }).ToList();

                        context.Result = new BadRequestObjectResult(new
                        {
                            Code = GResponseCodes.G_BAD_REQUEST,
                            Message = "验证失败",
                            Errors = errors
                        });
                        return;
                    }
                }
            }
        }
        base.OnActionExecuting(context);
    }
}

/// <summary>
/// FluentValidation验证器工厂
/// </summary>
public static class FluentValidatorFactory
{
    private static readonly Dictionary<Type, IValidator> _validators = new();

    /// <summary>
    /// 创建指定类型的验证器
    /// </summary>
    /// <param name="modelType">模型类型</param>
    /// <returns></returns>
    public static IValidator CreateValidator(Type modelType)
    {
        if (_validators.TryGetValue(modelType, out var cachedValidator))
        {
            return cachedValidator;
        }

        var validatorType = typeof(AttributeBasedValidator<>).MakeGenericType(modelType);
        var validator = (IValidator)Activator.CreateInstance(validatorType);
        
        _validators[modelType] = validator;
        return validator;
    }

    /// <summary>
    /// 创建指定类型的泛型验证器
    /// </summary>
    /// <typeparam name="T">模型类型</typeparam>
    /// <returns></returns>
    public static IValidator<T> CreateValidator<T>()
    {
        return (IValidator<T>)CreateValidator(typeof(T));
    }
}

/// <summary>
/// 基于特性的验证器实现
/// </summary>
/// <typeparam name="T">验证对象类型</typeparam>
public class AttributeBasedValidator<T> : AbstractValidator<T>
{
    public AttributeBasedValidator()
    {
        ConfigureRules();
    }

    private void ConfigureRules()
    {
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var validationAttributes = property.GetCustomAttributes<FluentValidationAttribute>()
                .OrderBy(attr => attr.Order)
                .ToArray();

            if (validationAttributes.Any())
            {
                ConfigurePropertyRules(property, validationAttributes);
            }
        }

        // 处理类级别的验证特性
        var classValidationAttributes = type.GetCustomAttributes<FluentValidationAttribute>()
            .OrderBy(attr => attr.Order)
            .ToArray();

        foreach (var attr in classValidationAttributes)
        {
            // 类级别验证可以在这里处理自定义逻辑
        }
    }

    private void ConfigurePropertyRules(PropertyInfo property, FluentValidationAttribute[] attributes)
    {
        var propertyType = property.PropertyType;
        
        // 使用反射创建 RuleFor 表达式
        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.Property(parameter, property);
        var lambda = Expression.Lambda(propertyAccess, parameter);
        
        // 获取 RuleFor 方法
        var ruleForMethod = typeof(AbstractValidator<T>)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == "RuleFor" && m.GetParameters().Length == 1);
            
        if (ruleForMethod != null)
        {
            var genericMethod = ruleForMethod.MakeGenericMethod(propertyType);
            var ruleBuilder = genericMethod.Invoke(this, new object[] { lambda });

            foreach (var attr in attributes)
            {
                var applyRuleMethod = attr.GetType()
                    .GetMethod("ApplyRule")
                    ?.MakeGenericMethod(typeof(T), propertyType);

                if (applyRuleMethod != null)
                {
                    ruleBuilder = applyRuleMethod.Invoke(attr, new[] { ruleBuilder, property.Name });
                }
            }
        }
    }


}