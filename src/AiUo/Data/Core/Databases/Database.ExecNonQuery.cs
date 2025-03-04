using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace AiUo.Data;

public abstract partial class Database
{
    #region ExecSqlNonQuery
    /// <summary>
    /// 执行SQL语句并返回受影响的行数
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <returns></returns>
    public int ExecSqlNonQuery(string sql)
    {
        CommandWrapper command = CreateCommand(sql, CommandType.Text, null);
        return ExecNonQuery(command);
    }
    public async Task<int> ExecSqlNonQueryAsync(string sql)
        => await ExecNonQueryAsync(CreateCommand(sql, CommandType.Text, null));

    /// <summary>
    /// 执行SQL语句并返回受影响的行数
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public int ExecSqlNonQuery(string sql, IEnumerable<DbParameter> paras, TransactionManager tm)
    {
        CommandWrapper command = CreateCommand(sql, CommandType.Text, paras, tm);
        return ExecNonQuery(command);
    }
    public async Task<int> ExecSqlNonQueryAsync(string sql, IEnumerable<DbParameter> paras, TransactionManager tm)
        => await ExecNonQueryAsync(CreateCommand(sql, CommandType.Text, paras, tm));

    /// <summary>
    /// 执行SQL语句并返回受影响的行数
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public int ExecSqlNonQuery(string sql, TransactionManager tm)
    {
        CommandWrapper command = CreateCommand(sql, CommandType.Text, null, tm);
        return ExecNonQuery(command);
    }
    public async Task<int> ExecSqlNonQueryAsync(string sql, TransactionManager tm)
        => await ExecNonQueryAsync(CreateCommand(sql, CommandType.Text, null, tm));

    /// <summary>
    /// 执行SQL语句并返回受影响的行数
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="paras">参数集合</param>
    /// <returns></returns>
    public int ExecSqlNonQuery(string sql, IEnumerable<DbParameter> paras)
    {
        CommandWrapper command = CreateCommand(sql, CommandType.Text, paras, null);
        return ExecNonQuery(command);
    }
    public async Task<int> ExecSqlNonQueryAsync(string sql, IEnumerable<DbParameter> paras)
        => await ExecNonQueryAsync(CreateCommand(sql, CommandType.Text, paras, null));

    /// <summary>
    /// 执行SQL语句并返回受影响的行数
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public int ExecSqlNonQuery(string sql, TransactionManager tm, params object[] values)
    {
        CommandWrapper command = CreateCommand(sql, CommandType.Text, tm, values);
        return ExecNonQuery(command);
    }
    public async Task<int> ExecSqlNonQueryAsync(string sql, TransactionManager tm, params object[] values)
        => await ExecNonQueryAsync(CreateCommand(sql, CommandType.Text, tm, values));

    /// <summary>
    /// 执行SQL语句并返回受影响的行数
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public int ExecSqlNonQuery(string sql, params object[] values)
        => ExecSqlNonQuery(sql, null, values);
    public async Task<int> ExecSqlNonQueryAsync(string sql, params object[] values)
        => await ExecSqlNonQueryAsync(sql, null, values);
    #endregion

    #region ExecSqlNonQueryFormat
    /// <summary>
    /// 执行SQL语句并返回受影响的行数（SQL语句通过string.Format格式化项）
    /// </summary>
    /// <param name="sql">SQL语句，如：delete from {0} where id={1}</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <param name="values">包含零个或多个替换SQL语句中的格式项的对象</param>
    /// <returns></returns>
    public int ExecSqlNonQueryFormat(string sql, TransactionManager tm, params object[] values)
    {
        CheckSqlInjection(values);
        return ExecSqlNonQuery(string.Format(sql, values), tm);
    }
    public async Task<int> ExecSqlNonQueryFormatAsync(string sql, TransactionManager tm, params object[] values)
    {
        CheckSqlInjection(values);
        return await ExecSqlNonQueryAsync(string.Format(sql, values), tm);
    }


    /// <summary>
    /// 执行SQL语句并返回受影响的行数（SQL语句通过string.Format格式化项）
    /// </summary>
    /// <param name="sql">SQL语句，如：delete from {0} where id={1}</param>
    /// <param name="values">包含零个或多个替换SQL语句中的格式项的对象</param>
    /// <returns></returns>
    public int ExecSqlNonQueryFormat(string sql, params object[] values)
        => ExecSqlNonQueryFormat(sql, null, values);
    public async Task<int> ExecSqlNonQueryFormatAsync(string sql, params object[] values)
        => await ExecSqlNonQueryFormatAsync(sql, null, values);
    #endregion

    #region ExecProcNonQuery
    /// <summary>
    /// 执行存储过程并返回受影响的行数
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <returns></returns>
    public int ExecProcNonQuery(string proc)
    {
        CommandWrapper command = CreateCommand(proc, CommandType.StoredProcedure, null);
        return ExecNonQuery(command);
    }
    public async Task<int> ExecProcNonQueryAsync(string proc)
        => await ExecNonQueryAsync(CreateCommand(proc, CommandType.StoredProcedure, null));

    /// <summary>
    /// 执行存储过程并返回受影响的行数
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public int ExecProcNonQuery(string proc, IEnumerable<DbParameter> paras, TransactionManager tm)
    {
        CommandWrapper command = CreateCommand(proc, CommandType.StoredProcedure, paras, tm);
        return ExecNonQuery(command);
    }
    public async Task<int> ExecProcNonQueryAsync(string proc, IEnumerable<DbParameter> paras, TransactionManager tm)
        => await ExecNonQueryAsync(CreateCommand(proc, CommandType.StoredProcedure, paras, tm));

    /// <summary>
    /// 执行存储过程并返回受影响的行数
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public int ExecProcNonQuery(string proc, TransactionManager tm)
    {
        CommandWrapper command = CreateCommand(proc, CommandType.StoredProcedure, null, tm);
        return ExecNonQuery(command);
    }
    public async Task<int> ExecProcNonQueryAsync(string proc, TransactionManager tm)
        => await ExecNonQueryAsync(CreateCommand(proc, CommandType.StoredProcedure, null, tm));

    /// <summary>
    /// 执行存储过程并返回受影响的行数
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <returns></returns>
    public int ExecProcNonQuery(string proc, IEnumerable<DbParameter> paras)
    {
        CommandWrapper command = CreateCommand(proc, CommandType.StoredProcedure, paras, null);
        return ExecNonQuery(command);
    }
    public async Task<int> ExecProcNonQueryAsync(string proc, IEnumerable<DbParameter> paras)
        => await ExecNonQueryAsync(CreateCommand(proc, CommandType.StoredProcedure, paras, null));

    /// <summary>
    /// 执行存储过程并返回受影响的行数
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public int ExecProcNonQuery(string proc, TransactionManager tm, params object[] values)
    {
        CommandWrapper command = CreateCommand(proc, CommandType.StoredProcedure, tm, values);
        return ExecNonQuery(command);
    }
    public async Task<int> ExecProcNonQueryAsync(string proc, TransactionManager tm, params object[] values)
        => await ExecNonQueryAsync(CreateCommand(proc, CommandType.StoredProcedure, tm, values));

    /// <summary>
    /// 执行存储过程并返回受影响的行数
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public int ExecProcNonQuery(string proc, params object[] values)
    {
        return ExecProcNonQuery(proc, null, values);
    }
    public async Task<int> ExecProcNonQueryAsync(string proc, params object[] values)
        => await ExecProcNonQueryAsync(proc, null, values);
    #endregion
}