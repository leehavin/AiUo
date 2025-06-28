using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace AiUo.AspNet.Validations.FluentValidation.Models;

/// <summary>
/// 表示验证操作的结果
/// </summary>
/// <typeparam name="T">The type being validated</typeparam>
public sealed class ValidationResult<T>
{
    private ValidationResult(bool isValid, IReadOnlyList<ValidationError> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }
    
    /// <summary>
    /// 获取指示验证是否成功的值
    /// </summary>
    public bool IsValid { get; }
    
    /// <summary>
    /// 获取验证错误的集合
    /// </summary>
    public IReadOnlyList<ValidationError> Errors { get; }
    
    /// <summary>
    /// 获取第一个错误消息，如果验证成功则为null
    /// </summary>
    public string? FirstErrorMessage => Errors.FirstOrDefault()?.ErrorMessage;
    
    /// <summary>
    /// 创建成功的验证结果
    /// </summary>
    /// <returns>A successful validation result</returns>
    public static ValidationResult<T> Success() => new(true, Array.Empty<ValidationError>());
    
    /// <summary>
    /// 创建带有指定错误的失败验证结果
    /// </summary>
    /// <param name="errors">The validation errors</param>
    /// <returns>A failed validation result</returns>
    public static ValidationResult<T> Failure(IEnumerable<ValidationError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        var errorList = errors.ToList();
        return new ValidationResult<T>(false, errorList);
    }
    
    /// <summary>
    /// 从FluentValidation结果创建验证结果
    /// </summary>
    /// <param name="result">The FluentValidation result</param>
    /// <returns>A validation result</returns>
    internal static ValidationResult<T> FromFluentValidationResult(global::FluentValidation.Results.ValidationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        
        if (result.IsValid)
        {
            return Success();
        }
        
        var errors = result.Errors.Select(e => new ValidationError(
            e.PropertyName,
            e.ErrorMessage,
            e.ErrorCode,
            e.AttemptedValue));
            
        return Failure(errors);
    }
    
    /// <summary>
    /// 获取特定属性的错误
    /// </summary>
    /// <param name="propertyName">The property name</param>
    /// <returns>The errors for the specified property</returns>
    public IEnumerable<ValidationError> GetErrorsForProperty(string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        return Errors.Where(e => string.Equals(e.PropertyName, propertyName, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// 将验证结果转换为字典格式
    /// </summary>
    /// <returns>A dictionary with property names as keys and error messages as values</returns>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> ToDictionary()
    {
        return Errors
            .GroupBy(e => e.PropertyName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<string>)g.Select(e => e.ErrorMessage).ToList(),
                StringComparer.OrdinalIgnoreCase);
    }
}