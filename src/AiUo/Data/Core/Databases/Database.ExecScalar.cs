using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace AiUo.Data;

public abstract partial class Database
{
    #region ExecSqlScalar
    /// <summary>
    /// 执行SQL语句并返回首行首列数据。
    /// 如果返回null表示一行数据都没有，返回DBNull表示首行首列的值为空
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <returns></returns>
    public object ExecSqlScalar(string sql)
    {
        CommandWrapper command = CreateCommand(sql, CommandType.Text, null);
        return ExecScalar(command);
    }

    public async Task<object> ExecSqlScalarAsync(string sql)
        => await ExecScalarAsync(CreateCommand(sql, CommandType.Text, null));
    /// <summary>
    /// 执行SQL语句并返回首行首列数据
    /// 如果返回null表示一行数据都没有，返回DBNull表示首行首列的值为空
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public object ExecSqlScalar(string sql, IEnumerable<DbParameter> paras, TransactionManager tm)
    {
        CommandWrapper command = CreateCommand(sql, CommandType.Text, paras, tm);
        return ExecScalar(command);
    }
    public async Task<object> ExecSqlScalarAsync(string sql, IEnumerable<DbParameter> paras, TransactionManager tm)
        => await ExecScalarAsync(CreateCommand(sql, CommandType.Text, paras, tm));

    /// <summary>
    /// 执行SQL语句并返回首行首列数据
    /// 如果返回null表示一行数据都没有，返回DBNull表示首行首列的值为空
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public object ExecSqlScalar(string sql, TransactionManager tm)
    {
        CommandWrapper command = CreateCommand(sql, CommandType.Text, null, tm);
        return ExecScalar(command);
    }
    public async Task<object> ExecSqlScalarAsync(string sql, TransactionManager tm)
        => await ExecScalarAsync(CreateCommand(sql, CommandType.Text, null, tm));

    /// <summary>
    ///  执行SQL语句并返回首行首列数据
    ///  如果返回null表示一行数据都没有，返回DBNull表示首行首列的值为空
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="paras">参数集合</param>
    /// <returns></returns>
    public object ExecSqlScalar(string sql, IEnumerable<DbParameter> paras)
    {
        CommandWrapper command = CreateCommand(sql, CommandType.Text, paras, null);
        return ExecScalar(command);
    }
    public async Task<object> ExecSqlScalarAsync(string sql, IEnumerable<DbParameter> paras)
        => await ExecScalarAsync(CreateCommand(sql, CommandType.Text, paras, null));

    /// <summary>
    /// 执行SQL语句并返回首行首列数据
    /// 如果返回null表示一行数据都没有，返回DBNull表示首行首列的值为空
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public object ExecSqlScalar(string sql, TransactionManager tm, params object[] values)
    {
        CommandWrapper command = CreateCommand(sql, CommandType.Text, tm, values);
        return ExecScalar(command);
    }
    public async Task<object> ExecSqlScalarAsync(string sql, TransactionManager tm, params object[] values)
        => await ExecScalarAsync(CreateCommand(sql, CommandType.Text, tm, values));

    /// <summary>
    /// 执行SQL语句并返回首行首列数据
    /// 如果返回null表示一行数据都没有，返回DBNull表示首行首列的值为空
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public object ExecSqlScalar(string sql, params object[] values)
    {
        return ExecSqlScalar(sql, null, values);
    }
    public async Task<object> ExecSqlScalarAsync(string sql, params object[] values)
        => await ExecSqlScalarAsync(sql, null, values);

    /// <summary>
    /// 执行SQL语句并返回首行首列数据
    /// DBNull将转换成null返回
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="sql">SQL语句</param>
    /// <returns></returns>
    public T ExecSqlScalar<T>(string sql)
    {
        object value = ExecSqlScalar(sql);
        return AiUoUtil.ConvertTo<T>(value);
    }
    public async Task<T> ExecSqlScalarAsync<T>(string sql)
    {
        object value = await ExecSqlScalarAsync(sql);
        return AiUoUtil.ConvertTo<T>(value);
    }

    /// <summary>
    /// 执行SQL语句并返回首行首列数据
    /// DBNull将转换成null返回
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="sql">SQL语句</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public T ExecSqlScalar<T>(string sql, TransactionManager tm)
    {
        object value = ExecSqlScalar(sql, tm);
        return AiUoUtil.ConvertTo<T>(value);
    }
    public async Task<T> ExecSqlScalarAsync<T>(string sql, TransactionManager tm)
    {
        object value = await ExecSqlScalarAsync(sql, tm);
        return AiUoUtil.ConvertTo<T>(value);
    }

    /// <summary>
    /// 执行SQL语句并返回首行首列数据
    /// DBNull将转换成null返回
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="sql">SQL语句</param>
    /// <param name="paras">参数集合</param>
    /// <returns></returns>
    public T ExecSqlScalar<T>(string sql, IEnumerable<DbParameter> paras)
    {
        object value = ExecSqlScalar(sql, paras);
        return AiUoUtil.ConvertTo<T>(value);
    }
    public async Task<T> ExecSqlScalarAsync<T>(string sql, IEnumerable<DbParameter> paras)
    {
        object value = await ExecSqlScalarAsync(sql, paras);
        return AiUoUtil.ConvertTo<T>(value);
    }

    /// <summary>
    /// 执行SQL语句并返回首行首列数据
    /// DBNull将转换成null返回
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="sql">SQL语句</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public T ExecSqlScalar<T>(string sql, IEnumerable<DbParameter> paras, TransactionManager tm)
    {
        object value = ExecSqlScalar(sql, paras, tm);
        return AiUoUtil.ConvertTo<T>(value);
    }
    public async Task<T> ExecSqlScalarAsync<T>(string sql, IEnumerable<DbParameter> paras, TransactionManager tm)
    {
        object value = await ExecSqlScalarAsync(sql, paras, tm);
        return AiUoUtil.ConvertTo<T>(value);
    }

    /// <summary>
    /// 执行SQL语句并返回首行首列数据
    /// DBNull将转换成null返回
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="sql">SQL语句</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public T ExecSqlScalar<T>(string sql, params object[] values)
    {
        object value = ExecSqlScalar(sql, values);
        return AiUoUtil.ConvertTo<T>(value);
    }
    public async Task<T> ExecSqlScalarAsync<T>(string sql, params object[] values)
    {
        object value = await ExecSqlScalarAsync(sql, values);
        return AiUoUtil.ConvertTo<T>(value);
    }

    /// <summary>
    /// 执行SQL语句并返回首行首列数据
    /// DBNull将转换成null返回
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="sql">SQL语句</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public T ExecSqlScalar<T>(string sql, TransactionManager tm, params object[] values)
    {
        object value = ExecSqlScalar(sql, tm, values);
        return AiUoUtil.ConvertTo<T>(value);
    }
    public async Task<T> ExecSqlScalarAsync<T>(string sql, TransactionManager tm, params object[] values)
    {
        object value = await ExecSqlScalarAsync(sql, tm, values);
        return AiUoUtil.ConvertTo<T>(value);
    }
    #endregion

    #region ExecSqlScalarFormat
    /// <summary>
    /// 执行SQL语句并返回首行首列数据（SQL语句通过string.Format格式化项）
    /// 如果返回null表示一行数据都没有，返回DBNull表示首行首列的值为空
    /// </summary>
    /// <param name="sql">SQL语句，如：select name from {0} where id={1}</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <param name="values">包含零个或多个替换SQL语句中的格式项的对象</param>
    /// <returns></returns>
    public object ExecSqlScalarFormat(string sql, TransactionManager tm, params object[] values)
    {
        CheckSqlInjection(values);
        return ExecSqlScalar(string.Format(sql, values), tm);
    }
    public async Task<object> ExecSqlScalarFormatAsync(string sql, TransactionManager tm, params object[] values)
    {
        CheckSqlInjection(values);
        return await ExecSqlScalarAsync(string.Format(sql, values), tm);
    }

    /// <summary>
    /// 执行SQL语句并返回首行首列数据（SQL语句通过string.Format格式化项）
    /// 如果返回null表示一行数据都没有，返回DBNull表示首行首列的值为空
    /// </summary>
    /// <param name="sql">SQL语句，如：select name from {0} where id={1}</param>
    /// <param name="values">包含零个或多个替换SQL语句中的格式项的对象</param>
    /// <returns></returns>
    public object ExecSqlScalarFormat(string sql, params object[] values)
        => ExecSqlScalarFormat(sql, null, values);
    public async Task<object> ExecSqlScalarFormatAsync(string sql, params object[] values)
        => await ExecSqlScalarFormatAsync(sql, null, values);

    /// <summary>
    /// 执行SQL语句并返回首行首列数据（SQL语句通过string.Format格式化项）
    /// 如果返回null表示一行数据都没有，返回DBNull表示首行首列的值为空
    /// </summary>
    /// <param name="sql">SQL语句，如：select name from {0} where id={1}</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <param name="values">包含零个或多个替换SQL语句中的格式项的对象</param>
    /// <returns></returns>
    public T ExecSqlScalarFormat<T>(string sql, TransactionManager tm, params object[] values)
    {
        CheckSqlInjection(values);
        return ExecSqlScalar<T>(string.Format(sql, values), tm);
    }
    public async Task<T> ExecSqlScalarFormatAsync<T>(string sql, TransactionManager tm, params object[] values)
    {
        CheckSqlInjection(values);
        return await ExecSqlScalarAsync<T>(string.Format(sql, values), tm);
    }
    /// <summary>
    /// 执行SQL语句并返回首行首列数据（SQL语句通过string.Format格式化项）
    /// 如果返回null表示一行数据都没有，返回DBNull表示首行首列的值为空
    /// </summary>
    /// <param name="sql">SQL语句，如：select name from {0} where id={1}</param>
    /// <param name="values">包含零个或多个替换SQL语句中的格式项的对象</param>
    /// <returns></returns>
    public T ExecSqlScalarFormat<T>(string sql, params object[] values)
        => ExecSqlScalarFormat<T>(sql, null, values);
    public async Task<T> ExecSqlScalarFormatAsync<T>(string sql, params object[] values)
        => await ExecSqlScalarFormatAsync<T>(sql, null, values);
    #endregion

    #region ExecProcScalar
    /// <summary>
    /// 执行存储过程并返回首行首列数据
    /// </summary>
    /// <param name="proc">存储过程名称</param>
    /// <returns></returns>
    public object ExecProcScalar(string proc)
    {
        CommandWrapper command = CreateCommand(proc, CommandType.StoredProcedure, null);
        return ExecScalar(command);
    }
    public async Task<object> ExecProcScalarAsync(string proc)
        => await ExecScalarAsync(CreateCommand(proc, CommandType.StoredProcedure, null));

    /// <summary>
    /// 执行存储过程并返回首行首列数据
    /// </summary>
    /// <param name="proc">存储过程名称</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public object ExecProcScalar(string proc, TransactionManager tm)
    {
        CommandWrapper command = CreateCommand(proc, CommandType.StoredProcedure, null, tm);
        return ExecScalar(command);
    }
    public async Task<object> ExecProcScalarAsync(string proc, TransactionManager tm)
        => await ExecScalarAsync(CreateCommand(proc, CommandType.StoredProcedure, null, tm));

    /// <summary>
    /// 执行存储过程并返回首行首列数据
    /// </summary>
    /// <param name="proc">存储过程名称</param>
    /// <param name="paras">参数集合</param>
    /// <returns></returns>
    public object ExecProcScalar(string proc, IEnumerable<DbParameter> paras)
    {
        CommandWrapper command = CreateCommand(proc, CommandType.StoredProcedure, paras, null);
        return ExecScalar(command);
    }
    public async Task<object> ExecProcScalarAsync(string proc, IEnumerable<DbParameter> paras)
        => await ExecScalarAsync(CreateCommand(proc, CommandType.StoredProcedure, paras, null));

    /// <summary>
    /// 执行存储过程并返回首行首列数据
    /// </summary>
    /// <param name="proc">存储过程名称</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public object ExecProcScalar(string proc, IEnumerable<DbParameter> paras, TransactionManager tm)
    {
        CommandWrapper command = CreateCommand(proc, CommandType.StoredProcedure, paras, tm);
        return ExecScalar(command);
    }
    public async Task<object> ExecProcScalarAsync(string proc, IEnumerable<DbParameter> paras, TransactionManager tm)
        => await ExecScalarAsync(CreateCommand(proc, CommandType.StoredProcedure, paras, tm));

    /// <summary>
    /// 执行存储过程并返回首行首列数据
    /// </summary>
    /// <param name="proc">存储过程名称</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public object ExecProcScalar(string proc, params object[] values)
    {
        return ExecProcScalar(proc, null, values);
    }
    public async Task<object> ExecProcScalarAsync(string proc, params object[] values)
        => await ExecProcScalarAsync(proc, null, values);

    /// <summary>
    /// 执行存储过程并返回首行首列数据
    /// </summary>
    /// <param name="proc">存储过程名称</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public object ExecProcScalar(string proc, TransactionManager tm, params object[] values)
    {
        CommandWrapper command = CreateCommand(proc, CommandType.StoredProcedure, tm, values);
        return ExecScalar(command);
    }
    public async Task<object> ExecProcScalarAsync(string proc, TransactionManager tm, params object[] values)
        => await ExecScalarAsync(CreateCommand(proc, CommandType.StoredProcedure, tm, values));

    /// <summary>
    /// 执行存储过程并返回首行首列数据
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="proc">存储过程名称</param>
    /// <returns></returns>
    public T ExecProcScalar<T>(string proc)
    {
        object value = ExecProcScalar(proc);
        return AiUoUtil.ConvertTo<T>(value);
    }
    public async Task<T> ExecProcScalarAsync<T>(string proc)
    {
        object value = await ExecProcScalarAsync(proc);
        return AiUoUtil.ConvertTo<T>(value);
    }

    /// <summary>
    /// 执行存储过程并返回首行首列数据
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="proc">存储过程名称</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public T ExecProcScalar<T>(string proc, TransactionManager tm)
    {
        object value = ExecProcScalar(proc, tm);
        return AiUoUtil.ConvertTo<T>(value);
    }
    public async Task<T> ExecProcScalarAsync<T>(string proc, TransactionManager tm)
    {
        object value = await ExecProcScalarAsync(proc,tm);
        return AiUoUtil.ConvertTo<T>(value);
    }

    /// <summary>
    /// 执行存储过程并返回首行首列数据
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="proc">存储过程名称</param>
    /// <param name="paras">参数集合</param>
    /// <returns></returns>
    public T ExecProcScalar<T>(string proc, IEnumerable<DbParameter> paras)
    {
        object value = ExecProcScalar(proc, paras);
        return AiUoUtil.ConvertTo<T>(value);
    }
    public async Task<T> ExecProcScalarAsync<T>(string proc, IEnumerable<DbParameter> paras)
    {
        object value = await ExecProcScalarAsync(proc, paras);
        return AiUoUtil.ConvertTo<T>(value);
    }

    /// <summary>
    /// 执行存储过程并返回首行首列数据
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="proc">存储过程名称</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public T ExecProcScalar<T>(string proc, IEnumerable<DbParameter> paras, TransactionManager tm)
    {
        object value = ExecProcScalar(proc, paras, tm);
        return AiUoUtil.ConvertTo<T>(value);
    }
    public async Task<T> ExecProcScalarAsync<T>(string proc, IEnumerable<DbParameter> paras, TransactionManager tm)
    {
        object value = await ExecProcScalarAsync(proc, paras, tm);
        return AiUoUtil.ConvertTo<T>(value);
    }

    /// <summary>
    /// 执行存储过程并返回首行首列数据
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="proc">存储过程名称</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public T ExecProcScalar<T>(string proc, params object[] values)
    {
        object value = ExecProcScalar(proc, values);
        return AiUoUtil.ConvertTo<T>(value);
    }
    public async Task<T> ExecProcScalarAsync<T>(string proc, params object[] values)
    {
        object value = await ExecProcScalarAsync(proc, values);
        return AiUoUtil.ConvertTo<T>(value);
    }

    /// <summary>
    /// 执行存储过程并返回首行首列数据
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="proc">存储过程名称</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <param name="values">按顺序传入需要的参数值集合，程序会自动解析并添加参数集合，并把传入的参数值赋给对应的参数对象</param>
    /// <returns></returns>
    public T ExecProcScalar<T>(string proc, TransactionManager tm, params object[] values)
    {
        object value = ExecProcScalar(proc, tm, values);
        return AiUoUtil.ConvertTo<T>(value);
    }
    public async Task<T> ExecProcScalarAsync<T>(string proc, TransactionManager tm, params object[] values)
    {
        object value = await ExecProcScalarAsync(proc, tm, values);
        return AiUoUtil.ConvertTo<T>(value);
    }
    #endregion
}