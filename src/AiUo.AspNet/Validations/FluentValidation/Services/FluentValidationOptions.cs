using System;

namespace AiUo.AspNet.Validations.FluentValidation.Services;

/// <summary>
/// Configuration options for FluentValidation
/// </summary>
public sealed class FluentValidationOptions
{
    /// <summary>
    /// Gets or sets the default error code for validation failures
    /// </summary>
    public string DefaultErrorCode { get; set; } = "VALIDATION_ERROR";
    
    /// <summary>
    /// Gets or sets whether to stop validation on first failure
    /// </summary>
    public bool StopOnFirstFailure { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the maximum number of validation errors to collect
    /// </summary>
    public int MaxErrors { get; set; } = 100;
    
    /// <summary>
    /// Gets or sets whether to enable performance monitoring
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the performance monitoring threshold in milliseconds
    /// </summary>
    public int PerformanceThresholdMs { get; set; } = 1000;
    
    /// <summary>
    /// Validates the configuration options
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when configuration is invalid</exception>
    public void Validate()
    {
        if (MaxErrors <= 0)
            throw new ArgumentException("MaxErrors must be greater than 0", nameof(MaxErrors));
            
        if (string.IsNullOrWhiteSpace(DefaultErrorCode))
            throw new ArgumentException("DefaultErrorCode cannot be null or empty", nameof(DefaultErrorCode));
            
        if (PerformanceThresholdMs < 0)
            throw new ArgumentException("PerformanceThresholdMs cannot be negative", nameof(PerformanceThresholdMs));
    }
}