namespace AiUo.Extensions.EPPlus;

/// <summary>
/// Excel行和列值检测模式
/// </summary>
public enum CheckerMode
{
    /// <summary>
    /// 空值
    /// </summary>
    Empty,
    /// <summary>
    /// 相等
    /// </summary>
    Equals,
    /// <summary>
    /// 包含
    /// </summary>
    Contains,
    /// <summary>
    /// 起始
    /// </summary>
    StartsWith
}