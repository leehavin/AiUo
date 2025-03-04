using System;
using System.Data;
using System.Data.Common;

namespace AiUo.Data;

public abstract partial class Database
{
    #region Parameter
    // DbCommand参数缓存对象
    //private static readonly CommandParametersCache _parametersCache = new CommandParametersCache();

    /// <summary>
    /// 获得符合数据库提供者的参数名称
    /// </summary>
    /// <param name="parameterName"></param>
    /// <returns></returns>
    public abstract string GetParameterName(string parameterName);
    /// <summary>
    /// 参数前缀
    /// </summary>
    protected abstract char ParameterToken { get; }

    /// <summary>
    /// 根据语句或存储过程自动解析并填充DbCommand对象的参数集合
    /// </summary>
    /// <param name="command"></param>
    protected abstract void DeriveParameters(CommandWrapper command);

    //自动填充参数，使用参数缓存
    internal void AutoDeriveParameters(CommandWrapper command)
    {
        DeriveParameters(command);
        //存储过程DeriveParameters时无法区分InputOutput，所以全部设置成Output
        if (command.CommandType == CommandType.StoredProcedure)
        {
            foreach (IDataParameter para in command.Parameters)
            {
                if (para.Direction == ParameterDirection.InputOutput)
                    para.Direction = ParameterDirection.Output;
            }
        }
        //if (_parametersCache.Contains(command))
        //{
        //    IDataParameter[] paras = _parametersCache.Get(command, this);
        //    if (paras != null && paras.Length > 0)
        //        command.Parameters.AddRange(paras);
        //}
        //else
        //{
        //    DeriveParameters(command);
        //    //存储过程DeriveParameters时无法区分InputOutput，所以全部设置成Output
        //    if (command.CommandType == CommandType.StoredProcedure)
        //    {
        //        foreach (IDataParameter para in command.Parameters)
        //        {
        //            if (para.Direction == ParameterDirection.InputOutput)
        //                para.Direction = ParameterDirection.Output;
        //        }
        //    }
        //    _parametersCache.Set(command, this);
        //}
    }

    // 设置Command所有参数的值
    internal void SetParametersValue(CommandWrapper command, params object[] values)
    {
        switch (command.CommandType)
        {
            case CommandType.Text:
                if (values.Length != command.Parameters.Count)
                    throw new ArgumentException("传入的参数值的数量错误，请核实SQL语句中的参数顺序和数量重新传入。");
                for (int i = 0; i < values.Length; i++)
                {
                    var value = values[i];
                    command.Parameters[i].Value = value ?? DBNull.Value;
                    SetParameterDbType(command.Parameters[i], value);
                }
                break;
            case CommandType.StoredProcedure:
                int index = 0;
                foreach (IDataParameter para in command.Parameters)
                {
                    if (para.Direction == ParameterDirection.Input || para.Direction == ParameterDirection.InputOutput)
                    {
                        if (index >= values.Length)
                            throw new ArgumentException("传入的参数值的数量与存储过程所需参数值的数量不符，请核实存储过程中参数的数量和顺序。");
                        para.Value = values[index] ?? DBNull.Value;
                        index++;
                    }
                }
                break;
            case CommandType.TableDirect:
                throw new ArgumentException("TableDirect不支持自动配置参数集合。");
        }
    }
    protected abstract void SetParameterDbType(DbParameter para, object value);

    #endregion // Parameter

    #region CreateParameter

    /// <summary>
    /// 创建参数对象DbParameter
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="dbType">参数的 DbType</param>
    /// <param name="size">列中数据的最大大小（以字节为单位）</param>
    /// <param name="direction">指示参数是只可输入、只可输出、双向还是存储过程返回值参数</param>
    /// <param name="value">参数的值</param>
    /// <returns></returns>
    public virtual DbParameter CreateParameter(string name, DbType dbType = DbType.AnsiString, int size = 0, ParameterDirection direction = ParameterDirection.Input, object value = null)
    {
        DbParameter param = Factory.CreateParameter();
        param.ParameterName = GetParameterName(name);
        param.DbType = dbType;
        param.Size = size;
        param.Direction = direction;
        param.Value = value ?? DBNull.Value;
        return param;
    }

    /// <summary>
    /// 创建输入参数DbParameter对象
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="value">参数的值</param>
    /// <param name="dbType">参数的 DbType</param>
    /// <param name="size">列中数据的最大大小（以字节为单位）</param>
    /// <returns></returns>
    public virtual DbParameter CreateInParameter(string name, object value = null, DbType dbType = DbType.AnsiString, int size = 0)
        => CreateParameter(name, dbType, size, ParameterDirection.Input, value);

    /// <summary>
    /// 创建输出参数DbParameter
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="dbType">参数的 DbType</param>
    /// <param name="size">列中数据的最大大小（以字节为单位）</param>
    /// <returns></returns>
    public virtual DbParameter CreateOutParameter(string name, DbType dbType = DbType.AnsiString, int size = 0)
        => CreateParameter(name, dbType, size, ParameterDirection.Output, null);

    /// <summary>
    /// 创建双向参数DbParameter
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="value">参数的值</param>
    /// <param name="dbType">参数的 DbType</param>
    /// <param name="size">列中数据的最大大小（以字节为单位）</param>
    /// <returns></returns>
    public virtual DbParameter CreateInOutParameter(string name, object value = null, DbType dbType = DbType.AnsiString, int size = 0)
        => CreateParameter(name, dbType, size, ParameterDirection.InputOutput, null);

    /// <summary>
    /// 添加返回值参数DbParameter
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <returns></returns>
    public virtual DbParameter CreateReturnParameter(string name = "RETURN_VALUE")
        => CreateParameter("RETURN_VALUE", DbType.Int32, 0, ParameterDirection.ReturnValue, null);
    #endregion
}