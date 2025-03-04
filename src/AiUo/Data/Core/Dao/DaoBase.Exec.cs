using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace AiUo.Data;

public abstract partial class DaoBase
{
    #region Execute Methods
    /// <summary>
    /// 执行查询语句并返回首行首列数据
    /// 如果返回null表示一行数据都没有，返回DBNull表示首行首列的值为空
    /// </summary>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public object ExecScalar(TransactionManager tm = null)
        => Database.ExecScalar(GetCommand(tm));
    public async Task<object> ExecScalarAsync(TransactionManager tm = null)
        => await Database.ExecScalarAsync(GetCommand(tm));

    /// <summary>
    /// 执行查询语句并返回首行首列数据
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="tm">数据库事务管理对象</param>
    /// <param name="converter">类型转换器</param>
    /// <returns></returns>
    public T ExecScalar<T>(TransactionManager tm, Func<object, T> converter)
        => converter(ExecScalar(tm));
    public async Task<T> ExecScalarAsync<T>(TransactionManager tm, Func<object, T> converter)
        => converter(await ExecScalarAsync(tm));

    /// <summary>
    /// 执行查询语句并返回首行首列数据
    /// DBNull将转换成null返回
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public T ExecScalar<T>(TransactionManager tm = null)
        => ExecScalar(tm, AiUoUtil.ConvertTo<T>);
    public async Task<T> ExecScalarAsync<T>(TransactionManager tm = null)
        => await ExecScalarAsync(tm, AiUoUtil.ConvertTo<T>);

    /// <summary>
    /// 执行查询语句并返回首行首列数据
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="converter">类型转换器</param>
    /// <returns></returns>
    public T ExecScalar<T>(Func<object, T> converter)
        => ExecScalar(null, converter);
    public async Task<T> ExecScalarAsync<T>(Func<object, T> converter)
        => await ExecScalarAsync(null, converter);

    /// <summary>
    /// 执行SQL语句并返回受影响的行数
    /// </summary>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public int ExecNonQuery(TransactionManager tm = null)
        => Database.ExecNonQuery(GetCommand(tm));
    public async Task<int> ExecNonQueryAsync(TransactionManager tm = null)
        => await Database.ExecNonQueryAsync(GetCommand(tm));

    /// <summary>
    /// 执行SQL语句并返回结果集
    /// </summary>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public DataReaderWrapper ExecReader(TransactionManager tm = null)
        => Database.ExecReader(GetCommand(tm));
    public async Task<DataReaderWrapper> ExecReaderAsync(TransactionManager tm = null)
        => await Database.ExecReaderAsync(GetCommand(tm));

    /// <summary>
    /// 执行SQL语句并返回DataTable
    /// </summary>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public DataTable ExecTable(TransactionManager tm = null)
        => ExecReader(tm).ToTable();
    public async Task<DataTable> ExecTableAsync(TransactionManager tm = null)
    {
        var reader = await ExecReaderAsync(tm);
        return reader.ToTable();
    }
    #endregion

    #region ExecSingle
    /// <summary>
    /// 返回唯一一行记录，没有记录则返回NULL，如果存在多行抛出异常
    /// </summary>
    /// <param name="tm"></param>
    /// <returns></returns>
    public DataRow ExecSingle(TransactionManager tm = null)
    {
        DataTable dt = ExecTable(tm);
        if (dt.Rows.Count > 1)
            throw new Exception("获取的数据存在多条记录。");
        return (dt.Rows.Count == 1) ? dt.Rows[0] : null;
    }
    /// <summary>
    /// 执行SQL语句并返回唯一实体对象(T)，如果没有值返回 T 类型的默认值。
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <param name="rowMapper">IDataReader对象到实体对象的映射方法，如果为null，则使用接口IRowMapper(T)或ColumnMapperAttribute定义的元数据通过反射获取实体对象</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public T ExecSingle<T>(Func<IDataReader, T> rowMapper, TransactionManager tm)
        => ExecReader(tm).MapToSingle(rowMapper);

    /// <summary>
    /// 执行SQL语句并返回唯一实体对象(T)
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public T ExecSingle<T>(TransactionManager tm)
        => ExecSingle<T>(null, tm);

    /// <summary>
    /// 执行SQL语句并返回唯一实体对象(T)
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <param name="rowMapper">IDataReader对象到实体对象的映射方法，如果为null，则使用接口IRowMapper(T)或ColumnMapperAttribute定义的元数据通过反射获取实体对象</param>
    /// <returns></returns>
    public T ExecSingle<T>(Func<IDataReader, T> rowMapper)
        => ExecSingle<T>(rowMapper, null);

    /// <summary>
    /// 执行SQL语句并返回唯一实体对象(T)
    /// 使用ColumnMapperAttribute定义的元数据进行反射获取实体对象
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <returns></returns>
    public T ExecSingle<T>()
        => ExecSingle<T>(null, null);
    #endregion

    #region ExecFirst
    /// <summary>
    /// 返回首行记录，没有记录则返回NULL
    /// </summary>
    /// <param name="tm"></param>
    /// <returns></returns>
    public DataRow ExecFirst(TransactionManager tm = null)
    {
        DataTable dt = ExecTable(tm);
        return (dt.Rows.Count > 0) ? dt.Rows[0] : null;
    }
    /// <summary>
    /// 执行SQL语句并返回首行实体对象(T)，如果没有值返回 T 类型的默认值。
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <param name="rowMapper">IDataReader对象到实体对象的映射方法，如果为null，则使用接口IRowMapper(T)或ColumnMapperAttribute定义的元数据通过反射获取实体对象</param>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public T ExecFirst<T>(Func<IDataReader, T> rowMapper, TransactionManager tm)
        => ExecReader(tm).MapToFirst(rowMapper);

    /// <summary>
    /// 执行SQL语句并返回首行实体对象(T)
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public T ExecFirst<T>(TransactionManager tm)
        => ExecFirst<T>(null, tm);

    /// <summary>
    /// 执行SQL语句并返回首行实体对象(T)
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <param name="rowMapper">IDataReader对象到实体对象的映射方法，如果为null，则使用接口IRowMapper(T)或ColumnMapperAttribute定义的元数据通过反射获取实体对象</param>
    /// <returns></returns>
    public T ExecFirst<T>(Func<IDataReader, T> rowMapper)
        => ExecFirst<T>(rowMapper, null);

    /// <summary>
    /// 执行SQL语句并返回首行实体对象(T)
    /// 使用ColumnMapperAttribute定义的元数据进行反射获取实体对象
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <returns></returns>
    public T ExecFirst<T>()
        => ExecFirst<T>(null, null);
    #endregion

    #region ExecMulti
    /// <summary>
    /// 执行SQL语句并返回实体对象(T)枚举集合
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <param name="tm">数据库事务管理对象</param>
    /// <param name="rowMapper">IDataReader对象到实体对象的映射方法，如果为null，则使用接口IRowMapper(T)或ColumnMapperAttribute定义的元数据通过反射获取实体对象</param>
    /// <returns></returns>
    public IEnumerable<T> ExecMulti<T>(Func<IDataReader, T> rowMapper, TransactionManager tm)
        => ExecReader(tm).MapToMulti(rowMapper);

    /// <summary>
    /// 执行SQL语句并返回实体对象(T)枚举集合
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public IEnumerable<T> ExecMulti<T>(TransactionManager tm)
        => ExecMulti<T>(null, tm);

    /// <summary>
    /// 执行SQL语句并返回实体对象(T)枚举集合
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <param name="rowMapper">IDataReader对象到实体对象的映射方法，如果为null，则使用接口IRowMapper(T)或ColumnMapperAttribute定义的元数据通过反射获取实体对象</param>
    /// <returns></returns>
    public IEnumerable<T> ExecMulti<T>(Func<IDataReader, T> rowMapper)
        => ExecMulti(rowMapper, null);

    /// <summary>
    /// 执行SQL语句并返回实体对象(T)枚举集合
    /// 使用接口IRowMapper(T)或ColumnMapperAttribute定义的元数据通过反射获取实体对象
    /// </summary>
    /// <returns></returns>
    public IEnumerable<T> ExecMulti<T>()
        => ExecMulti<T>(null, null);

    /// <summary>
    /// 执行SQL语句并返回实体对象(T)枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mode"></param>
    /// <param name="tm"></param>
    /// <returns></returns>
    public IEnumerable<T> ExecMulti<T>(DataMappingMode mode, TransactionManager tm)
        => ExecMulti(DbHelper.GetRowMapper<T>(mode), tm);
        
    /// <summary>
    /// 执行SQL语句并返回实体对象(T)枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mode"></param>
    /// <returns></returns>
    public IEnumerable<T> ExecMulti<T>(DataMappingMode mode)
        => ExecMulti(DbHelper.GetRowMapper<T>(mode), null);
    #endregion

    #region ExecList
    /// <summary>
    /// 执行SQL语句并返回实体对象(T)列表集合
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <param name="tm">数据库事务管理对象</param>
    /// <param name="rowMapper">IDataReader对象到实体对象的映射方法，如果为null，则使用接口IRowMapper(T)或ColumnMapperAttribute定义的元数据通过反射获取实体对象</param>
    /// <returns></returns>
    public List<T> ExecList<T>(Func<IDataReader, T> rowMapper, TransactionManager tm)
        => ExecReader(tm).MapToList(rowMapper);

    /// <summary>
    /// 执行SQL语句并返回实体对象(T)列表集合
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    public List<T> ExecList<T>(TransactionManager tm)
        => ExecList<T>(null, tm);

    /// <summary>
    /// 执行SQL语句并返回实体对象(T)列表集合
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <param name="rowMapper">IDataReader对象到实体对象的映射方法，如果为null，则使用接口IRowMapper(T)或ColumnMapperAttribute定义的元数据通过反射获取实体对象</param>
    /// <returns></returns>
    public List<T> ExecList<T>(Func<IDataReader, T> rowMapper)
        => ExecList(rowMapper, null);

    /// <summary>
    /// 执行SQL语句并返回实体对象(T)列表集合
    /// 使用接口IRowMapper(T)或ColumnMapperAttribute定义的元数据通过反射获取实体对象
    /// </summary>
    /// <returns></returns>
    public List<T> ExecList<T>()
        => ExecList<T>(null, null);

    /// <summary>
    /// 执行SQL语句并返回实体对象(T)列表集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mode"></param>
    /// <param name="tm"></param>
    /// <returns></returns>
    public List<T> ExecList<T>(DataMappingMode mode, TransactionManager tm)
        => ExecList(DbHelper.GetRowMapper<T>(mode), tm);

    /// <summary>
    /// 执行SQL语句并返回实体对象(T)列表集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mode"></param>
    /// <returns></returns>
    public List<T> ExecList<T>(DataMappingMode mode)
        => ExecList(DbHelper.GetRowMapper<T>(mode), null);
    #endregion
}