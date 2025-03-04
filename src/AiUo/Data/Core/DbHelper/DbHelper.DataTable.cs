using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Reflection;
using AiUo.Data.DataMapping;

namespace AiUo.Data;

public static partial class DbHelper
{
    #region AddRow
    /// <summary>
    /// 将 System.Data.DataRow 集合复制到 System.Data.DataTable 中，保留任何属性设置以及初始值和当前值。
    /// 源DataTable与目标DataTable的表结构必须相同。
    /// </summary>
    /// <param name="src"></param>
    /// <param name="rows">要导入的 System.Data.DataRow 集合</param>
    public static void ImportRows(this DataTable src, DataRowCollection rows)
    {
        foreach (DataRow row in rows)
        {
            src.ImportRow(row);
        }
    }

    /// <summary>
    /// 添加行数据，内部调用src.NewRow()后按顺序赋值
    /// </summary>
    /// <param name="src"></param>
    /// <param name="values"></param>
    public static void AddRow(this DataTable src, params object[] values)
    {
        if (src.Columns.Count != values.Length)
            throw new Exception("行数据个数必须等于DataTable的字段数。DataTable的字段数："+src.Columns.Count);
        var newRow = src.NewRow();
        for (int i = 0; i < values.Length; i++)
        {
            var value = values[i];
            newRow[i] = value ?? DBNull.Value;
        }
        src.Rows.Add(newRow);
    }

    /// <summary>
    /// 添加DataRow集合，DataRow必须是此DataTable调用NewRow()创建的
    /// </summary>
    /// <param name="src"></param>
    /// <param name="rows"></param>
    public static void Add(this DataRowCollection src, IEnumerable<DataRow> rows)
    {
        foreach (var row in rows)
        {
            src.Add(row);
        }
    }
    #endregion

    #region SetValue
    /// <summary>
    /// 设置存储在指定的 System.Data.DataColumn 中的数据，等同SetField(T)()。支持Nullable类型数据
    /// </summary>
    /// <param name="row">数据行</param>
    /// <param name="columnName">列名</param>
    /// <param name="value"></param>
    public static void SetValue(this DataRow row, string columnName, object value)
        => row[columnName] = value ?? DBNull.Value;

    /// <summary>
    /// 设置存储在指定的 System.Data.DataColumn 中的数据，等同SetField(T)()。支持Nullable类型数据
    /// </summary>
    /// <param name="row">数据行</param>
    /// <param name="columnIndex">列索引</param>
    /// <param name="value"></param>
    public static void SetValue(this DataRow row, int columnIndex, object value)
        => row[columnIndex] = value ?? DBNull.Value;

    /// <summary>
    /// 将旧行中列名相同的数据赋值到新行中
    /// </summary>
    /// <param name="newRow">新行</param>
    /// <param name="oldRow">旧行数据</param>
    public static void SetValues(this DataRow newRow, DataRow oldRow)
    {
        foreach (DataColumn col in oldRow.Table.Columns)
        {
            string colName = col.ColumnName;
            if (newRow.Table.Columns.Contains(colName))
                newRow[colName] = oldRow[colName];
        }
    }
        
    /// <summary>
    /// 设置DataRow中的值
    /// </summary>
    /// <param name="row"></param>
    /// <param name="values"></param>
    public static void SetValues(this DataRow row, params object[] values)
    {
        int colNum = row.Table.Columns.Count;
        if (colNum < values.Length) throw new Exception("DataRow的列数小于提供的字段值的数量。");
        for (int i = 0; i < colNum; i++)
        {
            row[i] = values[i];
        }
    }

    /// <summary>
    /// 将旧行中列名相同的数据赋值到新行中
    /// </summary>
    /// <param name="newRow">新行</param>
    /// <param name="reader">旧行数据</param>
    public static void SetValues(this DataRow newRow, DbDataReader reader)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            string colName = reader.GetName(i);
            if (newRow.Table.Columns.Contains(colName))
                newRow[colName] = reader[colName];
        }
    }
    #endregion

    #region Entities|IDataReader|Array ==> DataTable
    /// <summary>
    /// 将实体对象集合加载到DataTable中，通过列名与属性名的对应
    /// Entities => DataTable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="src"></param>
    /// <param name="items"></param>
    public static void Load<T>(this DataTable src, IEnumerable<T> items)
    {
        Action<DataRow, T> mapper = EntityToDataRowMapping.GetEntityMapper<T>();
        foreach (var item in items)
        {
            DataRow newRow = src.NewRow();
            mapper(newRow, item);
            src.Rows.Add(newRow);
        }
    }

    /// <summary>
    /// 将实体对象转换为DataTable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static DataTable ToTable<T>(this IEnumerable<T> items)
        where T : class
    {
        DataTable ret = new DataTable();
        var properties = (from p in typeof(T).GetProperties()
            select p).ToArray<PropertyInfo>();
        foreach (var property in properties)
        {
            ret.Columns.Add(property.Name, property.PropertyType);
        }
        ret.Load<T>(items);
        return ret;
    }

    /// <summary>
    /// 从IDataReader对象填充成DataTable对象
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static DataTable ToTable(this IDataReader reader)
    {
        DataTable ret = new DataTable();
        try
        {
            // 添加表的数据列
            for (int i = 0; i < reader.FieldCount; i++)
            {
                DataColumn column = new DataColumn
                {
                    DataType = reader.GetFieldType(i),
                    ColumnName = reader.GetName(i)
                };
                ret.Columns.Add(column);
            }
            // 添加数据行
            while (reader.Read())
            {
                DataRow row = ret.NewRow();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[i] = reader[i];
                ret.Rows.Add(row);
            }
            //ret.Load(reader);
        }
        finally
        {
            reader.Close();
        }
        return ret;
    }

    /// <summary>
    /// 将二维数组转换成DataTable
    /// </summary>
    /// <param name="data">二维数据</param>
    /// <param name="columnNames">列名定义</param>
    /// <returns></returns>
    public static DataTable ArrayToTable(object[,] data, params string[] columnNames)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));
        int rowCount = data.GetLength(0);
        int columnCount = data.GetLength(1);
        DataTable ret = new DataTable();
        if (columnNames == null)
        {
            for (int i = 0; i < columnCount; i++)
            {
                ret.Columns.Add(i.ToString(), typeof(string));
            }
        }
        else
        {
            if (columnNames.Length != columnCount)
                throw new Exception(string.Format("定义的列数{0}和数据的列数{1}不相等。", columnNames.Length, columnCount));
            foreach (string name in columnNames)
            {
                ret.Columns.Add(name, typeof(string));
            }
        }
        for (int j = 0; j < rowCount; j++)
        {
            DataRow row = ret.NewRow();
            for (int k = 0; k < columnCount; k++)
            {
                row[k] = data[j, k];
            }
            ret.Rows.Add(row);
        }
        return ret;
    }
    #endregion

    #region DataTable ==> Entity
    /// <summary>
    /// 获取 DataTable 的首行映射的实体对象(T)
    /// </summary>
    /// <typeparam name="T">DataTable 对象映射的实体对象类型</typeparam>
    /// <param name="table">DataTable 对象</param>
    /// <param name="rowMapper">IDataReader 对象到实体对象的映射方法，如果为null，则使用接口IRowMapper(T)或ColumnMapperAttribute定义的元数据通过反射获取实体对象</param>
    /// <returns></returns>
    public static T MapToFirst<T>(this DataTable table, Func<IDataReader, T> rowMapper = null)
        => MapToFirst<T>(table.CreateDataReader(), rowMapper);

    /// <summary>
    /// 获取 DataTable 的首行映射的实体对象(T)
    /// </summary>
    /// <typeparam name="T">DataTable 对象映射的实体对象类型</typeparam>
    /// <param name="table">DataTable 对象</param>
    /// <param name="mode">行映射模式</param>
    /// <returns></returns>
    public static T MapToFirst<T>(this DataTable table, DataMappingMode mode)
        => MapToFirst<T>(table.CreateDataReader(), mode);
    /// <summary>
    /// 将DataTable中的单行单列值映射到简单数据类型（T），不唯一抛出异常
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="table"></param>
    /// <param name="rowMapper"></param>
    /// <returns></returns>
    public static T MapToSingle<T>(this DataTable table, Func<IDataReader, T> rowMapper = null)
        => MapToSingle<T>(table.CreateDataReader(), rowMapper);
    /// <summary>
    /// 将DataTable中的单行单列值映射到简单数据类型（T），不唯一抛出异常
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="table"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static T MapToSingle<T>(this DataTable table, DataMappingMode mode)
        => MapToSingle<T>(table.CreateDataReader(), mode);
    /// <summary>
    /// 将DataTable中的单行单列值映射到简单数据类型（T），不唯一抛出异常
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="table"></param>
    /// <param name="rowMapper"></param>
    /// <returns></returns>
    public static IEnumerable<T> MapToMulti<T>(this DataTable table, Func<IDataReader, T> rowMapper = null)
        => MapToMulti<T>(table.CreateDataReader(), rowMapper);
    /// <summary>
    /// 将DataTable中的单行单列值映射到简单数据类型（T），不唯一抛出异常
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="table"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static IEnumerable<T> MapToMulti<T>(this DataTable table, DataMappingMode mode)
        => MapToMulti<T>(table.CreateDataReader(), mode);
    /// <summary>
    /// 获取 DataTable 对象映射的实体对象(T)集合
    /// </summary>
    /// <typeparam name="T">DataTable 对象映射的实体对象类型</typeparam>
    /// <param name="table">DataTable 对象</param>
    /// <param name="rowMapper">IDataReader 对象到实体对象的映射方法，如果为null，则使用接口IRowMapper(T)或ColumnMapperAttribute定义的元数据通过反射获取实体对象</param>
    /// <returns></returns>
    public static List<T> MapToList<T>(this DataTable table, Func<IDataReader, T> rowMapper = null)
        => MapToList<T>(table.CreateDataReader(), rowMapper);

    /// <summary>
    /// 获取 DataTable 对象映射的实体对象(T)集合
    /// </summary>
    /// <typeparam name="T">DataTable 对象映射的实体对象类型</typeparam>
    /// <param name="table">DataTable 对象</param>
    /// <param name="mode">行映射模式</param>
    /// <returns></returns>
    public static List<T> MapToList<T>(this DataTable table, DataMappingMode mode)
        => MapToList<T>(table.CreateDataReader(), GetRowMapper<T>(mode));
    #endregion
}