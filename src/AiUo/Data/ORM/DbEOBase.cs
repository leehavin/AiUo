using System.Collections.Generic;

namespace AiUo.Data.ORM;

/// <summary>
/// EO基类
/// </summary>
public abstract class DbEOBase
{
    /// <summary> 
    /// 数据提供程序类型
    /// </summary>
    public abstract DbDataProvider Provider { get; }
    /// <summary>
    /// 数据对象类型
    /// </summary>
    public abstract DbObjectType SourceType { get; }

    /// <summary>
    /// 数据对象名称
    /// </summary>
    public abstract string SourceName { get; }
    /// <summary>
    /// 是否有主键
    /// </summary>
    public abstract bool HasPrimaryKeys { get; }
    /// <summary>
    /// 获取主键
    /// </summary>
    /// <returns></returns>
    public abstract Dictionary<string, object> GetPrimaryKeys();
    /// <summary>
    /// 获取主键JSON格式
    /// </summary>
    /// <returns></returns>
    public string GetPrimaryKeysJson() => SerializerUtil.SerializeJson(GetPrimaryKeys());
}