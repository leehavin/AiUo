using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using AiUo.Data.Schema;

namespace AiUo.Data.MySql;

/// <summary>
/// 
/// </summary>
public static class MySqlPrimaryKeyUtil
{
    /// <summary>
    /// 根据主键Schema序列化json。key: ColumnName
    /// </summary>
    /// <param name="primaryKey"></param>
    /// <returns></returns>
    public static string Serialize(PrimaryKeySchema primaryKey)
    {
        var primaryKeys = new Dictionary<string, MySqlPrimaryKeyColumn>();
        foreach (var column in primaryKey.Columns)
        {
            var pk = new MySqlPrimaryKeyColumn()
            {
                ColumnName = column.ColumnName,
                ParameterName = column.SqlParamName,
                MySqlDbType = ((MySqlColumnSchema)column).DbType,
                DotNetType = ((MySqlColumnSchema)column).DotNetType,
            };
            primaryKeys.Add(column.ColumnName, pk);
        }
        return SerializerUtil.SerializeJson(primaryKeys);
    }
    /// <summary>
    /// 反序列化json主键集合。key: ColumnName
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static Dictionary<string, MySqlPrimaryKeyColumn> Deserialize(string json)
        => SerializerUtil.DeserializeJson<Dictionary<string, MySqlPrimaryKeyColumn>>(json);
    //=> SerializerUtil.DeserializeJson<SerializableDictionary<string, MySqlPrimaryKeyColumn>>(json);

    /// <summary>
    /// 根据主键列名和值生成主键编码，格式: id1,123|id2,abc
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public static string BuildId(List<(string columnName, object value)> ids)
    {
        string ret = null;
        foreach (var id in ids)
        {
            ret += $"|{id.columnName},{HttpUtility.UrlEncode(Convert.ToString(id.value))}";
        }
        return ret.TrimStart('|');
    }

    /// <summary>
    /// 根据主键编码生成参数集合
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pk"></param>
    /// <returns></returns>
    public static List<MySqlParameter> BuildParameters(string id, string pk)
    {
        var pks = Deserialize(pk);
        var values = id.Split('|');
        if (values.Length != pks.Count)
            throw new Exception($"主键数量不相等。id: {id}");
        var paras = new List<MySqlParameter>();
        for (int i = 0; i < values.Length; i++)
        {
            var pair = values[i].Split(',');
            if (pair.Length != 2)
                throw new Exception($"主键值格式不正确,格式：'id,123' 。id: {values[i]}");
            var name = pair[0];
            var value = HttpUtility.UrlDecode(pair[1]);
            paras.Add(pks[name].BuildParameter(value));
        }
        return paras;

    }
}
/// <summary>
/// 
/// </summary>
public class MySqlPrimaryKeyColumn
{
    /// <summary>
    /// 字段名
    /// </summary>
    public string ColumnName { get; set; }
    /// <summary>
    /// 参数名
    /// </summary>
    public string ParameterName { get; set; }
    /// <summary>
    /// 参数类型
    /// </summary>
    public MySqlDbType MySqlDbType { get; set; }
    /// <summary>
    /// 值的DotNet数据类型
    /// </summary>
    public Type DotNetType { get; set; }
    /// <summary>
    /// 创建Parameter
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public MySqlParameter BuildParameter(object value = null)
    {
        return new MySqlParameter()
        {
            Direction = ParameterDirection.Input,
            ParameterName = ParameterName,
            MySqlDbType = MySqlDbType,
            Value = AiUoUtil.ConvertTo(value, DotNetType),
        };
    }
}