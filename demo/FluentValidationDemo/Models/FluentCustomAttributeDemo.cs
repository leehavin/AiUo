using AiUo.AspNet.Validations.FluentValidation.Attributes;
using System.ComponentModel.DataAnnotations;

namespace FluentValidationDemo.Models;

/// <summary>
/// FluentCustomAttribute 使用示例
/// </summary>
public class FluentCustomAttributeDemo
{
    /// <summary>
    /// 用户名 - 使用自定义验证：不能包含敏感词
    /// </summary>
    [FluentRequired("USER_NAME_REQUIRED", "用户名不能为空")]
    [FluentCustom(nameof(ValidateUserName), "USER_NAME_SENSITIVE", "用户名不能包含敏感词汇")]
    public string UserName { get; set; }

    /// <summary>
    /// 密码 - 使用自定义验证：必须包含特殊字符
    /// </summary>
    [FluentRequired("PASSWORD_REQUIRED", "密码不能为空")]
    [FluentCustom(nameof(ValidatePassword), "PASSWORD_SPECIAL_CHAR", "密码必须包含至少一个特殊字符(!@#$%^&*)")]
    public string Password { get; set; }

    /// <summary>
    /// 年龄 - 使用自定义验证：必须是偶数
    /// </summary>
    [FluentCustom(nameof(ValidateEvenAge), "AGE_MUST_BE_EVEN", "年龄必须是偶数")]
    public int Age { get; set; }

    /// <summary>
    /// 邮箱 - 使用自定义验证：不能是临时邮箱
    /// </summary>
    [FluentRequired("EMAIL_REQUIRED", "邮箱不能为空")]
    [FluentEmail("EMAIL_FORMAT", "邮箱格式不正确")]
    [FluentCustom(nameof(ValidateEmailDomain), "EMAIL_TEMP_DOMAIN", "不允许使用临时邮箱域名")]
    public string Email { get; set; }

    /// <summary>
    /// 自定义验证方法：检查用户名是否包含敏感词
    /// </summary>
    /// <param name="value">要验证的值</param>
    /// <returns>验证是否通过</returns>
    private static bool ValidateUserName(object value)
    {
        if (value is not string userName || string.IsNullOrEmpty(userName))
            return true; // 空值由其他验证器处理

        // 敏感词列表
        var sensitiveWords = new[] { "admin", "root", "test", "guest", "anonymous" };
        
        return !sensitiveWords.Any(word => userName.ToLower().Contains(word));
    }

    /// <summary>
    /// 自定义验证方法：检查密码是否包含特殊字符
    /// </summary>
    /// <param name="value">要验证的值</param>
    /// <returns>验证是否通过</returns>
    private static bool ValidatePassword(object value)
    {
        if (value is not string password || string.IsNullOrEmpty(password))
            return true; // 空值由其他验证器处理

        // 检查是否包含特殊字符
        var specialChars = "!@#$%^&*";
        return password.Any(c => specialChars.Contains(c));
    }

    /// <summary>
    /// 自定义验证方法：检查年龄是否为偶数
    /// </summary>
    /// <param name="value">要验证的值</param>
    /// <returns>验证是否通过</returns>
    private static bool ValidateEvenAge(object value)
    {
        if (value is int age)
        {
            return age % 2 == 0;
        }
        return true; // 非整数类型不验证
    }

    /// <summary>
    /// 自定义验证方法：检查邮箱域名是否为临时邮箱
    /// </summary>
    /// <param name="value">要验证的值</param>
    /// <returns>验证是否通过</returns>
    private static bool ValidateEmailDomain(object value)
    {
        if (value is not string email || string.IsNullOrEmpty(email))
            return true; // 空值由其他验证器处理

        // 临时邮箱域名列表
        var tempDomains = new[] 
        { 
            "10minutemail.com", 
            "tempmail.org", 
            "guerrillamail.com",
            "mailinator.com",
            "throwaway.email"
        };

        var domain = email.Split('@').LastOrDefault()?.ToLower();
        return domain != null && !tempDomains.Contains(domain);
    }
}

/// <summary>
/// 更复杂的自定义验证示例
/// </summary>
public class AdvancedCustomValidationDemo
{
    /// <summary>
    /// 产品代码 - 使用自定义验证方法
    /// </summary>
    [FluentRequired("PRODUCT_CODE_REQUIRED", "产品代码不能为空")]
    [FluentCustom(nameof(ValidateProductCodeWrapper), "PRODUCT_CODE_FORMAT", "产品代码格式不正确：必须是PRD-开头，后跟8位数字")]
    public string ProductCode { get; set; }

    /// <summary>
    /// 验证产品代码格式的包装方法
    /// </summary>
    /// <param name="value">要验证的值</param>
    /// <returns>验证是否通过</returns>
    private static bool ValidateProductCodeWrapper(object value)
    {
        return ValidateProductCode(value as string);
    }

    /// <summary>
    /// 验证产品代码格式
    /// </summary>
    /// <param name="productCode">产品代码</param>
    /// <returns>验证是否通过</returns>
    private static bool ValidateProductCode(string productCode)
    {
        if (string.IsNullOrEmpty(productCode))
            return true; // 空值由其他验证器处理

        // 格式：PRD-12345678
        if (!productCode.StartsWith("PRD-"))
            return false;

        var numberPart = productCode.Substring(4);
        return numberPart.Length == 8 && numberPart.All(char.IsDigit);
    }
}