using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
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
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentValidationGroupAttribute : FluentValidationAttribute
{
    public string GroupName { get; }

    public FluentValidationGroupAttribute(string groupName, string code = null, string message = null)
        : base(code, message)
    {
        GroupName = groupName;
    }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        // 验证组特性不直接应用规则，而是用于分组
        return ruleBuilder as IRuleBuilderOptions<T, TProperty>;
    }
}

/// <summary>
/// 异步验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentAsyncAttribute : FluentValidationAttribute
{
    public Type ValidatorType { get; }
    public string MethodName { get; }

    public FluentAsyncAttribute(Type validatorType, string methodName, string code = null, string message = null)
        : base(code, message)
    {
        ValidatorType = validatorType;
        MethodName = methodName;
    }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        return ruleBuilder.MustAsync(async (value, cancellation) =>
        {
            try
            {
                var validator = Activator.CreateInstance(ValidatorType);
                var method = ValidatorType.GetMethod(MethodName);
                if (method != null)
                {
                    var result = method.Invoke(validator, new object[] { value, cancellation });
                    if (result is Task<bool> taskResult)
                    {
                        return await taskResult;
                    }
                    return (bool)result;
                }
                return false;
            }
            catch
            {
                return false;
            }
        })
        .WithErrorCode(Code)
        .WithMessage(ErrorMessage ?? $"{propertyName}异步验证失败");
    }
}

/// <summary>
/// 集合验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentCollectionAttribute : FluentValidationAttribute
{
    public int MinCount { get; }
    public int MaxCount { get; }
    public bool AllowEmpty { get; }

    public FluentCollectionAttribute(int minCount = 0, int maxCount = int.MaxValue, bool allowEmpty = true, string code = null, string message = null)
        : base(code, message)
    {
        MinCount = minCount;
        MaxCount = maxCount;
        AllowEmpty = allowEmpty;
    }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        if (typeof(TProperty).IsAssignableFrom(typeof(IEnumerable)))
        {
            return ruleBuilder.Must(collection =>
            {
                if (collection == null) return AllowEmpty;
                
                var enumerable = collection as IEnumerable;
                var count = enumerable?.Cast<object>().Count() ?? 0;
                
                if (!AllowEmpty && count == 0) return false;
                return count >= MinCount && count <= MaxCount;
            })
            .WithErrorCode(Code)
            .WithMessage(ErrorMessage ?? $"{propertyName}集合元素数量必须在{MinCount}到{MaxCount}之间");
        }
        
        throw new InvalidOperationException($"FluentCollectionAttribute只能应用于集合类型的属性");
    }
}

/// <summary>
/// 依赖验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentDependentAttribute : FluentValidationAttribute
{
    public string DependentProperty { get; }
    public object ExpectedValue { get; }
    public ComparisonType Comparison { get; }

    public FluentDependentAttribute(string dependentProperty, object expectedValue, ComparisonType comparison = ComparisonType.Equal, string code = null, string message = null)
        : base(code, message)
    {
        DependentProperty = dependentProperty;
        ExpectedValue = expectedValue;
        Comparison = comparison;
    }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        return ruleBuilder.Must((model, value) =>
        {
            var dependentPropertyInfo = typeof(T).GetProperty(DependentProperty);
            if (dependentPropertyInfo == null) return true;
            
            var dependentValue = dependentPropertyInfo.GetValue(model);
            
            return Comparison switch
            {
                ComparisonType.Equal => Equals(dependentValue, ExpectedValue),
                ComparisonType.NotEqual => !Equals(dependentValue, ExpectedValue),
                ComparisonType.GreaterThan => Comparer.Default.Compare(dependentValue, ExpectedValue) > 0,
                ComparisonType.GreaterThanOrEqual => Comparer.Default.Compare(dependentValue, ExpectedValue) >= 0,
                ComparisonType.LessThan => Comparer.Default.Compare(dependentValue, ExpectedValue) < 0,
                ComparisonType.LessThanOrEqual => Comparer.Default.Compare(dependentValue, ExpectedValue) <= 0,
                _ => true
            };
        })
        .WithErrorCode(Code)
        .WithMessage(ErrorMessage ?? $"{propertyName}依赖于{DependentProperty}的值");
    }
}

/// <summary>
/// 比较类型枚举
/// </summary>
public enum ComparisonType
{
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual
}

/// <summary>
/// 复杂验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentComplexAttribute : FluentValidationAttribute
{
    public string ValidationExpression { get; }
    public string[] DependentProperties { get; }

    public FluentComplexAttribute(string validationExpression, string dependentProperties = null, string code = null, string message = null)
        : base(code, message)
    {
        ValidationExpression = validationExpression;
        DependentProperties = dependentProperties?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
    }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        return ruleBuilder.Must((model, value) =>
        {
            try
            {
                // 这里可以实现复杂的表达式验证逻辑
                // 为了简化，这里只做基本的非空验证
                return !string.IsNullOrEmpty(value?.ToString());
            }
            catch
            {
                return false;
            }
        })
        .WithErrorCode(Code)
        .WithMessage(ErrorMessage ?? $"{propertyName}复杂验证失败");
    }
}

/// <summary>
/// 条件验证增强特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentWhenAdvancedAttribute : FluentValidationAttribute
{
    public string PropertyName { get; }
    public object Value { get; }
    public ComparisonType Comparison { get; }
    public Type ValidatorType { get; }

    public FluentWhenAdvancedAttribute(string propertyName, object value, ComparisonType comparison = ComparisonType.Equal, Type validatorType = null, string code = null, string message = null)
        : base(code, message)
    {
        PropertyName = propertyName;
        Value = value;
        Comparison = comparison;
        ValidatorType = validatorType;
    }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        return ruleBuilder.Must((model, value) =>
        {
            var property = typeof(T).GetProperty(PropertyName);
            if (property == null) return true;
            
            var propertyValue = property.GetValue(model);
            bool conditionMet = Comparison switch
            {
                ComparisonType.Equal => Equals(propertyValue, Value),
                ComparisonType.NotEqual => !Equals(propertyValue, Value),
                ComparisonType.GreaterThan => Comparer.Default.Compare(propertyValue, Value) > 0,
                ComparisonType.GreaterThanOrEqual => Comparer.Default.Compare(propertyValue, Value) >= 0,
                ComparisonType.LessThan => Comparer.Default.Compare(propertyValue, Value) < 0,
                ComparisonType.LessThanOrEqual => Comparer.Default.Compare(propertyValue, Value) <= 0,
                _ => true
            };
            
            if (!conditionMet) return true; // 条件不满足时跳过验证
            
            // 如果指定了验证器类型，使用该验证器
            if (ValidatorType != null)
            {
                try
                {
                    var validator = Activator.CreateInstance(ValidatorType);
                    // 这里可以调用自定义验证器的方法
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            
            return !string.IsNullOrEmpty(value?.ToString());
        })
        .WithErrorCode(Code)
        .WithMessage(ErrorMessage ?? $"{propertyName}条件验证失败");
    }
}