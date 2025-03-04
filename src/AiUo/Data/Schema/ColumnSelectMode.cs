using System;

namespace AiUo.Data.Schema;

/// <summary>
/// 字段过滤模式
/// </summary>
[Serializable]
public enum ColumnSelectMode
{
    /// <summary>
    /// 全部
    /// </summary>
    All,
    /// <summary>
    /// 不包含自增字段
    /// </summary>
    NoAutoIncrement, // RemoveAutoIncrement
    /// <summary>
    /// 不包含单一主键
    /// </summary>
    NoOnePK, // RemoveOnePK
    /// <summary>
    /// 不包含单一主键和单一唯一键
    /// </summary>
    NoOneUnique, // RemoveOneUnique
    /// <summary>
    /// 不包含主键 // RemovePK
    /// </summary>
    NoPK,
    /// <summary>
    /// 可以添加和更新，不包括自增字段和timespane字段 
    /// </summary>
    CanInsertAndUpdate
}