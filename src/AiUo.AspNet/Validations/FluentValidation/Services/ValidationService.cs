using FluentValidation;
using System.Diagnostics;

namespace AiUo.AspNet.Validations.FluentValidation.Services;

/// <summary>
/// 验证服务实现
/// </summary>
public sealed class ValidationService : IValidationService
{
    private readonly IAttributeBasedValidatorFactory _validatorFactory;
    private readonly ILogger<ValidationService> _logger;
    private readonly FluentValidationOptions _options;
    
    public ValidationService(
        IAttributeBasedValidatorFactory validatorFactory,
        ILogger<ValidationService> logger,
        IOptions<FluentValidationOptions> options)
    {
        _validatorFactory = validatorFactory ?? throw new ArgumentNullException(nameof(validatorFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }
    
    public async Task<ValidationResult<T>> ValidateAsync<T>(T model, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(model);
        
        var stopwatch = _options.EnablePerformanceMonitoring ? Stopwatch.StartNew() : null;
        var modelTypeName = typeof(T).Name;
        
        try
        {
            var validator = _validatorFactory.CreateValidator<T>();
            if (validator == null)
            {
                _logger.LogDebug("No validator found for type {ModelType}", modelTypeName);
                return ValidationResult<T>.Success();
            }
            
            var context = new ValidationContext<T>(model);
            if (_options.StopOnFirstFailure)
            {
                context.RootContextData["StopOnFirstFailure"] = true;
            }
            
            var result = await validator.ValidateAsync(context, cancellationToken);
            var validationResult = ValidationResult<T>.FromFluentValidationResult(result);
            
            // Performance monitoring
            if (stopwatch != null)
            {
                stopwatch.Stop();
                var elapsedMs = stopwatch.ElapsedMilliseconds;
                
                if (elapsedMs > _options.PerformanceThresholdMs)
                {
                    _logger.LogWarning("Validation for {ModelType} took {ElapsedMs}ms, which exceeds threshold of {ThresholdMs}ms",
                        modelTypeName, elapsedMs, _options.PerformanceThresholdMs);
                }
                else
                {
                    _logger.LogDebug("Validation for {ModelType} completed in {ElapsedMs}ms",
                        modelTypeName, elapsedMs);
                }
            }
            
            return validationResult;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Validation was cancelled for type {ModelType}", modelTypeName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Validation failed for type {ModelType}", modelTypeName);
            throw new global::FluentValidation.ValidationException($"Validation failed for type {modelTypeName}");
        }
        finally
        {
            stopwatch?.Stop();
        }
        }
    
    public ValidationResult<T> Validate<T>(T model) where T : class
    {
        ArgumentNullException.ThrowIfNull(model);
        
        var stopwatch = _options.EnablePerformanceMonitoring ? Stopwatch.StartNew() : null;
        var modelTypeName = typeof(T).Name;
        
        try
        {
            var validator = _validatorFactory.CreateValidator<T>();
            if (validator == null)
            {
                _logger.LogDebug("No validator found for type {ModelType}", modelTypeName);
                return ValidationResult<T>.Success();
            }
            
            var context = new ValidationContext<T>(model);
            if (_options.StopOnFirstFailure)
            {
                context.RootContextData["StopOnFirstFailure"] = true;
            }
            
            var result = validator.Validate(context);
            var validationResult = ValidationResult<T>.FromFluentValidationResult(result);
            
            // Performance monitoring
            if (stopwatch != null)
            {
                stopwatch.Stop();
                var elapsedMs = stopwatch.ElapsedMilliseconds;
                
                if (elapsedMs > _options.PerformanceThresholdMs)
                {
                    _logger.LogWarning("Validation for {ModelType} took {ElapsedMs}ms, which exceeds threshold of {ThresholdMs}ms",
                        modelTypeName, elapsedMs, _options.PerformanceThresholdMs);
                }
                else
                {
                    _logger.LogDebug("Validation for {ModelType} completed in {ElapsedMs}ms",
                        modelTypeName, elapsedMs);
                }
            }
            
            return validationResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Validation failed for type {ModelType}", modelTypeName);
            throw new global::FluentValidation.ValidationException($"Validation failed for type {modelTypeName}");
        }
        finally
        {
            stopwatch?.Stop();
        }
    }
}