using System.Data.Common;

namespace AiUo.Data.ORM;

/// <summary>
/// 视图MO基类
/// </summary>
/// <typeparam name="TDatabase"></typeparam>
/// <typeparam name="TParameter"></typeparam>
/// <typeparam name="TDbType"></typeparam>
/// <typeparam name="TEntity"></typeparam>
public abstract class DbViewMO<TDatabase, TParameter, TDbType, TEntity> : DbEntityMO<TDatabase, TParameter, TDbType, TEntity>
    where TDatabase : Database<TParameter, TDbType>
    where TParameter : DbParameter
    where TDbType : struct
    where TEntity : IRowMapper<TEntity>
{
    /// <summary>
    /// 数据对象类型
    /// </summary>
    public override DbObjectType SourceType => DbObjectType.View;
    /// <summary>
    /// 数据对象源名称
    /// </summary>
    public override string SourceName => ViewName;
    /// <summary>
    /// 视图名
    /// </summary>
    public abstract string ViewName { get; set; }
}