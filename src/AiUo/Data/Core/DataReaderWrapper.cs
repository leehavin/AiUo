using System;
using System.Data.Common;
using System.Data;
using System.Collections;
using System.ComponentModel;
using AiUo.Logging;

namespace AiUo.Data;

/// <summary>
/// DbDataReader封装
/// </summary>
public class DataReaderWrapper : IDataReader, IDisposable, IDataRecord, IEnumerable
{
    /// <summary>
    /// 内部DbDataReader对象
    /// </summary>
    public DbDataReader Reader { get; set; }
    /// <summary>
    /// 内部CommandWrapper对象
    /// </summary>
    public CommandWrapper Command { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="command">Command对象</param>
    /// <param name="reader">DataReader对象</param>
    public DataReaderWrapper(CommandWrapper command, DbDataReader reader)
    {
        Command = command;
        Reader = reader;
    }

    /// <summary>
    /// 获取DbDataReader的具体类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetReader<T>() where T : DbDataReader
    {
        return Reader as T;
    }

    #region IDisposable
    /// <summary>
    /// 析构函数
    /// </summary>
    ~DataReaderWrapper()
    {
        Command?.Dispose();
        string msg = "CommandWrapper不应进入析构函数，请检查并释放资源。{transaction} connection:{connectionState} commandText:{commandText}";
        LogUtil.Error(msg
            , Command?.Transaction != null ? "Transaction对象未Commit或Rollback" : string.Empty
            , Command?.Connection?.State == ConnectionState.Closed ? "已关闭" : "未关闭"
            , Command?.CommandText
        );
    }

    /// <summary>
    /// 如果存在事务，并不会关闭连接
    /// </summary>
    public void Dispose()
    {
        Reader.Close(); //关闭Reader,是否关闭Connection根据是否设置了CommandBehavior.CloseConnection
        Command.Dispose(); // 有事务时不会关闭Connection
        GC.SuppressFinalize(this); // 不执行析构函数，交给事务对象监测释放情况
    }
    /// <summary>
    /// 如果存在事务，并不会关闭连接
    /// </summary>
    public void Close()
    {
        Dispose();
    }

    /// <summary>
    /// 将读取器前进到结果集中的下一个记录。
    /// </summary>
    /// <returns></returns>
    public bool Read()
    {
        bool ret = Reader.Read();
        if (!ret)
        {
            Dispose();
        }
        return ret;
    }
    #endregion

    #region DbDataReader
    /// <summary>
    /// 获取一个值，该值指示当前行的嵌套深度。
    /// </summary>
    public int Depth => Reader.Depth; 
        
    /// <summary>
    /// 获取当前行中的列数。
    /// </summary>
    public int FieldCount => Reader.FieldCount; 

    /// <summary>
    /// 获取一个值，它指示此 System.Data.Common.DbDataReader 是否包含一个或多个行。
    /// </summary>
    public bool HasRows => Reader.HasRows; 

    /// <summary>
    /// 获取一个值，该值指示 System.Data.Common.DbDataReader 是否已关闭。
    /// </summary>
    public bool IsClosed => Reader.IsClosed; 
        
    /// <summary>
    /// 通过执行 SQL 语句获取更改、插入或删除的行数。
    /// </summary>
    public int RecordsAffected => Reader.RecordsAffected; 

    /// <summary>
    /// 获取 System.Data.Common.DbDataReader 中未隐藏的字段的数目。
    /// </summary>
    public int VisibleFieldCount => Reader.VisibleFieldCount; 

    /// <summary>
    /// 获取指定列的作为 System.Object 的实例的值。
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    public object this[int ordinal] => Reader[ordinal]; 
        
    /// <summary>
    ///  获取指定列的作为 System.Object 的实例的值。
    /// </summary>
    /// <param name="name">列的名称。</param>
    /// <returns></returns>
    public object this[string name] => Reader[name]; 

    /// <summary>
    /// 获取指定列的布尔值形式的值。
    /// </summary>
    /// <param name="ordinal">从零开始的列序号。</param>
    /// <returns></returns>
    public bool GetBoolean(int ordinal) => Reader.GetBoolean(ordinal);

    /// <summary>
    /// 获取指定列的字节形式的值。
    /// </summary>
    /// <param name="ordinal">从零开始的列序号。</param>
    /// <returns></returns>
    public byte GetByte(int ordinal) => Reader.GetByte(ordinal); 

    /// <summary>
    /// 从指定列读取一个字节流（从 dataOffset 指示的位置开始），读到缓冲区中（从 bufferOffset 指示的位置开始）。
    /// </summary>
    /// <param name="ordinal">从零开始的列序号。</param>
    /// <param name="dataOffset">行中的索引，从其开始读取操作。</param>
    /// <param name="buffer">作为数据复制目标的缓冲区。</param>
    /// <param name="bufferOffset">具有作为数据复制目标的缓冲区的索引。</param>
    /// <param name="length">最多读取的字符数。</param>
    /// <returns></returns>
    public long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        => Reader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length); 

    /// <summary>
    /// 获取指定列的单个字符串形式的值。
    /// </summary>
    /// <param name="ordinal">从零开始的列序号。</param>
    /// <returns></returns>
    public char GetChar(int ordinal)
        => Reader.GetChar(ordinal); 

    /// <summary>
    /// 从指定列读取一个字符流，从 dataIndex 指示的位置开始，读到缓冲区中，从 bufferIndex 指示的位置开始。
    /// </summary>
    /// <param name="ordinal">从零开始的列序号。</param>
    /// <param name="dataOffset">行中的索引，从其开始读取操作。</param>
    /// <param name="buffer">作为数据复制目标的缓冲区。</param>
    /// <param name="bufferOffset">具有作为数据复制目标的缓冲区的索引。</param>
    /// <param name="length">最多读取的字符数。</param>
    /// <returns></returns>
    public long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        => Reader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length); 

    /// <summary>
    /// 返回被请求的列序号的 System.Data.Common.DbDataReader 对象。
    /// </summary>
    /// <param name="ordinal">从零开始的列序号。</param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public IDataReader GetData(int ordinal) => Reader.GetData(ordinal); 

    /// <summary>
    /// 获取指定列的数据类型的名称。
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    public string GetDataTypeName(int ordinal) => Reader.GetDataTypeName(ordinal); 

    /// <summary>
    /// 获取指定列的 System.DateTime 对象形式的值。
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    public DateTime GetDateTime(int ordinal) => Reader.GetDateTime(ordinal); 

    /// <summary>
    /// 获取指定列的 System.Decimal 对象形式的值。
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    public decimal GetDecimal(int ordinal) => Reader.GetDecimal(ordinal); 

    /// <summary>
    /// 获取指定列的双精度浮点数形式的值。
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    public double GetDouble(int ordinal) => Reader.GetDouble(ordinal); 

    /// <summary>
    /// 返回一个 System.Collections.IEnumerator，可用于循环访问数据读取器中的行。
    /// </summary>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public IEnumerator GetEnumerator() => Reader.GetEnumerator(); 

    /// <summary>
    /// 获取指定列的数据类型。
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    public Type GetFieldType(int ordinal) => Reader.GetFieldType(ordinal); 

    /// <summary>
    /// 获取指定列的单精度浮点数形式的值。
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    public float GetFloat(int ordinal) => Reader.GetFloat(ordinal); 

    /// <summary>
    /// 获取指定列的全局唯一标识符 (GUID) 形式的值。
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    public Guid GetGuid(int ordinal) => Reader.GetGuid(ordinal); 

    /// <summary>
    /// 获取指定列的 16 位带符号整数形式的值。
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    public short GetInt16(int ordinal) => Reader.GetInt16(ordinal); 

    /// <summary>
    /// 获取指定列的 32 位带符号整数形式的值。
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    public int GetInt32(int ordinal) => Reader.GetInt32(ordinal); 

    /// <summary>
    /// 获取指定列的 64 位带符号整数形式的值。
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    public long GetInt64(int ordinal) => Reader.GetInt64(ordinal); 

    /// <summary>
    /// 给定了从零开始的列序号时，获取列的名称。
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    public string GetName(int ordinal) => Reader.GetName(ordinal); 
        
    /// <summary>
    /// 给定列名称时，获取列序号。
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public int GetOrdinal(string name) => Reader.GetOrdinal(name); 
        
    /// <summary>
    /// 返回指定列的提供程序特定的字段类型。
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public virtual Type GetProviderSpecificFieldType(int ordinal)
        => Reader.GetProviderSpecificFieldType(ordinal); 
        
    /// <summary>
    /// 获取指定列的作为 System.Object 的实例的值。
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public virtual object GetProviderSpecificValue(int ordinal)
        => Reader.GetProviderSpecificValue(ordinal); 

    /// <summary>
    /// 获取集合中当前行的所有提供程序特定的属性列。
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public virtual int GetProviderSpecificValues(object[] values) => Reader.GetProviderSpecificValues(values); 
        
    /// <summary>
    /// 返回一个 System.Data.DataTable，它描述 System.Data.Common.DbDataReader 的列元数据。
    /// </summary>
    /// <returns></returns>
    public DataTable GetSchemaTable() => Reader.GetSchemaTable(); 
        
    /// <summary>
    /// 获取指定列的作为 System.String 的实例的值。
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    public string GetString(int ordinal) => Reader.GetString(ordinal); 

    /// <summary>
    /// 获取指定列的作为 System.Object 的实例的值。
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    public object GetValue(int ordinal) => Reader.GetValue(ordinal); 
        
    /// <summary>
    /// 使用当前行的列值来填充对象数组。
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public int GetValues(object[] values) => Reader.GetValues(values); 
        
    /// <summary>
    /// 获取一个值，该值指示列中是否包含不存在的或已丢失的值。
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    public bool IsDBNull(int ordinal) => Reader.IsDBNull(ordinal); 
        
    /// <summary>
    /// 读取批处理语句的结果时，使读取器前进到下一个结果。
    /// </summary>
    /// <returns></returns>
    public bool NextResult() => Reader.NextResult(); 
        
    #endregion
}