using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace AiUo.Data;

public abstract partial class Database
{
    #region ExecSqlTable
    /// <summary>
    /// 执行SQL语句并返回DataTable
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <returns></returns>
    public DataTable ExecSqlTable(string sql)
        => ExecSqlReader(sql).ToTable();
    public async Task<DataTable> ExecSqlTableAsync(string sql)
    {
        var reader = await ExecSqlReaderAsync(sql);
        return reader.ToTable();
    }

    /// <summary>
    /// 执行SQL语句并返回DataTable
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public DataTable ExecSqlTable(string sql, IEnumerable<DbParameter> paras, TransactionManager tm)
        => ExecSqlReader(sql, paras, tm).ToTable();
    public async Task<DataTable> ExecSqlTableAsync(string sql, IEnumerable<DbParameter> paras, TransactionManager tm)
    {
        var reader = await ExecSqlReaderAsync(sql, paras, tm);
        return reader.ToTable();
    }

    /// <summary>
    /// 执行SQL语句并返回DataTable
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public DataTable ExecSqlTable(string sql, TransactionManager tm)
        => ExecSqlReader(sql, tm).ToTable();
    public async Task<DataTable> ExecSqlTableAsync(string sql, TransactionManager tm)
    {
        var reader = await ExecSqlReaderAsync(sql, tm);
        return reader.ToTable();
    }

    /// <summary>
    /// 执行SQL语句并返回DataTable
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="paras">参数集合</param>
    /// <returns></returns>
    public DataTable ExecSqlTable(string sql, IEnumerable<DbParameter> paras)
        => ExecSqlReader(sql, paras).ToTable();
    public async Task<DataTable> ExecSqlTableAsync(string sql, IEnumerable<DbParameter> paras)
    {
        var reader = await ExecSqlReaderAsync(sql, paras);
        return reader.ToTable();
    }

    /// <summary>
    /// 执行SQL语句并返回DataTable
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public DataTable ExecSqlTable(string sql, TransactionManager tm, params object[] values)
        => ExecSqlReader(sql, tm, values).ToTable();
    public async Task<DataTable> ExecSqlTableAsync(string sql, TransactionManager tm, params object[] values)
    {
        var reader = await ExecSqlReaderAsync(sql, tm, values);
        return reader.ToTable();
    }

    /// <summary>
    /// 执行SQL语句并返回DataTable
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public DataTable ExecSqlTable(string sql, params object[] values)
        => ExecSqlReader(sql, values).ToTable();
    public async Task<DataTable> ExecSqlTableAsync(string sql, params object[] values)
    {
        var reader = await ExecSqlReaderAsync(sql, values);
        return reader.ToTable();
    }
    #endregion

    #region ExecSqlTableFormat
    /// <summary>
    /// 执行SQL语句并返回DataTable（SQL语句通过string.Format格式化项）
    /// </summary>
    /// <param name="sql">SQL语句，如：select * from {0} where id={1}</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <param name="values">包含零个或多个替换SQL语句中的格式项的对象</param>
    /// <returns></returns>
    public DataTable ExecSqlTableFormat(string sql, TransactionManager tm, params object[] values)
    {
        CheckSqlInjection(values);
        return ExecSqlTable(string.Format(sql, values), tm);
    }
    public async Task<DataTable> ExecSqlTableFormatAsync(string sql, TransactionManager tm, params object[] values)
    {
        CheckSqlInjection(values);
        return await ExecSqlTableAsync(string.Format(sql, values), tm);
    }

    /// <summary>
    /// 执行SQL语句并返回DataTable（SQL语句通过string.Format格式化项）
    /// </summary>
    /// <param name="sql">SQL语句，如：select * from {0} where id={1}</param>
    /// <param name="values">包含零个或多个替换SQL语句中的格式项的对象</param>
    /// <returns></returns>
    public DataTable ExecSqlTableFormat(string sql, params object[] values)
        => ExecSqlTableFormat(sql, null, values);
    public async Task<DataTable> ExecSqlTableFormatAsync(string sql, params object[] values)
        => await ExecSqlTableFormatAsync(sql, null, values);
    #endregion

    #region ExecProcTable
    /// <summary>
    /// 执行存储过程并返回DataTable
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <returns></returns>
    public DataTable ExecProcTable(string proc)
        => ExecProcReader(proc).ToTable();
    public async Task<DataTable> ExecProcTableAsync(string proc)
    {
        var reader = await ExecProcReaderAsync(proc);
        return reader.ToTable();
    }

    /// <summary>
    /// 执行存储过程并返回DataTable
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public DataTable ExecProcTable(string proc, IEnumerable<DbParameter> paras, TransactionManager tm)
        => ExecProcReader(proc, paras, tm).ToTable();
    public async Task<DataTable> ExecProcTableAsync(string proc, IEnumerable<DbParameter> paras, TransactionManager tm)
    {
        var reader = await ExecProcReaderAsync(proc, paras, tm);
        return reader.ToTable();
    }

    /// <summary>
    /// 执行存储过程并返回DataTable
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public DataTable ExecProcTable(string proc, TransactionManager tm)
        => ExecProcReader(proc, tm).ToTable();
    public async Task<DataTable> ExecProcTableAsync(string proc, TransactionManager tm)
    {
        var reader = await ExecProcReaderAsync(proc, tm);
        return reader.ToTable();
    }

    /// <summary>
    /// 执行存储过程并返回DataTable
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <returns></returns>
    public DataTable ExecProcTable(string proc, IEnumerable<DbParameter> paras)
        => ExecProcReader(proc, paras).ToTable();
    public async Task<DataTable> ExecProcTableAsync(string proc, IEnumerable<DbParameter> paras)
    {
        var reader = await ExecProcReaderAsync(proc, paras);
        return reader.ToTable();
    }

    /// <summary>
    /// 执行存储过程并返回DataTable
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public DataTable ExecProcTable(string proc, TransactionManager tm, params object[] values)
        => ExecProcReader(proc, tm, values).ToTable();
    public async Task<DataTable> ExecProcTableAsync(string proc, TransactionManager tm, params object[] values)
    {
        var reader = await ExecProcReaderAsync(proc, tm, values);
        return reader.ToTable();
    }

    /// <summary>
    /// 执行存储过程并返回DataTable
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public DataTable ExecProcTable(string proc, params object[] values)
        => ExecProcReader(proc, values).ToTable();
    public async Task<DataTable> ExecProcTableAsync(string proc, params object[] values)
    {
        var reader = await ExecProcReaderAsync(proc, values);
        return reader.ToTable();
    }
    #endregion
}