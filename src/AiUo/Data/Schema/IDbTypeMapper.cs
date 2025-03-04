using System;
using System.Data;

namespace AiUo.Data.Schema;

/// <summary>
/// 数据库字段类型与.net类型转换
/// engineTypeString(+engineTypeFull) => TEngineType => TDbType
///                                 => DotNetType => NetTypeString
/// </summary>
/// <typeparam name="TEngineType"></typeparam>
/// <typeparam name="TDbType"></typeparam>
public interface IDbTypeMapper<TEngineType, TDbType>
    where TEngineType : struct
    where TDbType : struct
{
    /// <summary>
    /// 映射数据库字段类型枚举表示。如：tinyint(1) => MySqlEngineType.Bool
    /// </summary>
    /// <param name="engineTypeString">数据库字段类型字符串表示，如 tinyint</param>
    /// <param name="engineTypeFull">数据库字段字符串全描述，如 tinyint(1)</param>
    /// <returns></returns>
    TEngineType MapEngineType(string engineTypeString, string engineTypeFull = null);

    /// <summary>
    /// 映射数据库类型到DbType。如：MySqlEngineType.Int => MySqlDbType.Int32
    /// </summary>
    /// <param name="engineType">数据库类型枚举表示</param>
    /// <returns></returns>
    TDbType MapDbType(TEngineType engineType);

    /// <summary>
    /// 映射数据库类型到DbType。
    /// </summary>
    /// <param name="engineTypeString">数据库字段类型字符串表示，如 tinyint</param>
    /// <param name="engineTypeFull">数据库字段字符串全描述，如 tinyint(1)</param>
    /// <returns></returns>
    TDbType MapDbType(string engineTypeString, string engineTypeFull = null);

    /// <summary>
    /// 映射系统DbType到数据库DbType。如：DbType.Single => MySqlDbType.Float
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    TDbType MapDbType(DbType type);

    /// <summary>
    /// 映射数据库DbType到系统DbType。如：MySqlDbType.Float => DbType.Single
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    DbType MapSysDbType(TDbType type);

    /// <summary>
    /// 根据数据库引擎类型推断ORM映射的.NET类型，依据可存下，尽量少类型，不考虑无符号类型
    /// </summary>
    /// <param name="engineType"></param>
    /// <returns></returns>
    Type MapDotNetType(TEngineType engineType);

    /// <summary>
    /// 根据MySqlDbType 类型推断.NET类型
    /// </summary>
    /// <param name="dbType"></param>
    /// <returns></returns>
    Type MapDotNetType(TDbType dbType);
}