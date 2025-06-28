using System.Threading;
using System.Threading.Tasks;
using AiUo.AspNet.Validations.FluentValidation.Models;

namespace AiUo.AspNet.Validations.FluentValidation.Services;

/// <summary>
/// Interface for validation service
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates a model and returns the validation result
    /// </summary>
    /// <typeparam name="T">The model type</typeparam>
    /// <param name="model">The model to validate</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The validation result</returns>
    Task<ValidationResult<T>> ValidateAsync<T>(T model, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Validates a model synchronously
    /// </summary>
    /// <typeparam name="T">The model type</typeparam>
    /// <param name="model">The model to validate</param>
    /// <returns>The validation result</returns>
    ValidationResult<T> Validate<T>(T model) where T : class;
}