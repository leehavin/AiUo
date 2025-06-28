using System;

namespace AiUo.AspNet.Validations.FluentValidation.Models;

/// <summary>
/// 表示验证错误
/// </summary>
public sealed class ValidationError
{
    public ValidationError(string propertyName, string errorMessage, string? errorCode = null, object? attemptedValue = null)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
        ErrorCode = errorCode;
        AttemptedValue = attemptedValue;
    }
    
    /// <summary>
    /// 获取验证失败的属性名称
    /// </summary>
    public string PropertyName { get; }
    
    /// <summary>
    /// 获取错误消息
    /// </summary>
    public string ErrorMessage { get; }
    
    /// <summary>
    /// 获取错误代码（如果有）
    /// </summary>
    public string? ErrorCode { get; }
    
    /// <summary>
    /// 获取尝试设置的值
    /// </summary>
    public object? AttemptedValue { get; }
    
    /// <summary>
    /// 返回验证错误的字符串表示
    /// </summary>
    /// <returns>A string representation of the error</returns>
    public override string ToString() => $"{PropertyName}: {ErrorMessage}";
}