using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AiUo.AspNet.Validations;

/// <summary>
/// FluentValidation服务扩展
/// </summary>
public static class FluentValidationExtensions
{
    /// <summary>
    /// 添加基于特性的FluentValidation支持
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="assemblies">要扫描的程序集</param>
    /// <returns></returns>
    public static IServiceCollection AddAttributeBasedFluentValidation(this IServiceCollection services, params Assembly[] assemblies)
    {
        // 添加FluentValidation服务
        services.AddValidatorsFromAssemblies(assemblies ?? new[] { Assembly.GetCallingAssembly() });
        
        // 注册基于特性的验证器服务（替代过时的IValidatorFactory）
        services.AddSingleton<AttributeBasedValidatorService>();
        
        // 配置MVC选项以禁用默认模型验证
        services.Configure<MvcOptions>(options =>
        {
            // 添加FluentValidation动作过滤器
            options.Filters.Add<FluentValidationActionFilter>();
        });
        
        return services;
    }
    
    /// <summary>
    /// 添加基于特性的FluentValidation支持（自动扫描当前程序集）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns></returns>
    public static IServiceCollection AddAttributeBasedFluentValidation(this IServiceCollection services)
    {
        return services.AddAttributeBasedFluentValidation(Assembly.GetCallingAssembly());
    }
    
    /// <summary>
    /// 验证模型并返回验证结果
    /// </summary>
    /// <typeparam name="T">模型类型</typeparam>
    /// <param name="model">要验证的模型</param>
    /// <returns>验证结果</returns>
    public static FluentValidationResult ValidateModel<T>(this T model) where T : class
    {
        var validator = FluentValidatorFactory.CreateValidator<T>();
        if (validator == null)
        {
            return new FluentValidationResult { IsValid = true };
        }
        
        var result = validator.Validate(model);
        return new FluentValidationResult
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(e => new FluentValidationError
            {
                PropertyName = e.PropertyName,
                ErrorMessage = e.ErrorMessage,
                ErrorCode = e.ErrorCode,
                AttemptedValue = e.AttemptedValue
            }).ToList()
        };
    }
    
    /// <summary>
    /// 异步验证模型并返回验证结果
    /// </summary>
    /// <typeparam name="T">模型类型</typeparam>
    /// <param name="model">要验证的模型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>验证结果</returns>
    public static async Task<FluentValidationResult> ValidateModelAsync<T>(this T model, CancellationToken cancellationToken = default) where T : class
    {
        var validator = FluentValidatorFactory.CreateValidator<T>();
        if (validator == null)
        {
            return new FluentValidationResult { IsValid = true };
        }
        
        var result = await validator.ValidateAsync(model, cancellationToken);
        return new FluentValidationResult
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(e => new FluentValidationError
            {
                PropertyName = e.PropertyName,
                ErrorMessage = e.ErrorMessage,
                ErrorCode = e.ErrorCode,
                AttemptedValue = e.AttemptedValue
            }).ToList()
        };
    }
}

/// <summary>
/// 基于特性的验证器服务（替代过时的IValidatorFactory）
/// </summary>
public class AttributeBasedValidatorService
{
    private readonly IServiceProvider _serviceProvider;
    
    public AttributeBasedValidatorService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public IValidator<T> GetValidator<T>()
    {
        return FluentValidatorFactory.CreateValidator<T>();
    }
    
    public IValidator GetValidator(Type type)
    {
        var method = typeof(FluentValidatorFactory).GetMethod(nameof(FluentValidatorFactory.CreateValidator))
            ?.MakeGenericMethod(type);
        return method?.Invoke(null, null) as IValidator;
    }
}

/// <summary>
/// FluentValidation验证结果
/// </summary>
public class FluentValidationResult
{
    public bool IsValid { get; set; }
    public List<FluentValidationError> Errors { get; set; } = new();
    
    /// <summary>
    /// 获取第一个错误信息
    /// </summary>
    public string FirstErrorMessage => Errors.FirstOrDefault()?.ErrorMessage;
    
    /// <summary>
    /// 获取指定属性的错误信息
    /// </summary>
    /// <param name="propertyName">属性名称</param>
    /// <returns></returns>
    public List<FluentValidationError> GetErrorsForProperty(string propertyName)
    {
        return Errors.Where(e => e.PropertyName == propertyName).ToList();
    }
    
    /// <summary>
    /// 转换为字典格式
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, List<string>> ToDictionary()
    {
        return Errors.GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToList());
    }
}

/// <summary>
/// FluentValidation验证错误
/// </summary>
public class FluentValidationError
{
    public string PropertyName { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorCode { get; set; }
    public object AttemptedValue { get; set; }
}

/// <summary>
/// 验证组
/// </summary>
public static class ValidationGroups
{
    public const string Create = "Create";
    public const string Update = "Update";
    public const string Delete = "Delete";
    public const string Query = "Query";
}

/// <summary>
/// 条件验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentWhenAttribute : FluentValidationAttribute
{
    public Func<object, bool> Condition { get; }
    public FluentValidationAttribute InnerAttribute { get; }
    
    public FluentWhenAttribute(Func<object, bool> condition, FluentValidationAttribute innerAttribute, string code = null, string message = null)
        : base(code, message)
    {
        Condition = condition;
        InnerAttribute = innerAttribute;
    }
    
    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        var rule = InnerAttribute.ApplyRule(ruleBuilder, propertyName);
        return rule.When(model => Condition(model));
    }
}

/// <summary>
/// 验证组特性
/// </summary>
public class FluentValidationGroupAttribute : Attribute
{
    public string[] Groups { get; }
    
    public FluentValidationGroupAttribute(params string[] groups)
    {
        Groups = groups ?? new string[0];
    }
}