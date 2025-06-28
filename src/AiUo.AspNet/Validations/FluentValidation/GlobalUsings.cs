// Global using statements for FluentValidation components
// Note: Use these carefully to avoid naming conflicts

// Core attributes for validation configuration
global using AiUo.AspNet.Validations.FluentValidation.Attributes;

// Services and interfaces for dependency injection
global using AiUo.AspNet.Validations.FluentValidation.Services;

// Models and data structures for validation results
global using AiUo.AspNet.Validations.FluentValidation.Models;

// Validators for attribute-based validation
global using AiUo.AspNet.Validations.FluentValidation.Validators;

// Rule extensions for custom validation logic
global using AiUo.AspNet.Validations.FluentValidation.Rules;

// Service extensions for DI container configuration
global using AiUo.AspNet.Validations.FluentValidation.Extensions;

// Common system namespaces used throughout the validation framework
global using System.ComponentModel.DataAnnotations;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;