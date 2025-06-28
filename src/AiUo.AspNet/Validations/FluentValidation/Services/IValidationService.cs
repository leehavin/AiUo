namespace AiUo.AspNet.Validations.FluentValidation.Services;

/// <summary>
/// 验证服务接口
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// 验证模型并返回验证结果
    /// </summary>
    /// <typeparam name="T">The model type</typeparam>
    /// <param name="model">The model to validate</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The validation result</returns>
    Task<ValidationResult<T>> ValidateAsync<T>(T model, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// 同步验证模型
    /// </summary>
    /// <typeparam name="T">The model type</typeparam>
    /// <param name="model">The model to validate</param>
    /// <returns>The validation result</returns>
    ValidationResult<T> Validate<T>(T model) where T : class;
}