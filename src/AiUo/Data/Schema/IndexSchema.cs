using System;

namespace AiUo.Data.Schema;

/// <summary>
/// MySQL表的索引概要信息，不包含主键索引和外键索引
/// </summary>
[Serializable]
public class IndexSchema: TableObjectSchemaBase
{
    /// <summary>
    /// 索引名
    /// </summary>
    public string IndexName { get; set; }

    /// <summary>
    /// 是否主键
    /// </summary>
    public bool IsPrimaryKey { get; set; }
    private bool _isClustered = false;
    /// <summary>
    /// MySQL的聚集索引是通过主键判断的，如果没有则自动选不重复列，还没有自己建隐藏的字段
    /// </summary>
    public bool IsClustered
    {
        get
        {
            switch (DbDataProvider)
            {
                case DbDataProvider.MySqlClient:
                    _isClustered = IsPrimaryKey;
                    break;
            }
            return _isClustered;
        }
        set
        {
            _isClustered = value;
        }
    }

    /// <summary>
    /// 是否唯一
    /// </summary>
    public bool IsUnique { get; set; }
}