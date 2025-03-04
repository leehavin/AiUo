using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Threading.Tasks;

namespace AiUo.Data;

public abstract partial class Database
{
    #region ExecSqlMulti
    /// <summary>
    /// 执行SQL语句返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql)
        => ExecSqlReader(sql).MapToMulti<T>();
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql)
    {
        var reader = await ExecSqlReaderAsync(sql);
        return reader.MapToMulti<T>();
    }

    /// <summary>
    /// 执行SQL语句返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="rowMapper">映射方法</param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, Func<IDataReader, T> rowMapper)
        => ExecSqlReader(sql).MapToMulti<T>(rowMapper);
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecSqlReaderAsync(sql);
        return reader.MapToMulti<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL语句返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="mode">映射模式</param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, DataMappingMode mode)
        => ExecSqlReader(sql).MapToMulti<T>(mode);
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, DataMappingMode mode)
    {
        var reader = await ExecSqlReaderAsync(sql);
        return reader.MapToMulti<T>(mode);
    }

    /// <summary>
    /// 执行SQL语句返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">事务对象</param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, IEnumerable<DbParameter> paras, TransactionManager tm)
        => ExecSqlReader(sql, paras, tm).MapToMulti<T>();
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, IEnumerable<DbParameter> paras, TransactionManager tm)
    {
        var reader = await ExecSqlReaderAsync(sql, paras, tm);
        return reader.MapToMulti<T>();
    }

    /// <summary>
    /// 执行SQL语句返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">事务对象</param>
    /// <param name="rowMapper">映射方法</param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, IEnumerable<DbParameter> paras, TransactionManager tm, Func<IDataReader, T> rowMapper)
        => ExecSqlReader(sql, paras, tm).MapToMulti<T>(rowMapper);
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, IEnumerable<DbParameter> paras, TransactionManager tm, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecSqlReaderAsync(sql, paras, tm);
        return reader.MapToMulti<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL语句返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">事务对象</param>
    /// <param name="mode">映射模式</param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, IEnumerable<DbParameter> paras, TransactionManager tm, DataMappingMode mode)
        => ExecSqlReader(sql, paras, tm).MapToMulti<T>(mode);
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, IEnumerable<DbParameter> paras, TransactionManager tm, DataMappingMode mode)
    {
        var reader = await ExecSqlReaderAsync(sql, paras, tm);
        return reader.MapToMulti<T>(mode);
    }

    /// <summary>
    /// 执行SQL语句返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="tm">事务对象</param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, TransactionManager tm)
        => ExecSqlReader(sql, tm).MapToMulti<T>();
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, TransactionManager tm)
    {
        var reader = await ExecSqlReaderAsync(sql, tm);
        return reader.MapToMulti<T>();
    }

    /// <summary>
    /// 执行SQL语句返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="tm">事务对象</param>
    /// <param name="rowMapper">映射方法</param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, TransactionManager tm, Func<IDataReader, T> rowMapper)
        => ExecSqlReader(sql, tm).MapToMulti<T>(rowMapper);
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, TransactionManager tm, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecSqlReaderAsync(sql, tm);
        return reader.MapToMulti<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL语句返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="tm">事务对象</param>
    /// <param name="mode">映射模式</param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, TransactionManager tm, DataMappingMode mode)
        => ExecSqlReader(sql, tm).MapToMulti<T>(mode);
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, TransactionManager tm, DataMappingMode mode)
    {
        var reader = await ExecSqlReaderAsync(sql, tm);
        return reader.MapToMulti<T>(mode);
    }

    /// <summary>
    /// 执行SQL语句返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="paras">参数集合</param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, IEnumerable<DbParameter> paras)
        => ExecSqlReader(sql, paras).MapToMulti<T>();
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, IEnumerable<DbParameter> paras)
    {
        var reader = await ExecSqlReaderAsync(sql, paras);
        return reader.MapToMulti<T>();
    }

    /// <summary>
    /// 执行SQL语句返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="paras">参数集合</param>
    /// <param name="rowMapper">映射方法</param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, IEnumerable<DbParameter> paras, Func<IDataReader, T> rowMapper)
        => ExecSqlReader(sql, paras).MapToMulti<T>(rowMapper);
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, IEnumerable<DbParameter> paras, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecSqlReaderAsync(sql, paras);
        return reader.MapToMulti<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL语句返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="paras">参数集合</param>
    /// <param name="mode">映射模式</param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, IEnumerable<DbParameter> paras, DataMappingMode mode)
        => ExecSqlReader(sql, paras).MapToMulti<T>(mode);
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, IEnumerable<DbParameter> paras, DataMappingMode mode)
    {
        var reader = await ExecSqlReaderAsync(sql, paras);
        return reader.MapToMulti<T>(mode);
    }

    /// <summary>
    /// 执行SQL语句返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="tm">事务对象</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, TransactionManager tm, params object[] values)
        => ExecSqlReader(sql, tm, values).MapToMulti<T>();
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, TransactionManager tm, params object[] values)
    {
        var reader = await ExecSqlReaderAsync(sql, tm, values);
        return reader.MapToMulti<T>();
    }

    /// <summary>
    /// 执行SQL语句返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="tm">事务对象</param>
    /// <param name="rowMapper">映射方法</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, TransactionManager tm, Func<IDataReader, T> rowMapper, params object[] values)
        => ExecSqlReader(sql, tm, values).MapToMulti<T>(rowMapper);
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, TransactionManager tm, Func<IDataReader, T> rowMapper, params object[] values)
    {
        var reader = await ExecSqlReaderAsync(sql, tm, values);
        return reader.MapToMulti<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL语句返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="tm">事务对象</param>
    /// <param name="mode">映射模式</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, TransactionManager tm, DataMappingMode mode, params object[] values)
        => ExecSqlReader(sql, tm, values).MapToMulti<T>(mode);
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, TransactionManager tm, DataMappingMode mode, params object[] values)
    {
        var reader = await ExecSqlReaderAsync(sql, tm, values);
        return reader.MapToMulti<T>(mode);
    }

    /// <summary>
    /// 执行SQL语句返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, params object[] values)
        => ExecSqlReader(sql, values).MapToMulti<T>();
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, params object[] values)
    {
        var reader = await ExecSqlReaderAsync(sql, values);
        return reader.MapToMulti<T>();
    }

    /// <summary>
    /// 执行SQL语句返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="rowMapper">映射方法</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, Func<IDataReader, T> rowMapper, params object[] values)
        => ExecSqlReader(sql, values).MapToMulti<T>(rowMapper);
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, Func<IDataReader, T> rowMapper, params object[] values)
    {
        var reader = await ExecSqlReaderAsync(sql, values);
        return reader.MapToMulti<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL语句返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="mode">映射模式</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public IEnumerable<T> ExecSqlMulti<T>(string sql, DataMappingMode mode, params object[] values)
        => ExecSqlReader(sql, values).MapToMulti<T>(mode);
    public async Task<IEnumerable<T>> ExecSqlMultiAsync<T>(string sql, DataMappingMode mode, params object[] values)
    {
        var reader = await ExecSqlReaderAsync(sql, values);
        return reader.MapToMulti<T>(mode);
    }
    #endregion

    #region ExecProcMulti
    /// <summary>
    /// 执行存储过程返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc)
        => ExecProcReader(proc).MapToMulti<T>();
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc)
    {
        var reader = await ExecProcReaderAsync(proc);
        return reader.MapToMulti<T>();
    }

    /// <summary>
    /// 执行存储过程返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="rowMapper">映射方法</param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, Func<IDataReader, T> rowMapper)
        => ExecProcReader(proc).MapToMulti<T>(rowMapper);
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecProcReaderAsync(proc);
        return reader.MapToMulti<T>(rowMapper);
    }

    /// <summary>
    /// 执行存储过程返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="mode">映射模式</param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, DataMappingMode mode)
        => ExecProcReader(proc).MapToMulti<T>(mode);
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, DataMappingMode mode)
    {
        var reader = await ExecProcReaderAsync(proc);
        return reader.MapToMulti<T>(mode);
    }

    /// <summary>
    /// 执行存储过程返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">事务对象</param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, IEnumerable<DbParameter> paras, TransactionManager tm)
        => ExecProcReader(proc, paras, tm).MapToMulti<T>();
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, IEnumerable<DbParameter> paras, TransactionManager tm)
    {
        var reader = await ExecProcReaderAsync(proc, paras, tm);
        return reader.MapToMulti<T>();
    }

    /// <summary>
    /// 执行存储过程句返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">事务对象</param>
    /// <param name="rowMapper">映射方法</param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, IEnumerable<DbParameter> paras, TransactionManager tm, Func<IDataReader, T> rowMapper)
        => ExecProcReader(proc, paras, tm).MapToMulti<T>(rowMapper);
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, IEnumerable<DbParameter> paras, TransactionManager tm, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecProcReaderAsync(proc, paras, tm);
        return reader.MapToMulti<T>(rowMapper);
    }

    /// <summary>
    /// 执行存储过程返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm">事务对象</param>
    /// <param name="mode">映射模式</param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, IEnumerable<DbParameter> paras, TransactionManager tm, DataMappingMode mode)
        => ExecProcReader(proc, paras, tm).MapToMulti<T>(mode);
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, IEnumerable<DbParameter> paras, TransactionManager tm, DataMappingMode mode)
    {
        var reader = await ExecProcReaderAsync(proc, paras, tm);
        return reader.MapToMulti<T>(mode);
    }

    /// <summary>
    /// 执行存储过程返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="tm">事务对象</param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, TransactionManager tm)
        => ExecProcReader(proc,tm).MapToMulti<T>();
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, TransactionManager tm)
    {
        var reader = await ExecProcReaderAsync(proc, tm);
        return reader.MapToMulti<T>();
    }

    /// <summary>
    /// 执行存储过程返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="tm">事务对象</param>
    /// <param name="rowMapper">映射方法</param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, TransactionManager tm, Func<IDataReader, T> rowMapper)
        => ExecProcReader(proc, tm).MapToMulti<T>(rowMapper);
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, TransactionManager tm, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecProcReaderAsync(proc, tm);
        return reader.MapToMulti<T>(rowMapper);
    }

    /// <summary>
    /// 执行存储过程返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="tm">事务对象</param>
    /// <param name="mode">映射模式</param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, TransactionManager tm, DataMappingMode mode)
        => ExecProcReader(proc, tm).MapToMulti<T>(mode);
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, TransactionManager tm, DataMappingMode mode)
    {
        var reader = await ExecProcReaderAsync(proc, tm);
        return reader.MapToMulti<T>(mode);
    }

    /// <summary>
    /// 执行存储过程返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, IEnumerable<DbParameter> paras)
        => ExecProcReader(proc,paras).MapToMulti<T>();
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, IEnumerable<DbParameter> paras)
    {
        var reader = await ExecProcReaderAsync(proc, paras);
        return reader.MapToMulti<T>();
    }

    /// <summary>
    /// 执行存储过程返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <param name="rowMapper">映射方法</param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, IEnumerable<DbParameter> paras, Func<IDataReader, T> rowMapper)
        => ExecProcReader(proc, paras).MapToMulti<T>(rowMapper);
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, IEnumerable<DbParameter> paras, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecProcReaderAsync(proc, paras);
        return reader.MapToMulti<T>(rowMapper);
    }

    /// <summary>
    /// 执行存储过程返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <param name="mode">映射模式</param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, IEnumerable<DbParameter> paras, DataMappingMode mode)
        => ExecProcReader(proc, paras).MapToMulti<T>(mode);
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, IEnumerable<DbParameter> paras, DataMappingMode mode)
    {
        var reader = await ExecProcReaderAsync(proc, paras);
        return reader.MapToMulti<T>(mode);
    }

    /// <summary>
    /// 执行存储过程返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="tm">事务对象</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, TransactionManager tm, params object[] values)
        => ExecProcReader(proc, tm, values).MapToMulti<T>();
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, TransactionManager tm, params object[] values)
    {
        var reader = await ExecProcReaderAsync(proc, tm, values);
        return reader.MapToMulti<T>();
    }

    /// <summary>
    /// 执行存储过程返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="tm">事务对象</param>
    /// <param name="rowMapper">映射方法</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, TransactionManager tm, Func<IDataReader, T> rowMapper, params object[] values)
        => ExecProcReader(proc, tm, values).MapToMulti<T>(rowMapper);
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, TransactionManager tm, Func<IDataReader, T> rowMapper, params object[] values)
    {
        var reader = await ExecProcReaderAsync(proc, tm, values);
        return reader.MapToMulti<T>(rowMapper);
    }

    /// <summary>
    /// 执行存储过程返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="tm">事务对象</param>
    /// <param name="mode">映射模式</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, TransactionManager tm, DataMappingMode mode, params object[] values)
        => ExecProcReader(proc, tm, values).MapToMulti<T>(mode);
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, TransactionManager tm, DataMappingMode mode, params object[] values)
    {
        var reader = await ExecProcReaderAsync(proc, tm, values);
        return reader.MapToMulti<T>(mode);
    }

    /// <summary>
    /// 执行存储过程返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, params object[] values)
        => ExecProcReader(proc, values).MapToMulti<T>();
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, params object[] values)
    {
        var reader = await ExecProcReaderAsync(proc, values);
        return reader.MapToMulti<T>();
    }

    /// <summary>
    /// 执行存储过程返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="rowMapper">映射方法</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, Func<IDataReader, T> rowMapper, params object[] values)
        => ExecProcReader(proc, values).MapToMulti<T>(rowMapper);
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, Func<IDataReader, T> rowMapper, params object[] values)
    {
        var reader = await ExecProcReaderAsync(proc, values);
        return reader.MapToMulti<T>(rowMapper);
    }

    /// <summary>
    /// 执行存储过程返回枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="mode">映射模式</param>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    public IEnumerable<T> ExecProcMulti<T>(string proc, DataMappingMode mode, params object[] values)
        => ExecProcReader(proc, values).MapToMulti<T>(mode);
    public async Task<IEnumerable<T>> ExecProcMultiAsync<T>(string proc, DataMappingMode mode, params object[] values)
    {
        var reader = await ExecProcReaderAsync(proc, values);
        return reader.MapToMulti<T>(mode);
    }
    #endregion
}