using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace AiUo.AspNet.Validations.FluentValidation.Models;

/// <summary>
/// Represents the result of a validation operation
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
    /// Gets a value indicating whether the validation was successful
    /// </summary>
    public bool IsValid { get; }
    
    /// <summary>
    /// Gets the collection of validation errors
    /// </summary>
    public IReadOnlyList<ValidationError> Errors { get; }
    
    /// <summary>
    /// Gets the first error message, or null if validation was successful
    /// </summary>
    public string? FirstErrorMessage => Errors.FirstOrDefault()?.ErrorMessage;
    
    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    /// <returns>A successful validation result</returns>
    public static ValidationResult<T> Success() => new(true, Array.Empty<ValidationError>());
    
    /// <summary>
    /// Creates a failed validation result with the specified errors
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
    /// Creates a validation result from a FluentValidation result
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
    /// Gets errors for a specific property
    /// </summary>
    /// <param name="propertyName">The property name</param>
    /// <returns>The errors for the specified property</returns>
    public IEnumerable<ValidationError> GetErrorsForProperty(string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        return Errors.Where(e => string.Equals(e.PropertyName, propertyName, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Converts the validation result to a dictionary format
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