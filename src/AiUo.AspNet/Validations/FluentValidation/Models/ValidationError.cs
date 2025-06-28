using System;

namespace AiUo.AspNet.Validations.FluentValidation.Models;

/// <summary>
/// Represents a validation error
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
    /// Gets the name of the property that failed validation
    /// </summary>
    public string PropertyName { get; }
    
    /// <summary>
    /// Gets the error message
    /// </summary>
    public string ErrorMessage { get; }
    
    /// <summary>
    /// Gets the error code, if any
    /// </summary>
    public string? ErrorCode { get; }
    
    /// <summary>
    /// Gets the value that was attempted to be set
    /// </summary>
    public object? AttemptedValue { get; }
    
    /// <summary>
    /// Returns a string representation of the validation error
    /// </summary>
    /// <returns>A string representation of the error</returns>
    public override string ToString() => $"{PropertyName}: {ErrorMessage}";
}