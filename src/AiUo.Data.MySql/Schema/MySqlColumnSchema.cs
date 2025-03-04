
using MySql.Data.MySqlClient;
using System;
using AiUo.Data.Schema;

namespace AiUo.Data.MySql;

/// <summary>
/// MySqlColumnSchema
/// </summary>
[Serializable]
public class MySqlColumnSchema : ColumnSchema<MySqlEngineType, MySqlDbType>
{
    /// <summary>
    /// MySqlTypeMapper
    /// </summary>
    public override IDbTypeMapper<MySqlEngineType, MySqlDbType> TypeMapper => new MySqlTypeMapper();
    /*
    private MySqlEngineType _engineType = MySqlEngineType.Unknow;
    /// <summary>
    /// MySQL字段原始数据类型
    /// </summary>
    public MySqlEngineType EngineType
    {
        get
        {
            if (string.IsNullOrEmpty(EngineTypeString) || string.IsNullOrEmpty(EngineTypeStringFull))
                throw new Exception("字段原始数据类型EngineTypeString不能为空。");
            if (_engineType == MySqlEngineType.Unknow)
                _engineType = new MySqlTypeMapper().MapEngineType(EngineTypeString, EngineTypeStringFull);
            return _engineType;
        }
    }
    /// <summary>
    /// 通过MySQL字段原始类型推断的 MySqlDbType 类型
    /// </summary>
    public MySqlDbType MySqlDbType
    {
        get
        {
            if (EngineType == MySqlEngineType.Unknow)
                throw new Exception("未知的MySqlEngineType。");
            return new MySqlTypeMapper().MapDbType(EngineType);
        }
    }
    /// <summary>
    /// MySqlDbType 的字符串表示：MySqlDbTypeAll
    /// </summary>
    public string MySqlDbTypeString { get { return "MySqlDbType." + MySqlDbType.ToString(); } }
    */
}