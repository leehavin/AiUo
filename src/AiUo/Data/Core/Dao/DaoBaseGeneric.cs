using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace AiUo.Data;

/// <summary>
/// DaoBase泛型基类
/// </summary>
/// <typeparam name="TParameter">DbParameter类型，如：MySqlParameter</typeparam>
/// <typeparam name="TDbType">DbParameter的参数类型，如：MySqlDbType</typeparam>
public abstract partial class DaoBase<TParameter, TDbType> : DaoBase
    where TParameter : DbParameter
    where TDbType : struct
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="commandType"></param>
    /// <param name="database"></param>
    public DaoBase(string commandText, CommandType commandType, Database database) 
        : base(commandText, commandType, database) { }

    /// <summary>
    /// 获取最后一个参数
    /// </summary>
    public TParameter LastParameter => Command.Parameters.Count > 0 
        ? Command.Parameters[Command.Parameters.Count - 1] as TParameter 
        : null;

    /// <summary>
    /// 子类设置Parameter的DbType属性
    /// </summary>
    /// <param name="para"></param>
    /// <param name="dbType"></param>
    protected abstract void SetParameterDbType(TParameter para, TDbType dbType);

    /// <summary>
    /// 创建DbParameter对象
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="dbType">参数的 DbType</param>
    /// <param name="size">列中数据的最大大小（以字节为单位）</param>
    /// <param name="direction">指示参数是只可输入、只可输出、双向还是存储过程返回值参数</param>
    /// <param name="value">参数的值</param>
    /// <returns></returns>
    public TParameter CreateParameter(string name, ParameterDirection direction = ParameterDirection.Input, object value = null, TDbType dbType = default(TDbType), int size = 0)
    {
        TParameter para = Database.Factory.CreateParameter() as TParameter;
        para.ParameterName = Database.GetParameterName(name);
        para.Size = size;
        para.Direction = direction;
        para.Value = value ?? DBNull.Value;
        SetParameterDbType(para, dbType);
        return para;
    }

    #region AddParameter by TParameter
    /// <summary>
    /// 添加DbParameter参数
    /// </summary>
    /// <param name="param">DbParameter对象</param>
    /// <returns></returns>
    public DaoBase<TParameter, TDbType> AddParameter(TParameter param)
    {
        if (param.Value == null) param.Value = DBNull.Value;
        Command.Parameters.Add(param);
        return this;
    }

    /// <summary>
    /// 添加DbParameter参数集合
    /// </summary>
    /// <param name="paras">DbParameter集合对象</param>
    /// <returns></returns>
    public DaoBase<TParameter, TDbType> AddParameters(params TParameter[] paras)
        => AddParameters(paras.AsEnumerable());

    /// <summary>
    /// 添加DbParameter参数集合
    /// </summary>
    /// <param name="paras">DbParameter集合对象</param>
    /// <returns></returns>
    public DaoBase<TParameter, TDbType> AddParameters(IEnumerable<TParameter> paras)
    {
        if (paras != null)
        {
            foreach (var para in paras)
            {
                AddParameter(para);
            }
        }
        return this;
    }
    #endregion

    #region AddParameter by CreateParameter
    /// <summary>
    /// 添加参数
    /// </summary>
    /// <param name="name"></param>
    /// <param name="direction"></param>
    /// <param name="value"></param>
    /// <param name="dbType"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public DaoBase<TParameter, TDbType> AddParameter(string name, ParameterDirection direction, object value, TDbType dbType, int size)
    {
        TParameter para = CreateParameter(name, direction, value, dbType, size);
        AddParameter(para);
        return this;
    }

    /// <summary>
    /// 添加DbParameter参数,不包括值
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="dbType">参数的 DbType</param>
    /// <param name="direction">指示参数是只可输入、只可输出、双向还是存储过程返回值参数</param>
    /// <param name="size">列中数据的最大大小（以字节为单位）</param>
    /// <returns></returns>
    public DaoBase<TParameter, TDbType> AddParameter(string name, ParameterDirection direction = ParameterDirection.Input, TDbType dbType = default(TDbType), int size = 0)
        => AddParameter(name, direction, null, dbType, size);

    /// <summary>
    /// 添加输入DbParameter参数和值
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="value">参数的值</param>
    /// <param name="dbType">参数的 DbType</param>
    /// <param name="size">列中数据的最大大小（以字节为单位）</param>
    /// <returns></returns>
    public DaoBase<TParameter, TDbType> AddInParameter(string name, object value = null, TDbType dbType = default(TDbType), int size = 0)
        => AddParameter(name, ParameterDirection.Input, value, dbType, size);

    /// <summary>
    /// 添加输出DbParameter参数
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="dbType">参数的 DbType</param>
    /// <param name="size">列中数据的最大大小（以字节为单位）</param>
    /// <returns></returns>
    public DaoBase<TParameter, TDbType> AddOutParameter(string name, TDbType dbType = default(TDbType), int size = 0)
        => AddParameter(name, ParameterDirection.Output, null, dbType, size);

    /// <summary>
    /// 添加双向DbParameter参数
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="value">参数的值</param>
    /// <param name="dbType">参数的 DbType</param>
    /// <param name="size">列中数据的最大大小（以字节为单位）</param>
    /// <returns></returns>
    public DaoBase<TParameter, TDbType> AddInOutParameter(string name, object value = null, TDbType dbType = default(TDbType), int size = 0)
        => AddParameter(name, ParameterDirection.InputOutput, value, dbType, size);
    #endregion
}