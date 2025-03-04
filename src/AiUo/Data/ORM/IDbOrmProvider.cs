using System;
using System.Collections.Generic;
using System.Data.Common;

namespace AiUo.Data.ORM;

/// <summary>
/// 特定数据库的ORM数据提供接口，需要每个特定数据库实现
/// </summary>
public interface IDbOrmProvider<TDatabase, TParameter, TDbType>
    where TDatabase : Database<TParameter, TDbType>
    where TParameter : DbParameter
    where TDbType : struct
{
    /// <summary>
    /// 根据参数获取Select SQL语句
    /// </summary>
    /// <param name="sourceName"></param>
    /// <param name="where"></param>
    /// <param name="top"></param>
    /// <param name="sort"></param>
    /// <param name="fields"></param>
    /// <param name="isForUpdate"></param>
    /// <returns></returns>
    string BuildSelectSQL(string sourceName, string where, int top, string sort, string fields = null, bool isForUpdate = false);

    /// <summary>
    /// 获取参数名称（字段对应的参数名）与Command参数类型的映射
    ///     key: @UserID value: MySqlDbType.VarChar
    /// </summary>
    /// <param name="database"></param>
    /// <param name="sourceName"></param>
    /// <param name="objectType"></param>
    /// <returns></returns>
    Dictionary<string, TDbType> GetDbTypeMappings(TDatabase database, string sourceName, DbObjectType objectType);

    TDbType MapDotNetTypeToDbType(Type type);
}