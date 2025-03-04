using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace AiUo.Data;

public abstract partial class DaoBase
{
    /// <summary>
    /// 创建DbParameter
    /// </summary>
    /// <param name="name"></param>
    /// <param name="direction"></param>
    /// <param name="value"></param>
    /// <param name="dbType"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public DbParameter CreateParameter(string name, ParameterDirection direction = ParameterDirection.Input, object value = null, DbType dbType = default(DbType), int size = 0)
    {
        DbParameter para = Database.Factory.CreateParameter();
        para.ParameterName = Database.GetParameterName(name);
        para.Size = size;
        para.Direction = direction;
        para.Value = value ?? DBNull.Value;
        para.DbType = dbType;
        return para;
    }

    #region AddParameter by DbParameter
    /// <summary>
    /// 添加DbParameter参数
    /// </summary>
    /// <param name="param">DbParameter对象</param>
    /// <returns></returns>
    public DaoBase AddParameter(DbParameter param)
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
    public DaoBase AddParameters(params DbParameter[] paras)
        => AddParameters(paras.AsEnumerable());

    /// <summary>
    /// 添加DbParameter参数集合
    /// </summary>
    /// <param name="paras">DbParameter集合对象</param>
    /// <returns></returns>
    public DaoBase AddParameters(IEnumerable<DbParameter> paras)
    {
        if (paras != null)
        {
            foreach (DbParameter para in paras)
            {
                AddParameter(para);
            }
        }
        return this;
    }
    #endregion

    #region AddParameter by CreateParameter
    /// <summary>
    /// 添加DbParameter参数
    /// </summary>
    /// <param name="name"></param>
    /// <param name="direction"></param>
    /// <param name="value"></param>
    /// <param name="dbType"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public DaoBase AddParameter(string name, ParameterDirection direction, object value, DbType dbType, int size)
    {
        var para = CreateParameter(name, direction, value, dbType, size);
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
    public DaoBase AddParameter(string name, ParameterDirection direction = ParameterDirection.Input, DbType dbType = default(DbType), int size = 0)
        => AddParameter(name, direction, null, dbType, size);

    /// <summary>
    /// 添加输入DbParameter参数和值
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="value">参数的值</param>
    /// <param name="dbType">参数的 DbType</param>
    /// <param name="size">列中数据的最大大小（以字节为单位）</param>
    /// <returns></returns>
    public DaoBase AddInParameter(string name, object value = null, DbType dbType = default(DbType), int size = 0)
        => AddParameter(name, ParameterDirection.Input, value, dbType, size);

    /// <summary>
    /// 添加输出DbParameter参数
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="dbType">参数的 DbType</param>
    /// <param name="size">列中数据的最大大小（以字节为单位）</param>
    /// <returns></returns>
    public DaoBase AddOutParameter(string name, DbType dbType = default(DbType), int size = 0)
        => AddParameter(name, ParameterDirection.Output, null, dbType, size);

    /// <summary>
    /// 添加双向DbParameter参数
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="value">参数的值</param>
    /// <param name="dbType">参数的 DbType</param>
    /// <param name="size">列中数据的最大大小（以字节为单位）</param>
    /// <returns></returns>
    public DaoBase AddInOutParameter(string name, object value = null, DbType dbType = default(DbType), int size = 0)
        => AddParameter(name, ParameterDirection.InputOutput, value, dbType, size);
    #endregion

    /// <summary>
    /// 清除所有定义的参数集合
    /// </summary>
    public DaoBase ClearParameters()
    {
        Command.Parameters.Clear();
        return this;
    }

    /// <summary>
    /// 自动添加存储过程的输入输出参数，并给输入参数按顺序赋值
    /// </summary>
    /// <param name="values">输入参数值</param>
    /// <returns></returns>
    public DaoBase AddInParameters(params object[] values)
    {
        if (values == null || values.Length == 0)
            throw new ArgumentException("参数values不能为null");
        ClearParameters();
        Database.AutoDeriveParameters(Command);
        Database.SetParametersValue(Command, values);
        return this;
    }

    #region Get/Set Parameter
    /// <summary>
    /// 设置DbParameter参数的值
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="value">参数的值</param>
    /// <returns></returns>
    public DaoBase SetParameterValue(string name, object value)
    {
        name = Database.GetParameterName(name);
        Command.Parameters[name].Value = value ?? DBNull.Value;
        return this;
    }

    /// <summary>
    /// 获取DbParameter参数的值，可通过此方法获取输出参数的值
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <returns></returns>
    public object GetParameterValue(string name)
    {
        name = Database.GetParameterName(name);
        return Command.Parameters[name].Value;
    }

    /// <summary>
    /// 获取DbParameter参数的值，可通过此方法获取输出参数的值
    /// </summary>
    /// <param name="index">参数索引</param>
    /// <returns></returns>
    public object GetParameterValue(int index)
        => Command.Parameters[index].Value;

    /// <summary>
    /// 获取DbParameter参数的值，可通过此方法获取输出参数的值
    /// </summary>
    /// <typeparam name="T">参数值类型</typeparam>
    /// <param name="name">参数名称</param>
    /// <returns></returns>
    public T GetParameterValue<T>(string name)
        => AiUoUtil.ConvertTo<T>(GetParameterValue(name));

    /// <summary>
    /// 获取DbParameter参数的值，可通过此方法获取输出参数的值
    /// </summary>
    /// <typeparam name="T">参数值类型</typeparam>
    /// <param name="index">参数索引</param>
    /// <returns></returns>
    public T GetParameterValue<T>(int index)
        => AiUoUtil.ConvertTo<T>(GetParameterValue(index));

    /// <summary>
    /// 获取输出参数的值，如果只存在一个输出参数不用传入参数
    /// </summary>
    /// <typeparam name="T">参数值类型</typeparam>
    /// <param name="index">输出参数索引，0开始,如果存在2个输出参数，需要获取第二个则传入1</param>
    /// <returns></returns>
    public T GetOutParameterValue<T>(int index = 0)
    {
        object ret = null;
        int i = 0;
        foreach (IDataParameter para in Command.Parameters)
        {
            if (para.Direction == ParameterDirection.InputOutput || para.Direction == ParameterDirection.Output)
            {
                if (index == i)
                {
                    ret = para.Value;
                    break;
                }
                else
                    i++;
            }
        }
        return AiUoUtil.ConvertTo<T>(ret);
    }

    #endregion
}