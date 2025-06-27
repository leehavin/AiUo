using FluentValidation;
using System.Text.RegularExpressions;

namespace AiUo.AspNet.Validations;

/// <summary>
/// 必填验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentRequiredAttribute : FluentValidationAttribute
{
    public FluentRequiredAttribute(string code = null, string message = "字段不能为空")
        : base(code, message) { }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        return ruleBuilder.NotEmpty()
            .WithErrorCode(Code)
            .WithMessage(ErrorMessage ?? $"{propertyName}不能为空");
    }
}

/// <summary>
/// 最小长度验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentMinLengthAttribute : FluentValidationAttribute
{
    public int MinLength { get; }

    public FluentMinLengthAttribute(int minLength, string code = null, string message = null)
        : base(code, message)
    {
        MinLength = minLength;
    }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        if (typeof(TProperty) == typeof(string))
        {
            var stringRuleBuilder = ruleBuilder as IRuleBuilder<T, string>;
            return stringRuleBuilder.MinimumLength(MinLength)
                .WithErrorCode(Code)
                .WithMessage(ErrorMessage ?? $"{propertyName}最小长度为{MinLength}") as IRuleBuilderOptions<T, TProperty>;
        }
        
        throw new InvalidOperationException($"FluentMinLengthAttribute只能应用于string类型的属性");
    }
}

/// <summary>
/// 最大长度验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentMaxLengthAttribute : FluentValidationAttribute
{
    public int MaxLength { get; }

    public FluentMaxLengthAttribute(int maxLength, string code = null, string message = null)
        : base(code, message)
    {
        MaxLength = maxLength;
    }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        if (typeof(TProperty) == typeof(string))
        {
            var stringRuleBuilder = ruleBuilder as IRuleBuilder<T, string>;
            return stringRuleBuilder.MaximumLength(MaxLength)
                .WithErrorCode(Code)
                .WithMessage(ErrorMessage ?? $"{propertyName}最大长度为{MaxLength}") as IRuleBuilderOptions<T, TProperty>;
        }
        
        throw new InvalidOperationException($"FluentMaxLengthAttribute只能应用于string类型的属性");
    }
}

/// <summary>
/// 长度范围验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentLengthAttribute : FluentValidationAttribute
{
    public int MinLength { get; }
    public int MaxLength { get; }

    public FluentLengthAttribute(int minLength, int maxLength, string code = null, string message = null)
        : base(code, message)
    {
        MinLength = minLength;
        MaxLength = maxLength;
    }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        if (typeof(TProperty) == typeof(string))
        {
            var stringRuleBuilder = ruleBuilder as IRuleBuilder<T, string>;
            return stringRuleBuilder.Length(MinLength, MaxLength)
                .WithErrorCode(Code)
                .WithMessage(ErrorMessage ?? $"{propertyName}长度必须在{MinLength}到{MaxLength}之间") as IRuleBuilderOptions<T, TProperty>;
        }
        
        throw new InvalidOperationException($"FluentLengthAttribute只能应用于string类型的属性");
    }
}

/// <summary>
/// 数值范围验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentRangeAttribute : FluentValidationAttribute
{
    public object Minimum { get; }
    public object Maximum { get; }

    public FluentRangeAttribute(int minimum, int maximum, string code = null, string message = null)
        : base(code, message)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    public FluentRangeAttribute(double minimum, double maximum, string code = null, string message = null)
        : base(code, message)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        if (typeof(TProperty).IsNumericType())
        {
            return ruleBuilder.Must(value => 
            {
                if (value == null) return true;
                var comparable = value as IComparable;
                return comparable != null && 
                       comparable.CompareTo(Minimum) >= 0 && 
                       comparable.CompareTo(Maximum) <= 0;
            })
            .WithErrorCode(Code)
            .WithMessage(ErrorMessage ?? $"{propertyName}必须在{Minimum}到{Maximum}之间");
        }
        
        throw new InvalidOperationException($"FluentRangeAttribute只能应用于数值类型的属性");
    }
}

/// <summary>
/// 正则表达式验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentRegularExpressionAttribute : FluentValidationAttribute
{
    public string Pattern { get; }
    public RegexOptions Options { get; }

    public FluentRegularExpressionAttribute(string pattern, string code = null, string message = null, RegexOptions options = RegexOptions.None)
        : base(code, message)
    {
        Pattern = pattern;
        Options = options;
    }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        if (typeof(TProperty) == typeof(string))
        {
            var stringRuleBuilder = ruleBuilder as IRuleBuilder<T, string>;
            return stringRuleBuilder.Matches(Pattern, Options)
                .WithErrorCode(Code)
                .WithMessage(ErrorMessage ?? $"{propertyName}格式不正确") as IRuleBuilderOptions<T, TProperty>;
        }
        
        throw new InvalidOperationException($"FluentRegularExpressionAttribute只能应用于string类型的属性");
    }
}

/// <summary>
/// 邮箱验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentEmailAttribute : FluentValidationAttribute
{
    public FluentEmailAttribute(string code = null, string message = "邮箱格式不正确")
        : base(code, message) { }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        if (typeof(TProperty) == typeof(string))
        {
            var stringRuleBuilder = ruleBuilder as IRuleBuilder<T, string>;
            return stringRuleBuilder.EmailAddress()
                .WithErrorCode(Code)
                .WithMessage(ErrorMessage ?? $"{propertyName}邮箱格式不正确") as IRuleBuilderOptions<T, TProperty>;
        }
        
        throw new InvalidOperationException($"FluentEmailAttribute只能应用于string类型的属性");
    }
}

/// <summary>
/// 比较验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentCompareAttribute : FluentValidationAttribute
{
    public string OtherProperty { get; }

    public FluentCompareAttribute(string otherProperty, string code = null, string message = null)
        : base(code, message)
    {
        OtherProperty = otherProperty;
    }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        return ruleBuilder.Must((model, value) =>
        {
            var otherPropertyInfo = typeof(T).GetProperty(OtherProperty);
            if (otherPropertyInfo == null) return false;
            
            var otherValue = otherPropertyInfo.GetValue(model);
            return Equals(value, otherValue);
        })
        .WithErrorCode(Code)
        .WithMessage(ErrorMessage ?? $"{propertyName}必须与{OtherProperty}相同");
    }
}

/// <summary>
/// 自定义验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentCustomAttribute : FluentValidationAttribute
{
    public Func<object, bool> ValidationFunc { get; }

    public FluentCustomAttribute(Func<object, bool> validationFunc, string code = null, string message = null)
        : base(code, message)
    {
        ValidationFunc = validationFunc;
    }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        return ruleBuilder.Must(value => ValidationFunc(value))
            .WithErrorCode(Code)
            .WithMessage(ErrorMessage ?? $"{propertyName}验证失败");
    }
}

/// <summary>
/// 类型扩展方法
/// </summary>
public static class TypeExtensions
{
    public static bool IsNumericType(this Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        return underlyingType == typeof(byte) ||
               underlyingType == typeof(sbyte) ||
               underlyingType == typeof(short) ||
               underlyingType == typeof(ushort) ||
               underlyingType == typeof(int) ||
               underlyingType == typeof(uint) ||
               underlyingType == typeof(long) ||
               underlyingType == typeof(ulong) ||
               underlyingType == typeof(float) ||
               underlyingType == typeof(double) ||
               underlyingType == typeof(decimal);
    }
}