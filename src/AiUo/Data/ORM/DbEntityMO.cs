using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Data.Common;
using AiUo.Collections;
using System.Data;

namespace AiUo.Data.ORM;

/// <summary>
/// Table and View MO基类
/// </summary>
/// <typeparam name="TDatabase"></typeparam>
/// <typeparam name="TParameter"></typeparam>
/// <typeparam name="TDbType"></typeparam>
/// <typeparam name="TEntity"></typeparam>
public abstract class DbEntityMO<TDatabase, TParameter, TDbType, TEntity> : DbMOBase<TDatabase, TParameter, TDbType>
    where TDatabase : Database<TParameter, TDbType>
    where TParameter : DbParameter
    where TDbType : struct
    where TEntity : IRowMapper<TEntity>
{
    #region GetWhere

    /// <summary>
    /// 获取所有数据
    /// </summary>
    /// <param name="tm">事务管理对象</param>
    /// <return>实体对象集合</return>
    public List<TEntity> GetAll(TransactionManager tm = null)
        => GetTopSort(string.Empty, null, -1, string.Empty, tm);
    public async Task<List<TEntity>> GetAllAsync(TransactionManager tm = null)
        => await GetTopSortAsync(string.Empty, null, -1, string.Empty, tm);

    #region Get
    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <return>实体对象集合</return>
    public List<TEntity> Get(string where)
        => GetTopSort(where, null, -1, string.Empty, null);
    public async Task<List<TEntity>> GetAsync(string where)
        => await GetTopSortAsync(where, null, -1, string.Empty, null);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "values">where子句中定义的参数值集合</param>
    /// <return>实体对象集合</return>
    public List<TEntity> Get(string where, params object[] values)
        => GetTopSort(where, -1, string.Empty, null, values);
    public async Task<List<TEntity>> GetAsync(string where, params object[] values)
        => await GetTopSortAsync(where, -1, string.Empty, null, values);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="where"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public List<TEntity> Get(string where, params (TDbType dbType, object value)[] parameters)
        => GetTopSort(where, -1, string.Empty, null, parameters);
    public async Task<List<TEntity>> GetAsync(string where, params (TDbType dbType, object value)[] parameters)
        => await GetTopSortAsync(where, -1, string.Empty, null, parameters);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "paras">where子句中定义的参数集合</param>
    /// <return>实体对象集合</return>
    public List<TEntity> Get(string where, IEnumerable<TParameter> paras)
        => GetTopSort(where, paras, -1, string.Empty, null);
    public async Task<List<TEntity>> GetAsync(string where, IEnumerable<TParameter> paras)
        => await GetTopSortAsync(where, paras, -1, string.Empty, null);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name="tm">事务管理对象</param>
    /// <return>实体对象集合</return>
    public List<TEntity> Get(string where, TransactionManager tm)
        => GetTopSort(where, null, -1, string.Empty, tm);
    public async Task<List<TEntity>> GetAsync(string where, TransactionManager tm)
        => await GetTopSortAsync(where, null, -1, string.Empty, tm);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name="tm">事务管理对象</param>
    /// <param name = "values">where子句中定义的参数值集合</param>
    /// <return>实体对象集合</return>
    public List<TEntity> Get(string where, TransactionManager tm, params object[] values)
        => GetTopSort(where, -1, string.Empty, tm, values);
    public async Task<List<TEntity>> GetAsync(string where, TransactionManager tm, params object[] values)
        => await GetTopSortAsync(where, -1, string.Empty, tm, values);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="where"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public List<TEntity> Get(string where, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => GetTopSort(where, -1, string.Empty, tm, parameters);
    public async Task<List<TEntity>> GetAsync(string where, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => await GetTopSortAsync(where, -1, string.Empty, tm, parameters);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "paras">where子句中定义的参数集合</param>
    /// <param name="tm">事务管理对象</param>
    /// <return>实体对象集合</return>
    public List<TEntity> Get(string where, IEnumerable<TParameter> paras, TransactionManager tm)
        => GetTopSort(where, paras, -1, string.Empty, tm);
    public async Task<List<TEntity>> GetAsync(string where, IEnumerable<TParameter> paras, TransactionManager tm)
        => await GetTopSortAsync(where, paras, -1, string.Empty, tm);
    #endregion // Get

    #region GetTop
    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "top">获取行数</param>
    /// <return>实体对象集合</return>
    public List<TEntity> GetTop(string where, int top)
        => GetTopSort(where, null, top, string.Empty, null);
    public async Task<List<TEntity>> GetTopAsync(string where, int top)
        => await GetTopSortAsync(where, null, top, string.Empty, null);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "top">获取行数</param>
    /// <param name = "values">where子句中定义的参数值集合</param>
    /// <return>实体对象集合</return>
    public List<TEntity> GetTop(string where, int top, params object[] values)
        => GetTopSort(where, top, string.Empty, null, values);
    public async Task<List<TEntity>> GetTopAsync(string where, int top, params object[] values)
        => await GetTopSortAsync(where, top, string.Empty, null, values);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="where"></param>
    /// <param name="top"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public List<TEntity> GetTop(string where, int top, params (TDbType dbType, object value)[] parameters)
        => GetTopSort(where, top, string.Empty, null, parameters);
    public async Task<List<TEntity>> GetTopAsync(string where, int top, params (TDbType dbType, object value)[] parameters)
        => await GetTopSortAsync(where, top, string.Empty, null, parameters);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "paras">where子句中定义的参数集合</param>
    /// <param name = "top">获取行数</param>
    /// <return>实体对象集合</return>
    public List<TEntity> GetTop(string where, IEnumerable<TParameter> paras, int top)
        => GetTopSort(where, paras, top, string.Empty, null);
    public async Task<List<TEntity>> GetTopAsync(string where, IEnumerable<TParameter> paras, int top)
        => await GetTopSortAsync(where, paras, top, string.Empty, null);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "top">获取行数</param>
    /// <param name="tm">事务管理对象</param>
    /// <return>实体对象集合</return>
    public List<TEntity> GetTop(string where, int top, TransactionManager tm)
        => GetTopSort(where, null, top, string.Empty, tm);
    public async Task<List<TEntity>> GetTopAsync(string where, int top, TransactionManager tm)
        => await GetTopSortAsync(where, null, top, string.Empty, tm);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "top">获取行数</param>
    /// <param name="tm">事务管理对象</param>
    /// <param name = "values">where子句中定义的参数值集合</param>
    /// <return>实体对象集合</return>
    public List<TEntity> GetTop(string where, int top, TransactionManager tm, params object[] values)
        => GetTopSort(where, top, string.Empty, tm, values);
    public async Task<List<TEntity>> GetTopAsync(string where, int top, TransactionManager tm, params object[] values)
        => await GetTopSortAsync(where, top, string.Empty, tm, values);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name="where"></param>
    /// <param name="top"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public List<TEntity> GetTop(string where, int top, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => GetTopSort(where, top, string.Empty, tm, parameters);
    public async Task<List<TEntity>> GetTopAsync(string where, int top, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => await GetTopSortAsync(where, top, string.Empty, tm, parameters);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "paras">where子句中定义的参数集合</param>
    /// <param name = "top">获取行数</param>
    /// <param name="tm">事务管理对象</param>
    /// <return>实体对象集合</return>
    public List<TEntity> GetTop(string where, IEnumerable<TParameter> paras, int top, TransactionManager tm)
        => GetTopSort(where, paras, top, string.Empty, tm);
    public async Task<List<TEntity>> GetTopAsync(string where, IEnumerable<TParameter> paras, int top, TransactionManager tm)
        => await GetTopSortAsync(where, paras, top, string.Empty, tm);
    #endregion // GetTop

    #region GetSort
    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "sort">排序表达式</param>
    /// <return>实体对象集合</return>
    public List<TEntity> GetSort(string where, string sort)
        => GetTopSort(where, null, -1, sort, null);
    public async Task<List<TEntity>> GetSortAsync(string where, string sort)
        => await GetTopSortAsync(where, null, -1, sort, null);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "sort">排序表达式</param>
    /// <param name = "values">where子句中定义的参数值集合</param>
    /// <return>实体对象集合</return>
    public List<TEntity> GetSort(string where, string sort, params object[] values)
        => GetTopSort(where, -1, sort, null, values);
    public async Task<List<TEntity>> GetSortAsync(string where, string sort, params object[] values)
        => await GetTopSortAsync(where, -1, sort, null, values);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name="where"></param>
    /// <param name="sort"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public List<TEntity> GetSort(string where, string sort, params (TDbType dbType, object value)[] parameters)
        => GetTopSort(where, -1, sort, null, parameters);
    public async Task<List<TEntity>> GetSortAsync(string where, string sort, params (TDbType dbType, object value)[] parameters)
        => await GetTopSortAsync(where, -1, sort, null, parameters);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "paras">where子句中定义的参数集合</param>
    /// <param name = "sort">排序表达式</param>
    /// <return>实体对象集合</return>
    public List<TEntity> GetSort(string where, IEnumerable<TParameter> paras, string sort)
        => GetTopSort(where, paras, -1, sort, null);
    public async Task<List<TEntity>> GetSortAsync(string where, IEnumerable<TParameter> paras, string sort)
        => await GetTopSortAsync(where, paras, -1, sort, null);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "sort">排序表达式</param>
    /// <param name="tm">事务管理对象</param>
    /// <return>实体对象集合</return>
    public List<TEntity> GetSort(string where, string sort, TransactionManager tm)
        => GetTopSort(where, null, -1, sort, tm);
    public async Task<List<TEntity>> GetSortAsync(string where, string sort, TransactionManager tm)
        => await GetTopSortAsync(where, null, -1, sort, tm);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "sort">排序表达式</param>
    /// <param name="tm">事务管理对象</param>
    /// <param name = "values">where子句中定义的参数值集合</param>
    /// <return>实体对象集合</return>
    public List<TEntity> GetSort(string where, string sort, TransactionManager tm, params object[] values)
        => GetTopSort(where, -1, sort, tm, values);
    public async Task<List<TEntity>> GetSortAsync(string where, string sort, TransactionManager tm, params object[] values)
        => await GetTopSortAsync(where, -1, sort, tm, values);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name="where"></param>
    /// <param name="sort"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public List<TEntity> GetSort(string where, string sort, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => GetTopSort(where, -1, sort, tm, parameters);
    public async Task<List<TEntity>> GetSortAsync(string where, string sort, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => await GetTopSortAsync(where, -1, sort, tm, parameters);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "paras">where子句中定义的参数集合</param>
    /// <param name = "sort">排序表达式</param>
    /// <param name="tm">事务管理对象</param>
    /// <return>实体对象集合</return>
    public List<TEntity> GetSort(string where, IEnumerable<TParameter> paras, string sort, TransactionManager tm)
        => GetTopSort(where, paras, -1, sort, tm);
    public async Task<List<TEntity>> GetSortAsync(string where, IEnumerable<TParameter> paras, string sort, TransactionManager tm)
        => await GetTopSortAsync(where, paras, -1, sort, tm);
    #endregion // GetSort

    #region GetTopSort
    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "top">获取行数</param>
    /// <param name = "sort">排序表达式</param>
    /// <return>实体对象集合</return>
    public List<TEntity> GetTopSort(string where, int top, string sort)
        => GetTopSort(where, null, top, sort, null);
    public async Task<List<TEntity>> GetTopSortAsync(string where, int top, string sort)
        => await GetTopSortAsync(where, null, top, sort, null);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "top">获取行数</param>
    /// <param name = "sort">排序表达式</param>
    /// <param name = "values">where子句中定义的参数值集合</param>
    /// <return>实体对象集合</return>
    public List<TEntity> GetTopSort(string where, int top, string sort, params object[] values)
        => GetTopSort(where, top, sort, null, values);
    public async Task<List<TEntity>> GetTopSortAsync(string where, int top, string sort, params object[] values)
        => await GetTopSortAsync(where, top, sort, null, values);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name="where"></param>
    /// <param name="top"></param>
    /// <param name="sort"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public List<TEntity> GetTopSort(string where, int top, string sort, params (TDbType dbType, object value)[] parameters)
        => GetTopSort(where, top, sort, null, parameters);
    public async Task<List<TEntity>> GetTopSortAsync(string where, int top, string sort, params (TDbType dbType, object value)[] parameters)
        => await GetTopSortAsync(where, top, sort, null, parameters);

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "paras">where子句中定义的参数集合</param>
    /// <param name = "top">获取行数</param>
    /// <param name = "sort">排序表达式</param>
    /// <return>实体对象集合</return>
    public List<TEntity> GetTopSort(string where, IEnumerable<TParameter> paras, int top, string sort)
        => GetTopSort(where, paras, top, sort, null);
    public async Task<List<TEntity>> GetTopSortAsync(string where, IEnumerable<TParameter> paras, int top, string sort)
        => await GetTopSortAsync(where, paras, top, sort, null);
    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "top">获取行数</param>
    /// <param name = "sort">排序表达式</param>
    /// <param name="tm">事务管理对象</param>
    /// <return>实体对象集合</return>
    public List<TEntity> GetTopSort(string where, int top, string sort, TransactionManager tm)
        => GetTopSort(where, null, top, sort, tm);
    public async Task<List<TEntity>> GetTopSortAsync(string where, int top, string sort, TransactionManager tm)
        => await GetTopSortAsync(where, null, top, sort, tm);
    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "top">获取行数</param>
    /// <param name = "sort">排序表达式</param>
    /// <param name="tm">事务管理对象</param>
    /// <param name = "values">where子句中定义的参数值集合</param>
    /// <return>实体对象集合</return>
    public List<TEntity> GetTopSort(string where, int top, string sort, TransactionManager tm, params object[] values)
    {
        var sql = BuildSelectSQL(where, top, sort);
        var paras = GetParametersByDerive(sql, values);
        return Database.ExecSqlList<TEntity>(sql, paras, tm, DataMappingMode.Interface);
    }
    public async Task<List<TEntity>> GetTopSortAsync(string where, int top, string sort, TransactionManager tm, params object[] values)
    {
        var sql = BuildSelectSQL(where, top, sort);
        var paras = GetParametersByDerive(sql, values);
        return await Database.ExecSqlListAsync<TEntity>(sql, paras, tm, DataMappingMode.Interface);
    }

    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "paras">where子句中定义的参数集合</param>
    /// <param name = "top">获取行数</param>
    /// <param name = "sort">排序表达式</param>
    /// <param name="tm">事务管理对象</param>
    /// <return>实体对象集合</return>
    public List<TEntity> GetTopSort(string where, IEnumerable<TParameter> paras, int top, string sort, TransactionManager tm)
    {
        var sql = BuildSelectSQL(where, top, sort);
        return Database.ExecSqlList<TEntity>(sql, paras, tm, DataMappingMode.Interface);
    }
    public async Task<List<TEntity>> GetTopSortAsync(string where, IEnumerable<TParameter> paras, int top, string sort, TransactionManager tm)
    {
        var sql = BuildSelectSQL(where, top, sort);
        return await Database.ExecSqlListAsync<TEntity>(sql, paras, tm, DataMappingMode.Interface);
    }
    /// <summary>
    /// 按自定义条件查询
    /// </summary>
    /// <param name="where"></param>
    /// <param name="top"></param>
    /// <param name="sort"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public List<TEntity> GetTopSort(string where, int top, string sort, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
    {
        var sql = BuildSelectSQL(where, top, sort);
        return Database.ExecSqlList<TEntity>(sql, tm, DataMappingMode.Interface, parameters);
    }
    public async Task<List<TEntity>> GetTopSortAsync(string where, int top, string sort, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
    {
        var sql = BuildSelectSQL(where, top, sort);
        return await Database.ExecSqlListAsync<TEntity>(sql, tm, DataMappingMode.Interface, parameters);
    }
    #endregion // GetTopSort

    #region GetFields
    /// <summary>
    /// 获得指定列的首行值
    /// </summary>
    /// <param name="field"></param>
    /// <param name="where"></param>
    /// <param name="paras"></param>
    /// <param name="tm"></param>
    /// <returns></returns>
    protected object GetScalar(string field, string where, IEnumerable<TParameter> paras, TransactionManager tm = null)
    {
        var sql = BuildSelectSQL(where, 1, null, field);
        var ret = Database.ExecSqlScalar(sql, paras, tm);
        return (ret == DBNull.Value) ? null : ret;
    }
    protected async Task<object> GetScalarAsync(string field, string where, IEnumerable<TParameter> paras, TransactionManager tm = null)
    {
        var sql = BuildSelectSQL(where, 1, null, field);
        var ret = await Database.ExecSqlScalarAsync(sql, paras, tm);
        return (ret == DBNull.Value) ? null : ret;
    }

    /// <summary>
    /// 获取指定列的DataTable
    /// </summary>
    /// <param name="fields"></param>
    /// <param name="where"></param>
    /// <param name="top"></param>
    /// <param name="sort"></param>
    /// <param name="paras"></param>
    /// <param name="tm"></param>
    /// <returns></returns>
    public DataTable GetTable(string fields, string where, int top = 0, string sort = null, IEnumerable<TParameter> paras = null, TransactionManager tm = null)
    {
        var sql = BuildSelectSQL(where, top, sort, fields);
        return Database.ExecSqlTable(sql, paras, tm);
    }
    public async Task<DataTable> GetTableAsync(string fields, string where, int top = 0, string sort = null, IEnumerable<TParameter> paras = null, TransactionManager tm = null)
    {
        var sql = BuildSelectSQL(where, top, sort, fields);
        return await Database.ExecSqlTableAsync(sql, paras, tm);
    }
    public DataTable GetTable(string fields, string where, int top = 0, string sort = null, TransactionManager tm = null, params object[] values)
    {
        var sql = BuildSelectSQL(where, top, sort, fields);
        var paras = GetParametersByDerive(sql, values);
        return Database.ExecSqlTable(sql, paras, tm);
    }
    public async Task<DataTable> GetTableAsync(string fields, string where, int top = 0, string sort = null, TransactionManager tm = null, params object[] values)
    {
        var sql = BuildSelectSQL(where, top, sort, fields);
        var paras = GetParametersByDerive(sql, values);
        return await Database.ExecSqlTableAsync(sql, paras, tm);
    }

    /// <summary>
    /// 获取指定列的DataRow
    /// </summary>
    /// <param name="fields"></param>
    /// <param name="where"></param>
    /// <param name="sort"></param>
    /// <param name="paras"></param>
    /// <param name="tm"></param>
    /// <returns></returns>
    public DataRow GetSingleRow(string fields, string where, string sort = null, IEnumerable<TParameter> paras = null, TransactionManager tm = null)
    {
        var sql = BuildSelectSQL(where, 1, sort, fields);
        return Database.ExecSqlSingle(sql, paras, tm);
    }
    public async Task<DataRow> GetSingleRowAsync(string fields, string where, string sort = null, IEnumerable<TParameter> paras = null, TransactionManager tm = null)
    {
        var sql = BuildSelectSQL(where, 1, sort, fields);
        return await Database.ExecSqlSingleAsync(sql, paras, tm);
    }
    public DataRow GetSingleRow(string fields, string where, string sort = null, TransactionManager tm = null, params object[] values)
    {
        var sql = BuildSelectSQL(where, 1, sort, fields);
        var paras = GetParametersByDerive(sql, values);
        return Database.ExecSqlSingle(sql, paras, tm);
    }
    public async Task<DataRow> GetSingleRowAsync(string fields, string where, string sort = null, TransactionManager tm = null, params object[] values)
    {
        var sql = BuildSelectSQL(where, 1, sort, fields);
        var paras = GetParametersByDerive(sql, values);
        return await Database.ExecSqlSingleAsync(sql, paras, tm);
    }
    #endregion

    #endregion // GetWhere

    #region GetSingle
    /// <summary>
    /// 按自定义条件查询，返回唯一一条记录
    /// </summary>
    /// <return>实体对象</return>
    public TEntity GetSingle()
        => GetSingle(string.Empty, null, (TransactionManager)null);
    public async Task<TEntity> GetSingleAsync()
        => await GetSingleAsync(string.Empty, null, (TransactionManager)null);

    /// <summary>
    /// 按自定义条件查询，返回唯一一条记录
    /// </summary>
    /// <param name="where"></param>
    /// <returns></returns>
    public TEntity GetSingle(string where)
        => GetSingle(where, null, (TransactionManager)null);
    public async Task<TEntity> GetSingleAsync(string where)
        => await GetSingleAsync(where, null, (TransactionManager)null);

    /// <summary>
    /// 按自定义条件查询，返回唯一一条记录
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name="tm">事务管理对象</param>
    /// <return>实体对象</return>
    public TEntity GetSingle(string where, TransactionManager tm)
        => GetSingle(where, null, tm);
    public async Task<TEntity> GetSingleAsync(string where, TransactionManager tm)
        => await GetSingleAsync(where, null, tm);

    /// <summary>
    /// 按自定义条件查询，返回唯一一条记录
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "values">where子句中定义的参数值集合</param>
    /// <return>实体对象</return>
    public TEntity GetSingle(string where, params object[] values)
        => GetSingle(where, null, values);
    public async Task<TEntity> GetSingleAsync(string where, params object[] values)
        => await GetSingleAsync(where, null, values);

    /// <summary>
    /// 按自定义条件查询，返回唯一一条记录
    /// </summary>
    /// <param name="where">自定义条件,where子句</param>
    /// <param name="parameters">where子句中定义的参数集合</param>
    /// <returns></returns>
    public TEntity GetSingle(string where, params (TDbType dbType, object value)[] parameters)
        => GetSingle(where, null, parameters);
    public async Task<TEntity> GetSingleAsync(string where, params (TDbType dbType, object value)[] parameters)
        => await GetSingleAsync(where, null, parameters);

    /// <summary>
    /// 按自定义条件查询，返回唯一一条记录
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "paras">where子句中定义的参数集合</param>
    /// <return>实体对象</return>
    public TEntity GetSingle(string where, IEnumerable<TParameter> paras)
        => GetSingle(where, paras, (TransactionManager)null);
    public async Task<TEntity> GetSingleAsync(string where, IEnumerable<TParameter> paras)
        => await GetSingleAsync(where, paras, (TransactionManager)null);

    /// <summary>
    /// 按自定义条件查询，返回唯一一条记录
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name="tm">事务管理对象</param>
    /// <param name = "values">where子句中定义的参数值集合</param>
    /// <return>实体对象</return>
    public TEntity GetSingle(string where, TransactionManager tm, params object[] values)
    {
        var sql = BuildSelectSQL(where, 1, string.Empty);
        var paras = GetParametersByDerive(sql, values);
        return Database.ExecSqlSingle<TEntity>(sql, paras, tm, DataMappingMode.Interface);
    }
    public async Task<TEntity> GetSingleAsync(string where, TransactionManager tm, params object[] values)
    {
        var sql = BuildSelectSQL(where, 1, string.Empty);
        var paras = GetParametersByDerive(sql, values);
        return await Database.ExecSqlSingleAsync<TEntity>(sql, paras, tm, DataMappingMode.Interface);
    }

    /// <summary>
    /// 按自定义条件查询，返回唯一一条记录
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "paras">where子句中定义的参数集合</param>
    /// <param name="tm">事务管理对象</param>
    /// <return>实体对象</return>
    public TEntity GetSingle(string where, IEnumerable<TParameter> paras, TransactionManager tm)
    {
        var sql = BuildSelectSQL(where, 1, string.Empty);
        return Database.ExecSqlSingle<TEntity>(sql, paras, tm, DataMappingMode.Interface);
    }
    public async Task<TEntity> GetSingleAsync(string where, IEnumerable<TParameter> paras, TransactionManager tm)
    {
        var sql = BuildSelectSQL(where, 1, string.Empty);
        return await Database.ExecSqlSingleAsync<TEntity>(sql, paras, tm, DataMappingMode.Interface);
    }

    /// <summary>
    /// 按自定义条件查询，返回唯一一条记录
    /// </summary>
    /// <param name="where">自定义条件,where子句</param>
    /// <param name="tm">事务管理对象</param>
    /// <param name="parameters">where子句中定义的参数集合</param>
    /// <returns></returns>
    public TEntity GetSingle(string where, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
    {
        var sql = BuildSelectSQL(where, 1, string.Empty);
        return Database.ExecSqlSingle<TEntity>(sql, tm, DataMappingMode.Interface, parameters);
    }
    public async Task<TEntity> GetSingleAsync(string where, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
    {
        var sql = BuildSelectSQL(where, 1, string.Empty);
        return await Database.ExecSqlSingleAsync<TEntity>(sql, tm, DataMappingMode.Interface, parameters);
    }
    #endregion //GetSingle

    #region  GetCount
    /// <summary>
    /// 获取记录数
    /// </summary>
    /// <param name="tm">事务管理对象</param>
    /// <return>数据个数</return>
    public long GetCount(TransactionManager tm = null)
        => GetCount(string.Empty, null, tm);
    public async Task<long> GetCountAsync(TransactionManager tm = null)
        => await GetCountAsync(string.Empty, null, tm);

    /// <summary>
    /// 获取记录数
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <return>数据个数</return>
    public long GetCount(string where)
        => GetCount(where, null, (TransactionManager)null);
    public async Task<long> GetCountAsync(string where)
        => await GetCountAsync(where, null, (TransactionManager)null);

    /// <summary>
    /// 获取记录数
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "values">where子句中定义的参数值集合</param>
    /// <return>数据个数</return>
    public long GetCount(string where, params object[] values)
        => GetCount(where, null, values);
    public async Task<long> GetCountAsync(string where, params object[] values)
        => await GetCountAsync(where, null, values);

    /// <summary>
    /// 获取记录数
    /// </summary>
    /// <param name="where"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public long GetCount(string where, params (TDbType dbType, object value)[] parameters)
        => GetCount(where, null, parameters);
    public async Task<long> GetCountAsync(string where, params (TDbType dbType, object value)[] parameters)
        => await GetCountAsync(where, null, parameters);

    /// <summary>
    /// 获取记录数
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "paras">where子句中定义的参数集合</param>
    /// <return>数据个数</return>
    public long GetCount(string where, IEnumerable<TParameter> paras)
        => GetCount(where, paras, null);
    public async Task<long> GetCountAsync(string where, IEnumerable<TParameter> paras)
        => await GetCountAsync(where, paras, null);

    /// <summary>
    /// 获取记录数
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name="tm">事务管理对象</param>
    /// <return>数据个数</return>
    public long GetCount(string where, TransactionManager tm)
        => GetCount(where, null, tm);
    public async Task<long> GetCountAsync(string where, TransactionManager tm)
        => await GetCountAsync(where, null, tm);

    /// <summary>
    /// 获取记录数
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name="tm">事务管理对象</param>
    /// <param name = "values">where子句中定义的参数值集合</param>
    /// <return>数据个数</return>
    public long GetCount(string where, TransactionManager tm, params object[] values)
    {
        string sql = BuildCountSQL(where);
        var paras = GetParametersByDerive(sql, values);
        return Database.ExecSqlScalar<long>(sql, paras, tm);
    }
    public async Task<long> GetCountAsync(string where, TransactionManager tm, params object[] values)
    {
        string sql = BuildCountSQL(where);
        var paras = GetParametersByDerive(sql, values);
        return await Database.ExecSqlScalarAsync<long>(sql, paras, tm);
    }

    /// <summary>
    /// 获取记录数
    /// </summary>
    /// <param name="where"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public long GetCount(string where, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
    {
        string sql = BuildCountSQL(where);
        return Database.ExecSqlScalar<long>(sql, tm, parameters);
    }
    public async Task<long> GetCountAsync(string where, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
    {
        string sql = BuildCountSQL(where);
        return await Database.ExecSqlScalarAsync<long>(sql, tm, parameters);
    }

    /// <summary>
    /// 获取记录数
    /// </summary>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "paras">where子句中定义的参数集合</param>
    /// <param name="tm">事务管理对象</param>
    /// <return>数据个数</return>
    public long GetCount(string where, IEnumerable<TParameter> paras, TransactionManager tm)
    {
        string sql = BuildCountSQL(where);
        return Database.ExecSqlScalar<long>(sql, paras, tm);
    }
    public async Task<long> GetCountAsync(string where, IEnumerable<TParameter> paras, TransactionManager tm)
    {
        string sql = BuildCountSQL(where);
        return await Database.ExecSqlScalarAsync<long>(sql, paras, tm);
    }
    #endregion //GetCount

    #region  分页

    /// <summary>
    /// 获取分页操作对象
    /// </summary>
    /// <param name = "pageSize">页大小</param>
    /// <param name = "where">自定义条件,where子句</param>
    /// <param name = "sort">排序表达式</param>
    /// <param name = "values">where 和 sort 中的参数值</param>
    /// <return>分页操作对象</return>
    public IDataPager GetPager(int pageSize, string where = null, string sort = null, params object[] values)
    {
        string sql = BuildSelectSQL(where, 0, sort);
        var paras = GetParametersByDerive(sql, values);
        var ret = Database.GetPager(sql, pageSize);
        ret.AddParameters(paras);
        return ret;
    }
    public List<TEntity> GetPagerList(int pageSize, int pageIndex, string where = null, string sort = null, params object[] values)
    {
        var pager = GetPager(pageSize, where, sort, values);
        var reader = pager.GetPageReader(pageIndex);
        return reader.MapToList<TEntity>(DataMappingMode.Interface);
    }
    public async Task<List<TEntity>> GetPagerListAsync(int pageSize, int pageIndex, string where = null, string sort = null, params object[] values)
    {
        var pager = GetPager(pageSize, where, sort, values);
        var reader = await pager.GetPageReaderAsync(pageIndex);
        return reader.MapToList<TEntity>(DataMappingMode.Interface);
    }
    public async Task<long> GetPageCountAsync(int pageSize, string where = null, string sort = null, bool refresh = true, params object[] values)
    {
        var pager = GetPager(pageSize, where, sort, values);
        return await pager.GetPageCountAsync(refresh);
    }
    #endregion

    #region Utils
    /// <summary>
    /// 获取Select count语句
    /// </summary>
    /// <param name="where"></param>
    /// <returns></returns>
    protected string BuildCountSQL(string where)
    {
        var ret = $"SELECT COUNT(*) FROM {SourceName}";
        if (!string.IsNullOrEmpty(where))
            ret += " WHERE " + where;
        return ret;
    }

    /// <summary>
    /// 获取SelectSQL
    /// </summary>
    /// <param name="where"></param>
    /// <param name="top"></param>
    /// <param name="sort"></param>
    /// <param name="fields"></param>
    /// <param name="isForUpdate"></param>
    /// <returns></returns>
    protected string BuildSelectSQL(string where, int top, string sort, string fields = null, bool isForUpdate = false)
        => OrmProvider.BuildSelectSQL(SourceName, where, top, sort, fields, isForUpdate);
    /// <summary>
    /// 获取Update语句
    /// </summary>
    /// <param name="set"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    protected string BuildUpdateSQL(string set, string where)
    {
        if (string.IsNullOrEmpty(set))
            throw new ArgumentException("生成update语句是set参数不能为空。");
        var ret = $"UPDATE {SourceName} SET {set}";
        if (!string.IsNullOrEmpty(where))
            ret += " WHERE " + where;
        return ret;
    }
    #endregion

    #region GetParametersByDerive
    /// <summary>
    /// 合并参数值
    /// </summary>
    /// <param name="values"></param>
    /// <param name="newValues"></param>
    /// <returns></returns>
    protected object[] ConcatValues(object[] values, params object[] newValues)
        => values.AddRange(newValues);

    /// <summary>
    /// 合并参数
    /// </summary>
    /// <param name="paras"></param>
    /// <param name="newParas"></param>
    /// <returns></returns>
    protected IEnumerable<TParameter> ConcatParameters(IEnumerable<TParameter> paras, IEnumerable<TParameter> newParas)
        => paras.Concat(newParas);

    /// <summary>
    /// key : SourceName, value: 所有字段类型
    /// </summary>
    protected static ConcurrentDictionary<string, Dictionary<string, TDbType>> _objParametersCache = new ConcurrentDictionary<string, Dictionary<string, TDbType>>();
    /// <summary>
    /// 解析SQL配置参数值
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    protected IEnumerable<TParameter> GetParametersByDerive(string sql, object[] values)
    {
        if (values == null || values.Length == 0)
            return null;
        //var key = $"{Database.ConnectionString}|{sql}";
        //var paras = _sqlParametersCache.GetOrAdd(key, (_) =>
        //{
        //    return GetSqlCacheItem(sql, values);
        //});
        var paras = GetSqlCacheItem(sql, values);
        var ret = new List<TParameter>();
        for (int i = 0; i < paras.Length; i++)
            ret.Add(Database.CreateInParameter(paras[i].parameterName, values[i], paras[i].dbType));
        return ret;
    }
    /// <summary>
    /// 获取参数缓存项,没有解析SQL并缓存
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    protected (string parameterName, TDbType dbType)[] GetSqlCacheItem(string sql, object[] values)
    {
        var key = $"{SourceType}|{SourceName}";
        Dictionary<string, TDbType> paras = _objParametersCache.GetOrAdd(key, (_) =>
        {
            return OrmProvider.GetDbTypeMappings(Database, SourceName, SourceType);
        });
        var parameterNames = Database.ParseSqlParameterNames(sql);
        var namesHash = new HashSet<string>();

        var ret = new List<(string parameterName, TDbType dbType)>();
        foreach (var parameterName in parameterNames)
        {
            var paramKey = parameterName.ToUpper();
            if (namesHash.Contains(paramKey))
                continue;
            if (!paras.TryGetValue(paramKey, out TDbType dbType))
            {
                var valueType = values[namesHash.Count].GetType();
                dbType = OrmProvider.MapDotNetTypeToDbType(valueType);
            }
            ret.Add((parameterName, dbType));
            namesHash.Add(paramKey);
        }
        if (ret.Count != values.Length)
            throw new Exception($"解析SQL语句获得的参数数量与赋值的数量不一致。paras: {ret.Count} values: {values.Length}");
        return ret.ToArray();
    }
    #endregion
}