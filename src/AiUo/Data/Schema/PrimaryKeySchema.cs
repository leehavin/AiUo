using System;

namespace AiUo.Data.Schema;

/// <summary>
/// MySQL表的主键概要信息
/// </summary>
[Serializable]
public class PrimaryKeySchema: TableObjectSchemaBase
{
    /// <summary>
    /// 是否聚集，注意MySQL是使用主键或使用唯一的非空索引或定义隐藏的主键进行聚集，所以MySQL中主键必然聚集
    /// </summary>
    public bool IsClustered { get; set; }
}