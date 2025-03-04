using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace AiUo.Data;

public abstract partial class Database
{
    #region ExecSqlList
    /// <summary>
    /// 执行SQL语句返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql)
        => ExecSqlReader(sql).MapToList<T>();

    public async Task<List<T>> ExecSqlListAsync<T>(string sql)
    {
        var reader = await ExecSqlReaderAsync(sql);
        return reader.MapToList<T>();
    }

    /// <summary>
    /// 执行SQL语句返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="rowMapper">映射方法</param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, Func<IDataReader, T> rowMapper)
        => ExecSqlReader(sql).MapToList<T>(rowMapper);
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecSqlReaderAsync(sql);
        return reader.MapToList<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL语句返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="mode">映射模式</param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, DataMappingMode mode)
        => ExecSqlReader(sql).MapToList<T>(mode);
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, DataMappingMode mode)
    {
        var reader = await ExecSqlReaderAsync(sql);
        return reader.MapToList<T>(mode);
    }

    /// <summary>
    /// 执行SQL语句返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">事务对象</param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, IEnumerable<DbParameter> paras, TransactionManager tm)
        => ExecSqlReader(sql, paras, tm).MapToList<T>();
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, IEnumerable<DbParameter> paras, TransactionManager tm)
    {
        var reader = await ExecSqlReaderAsync(sql, paras, tm);
        return reader.MapToList<T>();
    }

    /// <summary>
    /// 执行SQL语句返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">事务对象</param>
    /// <param name="rowMapper">映射方法</param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, IEnumerable<DbParameter> paras, TransactionManager tm, Func<IDataReader, T> rowMapper)
        => ExecSqlReader(sql, paras, tm).MapToList<T>(rowMapper);
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, IEnumerable<DbParameter> paras, TransactionManager tm, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecSqlReaderAsync(sql, paras, tm);
        return reader.MapToList<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL语句返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">事务对象</param>
    /// <param name="mode">映射模式</param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, IEnumerable<DbParameter> paras, TransactionManager tm, DataMappingMode mode)
        => ExecSqlReader(sql, paras, tm).MapToList<T>(mode);
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, IEnumerable<DbParameter> paras, TransactionManager tm, DataMappingMode mode)
    {
        var reader = await ExecSqlReaderAsync(sql, paras, tm);
        return reader.MapToList<T>(mode);
    }

    /// <summary>
    /// 执行SQL语句返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="tm">事务对象</param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, TransactionManager tm)
        => ExecSqlReader(sql, tm).MapToList<T>();
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, TransactionManager tm)
    {
        var reader = await ExecSqlReaderAsync(sql, tm);
        return reader.MapToList<T>();
    }

    /// <summary>
    /// 执行SQL语句返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="tm">事务对象</param>
    /// <param name="rowMapper">映射方法</param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, TransactionManager tm, Func<IDataReader, T> rowMapper)
        => ExecSqlReader(sql, tm).MapToList<T>(rowMapper);
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, TransactionManager tm, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecSqlReaderAsync(sql, tm);
        return reader.MapToList<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL语句返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SLQ</param>
    /// <param name="tm">事务对象</param>
    /// <param name="mode">映射模式</param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, TransactionManager tm, DataMappingMode mode)
        => ExecSqlReader(sql, tm).MapToList<T>(mode);
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, TransactionManager tm, DataMappingMode mode)
    {
        var reader = await ExecSqlReaderAsync(sql, tm);
        return reader.MapToList<T>(mode);
    }

    /// <summary>
    /// 执行SQL语句返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="paras">参数集合</param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, IEnumerable<DbParameter> paras)
        => ExecSqlReader(sql, paras).MapToList<T>();
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, IEnumerable<DbParameter> paras)
    {
        var reader = await ExecSqlReaderAsync(sql, paras);
        return reader.MapToList<T>();
    }

    /// <summary>
    /// 执行SQL语句返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="paras">参数集合</param>
    /// <param name="rowMapper">映射方法</param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, IEnumerable<DbParameter> paras, Func<IDataReader, T> rowMapper)
        => ExecSqlReader(sql, paras).MapToList<T>(rowMapper);
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, IEnumerable<DbParameter> paras, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecSqlReaderAsync(sql, paras);
        return reader.MapToList<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL语句返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="paras">参数集合</param>
    /// <param name="mode">映射模式</param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, IEnumerable<DbParameter> paras, DataMappingMode mode)
        => ExecSqlReader(sql, paras).MapToList<T>(mode);
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, IEnumerable<DbParameter> paras, DataMappingMode mode)
    {
        var reader = await ExecSqlReaderAsync(sql, paras);
        return reader.MapToList<T>(mode);
    }

    /// <summary>
    /// 执行SQL语句返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="tm">事务对象</param>
    /// <param name="values">参数值列表</param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, TransactionManager tm, params object[] values)
        => ExecSqlReader(sql, tm, values).MapToList<T>();
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, TransactionManager tm, params object[] values)
    {
        var reader = await ExecSqlReaderAsync(sql, tm, values);
        return reader.MapToList<T>();
    }

    /// <summary>
    /// 执行SQL语句返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="tm">事务对象</param>
    /// <param name="rowMapper">映射方法</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, TransactionManager tm, Func<IDataReader, T> rowMapper, params object[] values)
        => ExecSqlReader(sql, tm, values).MapToList<T>(rowMapper);
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, TransactionManager tm, Func<IDataReader, T> rowMapper, params object[] values)
    {
        var reader = await ExecSqlReaderAsync(sql, tm, values);
        return reader.MapToList<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL语句返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="tm">事务对象</param>
    /// <param name="mode">映射模式</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, TransactionManager tm, DataMappingMode mode, params object[] values)
        => ExecSqlReader(sql, tm, values).MapToList<T>(mode);
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, TransactionManager tm, DataMappingMode mode, params object[] values)
    {
        var reader = await ExecSqlReaderAsync(sql, tm, values);
        return reader.MapToList<T>(mode);
    }

    /// <summary>
    /// 执行SQL语句返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, params object[] values)
        => ExecSqlReader(sql, values).MapToList<T>();
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, params object[] values)
    {
        var reader = await ExecSqlReaderAsync(sql, values);
        return reader.MapToList<T>();
    }

    /// <summary>
    /// 执行SQL语句返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="rowMapper">映射方法</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, Func<IDataReader, T> rowMapper, params object[] values)
        => ExecSqlReader(sql, values).MapToList<T>(rowMapper);
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, Func<IDataReader, T> rowMapper, params object[] values)
    {
        var reader = await ExecSqlReaderAsync(sql, values);
        return reader.MapToList<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL语句返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="mode">映射模式</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public List<T> ExecSqlList<T>(string sql, DataMappingMode mode, params object[] values)
        => ExecSqlReader(sql, values).MapToList<T>(mode);
    public async Task<List<T>> ExecSqlListAsync<T>(string sql, DataMappingMode mode, params object[] values)
    {
        var reader = await ExecSqlReaderAsync(sql, values);
        return reader.MapToList<T>(mode);
    }
    #endregion

    #region ExecProcList
    /// <summary>
    /// 执行存储过程返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string proc)
        => ExecProcReader(proc).MapToList<T>();
    public async Task<List<T>> ExecProcListAsync<T>(string proc)
    {
        var reader = await ExecProcReaderAsync(proc);
        return reader.MapToList<T>();
    }

    /// <summary>
    /// 执行存储过程返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="rowMapper">映射方法</param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string proc, Func<IDataReader, T> rowMapper)
        => ExecProcReader(proc).MapToList<T>(rowMapper);
    public async Task<List<T>> ExecProcListAsync<T>(string proc, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecProcReaderAsync(proc);
        return reader.MapToList<T>(rowMapper);
    }

    /// <summary>
    /// 执行存储过程返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="mode">映射模式</param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string proc, DataMappingMode mode)
        => ExecProcReader(proc).MapToList<T>(mode);
    public async Task<List<T>> ExecProcListAsync<T>(string proc, DataMappingMode mode)
    {
        var reader = await ExecProcReaderAsync(proc);
        return reader.MapToList<T>(mode);
    }

    /// <summary>
    /// 执行存储过程返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">事务对象</param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string proc, IEnumerable<DbParameter> paras, TransactionManager tm)
        => ExecProcReader(proc, paras, tm).MapToList<T>();
    public async Task<List<T>> ExecProcListAsync<T>(string proc, IEnumerable<DbParameter> paras, TransactionManager tm)
    {
        var reader = await ExecProcReaderAsync(proc, paras, tm);
        return reader.MapToList<T>();
    }

    /// <summary>
    /// 执行存储过程返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">事务对象</param>
    /// <param name="rowMapper">映射方法</param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string proc, IEnumerable<DbParameter> paras, TransactionManager tm, Func<IDataReader, T> rowMapper)
        => ExecProcReader(proc, paras, tm).MapToList<T>(rowMapper);
    public async Task<List<T>> ExecProcListAsync<T>(string proc, IEnumerable<DbParameter> paras, TransactionManager tm, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecProcReaderAsync(proc, paras, tm);
        return reader.MapToList<T>(rowMapper);
    }

    /// <summary>
    /// 执行存储过程返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">事务对象</param>
    /// <param name="mode">映射模式</param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string proc, IEnumerable<DbParameter> paras, TransactionManager tm, DataMappingMode mode)
        => ExecProcReader(proc, paras, tm).MapToList<T>(mode);
    public async Task<List<T>> ExecProcListAsync<T>(string proc, IEnumerable<DbParameter> paras, TransactionManager tm, DataMappingMode mode)
    {
        var reader = await ExecProcReaderAsync(proc, paras, tm);
        return reader.MapToList<T>(mode);
    }

    /// <summary>
    /// 执行存储过程返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="tm">事务对象</param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string proc, TransactionManager tm)
        => ExecProcReader(proc, tm).MapToList<T>();
    public async Task<List<T>> ExecProcListAsync<T>(string proc, TransactionManager tm)
    {
        var reader = await ExecProcReaderAsync(proc, tm);
        return reader.MapToList<T>();
    }

    /// <summary>
    /// 执行存储过程返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="tm">事务对象</param>
    /// <param name="rowMapper">映射方法</param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string proc, TransactionManager tm, Func<IDataReader, T> rowMapper)
        => ExecProcReader(proc, tm).MapToList<T>(rowMapper);
    public async Task<List<T>> ExecProcListAsync<T>(string proc, TransactionManager tm, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecProcReaderAsync(proc, tm);
        return reader.MapToList<T>(rowMapper);
    }

    /// <summary>
    /// 执行存储过程返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="tm">事务对象</param>
    /// <param name="mode">映射模式</param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string proc, TransactionManager tm, DataMappingMode mode)
        => ExecProcReader(proc, tm).MapToList<T>(mode);
    public async Task<List<T>> ExecProcListAsync<T>(string proc, TransactionManager tm, DataMappingMode mode)
    {
        var reader = await ExecProcReaderAsync(proc, tm);
        return reader.MapToList<T>(mode);
    }

    /// <summary>
    /// 执行存储过程返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string proc, IEnumerable<DbParameter> paras)
        => ExecProcReader(proc, paras).MapToList<T>();
    public async Task<List<T>> ExecProcListAsync<T>(string proc, IEnumerable<DbParameter> paras)
    {
        var reader = await ExecProcReaderAsync(proc, paras);
        return reader.MapToList<T>();
    }

    /// <summary>
    /// 执行存储过程返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <param name="rowMapper">映射方法</param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string proc, IEnumerable<DbParameter> paras, Func<IDataReader, T> rowMapper)
        => ExecProcReader(proc, paras).MapToList<T>(rowMapper);
    public async Task<List<T>> ExecProcListAsync<T>(string proc, IEnumerable<DbParameter> paras, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecProcReaderAsync(proc, paras);
        return reader.MapToList<T>(rowMapper);
    }

    /// <summary>
    /// 执行存储过程返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <param name="mode">映射模式</param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string proc, IEnumerable<DbParameter> paras, DataMappingMode mode)
        => ExecProcReader(proc, paras).MapToList<T>(mode);
    public async Task<List<T>> ExecProcListAsync<T>(string proc, IEnumerable<DbParameter> paras, DataMappingMode mode)
    {
        var reader = await ExecProcReaderAsync(proc, paras);
        return reader.MapToList<T>(mode);
    }

    /// <summary>
    /// 执行存储过程返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="tm">事务对象</param>
    /// <param name="values">参数值列表</param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string proc, TransactionManager tm, params object[] values)
        => ExecProcReader(proc, tm, values).MapToList<T>();
    public async Task<List<T>> ExecProcListAsync<T>(string proc, TransactionManager tm, params object[] values)
    {
        var reader = await ExecProcReaderAsync(proc, tm, values);
        return reader.MapToList<T>();
    }

    /// <summary>
    /// 执行存储过程返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="tm">事务对象</param>
    /// <param name="rowMapper">映射方法</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string proc, TransactionManager tm, Func<IDataReader, T> rowMapper, params object[] values)
        => ExecProcReader(proc, tm, values).MapToList<T>(rowMapper);
    public async Task<List<T>> ExecProcListAsync<T>(string proc, TransactionManager tm, Func<IDataReader, T> rowMapper, params object[] values)
    {
        var reader = await ExecProcReaderAsync(proc, tm, values);
        return reader.MapToList<T>(rowMapper);
    }

    /// <summary>
    /// 执行存储过程返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="tm">事务对象</param>
    /// <param name="mode">映射模式</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string proc, TransactionManager tm, DataMappingMode mode, params object[] values)
        => ExecProcReader(proc, tm, values).MapToList<T>(mode);
    public async Task<List<T>> ExecProcListAsync<T>(string proc, TransactionManager tm, DataMappingMode mode, params object[] values)
    {
        var reader = await ExecProcReaderAsync(proc, tm, values);
        return reader.MapToList<T>(mode);
    }

    /// <summary>
    /// 执行存储过程返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string proc, params object[] values)
        => ExecProcReader(proc, values).MapToList<T>();
    public async Task<List<T>> ExecProcListAsync<T>(string proc, params object[] values)
    {
        var reader = await ExecProcReaderAsync(proc, values);
        return reader.MapToList<T>();
    }

    /// <summary>
    /// 执行存储过程返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="rowMapper">映射方法</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string proc, Func<IDataReader, T> rowMapper, params object[] values)
        => ExecProcReader(proc, values).MapToList<T>(rowMapper);
    public async Task<List<T>> ExecProcListAsync<T>(string proc, Func<IDataReader, T> rowMapper, params object[] values)
    {
        var reader = await ExecProcReaderAsync(proc, values);
        return reader.MapToList<T>(rowMapper);
    }

    /// <summary>
    /// 执行存储过程返回集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="mode">映射模式</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public List<T> ExecProcList<T>(string proc, DataMappingMode mode, params object[] values)
        => ExecProcReader(proc, values).MapToList<T>(mode);
    public async Task<List<T>> ExecProcListAsync<T>(string proc, DataMappingMode mode, params object[] values)
    {
        var reader = await ExecProcReaderAsync(proc, values);
        return reader.MapToList<T>(mode);
    }
    #endregion
}