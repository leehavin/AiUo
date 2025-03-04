using System.Data.Common;

namespace AiUo.Data.ORM;

/// <summary>
/// 存储过程MO基类
/// </summary>
/// <typeparam name="TDatabase"></typeparam>
/// <typeparam name="TParameter"></typeparam>
/// <typeparam name="TDbType"></typeparam>
public abstract class DbProcMO<TDatabase, TParameter, TDbType> : DbMOBase<TDatabase, TParameter, TDbType>
    where TDatabase : Database<TParameter, TDbType>
    where TParameter : DbParameter
    where TDbType : struct
{
    /// <summary>
    /// 数据对象类型
    /// </summary>
    public override DbObjectType SourceType => DbObjectType.Proc;
        
    /// <summary>
    /// 数据库对象名 = 存储过程名
    /// </summary>
    public override string SourceName => ProcName;

    /// <summary>
    /// 存储过程名
    /// </summary>
    public abstract string ProcName { get; set; }
}