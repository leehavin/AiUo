using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace AiUo.Data;

public abstract partial class Database
{
    #region ExecSqlSingle<T>
    /// <summary>
    /// 执行SQL返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <returns></returns>
    public T ExecSqlSingle<T>(string sql)
        => ExecSqlReader(sql).MapToSingle<T>();
    public async Task<T> ExecSqlSingleAsync<T>(string sql)
    {
        var reader = await ExecSqlReaderAsync(sql);
        return reader.MapToSingle<T>();
    }

    /// <summary>
    /// 执行SQL返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="rowMapper">行映射方法</param>
    /// <returns></returns>
    public T ExecSqlSingle<T>(string sql, Func<IDataReader, T> rowMapper)
        => ExecSqlReader(sql).MapToSingle(rowMapper);
    public async Task<T> ExecSqlSingleAsync<T>(string sql, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecSqlReaderAsync(sql);
        return reader.MapToSingle<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="paras">参数列表</param>
    /// <param name="tm">事务对象</param>
    /// <returns></returns>
    public T ExecSqlSingle<T>(string sql, IEnumerable<DbParameter> paras, TransactionManager tm)
        => ExecSqlReader(sql, paras, tm).MapToSingle<T>();
    public async Task<T> ExecSqlSingleAsync<T>(string sql, IEnumerable<DbParameter> paras, TransactionManager tm)
    {
        var reader = await ExecSqlReaderAsync(sql, paras, tm);
        return reader.MapToSingle<T>();
    }

    /// <summary>
    /// 执行SQL返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="paras">参数列表</param>
    /// <param name="tm">事务对象</param>
    /// <param name="rowMapper">行映射方法</param>
    /// <returns></returns>
    public T ExecSqlSingle<T>(string sql, IEnumerable<DbParameter> paras, TransactionManager tm, Func<IDataReader, T> rowMapper)
        => ExecSqlReader(sql, paras, tm).MapToSingle<T>(rowMapper);
    public async Task<T> ExecSqlSingleAsync<T>(string sql, IEnumerable<DbParameter> paras, TransactionManager tm, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecSqlReaderAsync(sql, paras, tm);
        return reader.MapToSingle<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="paras">参数列表</param>
    /// <param name="tm">事务对象</param>
    /// <param name="mode">行映射模式</param>
    /// <returns></returns>
    public T ExecSqlSingle<T>(string sql, IEnumerable<DbParameter> paras, TransactionManager tm, DataMappingMode mode)
        => ExecSqlReader(sql, paras, tm).MapToSingle<T>(mode);
    public async Task<T> ExecSqlSingleAsync<T>(string sql, IEnumerable<DbParameter> paras, TransactionManager tm, DataMappingMode mode)
    {
        var reader = await ExecSqlReaderAsync(sql, paras, tm);
        return reader.MapToSingle<T>(mode);
    }

    /// <summary>
    /// 执行SQL返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="tm">事务对象</param>
    /// <returns></returns>
    public T ExecSqlSingle<T>(string sql, TransactionManager tm)
        => ExecSqlReader(sql, tm).MapToSingle<T>(null);
    public async Task<T> ExecSqlSingleAsync<T>(string sql, TransactionManager tm)
    {
        var reader = await ExecSqlReaderAsync(sql, tm);
        return reader.MapToSingle<T>();
    }

    /// <summary>
    /// 执行SQL返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="tm">事务对象</param>
    /// <param name="rowMapper">行映射方法</param>
    /// <returns></returns>
    public T ExecSqlSingle<T>(string sql, TransactionManager tm, Func<IDataReader, T> rowMapper)
        => ExecSqlReader(sql, tm).MapToSingle(rowMapper);
    public async Task<T> ExecSqlSingleAsync<T>(string sql, TransactionManager tm, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecSqlReaderAsync(sql, tm);
        return reader.MapToSingle<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="paras">参数列表</param>
    /// <returns></returns>
    public T ExecSqlSingle<T>(string sql, IEnumerable<DbParameter> paras)
        => ExecSqlReader(sql, paras).MapToSingle<T>();
    public async Task<T> ExecSqlSingleAsync<T>(string sql, IEnumerable<DbParameter> paras)
    {
        var reader = await ExecSqlReaderAsync(sql, paras);
        return reader.MapToSingle<T>();
    }

    /// <summary>
    /// 执行SQL返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="paras">参数列表</param>
    /// <param name="rowMapper">行映射方法</param>
    /// <returns></returns>
    public T ExecSqlSingle<T>(string sql, IEnumerable<DbParameter> paras, Func<IDataReader, T> rowMapper)
        => ExecSqlReader(sql, paras).MapToSingle(rowMapper);
    public async Task<T> ExecSqlSingleAsync<T>(string sql, IEnumerable<DbParameter> paras, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecSqlReaderAsync(sql, paras);
        return reader.MapToSingle<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="tm">事务对象</param>
    /// <param name="values">参数值列表</param>
    /// <returns></returns>
    public T ExecSqlSingle<T>(string sql, TransactionManager tm, params object[] values)
        => ExecSqlReader(sql, tm, values).MapToSingle<T>();
    public async Task<T> ExecSqlSingleAsync<T>(string sql, TransactionManager tm, params object[] values)
    {
        var reader = await ExecSqlReaderAsync(sql, tm, values);
        return reader.MapToSingle<T>();
    }

    /// <summary>
    /// 执行SQL返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="tm">事务对象</param>
    /// <param name="rowMapper">行映射方法</param>
    /// <param name="values">参数值列表</param>
    /// <returns></returns>
    public T ExecSqlSingle<T>(string sql, TransactionManager tm, Func<IDataReader, T> rowMapper, params object[] values)
        => ExecSqlReader(sql, tm, values).MapToSingle(rowMapper);
    public async Task<T> ExecSqlSingleAsync<T>(string sql, TransactionManager tm, Func<IDataReader, T> rowMapper, params object[] values)
    {
        var reader = await ExecSqlReaderAsync(sql, tm, values);
        return reader.MapToSingle<T>(rowMapper);
    }

    /// <summary>
    /// 执行SQL返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="tm">事务对象</param>
    /// <param name="mode">行映射模式</param>
    /// <param name="values">参数值列表</param>
    /// <returns></returns>
    public T ExecSqlSingle<T>(string sql, TransactionManager tm, DataMappingMode mode, params object[] values)
        => ExecSqlReader(sql, tm, values).MapToSingle<T>(mode);
    public async Task<T> ExecSqlSingleAsync<T>(string sql, TransactionManager tm, DataMappingMode mode, params object[] values)
    {
        var reader = await ExecSqlReaderAsync(sql, tm, values);
        return reader.MapToSingle<T>(mode);
    }

    /// <summary>
    /// 执行SQL返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="values">参数值列表</param>
    /// <returns></returns>
    public T ExecSqlSingle<T>(string sql, params object[] values)
        => ExecSqlReader(sql, values).MapToSingle<T>();
    public async Task<T> ExecSqlSingleAsync<T>(string sql, params object[] values)
    {
        var reader = await ExecSqlReaderAsync(sql, values);
        return reader.MapToSingle<T>();
    }

    /// <summary>
    /// 执行SQL返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">SQL</param>
    /// <param name="rowMapper">行映射方法</param>
    /// <param name="values">参数值列表</param>
    /// <returns></returns>
    public T ExecSqlSingle<T>(string sql, Func<IDataReader, T> rowMapper, params object[] values)
        => ExecSqlReader(sql, values).MapToSingle(rowMapper);
    public async Task<T> ExecSqlSingleAsync<T>(string sql, Func<IDataReader, T> rowMapper, params object[] values)
    {
        var reader = await ExecSqlReaderAsync(sql, values);
        return reader.MapToSingle<T>(rowMapper);
    }
    #endregion

    #region ExecProcSingle<T>

    /// <summary>
    /// 执行存储过程返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc)
        => ExecProcReader(proc).MapToSingle<T>();
    public async Task<T> ExecProcSingleAsync<T>(string proc)
    {
        var reader = await ExecProcReaderAsync(proc);
        return reader.MapToSingle<T>();
    }

    /// <summary>
    /// 执行存储过程返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="rowMapper">行映射方法</param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, Func<IDataReader, T> rowMapper)
        => ExecProcReader(proc).MapToSingle<T>(rowMapper);
    public async Task<T> ExecProcSingleAsync<T>(string proc, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecProcReaderAsync(proc);
        return reader.MapToSingle<T>(rowMapper);
    }

    /// <summary>
    /// 执行存储过程返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="mode">行映射模式</param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, DataMappingMode mode)
        => ExecProcReader(proc).MapToSingle<T>(mode);
    public async Task<T> ExecProcSingleAsync<T>(string proc, DataMappingMode mode)
    {
        var reader = await ExecProcReaderAsync(proc);
        return reader.MapToSingle<T>(mode);
    }

    /// <summary>
    /// 执行存储过程返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm"></param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, IEnumerable<DbParameter> paras, TransactionManager tm)
        => ExecProcReader(proc, paras, tm).MapToSingle<T>();
    public async Task<T> ExecProcSingleAsync<T>(string proc, IEnumerable<DbParameter> paras, TransactionManager tm)
    {
        var reader = await ExecProcReaderAsync(proc, paras, tm);
        return reader.MapToSingle<T>();
    }

    /// <summary>
    /// 执行存储过程返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm"></param>
    /// <param name="rowMapper">行映射方法</param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, IEnumerable<DbParameter> paras, TransactionManager tm, Func<IDataReader, T> rowMapper)
        => ExecProcReader(proc, paras, tm).MapToSingle<T>(rowMapper);
    public async Task<T> ExecProcSingleAsync<T>(string proc, IEnumerable<DbParameter> paras, TransactionManager tm, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecProcReaderAsync(proc, paras, tm);
        return reader.MapToSingle<T>(rowMapper);
    }

    /// <summary>
    /// 执行存储过程返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <param name="tm"></param>
    /// <param name="mode">行映射模式</param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, IEnumerable<DbParameter> paras, TransactionManager tm, DataMappingMode mode)
        => ExecProcReader(proc, paras, tm).MapToSingle<T>(mode);
    public async Task<T> ExecProcSingleAsync<T>(string proc, IEnumerable<DbParameter> paras, TransactionManager tm, DataMappingMode mode)
    {
        var reader = await ExecProcReaderAsync(proc, paras, tm);
        return reader.MapToSingle<T>(mode);
    }

    /// <summary>
    /// 执行存储过程返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="tm"></param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, TransactionManager tm)
        => ExecProcReader(proc, tm).MapToSingle<T>();
    public async Task<T> ExecProcSingleAsync<T>(string proc, TransactionManager tm)
    {
        var reader = await ExecProcReaderAsync(proc, tm);
        return reader.MapToSingle<T>();
    }

    /// <summary>
    /// 执行存储过程返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="tm"></param>
    /// <param name="rowMapper">行映射方法</param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, TransactionManager tm, Func<IDataReader, T> rowMapper)
        => ExecProcReader(proc, tm).MapToSingle<T>(rowMapper);
    public async Task<T> ExecProcSingleAsync<T>(string proc, TransactionManager tm, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecProcReaderAsync(proc, tm);
        return reader.MapToSingle<T>(rowMapper);
    }

    /// <summary>
    /// 执行存储过程返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="tm"></param>
    /// <param name="mode">行映射模式</param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, TransactionManager tm, DataMappingMode mode)
        => ExecProcReader(proc, tm).MapToSingle<T>(mode);
    public async Task<T> ExecProcSingleAsync<T>(string proc, TransactionManager tm, DataMappingMode mode)
    {
        var reader = await ExecProcReaderAsync(proc, tm);
        return reader.MapToSingle<T>(mode);
    }

    /// <summary>
    /// 执行存储过程返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, IEnumerable<DbParameter> paras)
        => ExecProcReader(proc, paras).MapToSingle<T>();
    public async Task<T> ExecProcSingleAsync<T>(string proc, IEnumerable<DbParameter> paras)
    {
        var reader = await ExecProcReaderAsync(proc, paras);
        return reader.MapToSingle<T>();
    }

    /// <summary>
    /// 执行存储过程返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <param name="rowMapper">行映射方法</param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, IEnumerable<DbParameter> paras, Func<IDataReader, T> rowMapper)
        => ExecProcReader(proc, paras).MapToSingle<T>(rowMapper);
    public async Task<T> ExecProcSingleAsync<T>(string proc, IEnumerable<DbParameter> paras, Func<IDataReader, T> rowMapper)
    {
        var reader = await ExecProcReaderAsync(proc, paras);
        return reader.MapToSingle<T>(rowMapper);
    }

    /// <summary>
    /// 执行存储过程返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="paras">参数集合</param>
    /// <param name="mode">行映射模式</param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, IEnumerable<DbParameter> paras, DataMappingMode mode)
        => ExecProcReader(proc, paras).MapToSingle<T>(mode);
    public async Task<T> ExecProcSingleAsync<T>(string proc, IEnumerable<DbParameter> paras, DataMappingMode mode)
    {
        var reader = await ExecProcReaderAsync(proc, paras);
        return reader.MapToSingle<T>(mode);
    }

    /// <summary>
    /// 执行存储过程返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="tm"></param>
    /// <param name="values">参数值列表</param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, TransactionManager tm, params object[] values)
        => ExecProcReader(proc, tm, values).MapToSingle<T>();
    public async Task<T> ExecProcSingleAsync<T>(string proc, TransactionManager tm, params object[] values)
    {
        var reader = await ExecProcReaderAsync(proc, tm, values);
        return reader.MapToSingle<T>();
    }

    /// <summary>
    /// 执行存储过程返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="tm"></param>
    /// <param name="rowMapper">行映射方法</param>
    /// <param name="values">参数值列表</param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, TransactionManager tm, Func<IDataReader, T> rowMapper, params object[] values)
        => ExecProcReader(proc, tm, values).MapToSingle<T>(rowMapper);
    public async Task<T> ExecProcSingleAsync<T>(string proc, TransactionManager tm, Func<IDataReader, T> rowMapper, params object[] values)
    {
        var reader = await ExecProcReaderAsync(proc, tm, values);
        return reader.MapToSingle<T>(rowMapper);
    }

    /// <summary>
    /// 执行存储过程返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="tm"></param>
    /// <param name="mode">行映射模式</param>
    /// <param name="values">参数值列表</param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, TransactionManager tm, DataMappingMode mode, params object[] values)
        => ExecProcReader(proc, tm, values).MapToSingle<T>(mode);
    public async Task<T> ExecProcSingleAsync<T>(string proc, TransactionManager tm, DataMappingMode mode, params object[] values)
    {
        var reader = await ExecProcReaderAsync(proc, tm, values);
        return reader.MapToSingle<T>(mode);
    }

    /// <summary>
    /// 执行存储过程返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="values">参数值列表</param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, params object[] values)
        => ExecProcReader(proc, values).MapToSingle<T>();
    public async Task<T> ExecProcSingleAsync<T>(string proc, params object[] values)
    {
        var reader = await ExecProcReaderAsync(proc, values);
        return reader.MapToSingle<T>();
    }

    /// <summary>
    /// 执行存储过程返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="rowMapper">行映射方法</param>
    /// <param name="values">参数值列表</param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, Func<IDataReader, T> rowMapper, params object[] values)
        => ExecProcReader(proc, values).MapToSingle<T>(rowMapper);
    public async Task<T> ExecProcSingleAsync<T>(string proc, Func<IDataReader, T> rowMapper, params object[] values)
    {
        var reader = await ExecProcReaderAsync(proc, values);
        return reader.MapToSingle<T>(rowMapper);
    }

    /// <summary>
    /// 执行存储过程返回单个实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proc">存储过程</param>
    /// <param name="mode">行映射模式</param>
    /// <param name="values">参数值列表</param>
    /// <returns></returns>
    public T ExecProcSingle<T>(string proc, DataMappingMode mode, params object[] values)
        => ExecProcReader(proc, values).MapToSingle<T>(mode);
    public async Task<T> ExecProcSingleAsync<T>(string proc, DataMappingMode mode, params object[] values)
    {
        var reader = await ExecProcReaderAsync(proc, values);
        return reader.MapToSingle<T>(mode);
    }
    #endregion
}