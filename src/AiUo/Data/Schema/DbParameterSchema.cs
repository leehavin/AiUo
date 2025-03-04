using System;
using System.Data;

namespace AiUo.Data.Schema;

/// <summary>
/// 参数Schema
/// </summary>
[Serializable]
public abstract class DbParameterSchema : ISchemaCollectionKey
{
    #region  基本属性
    /// <summary>
    /// 数据库类型
    /// </summary>
    public DbDataProvider DbDataProvider { get; set; } = DbDataProvider.Unknown;
    /// <summary>
    /// 所在对象名
    /// </summary>
    public string ParentName { get; set; }
    /// <summary>
    /// 字段所在的对象，存储过程
    /// </summary>
    public object Parent { get; set; }
    #endregion

    #region 原始信息
    //information_schema.PARAMETERS
        
    /// <summary>
    /// 字段在表中所有列的排列位置 ORDINAL_POSITION
    /// </summary>
    public int Ordinal { get; set; }
    /// <summary>
    /// 参数类型 PARAMETER_MODE IN OUT INOUT
    /// </summary>
    public ParameterDirection Direction { get; set; }
    /// <summary>
    /// 参数名称 PARAMETER_NAME
    /// </summary>
    public string ParameterName { get; set; }
    /// <summary>
    /// 字段原始数据类型，字符串类型，DATA_TYPE 如：tinyint
    /// </summary>
    public string EngineTypeString { get; set; }
    /// <summary>
    /// 字段原始数据类型，字符串类型，DTD_IDENTIFIER 如：tinyint(3) unsigned
    /// </summary>
    public string EngineTypeStringFull { get; set; }
    /// <summary>
    /// 是否允许DBNull
    /// </summary>
    public bool AllowDBNull { get; set; }
    /// <summary>
    /// 字段数值精度或字符串以字符为单位的最大长度，如char(4),float(length, precision)
    /// </summary>
    public long? Length { get; set; }
    /// <summary>
    /// 字段数值小数位数或字符串以字节为单位的最大长度
    /// </summary>
    public long? Precision { get; set; }
    #endregion

    #region 扩展信息
    /// <summary>
    /// 通过MySQL字段原始类型推断的 .NET 类型
    /// </summary>
    public Type DotNetType { get; set; }

    /// <summary>
    /// 属性名称，第一个字母大写: UserID
    /// </summary>
    public string OrmPropertyName { get { return StringUtil.PascalCase(ParameterName); } }
    /// <summary>
    /// 方法参数名称，第一个字母小写: userID
    /// </summary>
    public string OrmParamName
    {
        get
        {
            string ret = StringUtil.CamelCase(ParameterName);
            return StringUtil.IsCSharpReserved(ret) ? ret + "Value" : ret;
        }
    }
    /// <summary>
    /// 在SQL语句中的参数写法，如：@UserID 或 ?UserID
    /// </summary>
    public string SqlParamName { get { return SchemaUtil.GetSqlParamName(DbDataProvider, ParameterName); } }

    /// <summary>
    /// 获取通过参数存入数据库时取值信息：userID != null ? userID : (object)DBNull.Value
    /// </summary>
    public string OrmParamValueToDbCode { get { return SchemaUtil.GetOrmParamValueToDbCode(OrmParamName, AllowDBNull, DotNetTypeString); } }
    #endregion

    /// <summary>
    /// 通过MySQL字段原始类型推断的 DbType 类型
    /// </summary>
    public abstract DbType SysDbType { get; }
    /// <summary>
    /// .NET类型字符串表示，支持可空类型，如int?
    /// </summary>
    public abstract string DotNetTypeString { get; }

    #region ISchemaCollectionKey
    /// <summary>
    /// 比较器，用于在集合中排序
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(ColumnSchema other)
    {
        return this.Ordinal.CompareTo(other.Ordinal);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetKey()
    {
        return ParameterName;
    }
    #endregion
}
/// <summary>
/// 字段概要信息
/// </summary>
public abstract class DbParameterSchema<TEngineType, IDbType> : DbParameterSchema
    where TEngineType : struct
    where IDbType : struct
{
    /// <summary>
    /// 数据库字段类型与.net类型转换
    /// </summary>
    public abstract IDbTypeMapper<TEngineType, IDbType> TypeMapper { get; }

    #region 类型映射
    /// <summary>
    /// MySQL字段原始数据类型
    /// </summary>
    public TEngineType EngineType => TypeMapper.MapEngineType(EngineTypeString, EngineTypeStringFull);
    /// <summary>
    /// 通过MySQL字段原始类型推断的 MySqlDbType 类型
    /// </summary>
    public IDbType DbType => TypeMapper.MapDbType(EngineType);

    /// <summary>
    /// MySqlDbType 的字符串表示：MySqlDbTypeAll
    /// </summary>
    public string DbTypeString => $"{DbType.GetType().Name}.{DbType}";
    /// <summary>
    /// 通过MySQL字段原始类型推断的 DbType 类型
    /// </summary>
    public override DbType SysDbType => TypeMapper.MapSysDbType(DbType);

    /// <summary>
    /// 通过MySQL字段原始类型推断的 .NET 类型
    /// </summary>
    public new Type DotNetType => TypeMapper.MapDotNetType(EngineType);

    /// <summary>
    /// .NET类型字符串表示，支持可空类型，如int?
    /// </summary>
    public override string DotNetTypeString => SchemaUtil.InferDotNetTypeString(DotNetType, AllowDBNull);

    #endregion
}