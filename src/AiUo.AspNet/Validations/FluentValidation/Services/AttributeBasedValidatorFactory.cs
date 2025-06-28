using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AiUo.AspNet.Validations.FluentValidation.Validators;

namespace AiUo.AspNet.Validations.FluentValidation.Services;

/// <summary>
/// Implementation of attribute-based validator factory
/// </summary>
public sealed class AttributeBasedValidatorFactory : IAttributeBasedValidatorFactory, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AttributeBasedValidatorFactory> _logger;
    private readonly FluentValidationOptions _options;
    private readonly ConcurrentDictionary<Type, IValidator?> _validatorCache = new();
    private readonly Timer _cacheCleanupTimer;
    private readonly object _cleanupLock = new();
    
    public AttributeBasedValidatorFactory(
        IServiceProvider serviceProvider,
        ILogger<AttributeBasedValidatorFactory> logger,
        IOptions<FluentValidationOptions> options)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        
        // Setup cache cleanup timer (runs every 30 minutes)
        _cacheCleanupTimer = new Timer(CleanupCache, null, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));
    }
    
    public IValidator? CreateValidator(Type modelType)
    {
        ArgumentNullException.ThrowIfNull(modelType);
        
        return _validatorCache.GetOrAdd(modelType, type =>
        {
            try
            {
                _logger.LogDebug("Creating validator for type {ModelType}", type.Name);
                
                var validatorType = typeof(Validators.AttributeBasedValidator<>).MakeGenericType(type);
                var validator = (IValidator?)Activator.CreateInstance(validatorType, _options, _serviceProvider);
                
                if (validator != null)
                {
                    _logger.LogDebug("Successfully created validator for type {ModelType}", type.Name);
                }
                else
                {
                    _logger.LogWarning("Failed to create validator instance for type {ModelType}", type.Name);
                }
                
                return validator;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid arguments when creating validator for type {ModelType}", type.Name);
                return null;
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                _logger.LogError(ex.InnerException, "Constructor error when creating validator for type {ModelType}", type.Name);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when creating validator for type {ModelType}", type.Name);
                return null;
            }
        });
    }
    
    public IValidator<T>? CreateValidator<T>() where T : class
    {
        return (IValidator<T>?)CreateValidator(typeof(T));
    }
    
    private void CleanupCache(object? state)
    {
        if (!Monitor.TryEnter(_cleanupLock, TimeSpan.FromSeconds(1)))
            return;
            
        try
        {
            var cacheSize = _validatorCache.Count;
            if (cacheSize > 1000) // Only cleanup if cache is large
            {
                _logger.LogInformation("Starting validator cache cleanup. Current size: {CacheSize}", cacheSize);
                
                // Clear cache if it gets too large
                _validatorCache.Clear();
                
                _logger.LogInformation("Validator cache cleared");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during validator cache cleanup");
        }
        finally
        {
            Monitor.Exit(_cleanupLock);
        }
    }
    
    public void Dispose()
    {
        _cacheCleanupTimer?.Dispose();
    }
}