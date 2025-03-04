using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace AiUo.Data;

public abstract partial class Database
{
    // ExecReader在使用事务时必须Close，才可以继续下面的事务
    #region ExecSqlReader
    /// <summary>
    /// 执行SQL语句并返回结果集
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <returns></returns>
    public DataReaderWrapper ExecSqlReader(string sql)
        => ExecReader(CreateCommand(sql, CommandType.Text, null));
    public async Task<DataReaderWrapper> ExecSqlReaderAsync(string sql)
        => await ExecReaderAsync(CreateCommand(sql, CommandType.Text, null));

    /// <summary>
    /// 执行SQL语句并返回结果集
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public DataReaderWrapper ExecSqlReader(string sql, IEnumerable<DbParameter> paras, TransactionManager tm)
        => ExecReader(CreateCommand(sql, CommandType.Text, paras, tm));
    public async Task<DataReaderWrapper> ExecSqlReaderAsync(string sql, IEnumerable<DbParameter> paras, TransactionManager tm)
        => await ExecReaderAsync(CreateCommand(sql, CommandType.Text, paras, tm));

    /// <summary>
    /// 执行SQL语句并返回结果集
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public DataReaderWrapper ExecSqlReader(string sql, TransactionManager tm)
        => ExecReader(CreateCommand(sql, CommandType.Text, null, tm));
    public async Task<DataReaderWrapper> ExecSqlReaderAsync(string sql, TransactionManager tm)
        => await ExecReaderAsync(CreateCommand(sql, CommandType.Text, null, tm));

    /// <summary>
    /// 执行SQL语句并返回结果集
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="paras">参数集合</param>
    /// <returns></returns>
    public DataReaderWrapper ExecSqlReader(string sql, IEnumerable<DbParameter> paras)
        => ExecReader(CreateCommand(sql, CommandType.Text, paras, null));
    public async Task<DataReaderWrapper> ExecSqlReaderAsync(string sql, IEnumerable<DbParameter> paras)
        => await ExecReaderAsync(CreateCommand(sql, CommandType.Text, paras, null));

    /// <summary>
    /// 执行SQL语句并返回结果集
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public DataReaderWrapper ExecSqlReader(string sql, TransactionManager tm, params object[] values)
        => ExecReader(CreateCommand(sql, CommandType.Text, tm, values));
    public async Task<DataReaderWrapper> ExecSqlReaderAsync(string sql, TransactionManager tm, params object[] values)
        => await ExecReaderAsync(CreateCommand(sql, CommandType.Text, tm, values));

    /// <summary>
    /// 执行SQL语句并返回结果集
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public DataReaderWrapper ExecSqlReader(string sql, params object[] values)
        => ExecSqlReader(sql, null, values);
    public async Task<DataReaderWrapper> ExecSqlReaderAsync(string sql, params object[] values)
        => await ExecSqlReaderAsync(sql, null, values);
    #endregion

    #region ExecSqlReaderFormat
    /// <summary>
    /// 执行SQL语句并返回结果集（SQL语句通过string.Format格式化项）
    /// </summary>
    /// <param name="sql">SQL语句，如：select * from {0} where id={1}</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <param name="values">包含零个或多个替换SQL语句中的格式项的对象</param>
    /// <returns></returns>
    public DataReaderWrapper ExecSqlReaderFormat(string sql, TransactionManager tm, params object[] values)
    {
        CheckSqlInjection(values);
        return ExecSqlReader(string.Format(sql, values), tm);
    }
    public async Task<DataReaderWrapper> ExecSqlReaderFormatAsync(string sql, TransactionManager tm, params object[] values)
    {
        CheckSqlInjection(values);
        return await ExecSqlReaderAsync(string.Format(sql, values), tm);
    }
    /// <summary>
    /// 执行SQL语句并返回结果集（SQL语句通过string.Format格式化项）
    /// </summary>
    /// <param name="sql">SQL语句，如：select * from {0} where id={1}</param>
    /// <param name="values">包含零个或多个替换SQL语句中的格式项的对象</param>
    /// <returns></returns>
    public DataReaderWrapper ExecSqlReaderFormat(string sql, params object[] values)
        => ExecSqlReaderFormat(sql, null, values);
    public async Task<DataReaderWrapper> ExecSqlReaderFormatAsync(string sql, params object[] values)
        => await ExecSqlReaderFormatAsync(sql, null, values);
    #endregion

    #region ExecProcReader
    /// <summary>
    /// 执行存储过程并返回结果集
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <returns></returns>
    public DataReaderWrapper ExecProcReader(string proc)
        => ExecReader(CreateCommand(proc, CommandType.StoredProcedure, null));
    public async Task<DataReaderWrapper> ExecProcReaderAsync(string proc)
        => await ExecReaderAsync(CreateCommand(proc, CommandType.StoredProcedure, null));

    /// <summary>
    /// 执行存储过程并返回结果集
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public DataReaderWrapper ExecProcReader(string proc, IEnumerable<DbParameter> paras, TransactionManager tm)
        => ExecReader(CreateCommand(proc, CommandType.StoredProcedure, paras, tm));
    public async Task<DataReaderWrapper> ExecProcReaderAsync(string proc, IEnumerable<DbParameter> paras, TransactionManager tm)
        => await ExecReaderAsync(CreateCommand(proc, CommandType.StoredProcedure, paras, tm));

    /// <summary>
    /// 执行存储过程并返回结果集
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public DataReaderWrapper ExecProcReader(string proc, TransactionManager tm)
        => ExecReader(CreateCommand(proc, CommandType.StoredProcedure, null, tm));
    public async Task<DataReaderWrapper> ExecProcReaderAsync(string proc, TransactionManager tm)
        => await ExecReaderAsync(CreateCommand(proc, CommandType.StoredProcedure, null, tm));

    /// <summary>
    /// 执行存储过程并返回结果集
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <returns></returns>
    public DataReaderWrapper ExecProcReader(string proc, IEnumerable<DbParameter> paras)
        => ExecReader(CreateCommand(proc, CommandType.StoredProcedure, paras, null));
    public async Task<DataReaderWrapper> ExecProcReaderAsync(string proc, IEnumerable<DbParameter> paras)
        => await ExecReaderAsync(CreateCommand(proc, CommandType.StoredProcedure, paras, null));

    /// <summary>
    /// 执行存储过程并返回结果集
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public DataReaderWrapper ExecProcReader(string proc, TransactionManager tm, params object[] values)
        => ExecReader(CreateCommand(proc, CommandType.StoredProcedure, tm, values));
    public async Task<DataReaderWrapper> ExecProcReaderAsync(string proc, TransactionManager tm, params object[] values)
        => await ExecReaderAsync(CreateCommand(proc, CommandType.StoredProcedure, tm, values));

    /// <summary>
    /// 执行存储过程并返回结果集
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public DataReaderWrapper ExecProcReader(string proc, params object[] values)
        => ExecProcReader(proc, null, values);
    public async Task<DataReaderWrapper> ExecProcReaderAsync(string proc, params object[] values)
        => await ExecProcReaderAsync(proc, null, values);
    #endregion
}