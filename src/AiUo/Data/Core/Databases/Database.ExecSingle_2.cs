using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace AiUo.Data;

public abstract partial class Database
{
    #region ExecSqlSingle
    /// <summary>
    /// 执行SQL语句并返回DataRow
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <returns></returns>
    public DataRow ExecSqlSingle(string sql)
    {
        var dt = ExecSqlTable(sql);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }
    public async Task<DataRow> ExecSqlSingleAsync(string sql)
    {
        var dt = await ExecSqlTableAsync(sql);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }

    /// <summary>
    /// 执行SQL语句并返回DataRow
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public DataRow ExecSqlSingle(string sql, IEnumerable<DbParameter> paras, TransactionManager tm)
    {
        var dt = ExecSqlTable(sql, paras, tm);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }
    public async Task<DataRow> ExecSqlSingleAsync(string sql, IEnumerable<DbParameter> paras, TransactionManager tm)
    {
        var dt = await ExecSqlTableAsync(sql, paras, tm);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }

    /// <summary>
    /// 执行SQL语句并返回DataRow
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public DataRow ExecSqlSingle(string sql, TransactionManager tm)
    {
        var dt = ExecSqlTable(sql, tm);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }
    public async Task<DataRow> ExecSqlSingleAsync(string sql, TransactionManager tm)
    {
        var dt = await ExecSqlTableAsync(sql, tm);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }

    /// <summary>
    /// 执行SQL语句并返回DataRow
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="paras">参数集合</param>
    /// <returns></returns>
    public DataRow ExecSqlSingle(string sql, IEnumerable<DbParameter> paras)
    {
        var dt = ExecSqlTable(sql, paras);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }
    public async Task<DataRow> ExecSqlSingleAsync(string sql, IEnumerable<DbParameter> paras)
    {
        var dt = await ExecSqlTableAsync(sql, paras);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }

    /// <summary>
    /// 执行SQL语句并返回DataRow
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public DataRow ExecSqlSingle(string sql, TransactionManager tm, params object[] values)
    {
        var dt = ExecSqlTable(sql, tm, values);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }
    public async Task<DataRow> ExecSqlSingleAsync(string sql, TransactionManager tm, params object[] values)
    {
        var dt = await ExecSqlTableAsync(sql, tm, values);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }

    /// <summary>
    /// 执行SQL语句并返回DataRow
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public DataRow ExecSqlSingle(string sql, params object[] values)
    {
        var dt = ExecSqlTable(sql, values);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }
    public async Task<DataRow> ExecSqlSingleAsync(string sql, params object[] values)
    {
        var dt = await ExecSqlTableAsync(sql, values);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }
    #endregion

    #region ExecProcSingle
    /// <summary>
    /// 执行存储过程并返回DataRow
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <returns></returns>
    public DataRow ExecProcSingle(string proc)
    {
        var dt = ExecProcTable(proc);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }
    public async Task<DataRow> ExecProcSingleAsync(string proc)
    {
        var dt = await ExecProcTableAsync(proc);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }

    /// <summary>
    /// 执行存储过程并返回DataRow
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public DataRow ExecProcSingle(string proc, IEnumerable<DbParameter> paras, TransactionManager tm)
    {
        var dt = ExecProcTable(proc, paras, tm);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }
    public async Task<DataRow> ExecProcSingleAsync(string proc, IEnumerable<DbParameter> paras, TransactionManager tm)
    {
        var dt = await ExecProcTableAsync(proc, paras, tm);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }

    /// <summary>
    /// 执行存储过程并返回DataRow
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public DataRow ExecProcSingle(string proc, TransactionManager tm)
    {
        var dt = ExecProcTable(proc, tm);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }
    public async Task<DataRow> ExecProcSingleAsync(string proc, TransactionManager tm)
    {
        var dt = await ExecProcTableAsync(proc, tm);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }

    /// <summary>
    /// 执行存储过程并返回DataRow
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <returns></returns>
    public DataRow ExecProcSingle(string proc, IEnumerable<DbParameter> paras)
    {
        var dt = ExecProcTable(proc, paras);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }
    public async Task<DataRow> ExecProcSingleAsync(string proc, IEnumerable<DbParameter> paras)
    {
        var dt = await ExecProcTableAsync(proc, paras);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }

    /// <summary>
    /// 执行存储过程并返回DataRow
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public DataRow ExecProcSingle(string proc, TransactionManager tm, params object[] values)
    {
        var dt = ExecProcTable(proc, tm, values);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }
    public async Task<DataRow> ExecProcSingleAsync(string proc, TransactionManager tm, params object[] values)
    {
        var dt = await ExecProcTableAsync(proc, tm, values);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }

    /// <summary>
    /// 执行存储过程并返回DataRow
    /// </summary>
    /// <param name="proc">存储过程</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public DataRow ExecProcSingle(string proc, params object[] values)
    {
        var dt = ExecProcTable(proc, values);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }
    public async Task<DataRow> ExecProcSingleAsync(string proc, params object[] values)
    {
        var dt = await ExecProcTableAsync(proc, values);
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }
    #endregion
}