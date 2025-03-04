using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using AiUo.Data.Instrumentation;

namespace AiUo.Data;

/// <summary>
/// Database泛型类型
/// </summary>
/// <typeparam name="TParameter"></typeparam>
/// <typeparam name="TDbType"></typeparam>
public abstract class Database<TParameter, TDbType> : Database
    where TParameter : DbParameter
    where TDbType : struct
{
    #region Constructors
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="config"></param>
    public Database(ConnectionStringConfig config)
        : base(config) { }
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="connectionStringName"></param>
    public Database(string connectionStringName = null) 
        : base(connectionStringName) {  }
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="commandTimeout"></param>
    /// <param name="inst"></param>
    public Database(string connectionString, int commandTimeout, IDataInstProvider inst = null) 
        : base(connectionString, commandTimeout, inst) { }
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="readConnectionString"></param>
    /// <param name="commandTimeout"></param>
    /// <param name="inst"></param>
    public Database(string connectionString, string readConnectionString, int commandTimeout, IDataInstProvider inst = null) 
        : base(connectionString, readConnectionString, commandTimeout, inst) { }
    #endregion

    /// <summary>
    /// 设置TParameter的TDbType
    /// </summary>
    /// <param name="para"></param>
    /// <param name="dbType"></param>
    protected abstract void SetParameterDbType(TParameter para, TDbType dbType);

    #region CreateParameter
    /// <summary>
    /// 创建参数对象MySqlParameter
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="dbType">参数的 DbType</param>
    /// <param name="size">列中数据的最大大小（以字节为单位）</param>
    /// <param name="direction">指示参数是只可输入、只可输出、双向还是存储过程返回值参数</param>
    /// <param name="value">参数的值</param>
    /// <returns></returns>
    public TParameter CreateParameter(string name, TDbType dbType = default, int size = 0, ParameterDirection direction = ParameterDirection.Input, object value = null)
    {
        var param = Factory.CreateParameter() as TParameter;
        param.ParameterName = GetParameterName(name);
        param.Size = size;
        param.Direction = direction;
        param.Value = value ?? DBNull.Value;
        SetParameterDbType(param, dbType);
        return param;
    }

    /// <summary>
    /// 创建输入参数MySqlParameter对象
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="value">参数的值</param>
    /// <param name="dbType">参数的 DbType</param>
    /// <param name="size">列中数据的最大大小（以字节为单位）</param>
    /// <returns></returns>
    public TParameter CreateInParameter(string name, object value = null, TDbType dbType = default, int size = 0)
        => CreateParameter(name, dbType, size, ParameterDirection.Input, value);

    /// <summary>
    /// 创建输出参数MySqlParameter
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="dbType">参数的 DbType</param>
    /// <param name="size">列中数据的最大大小（以字节为单位）</param>
    /// <returns></returns>
    public TParameter CreateOutParameter(string name, TDbType dbType = default, int size = 0)
        => CreateParameter(name, dbType, size, ParameterDirection.Output, null);

    /// <summary>
    /// 创建双向参数MySqlParameter
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="value">参数的值</param>
    /// <param name="dbType">参数的 DbType</param>
    /// <param name="size">列中数据的最大大小（以字节为单位）</param>
    /// <returns></returns>
    public TParameter CreateInOutParameter(string name, object value = null, TDbType dbType = default, int size = 0)
        => CreateParameter(name, dbType, size, ParameterDirection.InputOutput, value);

    #endregion

    #region Execute (TDbType dbType, object value)[]
    private CommandWrapper GetExecCommand(string commandText, CommandType commandType, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
    {
        CommandWrapper command = CreateCommand(commandText, commandType, null, tm);
        AutoDeriveParameters(command);
        if (parameters != null)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                var param = command.Parameters[i] as TParameter;
                SetParameterDbType(param, parameters[i].dbType);
                param.Value = parameters[i].value;
            }
        }
        return command;
    }

    #region ExecSqlScalar
    /// <summary>
    /// 执行SQL语句并返回首行首列数据。
    /// 如果返回null表示一行数据都没有，返回DBNull表示首行首列的值为空
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public object ExecSqlScalar(string sql, params (TDbType dbType, object value)[] parameters)
        => ExecScalar(GetExecCommand(sql, CommandType.Text, null, parameters));
    public async Task<object> ExecSqlScalarAsync(string sql, params (TDbType dbType, object value)[] parameters)
        => await ExecScalarAsync(GetExecCommand(sql, CommandType.Text, null, parameters));

    /// <summary>
    /// 执行SQL语句并返回首行首列数据。
    /// 如果返回null表示一行数据都没有，返回DBNull表示首行首列的值为空
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public T ExecSqlScalar<T>(string sql, params (TDbType dbType, object value)[] parameters)
        => AiUoUtil.ConvertTo<T>(ExecSqlScalar(sql, parameters));
    public async Task<T> ExecSqlScalarAsync<T>(string sql, params (TDbType dbType, object value)[] parameters)
        => AiUoUtil.ConvertTo<T>(await ExecSqlScalarAsync(sql, parameters));

    /// <summary>
    /// 执行SQL语句并返回首行首列数据。
    /// 如果返回null表示一行数据都没有，返回DBNull表示首行首列的值为空
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public object ExecSqlScalar(string sql, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => ExecScalar(GetExecCommand(sql, CommandType.Text, tm, parameters));
    public async Task<object> ExecSqlScalarAsync(string sql, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => await ExecScalarAsync(GetExecCommand(sql, CommandType.Text, tm, parameters));

    /// <summary>
    /// 执行SQL语句并返回首行首列数据。
    /// 如果返回null表示一行数据都没有，返回DBNull表示首行首列的值为空
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public T ExecSqlScalar<T>(string sql, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => AiUoUtil.ConvertTo<T>(ExecSqlScalar(sql, tm, parameters));
    public async Task<T> ExecSqlScalarAsync<T>(string sql, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => AiUoUtil.ConvertTo<T>(await ExecSqlScalarAsync(sql, tm, parameters));
    #endregion

    #region ExecProcScalar
    /// <summary>
    /// 执行存储过程并返回首行首列数据
    /// </summary>
    /// <param name="proc"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public object ExecProcScalar(string proc, params (TDbType dbType, object value)[] parameters)
        => ExecScalar(GetExecCommand(proc, CommandType.StoredProcedure, null, parameters));
    public async Task<object> ExecProcScalarAsync(string proc, params (TDbType dbType, object value)[] parameters)
        => await ExecScalarAsync(GetExecCommand(proc, CommandType.StoredProcedure, null, parameters));

    /// <summary>
    /// 执行存储过程并返回首行首列数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public T ExecProcScalar<T>(string proc, params (TDbType dbType, object value)[] parameters)
        => AiUoUtil.ConvertTo<T>(ExecProcScalar(proc, parameters));
    public async Task<T> ExecProcScalarAsync<T>(string proc, params (TDbType dbType, object value)[] parameters)
        => AiUoUtil.ConvertTo<T>(await ExecProcScalarAsync(proc, parameters));

    /// <summary>
    /// 执行存储过程并返回首行首列数据
    /// </summary>
    /// <param name="proc"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public object ExecProcScalar(string proc, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => ExecScalar(GetExecCommand(proc, CommandType.StoredProcedure, tm, parameters));
    public async Task<object> ExecProcScalarAsync(string proc, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => await ExecScalarAsync(GetExecCommand(proc, CommandType.StoredProcedure, tm, parameters));

    /// <summary>
    /// 执行存储过程并返回首行首列数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public T ExecProcScalar<T>(string proc, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => AiUoUtil.ConvertTo<T>(ExecProcScalar(proc, tm, parameters));
    public async Task<T> ExecProcScalarAsync<T>(string proc, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => AiUoUtil.ConvertTo<T>(await ExecProcScalarAsync(proc, tm, parameters));
    #endregion

    #region ExecNonQuery
    /// <summary>
    /// 执行SQL语句并返回受影响的行数
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public int ExecSqlNonQuery(string sql, params (TDbType dbType, object value)[] parameters)
        => ExecNonQuery(GetExecCommand(sql, CommandType.Text, null, parameters));
    public async Task<int> ExecSqlNonQueryAsync(string sql, params (TDbType dbType, object value)[] parameters)
        => await ExecNonQueryAsync(GetExecCommand(sql, CommandType.Text, null, parameters));

    /// <summary>
    /// 执行SQL语句并返回受影响的行数
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public int ExecSqlNonQuery(string sql, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => ExecNonQuery(GetExecCommand(sql, CommandType.Text, tm, parameters));
    public async Task<int> ExecSqlNonQueryAsync(string sql, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => await ExecNonQueryAsync(GetExecCommand(sql, CommandType.Text, tm, parameters));

    /// <summary>
    /// 执行存储过程并返回受影响的行数
    /// </summary>
    /// <param name="proc"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public int ExecProcNonQuery(string proc, params (TDbType dbType, object value)[] parameters)
        => ExecNonQuery(GetExecCommand(proc, CommandType.StoredProcedure, null, parameters));
    public async Task<int> ExecProcNonQueryAsync(string proc, params (TDbType dbType, object value)[] parameters)
        => await ExecNonQueryAsync(GetExecCommand(proc, CommandType.StoredProcedure, null, parameters));

    /// <summary>
    /// 执行存储过程并返回受影响的行数
    /// </summary>
    /// <param name="proc"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public int ExecProcNonQuery(string proc, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => ExecNonQuery(GetExecCommand(proc, CommandType.StoredProcedure, tm, parameters));
    public async Task<int> ExecProcNonQueryAsync(string proc, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => await ExecNonQueryAsync(GetExecCommand(proc, CommandType.StoredProcedure, tm, parameters));
    #endregion

    #region ExecReader
    /// <summary>
    /// 执行SQL语句并返回结果集
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public DataReaderWrapper ExecSqlReader(string sql, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(sql, CommandType.Text, null, parameters));
    public async Task<DataReaderWrapper> ExecSqlReaderAsync(string sql, params (TDbType dbType, object value)[] parameters)
        => await ExecReaderAsync(GetExecCommand(sql, CommandType.Text, null, parameters));

    /// <summary>
    /// 执行SQL语句并返回结果集
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public DataReaderWrapper ExecSqlReader(string sql, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(sql, CommandType.Text, tm, parameters));
    public async Task<DataReaderWrapper> ExecSqlReaderAsync(string sql, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => await ExecReaderAsync(GetExecCommand(sql, CommandType.Text, tm, parameters));

    /// <summary>
    /// 执行存储过程并返回结果集
    /// </summary>
    /// <param name="proc"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public DataReaderWrapper ExecProcReader(string proc, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(proc, CommandType.StoredProcedure, null, parameters));
    public async Task<DataReaderWrapper> ExecProcReaderAsync(string proc, params (TDbType dbType, object value)[] parameters)
        => await ExecReaderAsync(GetExecCommand(proc, CommandType.StoredProcedure, null, parameters));

    /// <summary>
    /// 执行存储过程并返回结果集
    /// </summary>
    /// <param name="proc"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public DataReaderWrapper ExecProcReader(string proc, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(proc, CommandType.StoredProcedure, tm, parameters));
    public async Task<DataReaderWrapper> ExecProcReaderAsync(string proc, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => await ExecReaderAsync(GetExecCommand(proc, CommandType.StoredProcedure, tm, parameters));
    #endregion

    #region ExecTable
    /// <summary>
    /// 执行SQL语句并返回DataTable
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public DataTable ExecSqlTable(string sql, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(sql, CommandType.Text, null, parameters)).ToTable();
    public async Task<DataTable> ExecSqlTableAsync(string sql, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(sql, CommandType.Text, null, parameters));
        return reader.ToTable();
    }

    /// <summary>
    /// 执行SQL语句并返回DataTable
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public DataTable ExecSqlTable(string sql, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(sql, CommandType.Text, tm, parameters)).ToTable();
    public async Task<DataTable> ExecSqlTableAsync(string sql, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(sql, CommandType.Text, tm, parameters));
        return reader.ToTable();
    }

    /// <summary>
    /// 执行存储过程并返回DataTable
    /// </summary>
    /// <param name="proc"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public DataTable ExecProcTable(string proc, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(proc, CommandType.StoredProcedure, null, parameters)).ToTable();
    public async Task<DataTable> ExecProcTableAsync(string proc, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(proc, CommandType.StoredProcedure, null, parameters));
        return reader.ToTable();
    }

    /// <summary>
    /// 执行存储过程并返回DataTable
    /// </summary>
    /// <param name="proc"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public DataTable ExecProcTable(string proc, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(proc, CommandType.StoredProcedure, tm, parameters)).ToTable();
    public async Task<DataTable> ExecProcTableAsync(string proc, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(proc, CommandType.StoredProcedure, tm, parameters));
        return reader.ToTable();
    }
    #endregion

    #region ExecList
    /// <summary>
    /// 执行SQL语句获List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(sql, CommandType.Text, null, parameters)).MapToList<T>();
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(sql, CommandType.Text, null, parameters));
        return reader.MapToList<T>();
    }

    /// <summary>
    /// 执行SQL语句获List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(sql, CommandType.Text, tm, parameters)).MapToList<T>();
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(sql, CommandType.Text, tm, parameters));
        return reader.MapToList<T>();
    }

    /// <summary>
    /// 执行SQL语句获List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="tm"></param>
    /// <param name="rowMapper"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, TransactionManager tm, Func<IDataReader, T> rowMapper, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(sql, CommandType.Text, tm, parameters)).MapToList<T>(rowMapper);
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, TransactionManager tm, Func<IDataReader, T> rowMapper, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(sql, CommandType.Text, tm, parameters));
        return reader.MapToList<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL语句获List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="tm"></param>
    /// <param name="mode"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, TransactionManager tm, DataMappingMode mode, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(sql, CommandType.Text, tm, parameters)).MapToList<T>(mode);
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, TransactionManager tm, DataMappingMode mode, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(sql, CommandType.Text, tm, parameters));
        return reader.MapToList<T>(mode);
    }

    /// <summary>
    /// 执行存储过程获List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string sql, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(sql, CommandType.StoredProcedure, null, parameters)).MapToList<T>();
    public async Task<List<T>> ExecProcListAsync<T>(string sql, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(sql, CommandType.StoredProcedure, null, parameters));
        return reader.MapToList<T>();
    }

    /// <summary>
    /// 执行存储过程获List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string sql, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(sql, CommandType.StoredProcedure, tm, parameters)).MapToList<T>();
    public async Task<List<T>> ExecProcListAsync<T>(string sql, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(sql, CommandType.StoredProcedure, tm, parameters));
        return reader.MapToList<T>();
    }

    /// <summary>
    /// 执行存储过程获List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="tm"></param>
    /// <param name="rowMapper"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string sql, TransactionManager tm, Func<IDataReader, T> rowMapper, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(sql, CommandType.StoredProcedure, tm, parameters)).MapToList<T>(rowMapper);
    public async Task<List<T>> ExecProcListAsync<T>(string sql, TransactionManager tm, Func<IDataReader, T> rowMapper, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(sql, CommandType.StoredProcedure, tm, parameters));
        return reader.MapToList<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL语句获List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="tm"></param>
    /// <param name="mode"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string sql, TransactionManager tm, DataMappingMode mode, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(sql, CommandType.StoredProcedure, tm, parameters)).MapToList<T>(mode);
    public async Task<List<T>> ExecProcListAsync<T>(string sql, TransactionManager tm, DataMappingMode mode, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(sql, CommandType.StoredProcedure, tm, parameters));
        return reader.MapToList<T>(mode);
    }

    #endregion

    #region ExecMulti
    /// <summary>
    /// 执行SQL语句获IEnumerable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(sql, CommandType.Text, null, parameters)).MapToMulti<T>();
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(sql, CommandType.Text, null, parameters));
        return reader.MapToMulti<T>();
    }

    /// <summary>
    /// 执行SQL语句获IEnumerable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(sql, CommandType.Text, tm, parameters)).MapToMulti<T>();
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(sql, CommandType.Text, tm, parameters));
        return reader.MapToMulti<T>();
    }

    /// <summary>
    /// 执行SQL语句获IEnumerable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="tm"></param>
    /// <param name="rowMapper"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, TransactionManager tm, Func<IDataReader, T> rowMapper, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(sql, CommandType.Text, tm, parameters)).MapToMulti<T>(rowMapper);
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, TransactionManager tm, Func<IDataReader, T> rowMapper, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(sql, CommandType.Text, tm, parameters));
        return reader.MapToMulti<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL语句获IEnumerable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="tm"></param>
    /// <param name="mode"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, TransactionManager tm, DataMappingMode mode, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(sql, CommandType.Text, tm, parameters)).MapToMulti<T>(mode);
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, TransactionManager tm, DataMappingMode mode, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(sql, CommandType.Text, tm, parameters));
        return reader.MapToMulti<T>(mode);
    }

    /// <summary>
    /// 执行存储过程获IEnumerable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(proc, CommandType.StoredProcedure, null, parameters)).MapToMulti<T>();
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(proc, CommandType.StoredProcedure, null, parameters));
        return reader.MapToMulti<T>();
    }

    /// <summary>
    /// 执行存储过程获IEnumerable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(proc, CommandType.StoredProcedure, tm, parameters)).MapToMulti<T>();
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(proc, CommandType.StoredProcedure, tm, parameters));
        return reader.MapToMulti<T>();
    }

    /// <summary>
    /// 执行存储过程获IEnumerable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc"></param>
    /// <param name="tm"></param>
    /// <param name="rowMapper"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, TransactionManager tm, Func<IDataReader, T> rowMapper, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(proc, CommandType.StoredProcedure, tm, parameters)).MapToMulti<T>(rowMapper);
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, TransactionManager tm, Func<IDataReader, T> rowMapper, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(proc, CommandType.StoredProcedure, tm, parameters));
        return reader.MapToMulti<T>(rowMapper);
    }

    /// <summary>
    /// 执行存储过程获IEnumerable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc"></param>
    /// <param name="tm"></param>
    /// <param name="mode"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, TransactionManager tm, DataMappingMode mode, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(proc, CommandType.StoredProcedure, tm, parameters)).MapToMulti<T>(mode);
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, TransactionManager tm, DataMappingMode mode, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(proc, CommandType.StoredProcedure, tm, parameters));
        return reader.MapToMulti<T>(mode);
    }
    #endregion

    #region ExecSingle
    /// <summary>
    /// 执行SQL语句获取单行数据并进行数据并进行类型转换
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public T ExecSqlSingle<T>(string sql, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(sql, CommandType.Text, null, parameters)).MapToSingle<T>();
    public async Task<T> ExecSqlSingleAsync<T>(string sql, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(sql, CommandType.Text, null, parameters));
        return reader.MapToSingle<T>();
    }

    /// <summary>
    /// 执行SQL语句获取单行数据并进行数据并进行类型转换
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public T ExecSqlSingle<T>(string sql, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(sql, CommandType.Text, tm, parameters)).MapToSingle<T>();
    public async Task<T> ExecSqlSingleAsync<T>(string sql, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(sql, CommandType.Text, tm, parameters));
        return reader.MapToSingle<T>();
    }

    /// <summary>
    /// 执行SQL语句获取单行数据并进行数据并进行类型转换
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="tm"></param>
    /// <param name="rowMapper"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public T ExecSqlSingle<T>(string sql, TransactionManager tm, Func<IDataReader, T> rowMapper, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(sql, CommandType.Text, tm, parameters)).MapToSingle<T>(rowMapper);
    public async Task<T> ExecSqlSingleAsync<T>(string sql, TransactionManager tm, Func<IDataReader, T> rowMapper, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(sql, CommandType.Text, tm, parameters));
        return reader.MapToSingle<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL语句获取单行数据并进行数据并进行类型转换
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="tm"></param>
    /// <param name="mode"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public T ExecSqlSingle<T>(string sql, TransactionManager tm, DataMappingMode mode, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(sql, CommandType.Text, tm, parameters)).MapToSingle<T>(mode);
    public async Task<T> ExecSqlSingleAsync<T>(string sql, TransactionManager tm, DataMappingMode mode, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(sql, CommandType.Text, tm, parameters));
        return reader.MapToSingle<T>(mode);
    }

    /// <summary>
    /// 执行存储过程获取单行数据并进行数据并进行类型转换
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(proc, CommandType.StoredProcedure, null, parameters)).MapToSingle<T>();
    public async Task<T> ExecProcSingleAsync<T>(string proc, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(proc, CommandType.StoredProcedure, null, parameters));
        return reader.MapToSingle<T>();
    }

    /// <summary>
    /// 执行存储过程获取单行数据并进行数据并进行类型转换
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(proc, CommandType.StoredProcedure, tm, parameters)).MapToSingle<T>();
    public async Task<T> ExecProcSingleAsync<T>(string proc, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(proc, CommandType.StoredProcedure, tm, parameters));
        return reader.MapToSingle<T>();
    }

    /// <summary>
    /// 执行存储过程获取单行数据并进行数据并进行类型转换
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc"></param>
    /// <param name="tm"></param>
    /// <param name="rowMapper"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, TransactionManager tm, Func<IDataReader, T> rowMapper, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(proc, CommandType.StoredProcedure, tm, parameters)).MapToSingle<T>(rowMapper);
    public async Task<T> ExecProcSingleAsync<T>(string proc, TransactionManager tm, Func<IDataReader, T> rowMapper, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(proc, CommandType.StoredProcedure, tm, parameters));
        return reader.MapToSingle<T>(rowMapper);
    }

    /// <summary>
    /// 执行存储过程获取单行数据并进行数据并进行类型转换
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc"></param>
    /// <param name="tm"></param>
    /// <param name="mode"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, TransactionManager tm, DataMappingMode mode, params (TDbType dbType, object value)[] parameters)
        => ExecReader(GetExecCommand(proc, CommandType.StoredProcedure, tm, parameters)).MapToSingle<T>(mode);
    public async Task<T> ExecProcSingleAsync<T>(string proc, TransactionManager tm, DataMappingMode mode, params (TDbType dbType, object value)[] parameters)
    {
        var reader = await ExecReaderAsync(GetExecCommand(proc, CommandType.StoredProcedure, tm, parameters));
        return reader.MapToSingle<T>(mode);
    }
    #endregion

    #endregion // Execute
}