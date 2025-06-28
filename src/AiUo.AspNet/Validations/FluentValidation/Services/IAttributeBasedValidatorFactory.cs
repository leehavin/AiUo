using FluentValidation;

namespace AiUo.AspNet.Validations.FluentValidation.Services;

/// <summary>
/// 基于特性的验证器工厂接口
/// </summary>
public interface IAttributeBasedValidatorFactory
{
    /// <summary>
    /// 为指定类型创建验证器
    /// </summary>
    /// <param name="modelType">The model type</param>
    /// <returns>The validator instance</returns>
    IValidator? CreateValidator(Type modelType);
    
    /// <summary>
    /// 为指定类型创建泛型验证器
    /// </summary>
    /// <typeparam name="T">The model type</typeparam>
    /// <returns>The validator instance</returns>
    IValidator<T>? CreateValidator<T>() where T : class;
}