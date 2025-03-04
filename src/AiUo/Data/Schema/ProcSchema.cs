using System;
using System.Collections.Generic;
using System.Data;

namespace AiUo.Data.Schema;

/// <summary>
/// 存储过程Schema
/// </summary>
[Serializable]
public class ProcSchema : DBObjectSchemaBase, ISchemaCollectionKey
{
    /// <summary>
    /// 存储过程名称
    /// </summary>
    public string ProcName { get { return SourceName; } set { SourceName = value; } }
    /// <summary>
    /// SQL语句中使用的名称
    /// </summary>
    public string SqlProcName
    {
        get { return SqlSourceName; }
    }
    /// <summary>
    /// ROUTINE_DEFINITION
    /// </summary>
    public string Definition { get; set; }
    /// <summary>
    /// IS_DETERMINISTIC
    /// </summary>
    public bool IsDeterministic { get; set; }
    /// <summary>
    /// 创建时间 CREATED
    /// </summary>
    public DateTime CreateTime { get; set; }
    /// <summary>
    /// LAST_ALTERED
    /// </summary>
    public DateTime UpdateTime { get; set; }
    /// <summary> 
    /// 默认字符集 CHARACTER_SET_CLIENT
    /// </summary>
    public string CharSetName { get; set; }
    /// <summary>
    /// 排序规则 COLLATION_CONNECTION
    /// </summary>
    public string CollationName { get; set; }

    /// <summary>
    /// 存储过程参数集合
    /// </summary>
    public SchemaCollection<DbParameterSchema> Parameters = new SchemaCollection<DbParameterSchema>();
    /// <summary>
    /// 获取Key
    /// </summary>
    /// <returns></returns>
    public string GetKey()
    {
        return ProcName;
    }
    /// <summary>
    /// 是否有输入参数
    /// </summary>
    public bool HasInputParameter { get { return InputParameters.Count > 0; } }
    /// <summary>
    /// 是否有输出参数
    /// </summary>
    public bool HasOutputParameter { get { return OutputParameters.Count > 0; } }

    private List<DbParameterSchema> _inputParameters;
    /// <summary>
    /// 输入参数集合
    /// </summary>
    public List<DbParameterSchema> InputParameters
    {
        get
        {
            if (_inputParameters == null)
            {
                _inputParameters = new List<DbParameterSchema>();
                foreach (DbParameterSchema para in Parameters)
                {
                    if (para.Direction == ParameterDirection.Input || para.Direction == ParameterDirection.InputOutput)
                        _inputParameters.Add(para);
                }
            }
            return _inputParameters;
        }
    }
    private List<DbParameterSchema> _outputParameters;
    /// <summary>
    /// 输出参数集合
    /// </summary>
    public List<DbParameterSchema> OutputParameters
    {
        get
        {
            if (_outputParameters == null)
            {
                _outputParameters = new List<DbParameterSchema>();
                foreach (DbParameterSchema para in Parameters)
                {
                    if (para.Direction == ParameterDirection.Output)
                        _outputParameters.Add(para);
                }
            }
            return _outputParameters;
        }
    }
}