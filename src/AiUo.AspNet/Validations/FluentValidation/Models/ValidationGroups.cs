namespace AiUo.AspNet.Validations.FluentValidation.Models;

/// <summary>
/// Predefined validation groups for common scenarios
/// </summary>
public static class ValidationGroups
{
    /// <summary>
    /// Validation group for create operations
    /// </summary>
    public const string Create = "Create";
    
    /// <summary>
    /// Validation group for update operations
    /// </summary>
    public const string Update = "Update";
    
    /// <summary>
    /// Validation group for delete operations
    /// </summary>
    public const string Delete = "Delete";
    
    /// <summary>
    /// Validation group for query operations
    /// </summary>
    public const string Query = "Query";
}