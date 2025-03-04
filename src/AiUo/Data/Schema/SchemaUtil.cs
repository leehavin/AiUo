using System;

namespace AiUo.Data.Schema;

/// <summary>
/// Schema辅助类
/// </summary>
public static class SchemaUtil
{
    /// <summary>
    /// 获取数据库对象在SQL语句中使用的格式，如MySQL：`User` SQL Server： [User]
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="sourceName"></param>
    /// <returns></returns>
    public static string GetSqlName(DbDataProvider provider, string sourceName)
    {
        if (string.IsNullOrEmpty(sourceName))
            throw new ArgumentException("数据库对象名不能为空。", "sourceName");
        if (provider == DbDataProvider.Unknown)
            throw new ArgumentException("数据提供程序DbDataProvider不能未知。", "provider");
        string ret = string.Empty;
        switch (provider)
        {
            case DbDataProvider.MySqlClient:
                ret = string.Format("`{0}`", sourceName);
                break;
            //case DbDataProvider.OracleClient:
            case DbDataProvider.Odac:
                ret = string.Format("\"{0}]\"", sourceName);
                break;
            case DbDataProvider.SqlClient:
                //case DbDataProvider.SqlServerCe:
                ret = string.Format("[{0}]", sourceName);
                break;
            //case DbDataProvider.Odbc:
            case DbDataProvider.OleDb:
                ret = sourceName;
                break;
            default:
                throw new ArgumentException("未知的DbDataProvider"+provider.ToString(), "provider");
        }
        return ret;
    }
    /// <summary>
    /// 获取数据库对象参数名称：如MySQL：@UserID
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="sourceName"></param>s
    /// <returns></returns>
    public static string GetSqlParamName(DbDataProvider provider, string sourceName)
    {
        if (string.IsNullOrEmpty(sourceName))
            throw new ArgumentException("数据库对象名不能为空。", "sourceName");
        if (provider == DbDataProvider.Unknown)
            throw new ArgumentException("数据提供程序DbDataProvider不能未知。", "provider");
        string ret = string.Empty;
        switch (provider)
        {
            case DbDataProvider.MySqlClient:
                ret = string.Format("@{0}", sourceName);
                break;
            //case DbDataProvider.OracleClient:
            case DbDataProvider.Odac:
                ret = string.Format(":{0}", sourceName);
                break;
            case DbDataProvider.SqlClient:
                //case DbDataProvider.SqlServerCe:
                ret = string.Format("@{0}", sourceName);
                break;
            //case DbDataProvider.Odbc:
            case DbDataProvider.OleDb:
                ret = "?";
                break;
            default:
                throw new ArgumentException("未知的DbDataProvider" + provider.ToString(), "provider");
        }
        return ret;
    }

    /// <summary>
    /// 根据DotNet类型转换成代码字符串 Boolean => bool?
    /// </summary>
    /// <param name="dotNetType"></param>
    /// <param name="allowDBNull"></param>
    /// <returns></returns>
    public static string InferDotNetTypeString(Type dotNetType, bool allowDBNull)
    {
        string ret = dotNetType.Name;
        switch (dotNetType.Name)
        {
            case "SByte":
            case "Byte":
            case "Decimal":
            case "Double":
            case "String":
            case "Byte[]":
                ret = dotNetType.Name.ToLower();
                break;
            case "Boolean": ret = "bool"; break;
            case "Single": ret = "float"; break;
            case "Int16": ret = "short"; break;
            case "UInt16": ret = "ushort"; break;
            case "Int32": ret = "int"; break;
            case "UInt32": ret = "uint"; break;
            case "Int64": ret = "long"; break;
            case "UInt64": ret = "ulong"; break;
        }
        if (allowDBNull && ret != "string" && ret != "byte[]")
            ret += "?";
        return ret;
    }

    #region ORM代码
    /// <summary>
    /// 根据类型字符串获取Reader时使用的取值方法。reader.ToString("UserID")
    /// </summary>
    /// <param name="dotNetTypeString"></param>
    /// <param name="allowDBNull"></param>
    /// <returns></returns>
    public static string GetOrmReaderMethodName(string dotNetTypeString, bool allowDBNull)
    {
        string ret = null;
        #region 获取
        switch (dotNetTypeString.TrimEnd('?'))
        {
            case "byte[]":
                ret = "ToBytes";
                break;
            case "string":
                ret = "ToString";
                break;
            case "bool":
                ret = "ToBoolean";
                break;

            #region 数值
            case "sbyte":
                ret = "ToSByte";
                break;
            case "byte":
                ret = "ToByte";
                break;
            case "short":
                ret = "ToInt16";
                break;
            case "ushort":
                ret = "ToUInt16";
                break;
            case "int":
                ret = "ToInt32";
                break;
            case "uint":
                ret = "ToUInt32";
                break;
            case "long":
                ret = "ToInt64";
                break;
            case "ulong":
                ret = "ToUInt64";
                break;
            case "float":
                ret = "ToSingle";
                break;
            case "double":
                ret = "ToDouble";
                break;
            #endregion

            case "decimal":
                ret = "ToDecimal";
                break;
            case "DateTime":
                ret = "ToDateTime";
                break;
            case "TimeSpan":
                ret = "ToTimeSpan";
                break;
        }
        #endregion
        if (allowDBNull && dotNetTypeString != "string" && dotNetTypeString != "byte[]")
        {
            ret += "N";
        }
        if (string.IsNullOrEmpty(ret))
            throw new Exception("获取.NET类型对应的IDataReader获取值方法时无法识别此类型。" + dotNetTypeString);
        return ret;
    }

    /// <summary>
    /// 获取ORM使用的参数赋值DotNet代码。如：dao.AddInParameter("@UserName", userName != null ? userName : (object)DBNull.Value)
    /// </summary>
    /// <param name="ormParamName"></param>
    /// <param name="allowDBNull"></param>
    /// <param name="dotNetTypeString"></param>
    /// <returns></returns>
    public static string GetOrmParamValueToDbCode(string ormParamName, bool allowDBNull, string dotNetTypeString)
    {
        return GetOrmValueToDbCode(ormParamName, allowDBNull, dotNetTypeString);
    }
    /// <summary>
    /// 获取ORM使用的参数赋值DotNet代码。如：dao.AddInParameter("@UserName", Item.UserName.HasValue ? Item.UserName.Value : (object)DBNull.Value)
    /// </summary>
    /// <param name="ormPropertyName"></param>
    /// <param name="allowDBNull"></param>
    /// <param name="dotNetTypeString"></param>
    /// <returns></returns>
    public static string GetOrmItemValueToDbCode(string ormPropertyName, bool allowDBNull, string dotNetTypeString)
    {
        return GetOrmValueToDbCode("item." + ormPropertyName, allowDBNull, dotNetTypeString);
    }
    private static string GetOrmValueToDbCode(string source, bool allowDBNull, string dotNetTypeString)
    {
        string ret = source;
        if (allowDBNull)
        {
            if (dotNetTypeString == "string" || dotNetTypeString == "byte[]")
                ret = string.Format("{0} != null ? {0} : (object)DBNull.Value", ret);
            else
                ret = string.Format("{0}.HasValue ? {0}.Value : (object)DBNull.Value", ret);
        }
        return ret;
    }

    /// <summary>
    /// 获取字段DefaultValue的DotNet代码表示
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="dotNetTypeString"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static string ParseDefaultValueString(DbDataProvider provider, string dotNetTypeString,  string defaultValue)
    {
        string ret = string.Empty;
        switch (dotNetTypeString.TrimEnd('?'))
        {
            #region Content
            case "sbyte":
            case "byte":
            case "short":
            case "ushort":
            case "int":
            case "uint":
            case "long":
            case "ulong":
                ret = defaultValue;
                switch (provider)
                {
                    case DbDataProvider.MySqlClient:
                        if (defaultValue.StartsWith("b'"))
                        {
                            defaultValue = defaultValue.Substring(2, defaultValue.Length - 3);
                            ret = NumberSystemUtil.BaseToDec(defaultValue, 2).ToString();
                        }
                        break;
                }
                break;
            case "float":
                ret = defaultValue + "f";
                break;
            case "double":
                ret = defaultValue + "d";
                break;
            case "decimal":
                ret = defaultValue + "m";
                break;
            case "string":
                if (defaultValue == "NULL")
                    ret = "null";
                else if (defaultValue == "''")
                    ret = "string.Empty";
                else
                    ret = string.Format("\"{0}\"", defaultValue.Trim(new char[] {char.Parse("'") }));
                break;
            case "bool":
                ret = defaultValue.ToLower();
                if (ret == "1") ret = "true";
                else if (ret == "0") ret = "false";
                break;
            case "DateTime":
                switch (provider)
                {
                    case DbDataProvider.MySqlClient:
                        if (defaultValue.ToUpper().Contains("CURRENT_TIMESTAMP") || defaultValue.ToUpper().Contains("NOW"))
                            ret = "DateTime.Now";
                        break;
                }
                break;
            case "TimeSpan":
                break;
            case "byte[]":
                break;
            #endregion
        }
        return ret;
    }
    /// <summary>
    /// 解析默认值
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="dotNetTypeString"></param>
    /// <param name="dotNetType"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static Object ParseDefaultValue(DbDataProvider provider, string dotNetTypeString, Type dotNetType, string defaultValue)
    {
        object ret = defaultValue;
        switch (dotNetTypeString.TrimEnd('?'))
        {
            #region Content
            case "sbyte":
            case "byte":
            case "short":
            case "ushort":
            case "int":
            case "uint":
            case "long":
            case "ulong":
                switch (provider)
                {
                    case DbDataProvider.MySqlClient:
                        if (defaultValue.StartsWith("b'"))
                        {
                            defaultValue = defaultValue.Substring(2, defaultValue.Length - 3);
                            ret = NumberSystemUtil.BaseToDec(defaultValue, 2);
                        }
                        break;
                }
                break;
            case "DateTime":
                switch (provider)
                {
                    case DbDataProvider.MySqlClient:
                        if (defaultValue.Contains("CURRENT_TIMESTAMP") || defaultValue.Contains("NOW"))
                            ret = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        break;
                }
                break;
            #endregion
        }
        return AiUoUtil.ConvertTo(ret, dotNetType);
    }
    #endregion
}