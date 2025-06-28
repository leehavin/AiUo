using FluentValidation;
using System.Text.RegularExpressions;
using AiUo.AspNet.Validations.FluentValidation.Models;

namespace AiUo.AspNet.Validations.FluentValidation.Attributes;

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
/// URL验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentUrlAttribute : FluentValidationAttribute
{
    public FluentUrlAttribute(string code = null, string message = "URL格式不正确")
        : base(code, message) { }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        if (typeof(TProperty) == typeof(string))
        {
            var stringRuleBuilder = ruleBuilder as IRuleBuilder<T, string>;
            return stringRuleBuilder.Must(url =>
            {
                if (string.IsNullOrEmpty(url)) return true;
                return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
                       (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
            })
            .WithErrorCode(Code)
            .WithMessage(ErrorMessage ?? $"{propertyName}URL格式不正确") as IRuleBuilderOptions<T, TProperty>;
        }

        throw new InvalidOperationException($"FluentUrlAttribute只能应用于string类型的属性");
    }
}

/// <summary>
/// 信用卡验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentCreditCardAttribute : FluentValidationAttribute
{
    public FluentCreditCardAttribute(string code = null, string message = "信用卡号格式不正确")
        : base(code, message) { }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        if (typeof(TProperty) == typeof(string))
        {
            var stringRuleBuilder = ruleBuilder as IRuleBuilder<T, string>;
            return stringRuleBuilder.CreditCard()
                .WithErrorCode(Code)
                .WithMessage(ErrorMessage ?? $"{propertyName}信用卡号格式不正确") as IRuleBuilderOptions<T, TProperty>;
        }

        throw new InvalidOperationException($"FluentCreditCardAttribute只能应用于string类型的属性");
    }
}

/// <summary>
/// 枚举验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentEnumAttribute : FluentValidationAttribute
{
    public Type EnumType { get; }

    public FluentEnumAttribute(Type enumType, string code = null, string message = null)
        : base(code, message)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException("Type must be an enum", nameof(enumType));
        EnumType = enumType;
    }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        return ruleBuilder.IsInEnum()
            .WithErrorCode(Code)
            .WithMessage(ErrorMessage ?? $"{propertyName}必须是有效的枚举值");
    }
}

/// <summary>
/// 日期范围验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentDateRangeAttribute : FluentValidationAttribute
{
    public DateTime? MinDate { get; }
    public DateTime? MaxDate { get; }
    public bool AllowFuture { get; }
    public bool AllowPast { get; }

    public FluentDateRangeAttribute(string minDate = null, string maxDate = null, bool allowFuture = true, bool allowPast = true, string code = null, string message = null)
        : base(code, message)
    {
        MinDate = string.IsNullOrEmpty(minDate) ? null : DateTime.Parse(minDate);
        MaxDate = string.IsNullOrEmpty(maxDate) ? null : DateTime.Parse(maxDate);
        AllowFuture = allowFuture;
        AllowPast = allowPast;
    }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        if (typeof(TProperty) == typeof(DateTime) || typeof(TProperty) == typeof(DateTime?))
        {
            return ruleBuilder.Must(value =>
            {
                if (value == null) return true;
                var dateValue = (DateTime)(object)value;
                var now = DateTime.Now;

                if (!AllowFuture && dateValue > now) return false;
                if (!AllowPast && dateValue < now) return false;
                if (MinDate.HasValue && dateValue < MinDate.Value) return false;
                if (MaxDate.HasValue && dateValue > MaxDate.Value) return false;

                return true;
            })
            .WithErrorCode(Code)
            .WithMessage(ErrorMessage ?? $"{propertyName}日期不在允许的范围内");
        }

        throw new InvalidOperationException($"FluentDateRangeAttribute只能应用于DateTime类型的属性");
    }
}

/// <summary>
/// 文件扩展名验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentFileExtensionAttribute : FluentValidationAttribute
{
    public string[] AllowedExtensions { get; }

    public FluentFileExtensionAttribute(string allowedExtensions, string code = null, string message = null)
        : base(code, message)
    {
        AllowedExtensions = allowedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(ext => ext.Trim().ToLowerInvariant())
            .ToArray();
    }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        if (typeof(TProperty) == typeof(string))
        {
            var stringRuleBuilder = ruleBuilder as IRuleBuilder<T, string>;
            return stringRuleBuilder.Must(fileName =>
            {
                if (string.IsNullOrEmpty(fileName)) return true;
                var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
                return AllowedExtensions.Contains(extension);
            })
            .WithErrorCode(Code)
            .WithMessage(ErrorMessage ?? $"{propertyName}文件扩展名不被允许，允许的扩展名：{string.Join(", ", AllowedExtensions)}") as IRuleBuilderOptions<T, TProperty>;
        }

        throw new InvalidOperationException($"FluentFileExtensionAttribute只能应用于string类型的属性");
    }
}

/// <summary>
/// 中国身份证号验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentChineseIdCardAttribute : FluentValidationAttribute
{
    public FluentChineseIdCardAttribute(string code = null, string message = "身份证号格式不正确")
        : base(code, message) { }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        if (typeof(TProperty) == typeof(string))
        {
            var stringRuleBuilder = ruleBuilder as IRuleBuilder<T, string>;
            return stringRuleBuilder.Must(IsValidChineseIdCard)
                .WithErrorCode(Code)
                .WithMessage(ErrorMessage ?? $"{propertyName}身份证号格式不正确") as IRuleBuilderOptions<T, TProperty>;
        }

        throw new InvalidOperationException($"FluentChineseIdCardAttribute只能应用于string类型的属性");
    }

    private static bool IsValidChineseIdCard(string idCard)
    {
        if (string.IsNullOrEmpty(idCard)) return true;
        if (idCard.Length != 18) return false;

        // 验证前17位是否为数字
        for (int i = 0; i < 17; i++)
        {
            if (!char.IsDigit(idCard[i])) return false;
        }

        // 验证最后一位校验码
        var weights = new int[] { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };
        var checkCodes = new char[] { '1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2' };

        int sum = 0;
        for (int i = 0; i < 17; i++)
        {
            sum += (idCard[i] - '0') * weights[i];
        }

        var checkCode = checkCodes[sum % 11];
        return idCard[17] == checkCode;
    }
}

/// <summary>
/// 中国手机号验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentChinesePhoneAttribute : FluentValidationAttribute
{
    public FluentChinesePhoneAttribute(string code = null, string message = "手机号格式不正确")
        : base(code, message) { }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        if (typeof(TProperty) == typeof(string))
        {
            var stringRuleBuilder = ruleBuilder as IRuleBuilder<T, string>;
            return stringRuleBuilder.Matches(@"^1[3-9]\d{9}$")
                .WithErrorCode(Code)
                .WithMessage(ErrorMessage ?? $"{propertyName}手机号格式不正确") as IRuleBuilderOptions<T, TProperty>;
        }

        throw new InvalidOperationException($"FluentChinesePhoneAttribute只能应用于string类型的属性");
    }
}

/// <summary>
/// JSON格式验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentJsonAttribute : FluentValidationAttribute
{
    public FluentJsonAttribute(string code = null, string message = "JSON格式不正确")
        : base(code, message) { }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        if (typeof(TProperty) == typeof(string))
        {
            var stringRuleBuilder = ruleBuilder as IRuleBuilder<T, string>;
            return stringRuleBuilder.Must(json =>
            {
                if (string.IsNullOrEmpty(json)) return true;
                try
                {
                    System.Text.Json.JsonDocument.Parse(json);
                    return true;
                }
                catch
                {
                    return false;
                }
            })
            .WithErrorCode(Code)
            .WithMessage(ErrorMessage ?? $"{propertyName}JSON格式不正确") as IRuleBuilderOptions<T, TProperty>;
        }

        throw new InvalidOperationException($"FluentJsonAttribute只能应用于string类型的属性");
    }
}

/// <summary>
/// 统一社会信用代码验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentUnifiedSocialCreditCodeAttribute : FluentValidationAttribute
{
    public FluentUnifiedSocialCreditCodeAttribute(string code = null, string message = "统一社会信用代码格式不正确")
        : base(code, message) { }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        if (typeof(TProperty) == typeof(string))
        {
            var stringRuleBuilder = ruleBuilder as IRuleBuilder<T, string>;
            return stringRuleBuilder.Matches(@"^[0-9A-HJ-NPQRTUWXY]{2}\d{6}[0-9A-HJ-NPQRTUWXY]{10}$")
                .WithErrorCode(Code)
                .WithMessage(ErrorMessage ?? $"{propertyName}统一社会信用代码格式不正确") as IRuleBuilderOptions<T, TProperty>;
        }

        throw new InvalidOperationException($"FluentUnifiedSocialCreditCodeAttribute只能应用于string类型的属性");
    }
}

/// <summary>
/// 依赖验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentDependentAttribute : FluentValidationAttribute
{
    public string DependentProperty { get; }
    public object DependentValue { get; }
    public ComparisonType ComparisonType { get; }

    public FluentDependentAttribute(string dependentProperty, object dependentValue, ComparisonType comparisonType, string code = null, string message = null)
        : base(code, message)
    {
        DependentProperty = dependentProperty;
        DependentValue = dependentValue;
        ComparisonType = comparisonType;
    }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        return ruleBuilder.Must((model, value) =>
        {
            var dependentPropertyInfo = typeof(T).GetProperty(DependentProperty);
            if (dependentPropertyInfo == null) return true;

            var dependentPropertyValue = dependentPropertyInfo.GetValue(model);

            // 检查依赖条件
            bool dependentConditionMet = ComparisonType switch
            {
                ComparisonType.Equal => Equals(dependentPropertyValue, DependentValue),
                ComparisonType.NotEqual => !Equals(dependentPropertyValue, DependentValue),
                _ => false
            };

            // 如果依赖条件不满足，则跳过验证
            if (!dependentConditionMet) return true;

            // 如果依赖条件满足，则当前属性不能为空
            return value != null && !value.Equals(default(TProperty));
        })
        .WithErrorCode(Code)
        .WithMessage(ErrorMessage ?? $"{propertyName}是必需的");
    }
}

/// <summary>
/// 高级条件验证特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class FluentWhenAdvancedAttribute : FluentValidationAttribute
{
    public string DependentProperty { get; }
    public object DependentValue { get; }

    public FluentWhenAdvancedAttribute(string dependentProperty, object dependentValue, string code = null, string message = null)
        : base(code, message)
    {
        DependentProperty = dependentProperty;
        DependentValue = dependentValue;
    }

    public override IRuleBuilderOptions<T, TProperty> ApplyRule<T, TProperty>(
        IRuleBuilder<T, TProperty> ruleBuilder, string propertyName)
    {
        return ruleBuilder.Must((model, value) =>
        {
            var dependentPropertyInfo = typeof(T).GetProperty(DependentProperty);
            if (dependentPropertyInfo == null) return true;

            var dependentPropertyValue = dependentPropertyInfo.GetValue(model);

            // 如果依赖条件不满足，则跳过验证
            if (!Equals(dependentPropertyValue, DependentValue)) return true;

            // 如果依赖条件满足，则当前属性不能为空
            return value != null && !value.Equals(default(TProperty));
        })
        .WithErrorCode(Code)
        .WithMessage(ErrorMessage ?? $"{propertyName}是必需的");
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
        if (typeof(TProperty).IsAssignableFrom(typeof(System.Collections.IEnumerable)))
        {
            return ruleBuilder.Must(collection =>
            {
                if (collection == null) return AllowEmpty;

                var enumerable = collection as System.Collections.IEnumerable;
                var count = enumerable?.Cast<object>().Count() ?? 0;

                return count >= MinCount && count <= MaxCount;
            })
            .WithErrorCode(Code)
            .WithMessage(ErrorMessage ?? $"{propertyName}集合元素数量必须在{MinCount}-{MaxCount}之间");
        }

        throw new InvalidOperationException($"FluentCollectionAttribute只能应用于集合类型的属性");
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