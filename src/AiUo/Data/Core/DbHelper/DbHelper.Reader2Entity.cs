using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using AiUo.Data.DataMapping;
using AiUo.Reflection;

namespace AiUo.Data;

public static partial class DbHelper
{
    #region DataReader ==> Entity 映射
    /// <summary>
    /// 获得 IDataReader ==> Entity 的映射方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static Func<IDataReader, T> GetRowMapper<T>(DataMappingMode mode)
        => DataMappingUtil.GetRowMapper<T>(mode);
    private static Func<IDataReader, T> GetRowMapper<T>(Func<IDataReader, T> rowMapper)
    {
        var ret = rowMapper;
        if (ret == null)
        {
            // 考虑到性能：如果是单列的映射基元类型，必须使用RowMapperMode.PrimitiveType映射
            if (ReflectionUtil.IsSimpleType(typeof(T)))
                return DataMappingUtil.GetRowMapper<T>(DataMappingMode.PrimitiveType);
            // IRowMapper<T> 接口
            ret = InterfaceDataMapping.GetRowMapper<T>();
            // DataColumnMapperAttribute 或者 反射属性
            if (ret == null)
                ret = ReflectionDataMapping.GetRowMapper<T>();
        }
        if (ret == null)
            throw new ArgumentNullException("rowMapper", "rowMapper如果为null,则必T必须继承IRowMapper(T)或使用ColumnMapperAttribute定义。");
        return ret;
    }

    /// <summary>
    /// 映射IDataReader对象中唯一记录（可以没有记录）到实体对象(T)，但必须支持null到AiUoUtil.ConvertTo(T)的转换
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="rowMapper">IDataReader对象到实体对象的映射方法，如果为null，则使用接口IRowMapper(T)或ColumnMapperAttribute定义的元数据通过反射获取实体对象</param>
    /// <returns></returns>
    public static T MapToSingle<T>(this IDataReader reader, Func<IDataReader, T> rowMapper = null)
    {
        rowMapper = GetRowMapper(rowMapper);

        T ret = default;
        try
        {
            if (reader.Read())
            {
                ret = rowMapper(reader);
                if (reader.Read())
                    throw new ArgumentException("IDataReader记录不唯一。", "reader");
            }
            else
                ret = AiUoUtil.ConvertTo<T>(null);
        }
        finally
        {
            reader.Close();
        }
        return ret;
    }

    /// <summary>
    /// 获取唯一的IDataReader对象映射的实体对象(T)
    /// （IDataReader对象中有且只能有一条记录）
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="mode">行映射模式</param>
    /// <returns></returns>
    public static T MapToSingle<T>(this IDataReader reader, DataMappingMode mode)
        => MapToSingle<T>(reader, GetRowMapper<T>(mode));

    /// <summary>
    /// 获取IDataReader对象映射的实体对象(T)集合
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="rowMapper">IDataReader对象到实体对象的映射方法，如果为null，则使用接口IRowMapper(T)或ColumnMapperAttribute定义的元数据通过反射获取实体对象</param>
    /// <returns></returns>
    public static IEnumerable<T> MapToMulti<T>(this IDataReader reader, Func<IDataReader, T> rowMapper = null)
    {
        rowMapper = GetRowMapper(rowMapper);
        try
        {
            while (reader.Read())
            {
                yield return rowMapper(reader);
            }
        }
        finally
        {
            reader.Close();
        }
    }

    /// <summary>
    /// 获取IDataReader对象映射的实体对象(T)集合
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="mode">行映射模式</param>
    /// <returns></returns>
    public static IEnumerable<T> MapToMulti<T>(this IDataReader reader, DataMappingMode mode)
        => MapToMulti<T>(reader, GetRowMapper<T>(mode));

    /// <summary>
    /// 获取IDataReader对象映射的实体对象(T)集合
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="rowMapper">IDataReader对象到实体对象的映射方法，如果为null，则使用接口IRowMapper(T)或ColumnMapperAttribute定义的元数据通过反射获取实体对象</param>
    /// <returns></returns>
    public static List<T> MapToList<T>(this IDataReader reader, Func<IDataReader, T> rowMapper = null)
        => MapToMulti<T>(reader, rowMapper).ToList<T>();

    /// <summary>
    /// 获取IDataReader对象映射的实体对象(T)集合
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="mode">行映射模式</param>
    /// <returns></returns>
    public static List<T> MapToList<T>(this IDataReader reader, DataMappingMode mode)
        => MapToList<T>(reader, GetRowMapper<T>(mode));
    /// <summary>
    /// 获取IDataReader对象映射的首行实体对象(T)
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="rowMapper">IDataReader对象到实体对象的映射方法，如果为null，则使用接口IRowMapper(T)或ColumnMapperAttribute定义的元数据通过反射获取实体对象</param>
    /// <returns></returns>
    public static T MapToFirst<T>(this IDataReader reader, Func<IDataReader, T> rowMapper = null)
        => MapToMulti<T>(reader, rowMapper).First();
    /// <summary>
    /// 获取IDataReader对象映射的首行实体对象(T)
    /// </summary>
    /// <typeparam name="T">IDataReader对象映射的实体对象类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="mode">行映射模式</param>
    /// <returns></returns>
    public static T MapToFirst<T>(this IDataReader reader, DataMappingMode mode)
        => MapToMulti<T>(reader, mode).First();
    #endregion
}