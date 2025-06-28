using System;
using FluentValidation;

namespace AiUo.AspNet.Validations.FluentValidation.Services;

/// <summary>
/// Interface for attribute-based validator factory
/// </summary>
public interface IAttributeBasedValidatorFactory
{
    /// <summary>
    /// Creates a validator for the specified type
    /// </summary>
    /// <param name="modelType">The model type</param>
    /// <returns>The validator instance</returns>
    IValidator? CreateValidator(Type modelType);
    
    /// <summary>
    /// Creates a generic validator for the specified type
    /// </summary>
    /// <typeparam name="T">The model type</typeparam>
    /// <returns>The validator instance</returns>
    IValidator<T>? CreateValidator<T>() where T : class;
}