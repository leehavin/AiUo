using System;
using System.Data;

namespace AiUo.Data.Schema;

/// <summary>
/// ColumnSchema
/// </summary>
[Serializable]
public abstract class ColumnSchema : ISchemaCollectionKey
{
    #region  基本属性
    /// <summary>
    /// 数据库类型
    /// </summary>
    public DbDataProvider DbDataProvider { get; set; } = DbDataProvider.Unknown;
    /// <summary>
    /// 字段所在对象名
    /// </summary>
    public string ParentName { get; set; }
    /// <summary>
    /// 字段所在的对象, 可能是Table或者View
    /// </summary>
    public TableViewSchemaBase Parent { get; set; }
    #endregion

    #region 原始信息
    /// <summary>
    /// 表中原始字段名
    /// </summary>
    public string ColumnName { get; set; }
    /// <summary>
    /// 字段在表中所有列的排列位置
    /// </summary>
    public int Ordinal { get; set; }
    /// <summary>
    /// 描述
    /// </summary>
    public string Comment { get; set; }
    /// <summary>
    /// 注释的第一行信息
    /// </summary>
    public string CommentFirst => StringUtil.GetFirstLine(Comment).Trim();
    /// <summary>
    /// 字段原始数据类型，字符串类型，如：tinyint
    /// </summary>
    public string EngineTypeString { get; set; }
    /// <summary>
    /// 字段原始数据类型，字符串类型，如：tinyint(3) unsigned
    /// </summary>
    public string EngineTypeStringFull { get; set; }

    /// <summary>
    /// 是否是数值类型
    /// </summary>
    public bool IsNumeric { get; set; }
    /// <summary>
    /// 字段数值精度或字符串以字符为单位的最大长度，如char(4),float(length, precision)
    /// </summary>
    public long? Length { get; set; }
    /// <summary>
    /// 字段数值小数位数或字符串以字节为单位的最大长度
    /// </summary>
    public long? Precision { get; set; }
    /// <summary>
    /// 是否允许NULL
    /// </summary>
    public bool AllowDBNull { get; set; }
    /// <summary>
    /// 是否是有符号unsigned字段
    /// </summary>
    public bool IsUnsigned { get; set; }
    /// <summary>
    /// 是否有默认值
    /// </summary>
    public bool HasDefaultValue { get; set; }
    /// <summary>
    /// 默认值
    /// </summary>
    public string DefaultValue { get; set; }

    /// <summary>
    /// 列主键定义，如：PRI
    /// </summary>
    public string ColumnKey { get; set; }
    /// <summary>
    /// 扩展，如：auto_increment
    /// </summary>
    public string Extra { get; set; }
    #endregion

    #region 主键外键索引键
    /// <summary>
    /// 是否是auto_increment字段
    /// </summary>
    public bool IsAutoIncrement { get; set; }
    /// <summary>
    /// 是否是主键，注意其可能是联合主键中的一个字段
    /// </summary>
    public bool IsPrimaryKey { get; set; }
    /// <summary>
    /// 是否是单一主键字段
    /// </summary>
    public bool IsSinglePKColumn { get; set; }
    /// <summary>
    /// 是否属于唯一索引中的字段
    /// </summary>
    public bool IsUniqueColumn { get; set; }
    /// <summary>
    /// 是否是单一唯一索引字段
    /// </summary>
    public bool IsSingleUniqueColumn { get; set; }
    /// <summary>
    /// 是否是ForeignKey字段
    /// </summary>
    public bool IsForeignKey { get; set; }
    /// <summary>
    /// 是否唯一外键字段
    /// </summary>
    public bool IsSingleFKColumn { get; set; }
    #endregion

    #region 扩展信息

    /// <summary>
    /// 在SQL语句中字段的写法，如：'UserID' 或 [UserID]
    /// </summary>
    public string SqlColumnName => SchemaUtil.GetSqlName(DbDataProvider, ColumnName);

    /// <summary>
    /// 在SQL语句中的参数写法，如：@UserID 或 ?UserID
    /// </summary>
    public string SqlParamName => SchemaUtil.GetSqlParamName(DbDataProvider, ColumnName);

    /// <summary>
    /// 属性名称，第一个字母大写: UserID
    /// </summary>
    public string OrmPropertyName => StringUtil.PascalCase(ColumnName);

    /// <summary>
    /// 方法参数名称，第一个字母小写: userID
    /// </summary>
    public string OrmParamName
    {
        get
        {
            string ret = StringUtil.CamelCase(ColumnName);
            return StringUtil.IsCSharpReserved(ret) ? ret + "Value" : ret;
        }
    }

    /// <summary>
    /// 获取DataReader读取值时使用的语句: ToBytesN
    /// </summary>
    public string OrmReaderMethodName => SchemaUtil.GetOrmReaderMethodName(DotNetTypeString, AllowDBNull);

    /// <summary>
    /// 字段类型描述: 主键,外键,字段
    /// </summary>
    public string OrmColumnTypeString
    {
        get
        {
            if (IsPrimaryKey) return "主键";
            else if (IsForeignKey) return "外键";
            else return "字段";
        }
    }

    /// <summary>
    /// 获取通过参数存入数据库时取值信息：userID != null ? userID : (object)DBNull.Value
    /// </summary>
    public string OrmParamValueToDbCode => SchemaUtil.GetOrmParamValueToDbCode(OrmParamName, AllowDBNull, DotNetTypeString);
    /// <summary>
    /// userID userID.Value
    /// </summary>
    public string OrmParamValue
    {
        get
        {
            var ret = OrmParamName;
            if (AllowDBNull)
            {
                if (DotNetTypeString != "string" && DotNetTypeString != "byte[]")
                    ret = $"{OrmParamName}.Value";
            }
            return ret;
        }
    }
    /// <summary>
    /// 获取对象存入数据库时取值信息: item.UserID.HasValue ? item.Value : (object)DBNull.Value
    /// </summary>
    public string OrmItemValueToDbCode => SchemaUtil.GetOrmItemValueToDbCode(OrmPropertyName, AllowDBNull, DotNetTypeString);

    /// <summary>
    /// 数据库默认值解析成.net对象
    /// </summary>
    public abstract object OrmDefaultValue { get; }
    /// <summary>
    /// 数据库默认值的.net字符串表示,用于代码生成，如 DataTime.Now
    /// </summary>
    public abstract string OrmDefaultValueString { get; }
    //public string OrmDefaultValueString
    //    => (HasDefaultValue && !string.IsNullOrEmpty(DefaultValue))
    //    ? SchemaUtil.ParseDefaultValueString(DbDataProvider, DotNetTypeString, DefaultValue)
    //    : string.Empty;
    #endregion

    /// <summary>
    /// DbType类型
    /// </summary>
    public abstract DbType SysDbType { get; }
    public abstract Type DotNetType{get;}
    /// <summary>
    /// .Net 数据类型字符串表示
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
    /// 获得Key
    /// </summary>
    /// <returns></returns>
    public string GetKey()
    {
        return ColumnName;
    }
    #endregion
}
/// <summary>
/// 字段概要信息
/// </summary>
[Serializable]
public abstract class ColumnSchema<TEngineType, TDbType> : ColumnSchema
    where TEngineType : struct
    where TDbType : struct
{
    /// <summary>
    /// 数据库字段类型与.net类型转换
    /// </summary>
    public abstract IDbTypeMapper<TEngineType, TDbType> TypeMapper { get; }

    #region 类型映射
    /// <summary>
    /// MySQL字段原始数据类型
    /// </summary>
    public TEngineType EngineType => TypeMapper.MapEngineType(EngineTypeString, EngineTypeStringFull);
    /// <summary>
    /// 通过MySQL字段原始类型推断的 MySqlDbType 类型
    /// </summary>
    public TDbType DbType => TypeMapper.MapDbType(EngineType);

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
    public override Type DotNetType => TypeMapper.MapDotNetType(EngineType);

    /// <summary>
    /// .NET类型字符串表示，支持可空类型，如int?
    /// </summary>
    public override string DotNetTypeString => SchemaUtil.InferDotNetTypeString(DotNetType, AllowDBNull);

    #endregion
    /// <summary>
    /// 默认值
    /// </summary>
    public override object OrmDefaultValue
    {
        get
        {
            object ret = null;
            if (HasDefaultValue)
            {
                ret = SchemaUtil.ParseDefaultValue(DbDataProvider, DotNetTypeString, DotNetType, DefaultValue);
            }
            return ret;
        }
    }
    /// <summary>
    /// 默认值.net字符串表示，用于代码生成
    /// </summary>
    public override string OrmDefaultValueString
    {
        get
        {
            string ret = null;
            if (HasDefaultValue)
            {
                ret = SchemaUtil.ParseDefaultValueString(DbDataProvider, DotNetTypeString, DefaultValue);
            }
            return ret;
        }
    }
}