namespace AiUo.AspNet.Validations.FluentValidation.Models;

/// <summary>
/// 常见场景的预定义验证组
/// </summary>
public static class ValidationGroups
{
    /// <summary>
    /// 创建操作的验证组
    /// </summary>
    public const string Create = "Create";
    
    /// <summary>
    /// 更新操作的验证组
    /// </summary>
    public const string Update = "Update";
    
    /// <summary>
    /// 删除操作的验证组
    /// </summary>
    public const string Delete = "Delete";
    
    /// <summary>
    /// 查询操作的验证组
    /// </summary>
    public const string Query = "Query";
}