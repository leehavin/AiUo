using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace AiUo.Data;

/// <summary>
/// 数据操作抽象接口
/// </summary>
public interface IDao : IDisposable
{
    #region Fields
    /// <summary>
    /// 获取DbCommand封装对象
    /// </summary>
    CommandWrapper Command { get; }

    /// <summary>
    /// 获取或设置在终止执行命令的尝试并生成错误之前的等待时间，单位秒。默认30秒，0表示不限制时间
    /// </summary>
    int CommandTimeout { get; set; }

    /// <summary>
    /// 获取数据库访问对象
    /// </summary>
    Database Database { get; }

    /// <summary>
    /// 数据提供程序类型
    /// </summary>
    DbDataProvider Provider { get; }
    #endregion

    #region Manager Parameter
    /// <summary>
    /// 添加双向DbParameter参数
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="value">参数值</param>
    /// <param name="dbType">参数数据类型</param>
    /// <param name="size">参数大小</param>
    /// <returns></returns>
    DaoBase AddInOutParameter(string name, object value = null, DbType dbType = DbType.AnsiString, int size = 0);
    /// <summary>
    /// 添加输入DbParameter参数和值
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="value">参数值</param>
    /// <param name="dbType">参数数据类型</param>
    /// <param name="size">参数大小</param>
    /// <returns></returns>
    DaoBase AddInParameter(string name, object value = null, DbType dbType = DbType.AnsiString, int size = 0);
    /// <summary>
    /// 自动添加存储过程的输入输出参数，并给输入参数按顺序赋值
    /// </summary>
    /// <param name="values">参数值集合</param>
    /// <returns></returns>
    DaoBase AddInParameters(params object[] values);
    /// <summary>
    /// 添加输出DbParameter参数
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="dbType">参数数据类型</param>
    /// <param name="size">参数大小</param>
    /// <returns></returns>
    DaoBase AddOutParameter(string name, DbType dbType = DbType.AnsiString, int size = 0);
    /// <summary>
    /// 添加DbParameter参数
    /// </summary>
    /// <param name="param">参数</param>
    /// <returns></returns>
    DaoBase AddParameter(DbParameter param);
    /// <summary>
    /// 添加DbParameter参数
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="direction">参数类型</param>
    /// <param name="dbType">参数数据类型</param>
    /// <param name="size">参数大小</param>
    /// <returns></returns>
    DaoBase AddParameter(string name, ParameterDirection direction = ParameterDirection.Input, DbType dbType = DbType.AnsiString, int size = 0);
    /// <summary>
    /// 添加DbParameter参数
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="direction">参数类型</param>
    /// <param name="value">参数值</param>
    /// <param name="dbType">参数数据类型</param>
    /// <param name="size">参数大小</param>
    /// <returns></returns>
    DaoBase AddParameter(string name, ParameterDirection direction, object value, DbType dbType, int size);
    /// <summary>
    /// 添加DbParameter参数
    /// </summary>
    /// <param name="paras">参数集合</param>
    /// <returns></returns>
    DaoBase AddParameters(IEnumerable<DbParameter> paras);
    /// <summary>
    /// 添加DbParameter参数
    /// </summary>
    /// <param name="paras">参数集合</param>
    /// <returns></returns>
    DaoBase AddParameters(params DbParameter[] paras);
    /// <summary>
    /// 清除所有定义的参数集合
    /// </summary>
    /// <returns></returns>
    DaoBase ClearParameters();
    /// <summary>
    /// 创建DbParameter
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="direction">参数类型</param>
    /// <param name="value">参数值</param>
    /// <param name="dbType">参数数据类型</param>
    /// <param name="size">参数大小</param>
    /// <returns></returns>
    DbParameter CreateParameter(string name, ParameterDirection direction = ParameterDirection.Input, object value = null, DbType dbType = DbType.AnsiString, int size = 0);
    #endregion

    /// <summary>
    /// 释放所使用的所有资源
    /// </summary>
    void Close();

    #region Execute
    /// <summary>
    /// 返回首行记录，没有记录则返回NULL
    /// </summary>
    /// <param name="tm">数据库事务管理对象</param>
    /// <returns></returns>
    DataRow ExecFirst(TransactionManager tm = null);
    /// <summary>
    /// 执行SQL语句并返回首行实体对象(T)
    /// 使用ColumnMapperAttribute定义的元数据进行反射获取实体对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T ExecFirst<T>();
    /// <summary>
    /// 执行SQL语句并返回首行实体对象(T)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rowMapper"></param>
    /// <returns></returns>
    T ExecFirst<T>(Func<IDataReader, T> rowMapper);
    /// <summary>
    /// 执行SQL语句并返回首行实体对象(T)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rowMapper"></param>
    /// <param name="tm"></param>
    /// <returns></returns>
    T ExecFirst<T>(Func<IDataReader, T> rowMapper, TransactionManager tm);
    /// <summary>
    /// 执行SQL语句并返回首行实体对象(T)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tm"></param>
    /// <returns></returns>
    T ExecFirst<T>(TransactionManager tm);
    /// <summary>
    /// 执行SQL语句并返回实体对象(T)列表集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    List<T> ExecList<T>();
    /// <summary>
    /// 执行SQL语句并返回实体对象(T)列表集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mode"></param>
    /// <returns></returns>
    List<T> ExecList<T>(DataMappingMode mode);
    /// <summary>
    /// 执行SQL语句并返回实体对象(T)列表集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mode"></param>
    /// <param name="tm"></param>
    /// <returns></returns>
    List<T> ExecList<T>(DataMappingMode mode, TransactionManager tm);
    /// <summary>
    /// 执行SQL语句并返回实体对象(T)列表集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rowMapper"></param>
    /// <returns></returns>
    List<T> ExecList<T>(Func<IDataReader, T> rowMapper);
    /// <summary>
    /// 执行SQL语句并返回实体对象(T)列表集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rowMapper"></param>
    /// <param name="tm"></param>
    /// <returns></returns>
    List<T> ExecList<T>(Func<IDataReader, T> rowMapper, TransactionManager tm);
    /// <summary>
    /// 执行SQL语句并返回实体对象(T)列表集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tm"></param>
    /// <returns></returns>
    List<T> ExecList<T>(TransactionManager tm);
    /// <summary>
    /// 执行SQL语句并返回实体对象(T)枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IEnumerable<T> ExecMulti<T>();
    /// <summary>
    /// 执行SQL语句并返回实体对象(T)枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mode"></param>
    /// <returns></returns>
    IEnumerable<T> ExecMulti<T>(DataMappingMode mode);
    /// <summary>
    /// 执行SQL语句并返回实体对象(T)枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mode"></param>
    /// <param name="tm"></param>
    /// <returns></returns>
    IEnumerable<T> ExecMulti<T>(DataMappingMode mode, TransactionManager tm);
    /// <summary>
    /// 执行SQL语句并返回实体对象(T)枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rowMapper"></param>
    /// <returns></returns>
    IEnumerable<T> ExecMulti<T>(Func<IDataReader, T> rowMapper);
    /// <summary>
    /// 执行SQL语句并返回实体对象(T)枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rowMapper"></param>
    /// <param name="tm"></param>
    /// <returns></returns>
    IEnumerable<T> ExecMulti<T>(Func<IDataReader, T> rowMapper, TransactionManager tm);
    /// <summary>
    /// 执行SQL语句并返回实体对象(T)枚举集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tm"></param>
    /// <returns></returns>
    IEnumerable<T> ExecMulti<T>(TransactionManager tm);
    /// <summary>
    /// 执行SQL语句并返回受影响的行数
    /// </summary>
    /// <param name="tm"></param>
    /// <returns></returns>
    int ExecNonQuery(TransactionManager tm = null);
    Task<int> ExecNonQueryAsync(TransactionManager tm = null);
    /// <summary>
    /// 执行SQL语句并返回结果集
    /// </summary>
    /// <param name="tm"></param>
    /// <returns></returns>
    DataReaderWrapper ExecReader(TransactionManager tm = null);
    Task<DataReaderWrapper> ExecReaderAsync(TransactionManager tm = null);
    /// <summary>
    /// 执行查询语句并返回首行首列数据
    /// </summary>
    /// <param name="tm"></param>
    /// <returns></returns>
    object ExecScalar(TransactionManager tm = null);
    Task<object> ExecScalarAsync(TransactionManager tm = null);
    /// <summary>
    /// 执行查询语句并返回首行首列数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tm"></param>
    /// <returns></returns>
    T ExecScalar<T>(TransactionManager tm = null);
    Task<T> ExecScalarAsync<T>(TransactionManager tm = null);
    /// <summary>
    /// 执行查询语句并返回首行首列数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="converter"></param>
    /// <returns></returns>
    T ExecScalar<T>(Func<object, T> converter);
    Task<T> ExecScalarAsync<T>(Func<object, T> converter);
    /// <summary>
    /// 执行查询语句并返回首行首列数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tm"></param>
    /// <param name="converter"></param>
    /// <returns></returns>
    T ExecScalar<T>(TransactionManager tm, Func<object, T> converter);
    Task<T> ExecScalarAsync<T>(TransactionManager tm, Func<object, T> converter);
    /// <summary>
    /// 返回唯一一行记录，没有记录则返回NULL，如果存在多行抛出异常
    /// </summary>
    /// <param name="tm"></param>
    /// <returns></returns>
    DataRow ExecSingle(TransactionManager tm = null);
    /// <summary>
    /// 执行SQL语句并返回唯一实体对象(T)，如果没有值返回 T 类型的默认值。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T ExecSingle<T>();
    /// <summary>
    /// 执行SQL语句并返回唯一实体对象(T)，如果没有值返回 T 类型的默认值。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rowMapper"></param>
    /// <returns></returns>
    T ExecSingle<T>(Func<IDataReader, T> rowMapper);
    /// <summary>
    /// 执行SQL语句并返回唯一实体对象(T)，如果没有值返回 T 类型的默认值。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rowMapper"></param>
    /// <param name="tm"></param>
    /// <returns></returns>
    T ExecSingle<T>(Func<IDataReader, T> rowMapper, TransactionManager tm);
    /// <summary>
    /// 执行SQL语句并返回唯一实体对象(T)，如果没有值返回 T 类型的默认值。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tm"></param>
    /// <returns></returns>
    T ExecSingle<T>(TransactionManager tm);
    /// <summary>
    /// 执行SQL语句并返回DataTable
    /// </summary>
    /// <param name="tm"></param>
    /// <returns></returns>
    DataTable ExecTable(TransactionManager tm = null);
    Task<DataTable> ExecTableAsync(TransactionManager tm = null);
    #endregion

    /// <summary>
    /// 获取输出参数的值，如果只存在一个输出参数不用传入参数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="index"></param>
    /// <returns></returns>
    T GetOutParameterValue<T>(int index = 0);
    /// <summary>
    /// 获取DbParameter参数的值，可通过此方法获取输出参数的值
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    object GetParameterValue(int index);
    /// <summary>
    /// 获取DbParameter参数的值，可通过此方法获取输出参数的值
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    object GetParameterValue(string name);
    /// <summary>
    /// 获取DbParameter参数的值，可通过此方法获取输出参数的值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="index"></param>
    /// <returns></returns>
    T GetParameterValue<T>(int index);
    /// <summary>
    /// 获取DbParameter参数的值，可通过此方法获取输出参数的值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    T GetParameterValue<T>(string name);
    /// <summary>
    /// 设置DbParameter参数的值
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    DaoBase SetParameterValue(string name, object value);
}