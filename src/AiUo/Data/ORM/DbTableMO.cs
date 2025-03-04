using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace AiUo.Data.ORM;

/// <summary>
/// 数据表MO基类
/// </summary>
/// <typeparam name="TDatabase"></typeparam>
/// <typeparam name="TParameter"></typeparam>
/// <typeparam name="TDbType"></typeparam>
/// <typeparam name="TEntity"></typeparam>
public abstract class DbTableMO<TDatabase, TParameter, TDbType, TEntity> : DbEntityMO<TDatabase, TParameter, TDbType, TEntity>
    where TDatabase : Database<TParameter, TDbType>
    where TParameter : DbParameter
    where TDbType : struct
    where TEntity : IRowMapper<TEntity>
{
    #region Properties
    /// <summary>
    /// 数据对象类型
    /// </summary>
    public override DbObjectType SourceType => DbObjectType.Table;
    /// <summary>
    /// 数据对象源名称
    /// </summary>
    public override string SourceName => TableName;
    /// <summary>
    /// 表名
    /// </summary>
    public abstract string TableName { get; set; }
    public bool UseReadConnectionString
    {
        get => Database.UseReadConnectionString;
        set => Database.UseReadConnectionString = value;
    }
    #endregion

    #region Add
    /// <summary>
    /// 添加实体对象
    /// </summary>
    /// <param name="item"></param>
    /// <param name="tm"></param>
    /// <param name="useIgnore"></param>
    /// <returns></returns>
    public abstract int Add(TEntity item, TransactionManager tm = null, bool useIgnore = false);

    public abstract Task<int> AddAsync(TEntity item, TransactionManager tm = null, bool useIgnore = false);

    /// <summary>
    /// 插入一组数据
    /// </summary>
    /// <param name = "items">要插入的实体对象集合</param>
    /// <param name="tm">事务管理对象</param>
    /// <return>受影响的行数</return>
    public int Add(IEnumerable<TEntity> items, TransactionManager tm = null)
    {
        int ret = 0;
        foreach (var item in items)
        {
            ret += Add(item, tm);
        }
        return ret;
    }
    public async Task<int> AddAsync(IEnumerable<TEntity> items, TransactionManager tm = null)
    {
        int ret = 0;
        foreach (var item in items)
        {
            ret += await AddAsync(item, tm);
        }
        return ret;
    }
    #endregion

    #region Remove
    /// <summary>
    /// TRUNCATE全部数据
    /// </summary>
    public int Truncate()
        => Database.ExecSqlNonQuery($"TRUNCATE {TableName}");
    public async Task<int> TruncateAsync()
        => await Database.ExecSqlNonQueryAsync($"TRUNCATE {TableName}");

    /// <summary>
    /// 删除全部数据
    /// </summary>
    /// <param name="tm">事务管理对象</param>
    /// <return>受影响的行数</return>
    public int RemoveAll(TransactionManager tm = null)
        => Remove(string.Empty, tm);
    public async Task<int> RemoveAllAsync(TransactionManager tm = null)
        => await RemoveAsync(string.Empty, tm);

    /// <summary>
    /// 删除指定条件的数据
    /// </summary>
    /// <param name = "where">自定义删除条件，where子句。如：id=1 and name='aaa'</param>
    /// <return>受影响的行数</return>
    public int Remove(string where)
        => Remove(where, null, (TransactionManager)null);
    public async Task<int> RemoveAsync(string where)
        => await RemoveAsync(where, null, (TransactionManager)null);
    /// <summary>
    /// 删除指定条件的数据
    /// </summary>
    /// <param name = "where">自定义删除条件，where子句。如：id=1 and name='aaa'</param>
    /// <param name = "values">where子句中定义的参数值集合</param>
    /// <return>受影响的行数</return>
    public int Remove(string where, params object[] values)
        => Remove(where, null, values);
    public async Task<int> RemoveAsync(string where, params object[] values)
        => await RemoveAsync(where, null, values);

    /// <summary>
    /// 删除指定条件的数据
    /// </summary>
    /// <param name="where"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public int Remove(string where, params (TDbType dbType, object value)[] parameters)
        => Remove(where, null, parameters);
    public async Task<int> RemoveAsync(string where, params (TDbType dbType, object value)[] parameters)
        => await RemoveAsync(where, null, parameters);

    /// <summary>
    /// 删除指定条件的数据
    /// </summary>
    /// <param name = "where">自定义删除条件，where子句。如：id=1 and name='aaa'</param>
    /// <param name = "paras">where子句中定义的参数集合</param>
    /// <return>受影响的行数</return>
    public int Remove(string where, IEnumerable<TParameter> paras)
        => Remove(where, paras, null);
    public async Task<int> RemoveAsync(string where, IEnumerable<TParameter> paras)
        => await RemoveAsync(where, paras, null);

    /// <summary>
    /// 删除指定条件的数据
    /// </summary>
    /// <param name="where"></param>
    /// <param name="tm"></param>
    /// <returns></returns>
    public int Remove(string where, TransactionManager tm)
        => Remove(where, null, tm);
    public async Task<int> RemoveAsync(string where, TransactionManager tm)
        => await RemoveAsync(where, null, tm);

    /// <summary>
    /// 删除指定条件的数据
    /// </summary>
    /// <param name = "where">自定义删除条件，where子句。如：id=1 and name='aaa'</param>
    /// <param name="tm">事务管理对象</param>
    /// <param name = "values">where子句中定义的参数值集合</param>
    /// <return>受影响的行数</return>
    public int Remove(string where, TransactionManager tm, params object[] values)
    {
        string sql = BuildDeleteSQL(where);
        var paras = GetParametersByDerive(sql, values);
        return Database.ExecSqlNonQuery(sql, paras, tm);
    }
    public async Task<int> RemoveAsync(string where, TransactionManager tm, params object[] values)
    {
        string sql = BuildDeleteSQL(where);
        var paras = GetParametersByDerive(sql, values);
        return await Database.ExecSqlNonQueryAsync(sql, paras, tm);
    }
    /// <summary>
    /// 删除指定条件的数据
    /// </summary>
    /// <param name="where"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public int Remove(string where, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
    {
        string sql = BuildDeleteSQL(where);
        return Database.ExecSqlNonQuery(sql, tm, parameters);
    }
    public async Task<int> RemoveAsync(string where, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
    {
        string sql = BuildDeleteSQL(where);
        return await Database.ExecSqlNonQueryAsync(sql, tm, parameters);
    }
    /// <summary>
    /// 删除指定条件的数据
    /// </summary>
    /// <param name = "where">自定义删除条件，where子句。如：id=1 and name='aaa'</param>
    /// <param name="tm">事务管理对象</param>
    /// <param name = "paras">where子句中定义的参数集合</param>
    /// <return>受影响的行数</return>
    public int Remove(string where, IEnumerable<TParameter> paras, TransactionManager tm = null)
    {
        string sql = BuildDeleteSQL(where);
        return Database.ExecSqlNonQuery(sql, paras, tm);
    }
    public async Task<int> RemoveAsync(string where, IEnumerable<TParameter> paras, TransactionManager tm = null)
    {
        string sql = BuildDeleteSQL(where);
        return await Database.ExecSqlNonQueryAsync(sql, paras, tm);
    }
    #endregion

    #region Put
    /// <summary>
    /// 按条件和指定列更新表数据
    /// </summary>
    /// <param name="set">更新列数据</param>
    /// <param name="where">更新条件</param>
    /// <param name="paras">参数值</param>
    /// <param name="tm">事务对象</param>
    /// <returns></returns>
    public int Put(string set, string where, IEnumerable<TParameter> paras, TransactionManager tm = null)
        => Database.ExecSqlNonQuery(BuildUpdateSQL(set, where), paras, tm);
    public async Task<int> PutAsync(string set, string where, IEnumerable<TParameter> paras, TransactionManager tm = null)
        => await Database.ExecSqlNonQueryAsync(BuildUpdateSQL(set, where), paras, tm);

    /// <summary>
    /// 按条件和指定列更新表数据
    /// </summary>
    /// <param name="set">更新列数据</param>
    /// <param name="where">更新条件</param>
    /// <returns></returns>
    public int Put(string set, string where)
        => Put(set, where, null, (TransactionManager)null);
    public async Task<int> PutAsync(string set, string where)
        => await PutAsync(set, where, null, (TransactionManager)null);

    /// <summary>
    /// 按条件和指定列更新表数据
    /// </summary>
    /// <param name="set">更新列数据</param>
    /// <param name="where">更新条件</param>
    /// <param name="values">参数值</param>
    /// <returns></returns>
    public int Put(string set, string where, params object[] values)
        => Put(set, where, null, values);
    public async Task<int> PutAsync(string set, string where, params object[] values)
        => await PutAsync(set, where, null, values);

    /// <summary>
    /// 按条件和指定列更新表数据
    /// </summary>
    /// <param name="set"></param>
    /// <param name="where"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public int Put(string set, string where, params (TDbType dbType, object value)[] parameters)
        => Put(set, where, null, parameters);
    public async Task<int> PutAsync(string set, string where, params (TDbType dbType, object value)[] parameters)
        => await PutAsync(set, where, null, parameters);

    /// <summary>
    /// 按条件和指定列更新表数据
    /// </summary>
    /// <param name="set"></param>
    /// <param name="where"></param>
    /// <param name="tm"></param>
    /// <returns></returns>
    public int Put(string set, string where, TransactionManager tm)
        => Put(set, where, null, tm);
    public async Task<int> PutAsync(string set, string where, TransactionManager tm)
        => await PutAsync(set, where, null, tm);

    /// <summary>
    /// 按条件和指定列更新表数据
    /// </summary>
    /// <param name="set">更新列数据</param>
    /// <param name="where">更新条件</param>
    /// <param name="tm">事务对象</param>
    /// <param name="values">参数值</param>
    /// <returns></returns>
    public int Put(string set, string where, TransactionManager tm, params object[] values)
    {
        var sql = BuildUpdateSQL(set, where);
        var paras = GetParametersByDerive(sql, values);
        return Database.ExecSqlNonQuery(sql, paras, tm);
    }
    public async Task<int> PutAsync(string set, string where, TransactionManager tm, params object[] values)
    {
        var sql = BuildUpdateSQL(set, where);
        var paras = GetParametersByDerive(sql, values);
        return await Database.ExecSqlNonQueryAsync(sql, paras, tm);
    }

    /// <summary>
    /// 按条件和指定列更新表数据
    /// </summary>
    /// <param name="set"></param>
    /// <param name="where"></param>
    /// <param name="tm"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public int Put(string set, string where, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => Database.ExecSqlNonQuery(BuildUpdateSQL(set, where), tm, parameters);
    public async Task<int> PutAsync(string set, string where, TransactionManager tm, params (TDbType dbType, object value)[] parameters)
        => await Database.ExecSqlNonQueryAsync(BuildUpdateSQL(set, where), tm, parameters);
    #endregion

    #region Utils
    /// <summary>
    /// 获取DeleteSQL
    /// </summary>
    /// <param name="where"></param>
    /// <returns></returns>
    protected string BuildDeleteSQL(string where)
    {
        var ret = $"DELETE FROM {TableName}";
        if (!string.IsNullOrEmpty(where))
            ret += " WHERE " + where;
        return ret;
    }
    #endregion
}