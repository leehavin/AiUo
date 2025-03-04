using System;
using System.Data;

namespace AiUo.Data.DataMapping;

/// <summary>
/// 数据映射辅助类。Entity 与 DataReader/DataTable 互转
/// </summary>
public static class DataMappingUtil
{
    /// <summary>
    /// 获取 IDataReader ==> Entity 的映射方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static Func<IDataReader, T> GetRowMapper<T>(DataMappingMode mode)
    {
        Func<IDataReader, T> ret = null;
        switch (mode)
        {
            case DataMappingMode.Interface:
                ret = InterfaceDataMapping.GetRowMapper<T>();
                break;
            case DataMappingMode.Attribute:
            case DataMappingMode.Reflection:
                ret = ReflectionDataMapping.GetRowMapper<T>();
                break;
            case DataMappingMode.PrimitiveType:
                ret = (reader) => { return AiUoUtil.ConvertTo<T>(reader[0]); };
                break;
        }
        if (ret == null)
            throw new Exception($"无法根据指定的DataMappingMode {mode} 获得数据映射方法。目标转换类型 {typeof(T).FullName}");
        return ret;
    }

    /// <summary>
    /// 获取 Entity ==> DataRow 的映射方法
    /// TODO: 应增加接口，Attribute的映射实现
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static Action<DataRow, T> GetEntityMapper<T>(DataMappingMode mode)
    {
        return EntityToDataRowMapping.GetEntityMapper<T>();
    }
}