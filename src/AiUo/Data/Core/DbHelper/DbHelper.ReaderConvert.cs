using System;
using System.Data;

namespace AiUo.Data;

public static partial class DbHelper
{
    #region IDataReader字段值转换

    #region To<T>
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为指定类型，用到反射性能慢
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static T To<T>(this IDataReader reader, int i)
        => AiUoUtil.ConvertTo<T>(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为指定类型
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="converter">自定义转换器</param>
    /// <returns></returns>
    public static T To<T>(this IDataReader reader, int i, Func<object, T> converter)
        => converter(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为指定类型，如转换失败则使用默认值，用到反射性能慢
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static T To<T>(this IDataReader reader, int i, T defaultValue)
    {
        T ret = default(T);
        try
        {
            ret = AiUoUtil.ConvertTo<T>(reader[i]);
        }
        catch
        {
            ret = defaultValue;
        }
        return ret;
    }
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为指定类型，如转换失败则使用默认值
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <param name="converter">自定义转换器</param>
    /// <returns></returns>
    public static T To<T>(this IDataReader reader, int i, T defaultValue, Func<object, T> converter)
    {
        T ret = default(T);
        try
        {
            ret = converter(reader[i]);
        }
        catch
        {
            ret = defaultValue;
        }
        return ret;
    }

    /// <summary>
    /// 获取位于指定名称的列的值并转换为指定类型，用到反射性能慢
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static T To<T>(this IDataReader reader, string name)
        => AiUoUtil.ConvertTo<T>(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为指定类型
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="converter">自定义转换器</param>
    /// <returns></returns>
    public static T To<T>(this IDataReader reader, string name, Func<object, T> converter)
        => converter(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为指定类型，如转换失败则使用默认值，用到反射性能慢
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static T To<T>(this IDataReader reader, string name, T defaultValue)
    {
        T ret = default(T);
        try
        {
            ret = AiUoUtil.ConvertTo<T>(reader[name]);
        }
        catch
        {
            ret = defaultValue;
        }
        return ret;
    }

    /// <summary>
    /// 获取位于指定名称的列的值并转换为指定类型，如转换失败则使用默认值
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <param name="converter">自定义转换器</param>
    /// <returns></returns>
    public static T To<T>(this IDataReader reader, string name, T defaultValue, Func<object, T> converter)
    {
        T ret = default(T);
        try
        {
            ret = converter(reader[name]);
        }
        catch
        {
            ret = defaultValue;
        }
        return ret;
    }
    #endregion

    #region String
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为String
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static string ToString(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterR(Convert.ToString).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为String。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static string ToString(this IDataReader reader, int i, string defaultValue)
        => ConverterBuilder.GetConverterR(Convert.ToString, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为String
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static string ToString(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterR(Convert.ToString).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为String。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static string ToString(this IDataReader reader, string name, string defaultValue)
        => ConverterBuilder.GetConverterR(Convert.ToString, defaultValue).Invoke(reader[name]);

    #region StringFormat
    /// <summary>
    /// 将数据库值使用ToString()格式化转换
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="i"></param>
    /// <param name="format">格式化字符串</param>
    /// <returns></returns>
    public static string ToStringFormat(this IDataReader reader, int i, string format)
        => ToStringFormat(reader, reader.GetName(i), format);
    /// <summary>
    /// 将数据库值使用ToString()格式化转换
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="name"></param>
    /// <param name="format">格式化字符串</param>
    /// <returns></returns>
    public static string ToStringFormat(this IDataReader reader, string name, string format)
    {
        string ret = null;
        var value = reader[name];
        if (value == DBNull.Value || value == null)
            return null;
        var typeName = value.GetType().FullName;
        switch (typeName)
        {
            #region 简单类型转换
            case SimpleTypeNames.Byte:
                ret = ((byte)value).ToString(format);
                break;
            case SimpleTypeNames.SByte:
                ret = ((sbyte)value).ToString(format);
                break;
            case SimpleTypeNames.Int16:
                ret = ((short)value).ToString(format);
                break;
            case SimpleTypeNames.UInt16:
                ret = ((ushort)value).ToString(format);
                break;
            case SimpleTypeNames.Int32:
                ret = ((int)value).ToString(format);
                break;
            case SimpleTypeNames.UInt32:
                ret = ((uint)value).ToString(format);
                break;
            case SimpleTypeNames.Int64:
                ret = ((long)value).ToString(format);
                break;
            case SimpleTypeNames.UInt64:
                ret = ((ulong)value).ToString(format);
                break;
            case SimpleTypeNames.Single:
                ret = ((float)value).ToString(format);
                break;
            case SimpleTypeNames.Double:
                ret = ((double)value).ToString(format);
                break;
            case SimpleTypeNames.Decimal:
                ret = ((decimal)value).ToString(format);
                break;
            case SimpleTypeNames.DateTime:
                ret = ((DateTime)value).ToString(format);
                break;
            case SimpleTypeNames.TimeSpan:
                ret = ((TimeSpan)value).ToString(format);
                break;
            case SimpleTypeNames.DateTimeOffset:
                ret = ((DateTimeOffset)value).ToString(format);
                break;
            #endregion

            default:
                throw new Exception($"字符串格式化不支持此类型：{typeName}");
        }
        return ret;
    }
    /// <summary>
    /// 将数据库值使用ToString()格式化转换，值为空使用defaultValue
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="name"></param>
    /// <param name="format"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static string ToStringFormat(this IDataReader reader, string name, string format, string defaultValue)
    {
        var value = reader[name];
        if (value == DBNull.Value || value == null)
            return defaultValue;
        else
            return ToStringFormat(reader, name, format);
    }
    #endregion
    #endregion

    #region 数值类型

    #region Byte
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Byte
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static byte ToByte(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterV(Convert.ToByte).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Byte。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static byte ToByte(this IDataReader reader, int i, byte defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToByte, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Byte
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static byte ToByte(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterV(Convert.ToByte).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Byte。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static byte ToByte(this IDataReader reader, string name, byte defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToByte, defaultValue).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Byte
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static byte? ToByteN(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterN(Convert.ToByte).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Byte。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static byte? ToByteN(this IDataReader reader, int i, byte defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToByte, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Byte
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static byte? ToByteN(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterN(Convert.ToByte).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Byte。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static byte? ToByteN(this IDataReader reader, string name, byte defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToByte, defaultValue).Invoke(reader[name]);
    #endregion

    #region SByte
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为SByte
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static sbyte ToSByte(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterV(Convert.ToSByte).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为SByte。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static sbyte ToSByte(this IDataReader reader, int i, sbyte defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToSByte, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为SByte
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static sbyte ToSByte(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterV(Convert.ToSByte).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为SByte。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static sbyte ToSByte(this IDataReader reader, string name, sbyte defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToSByte, defaultValue).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Byte
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static sbyte? ToSByteN(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterN(Convert.ToSByte).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Byte。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static sbyte? ToSByteN(this IDataReader reader, int i, sbyte defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToSByte, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Byte
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static sbyte? ToSByteN(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterN(Convert.ToSByte).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Byte。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static sbyte? ToSByteN(this IDataReader reader, string name, sbyte defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToSByte, defaultValue).Invoke(reader[name]);
    #endregion

    #region Int16
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Int16
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static short ToInt16(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterV<short>(Convert.ToInt16).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Int16。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static short ToInt16(this IDataReader reader, int i, short defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToInt16, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Int16
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static short ToInt16(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterV(Convert.ToInt16).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Int16。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static short ToInt16(this IDataReader reader, string name, short defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToInt16, defaultValue).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Int16
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static short? ToInt16N(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterN(Convert.ToInt16).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Int16。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static short? ToInt16N(this IDataReader reader, int i, short defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToInt16, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Int16
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static short? ToInt16N(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterN(Convert.ToInt16).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Int16。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static short? ToInt16N(this IDataReader reader, string name, short defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToInt16, defaultValue).Invoke(reader[name]);
    #endregion

    #region UInt16
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为UInt16
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static ushort ToUInt16(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterV(Convert.ToUInt16).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为UInt16。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static ushort ToUInt16(this IDataReader reader, int i, ushort defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToUInt16, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为UInt16
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static ushort ToUInt16(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterV(Convert.ToUInt16).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为UInt16。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static ushort ToUInt16(this IDataReader reader, string name, ushort defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToUInt16, defaultValue).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空UInt16
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static ushort? ToUInt16N(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterN(Convert.ToUInt16).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空UInt16。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static ushort? ToUInt16N(this IDataReader reader, int i, ushort defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToUInt16, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空UInt16
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static ushort? ToUInt16N(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterN(Convert.ToUInt16).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空UInt16。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static ushort? ToUInt16N(this IDataReader reader, string name, ushort defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToUInt16, defaultValue).Invoke(reader[name]);
    #endregion

    #region Int32
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Int32
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static int ToInt32(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterV(Convert.ToInt32).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Int32。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static int ToInt32(this IDataReader reader, int i, int defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToInt32, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Int32
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static int ToInt32(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterV(Convert.ToInt32).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Int32。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static int ToInt32(this IDataReader reader, string name, int defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToInt32, defaultValue).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Int32
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static int? ToInt32N(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterN(Convert.ToInt32).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Int32。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static int? ToInt32N(this IDataReader reader, int i, int defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToInt32, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Int32
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static int? ToInt32N(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterN(Convert.ToInt32).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Int32。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static int? ToInt32N(this IDataReader reader, string name, int defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToInt32, defaultValue).Invoke(reader[name]);
    #endregion

    #region UInt32
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为UInt32
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static uint ToUInt32(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterV(Convert.ToUInt32).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为UInt32。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static uint ToUInt32(this IDataReader reader, int i, uint defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToUInt32, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为UInt32
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static uint ToUInt32(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterV(Convert.ToUInt32).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为UInt32。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static uint ToUInt32(this IDataReader reader, string name, uint defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToUInt32, defaultValue).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空UInt32
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static uint? ToUInt32N(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterN(Convert.ToUInt32).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空UInt32。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static uint? ToUInt32N(this IDataReader reader, int i, uint defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToUInt32, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空UInt32
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static uint? ToUInt32N(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterN(Convert.ToUInt32).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空UInt32。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static uint? ToUInt32N(this IDataReader reader, string name, uint defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToUInt32, defaultValue).Invoke(reader[name]);
    #endregion

    #region Int64
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Int64
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static long ToInt64(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterV(Convert.ToInt64).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Int64。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static long ToInt64(this IDataReader reader, int i, long defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToInt64, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Int64
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static long ToInt64(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterV(Convert.ToInt64).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Int64。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static long ToInt64(this IDataReader reader, string name, long defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToInt64, defaultValue).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Int64
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static long? ToInt64N(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterN(Convert.ToInt64).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Int64。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static long? ToInt64N(this IDataReader reader, int i, long defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToInt64, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Int64
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static long? ToInt64N(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterN(Convert.ToInt64).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Int64。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static long? ToInt64N(this IDataReader reader, string name, long defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToInt64, defaultValue).Invoke(reader[name]);
    #endregion

    #region UInt64
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为UInt64
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static ulong ToUInt64(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterV(Convert.ToUInt64).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为UInt64。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static ulong ToUInt64(this IDataReader reader, int i, ulong defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToUInt64, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为UInt64
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static ulong ToUInt64(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterV(Convert.ToUInt64).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为UInt64。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static ulong ToUInt64(this IDataReader reader, string name, ulong defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToUInt64, defaultValue).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空UInt64
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static ulong? ToUInt64N(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterN(Convert.ToUInt64).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空UInt64。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static ulong? ToUInt64N(this IDataReader reader, int i, ulong defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToUInt64, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空UInt64
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static ulong? ToUInt64N(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterN(Convert.ToUInt64).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空UInt64。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static ulong? ToUInt64N(this IDataReader reader, string name, ulong defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToUInt64, defaultValue).Invoke(reader[name]);
    #endregion

    #region Single
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Single
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static float ToSingle(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterV(Convert.ToSingle).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Single。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static float ToSingle(this IDataReader reader, int i, float defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToSingle, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Single
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static float ToSingle(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterV(Convert.ToSingle).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Single。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static float ToSingle(this IDataReader reader, string name, float defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToSingle, defaultValue).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Single
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static float? ToSingleN(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterN(Convert.ToSingle).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Single。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static float? ToSingleN(this IDataReader reader, int i, float defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToSingle, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Single
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static float? ToSingleN(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterN(Convert.ToSingle).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Single。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static float? ToSingleN(this IDataReader reader, string name, float defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToSingle, defaultValue).Invoke(reader[name]);
    #endregion

    #region Double
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Double
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static double ToDouble(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterV(Convert.ToDouble).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Double。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static double ToDouble(this IDataReader reader, int i, double defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToDouble, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Double
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static double ToDouble(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterV(Convert.ToDouble).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Double。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static double ToDouble(this IDataReader reader, string name, double defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToDouble, defaultValue).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Double
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static double? ToDoubleN(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterN(Convert.ToDouble).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Double。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static double? ToDoubleN(this IDataReader reader, int i, double defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToDouble, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Double
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static double? ToDoubleN(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterN(Convert.ToDouble).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Double。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static double? ToDoubleN(this IDataReader reader, string name, double defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToDouble, defaultValue).Invoke(reader[name]);
    #endregion

    #region Decimal
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Decimal
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static decimal ToDecimal(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterV(Convert.ToDecimal).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Decimal。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static decimal ToDecimal(this IDataReader reader, int i, decimal defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToDecimal, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Decimal
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static decimal ToDecimal(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterV(Convert.ToDecimal).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Decimal。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static decimal ToDecimal(this IDataReader reader, string name, decimal defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToDecimal, defaultValue).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Decimal
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static decimal? ToDecimalN(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterN(Convert.ToDecimal).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Decimal。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static decimal? ToDecimalN(this IDataReader reader, int i, decimal defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToDecimal, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Decimal
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static decimal? ToDecimalN(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterN(Convert.ToDecimal).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Decimal。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static decimal? ToDecimalN(this IDataReader reader, string name, decimal defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToDecimal, defaultValue).Invoke(reader[name]);
    #endregion

    #endregion //数值类型

    #region Char
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Char
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static char ToChar(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterV(Convert.ToChar).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Char。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static char ToChar(this IDataReader reader, int i, char defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToChar, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Char
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static char ToChar(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterV(Convert.ToChar).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Char。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static char ToChar(this IDataReader reader, string name, char defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToChar, defaultValue).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Char
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static char? ToCharN(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterN(Convert.ToChar).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Char。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static char? ToCharN(this IDataReader reader, int i, char defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToChar, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Byte
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static char? ToCharN(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterN(Convert.ToChar).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Byte。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static char? ToCharN(this IDataReader reader, string name, char defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToChar, defaultValue).Invoke(reader[name]);
    #endregion

    #region Boolean
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Boolean
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static bool ToBoolean(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetBooleanConverter()).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Boolean。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static bool ToBoolean(this IDataReader reader, int i, bool defaultValue)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetBooleanConverter(), defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Boolean
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static bool ToBoolean(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetBooleanConverter()).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Boolean。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static bool ToBoolean(this IDataReader reader, string name, bool defaultValue)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetBooleanConverter(), defaultValue).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Boolean
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static bool? ToBooleanN(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetBooleanConverter()).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Boolean。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static bool? ToBooleanN(this IDataReader reader, int i, bool defaultValue)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetBooleanConverter(), defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Boolean
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static bool? ToBooleanN(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetBooleanConverter()).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Boolean。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static bool? ToBooleanN(this IDataReader reader, string name, bool defaultValue)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetBooleanConverter(), defaultValue).Invoke(reader[name]);
    #endregion

    #region DateTime

    #region Convert.ToDateTime转换
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为DateTime
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterV(Convert.ToDateTime).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为DateTime。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this IDataReader reader, int i, DateTime defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToDateTime, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为DateTime
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterV(Convert.ToDateTime).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为DateTime。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this IDataReader reader, string name, DateTime defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToDateTime, defaultValue).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空DateTime
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static DateTime? ToDateTimeN(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterN(Convert.ToDateTime).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空DateTime。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static DateTime? ToDateTimeN(this IDataReader reader, int i, DateTime defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToDateTime, defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空DateTime
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static DateTime? ToDateTimeN(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterN(Convert.ToDateTime).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空DateTime。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static DateTime? ToDateTimeN(this IDataReader reader, string name, DateTime defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToDateTime, defaultValue).Invoke(reader[name]);
    #endregion

    #region DateTime.ParseExact转换

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为DateTime
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="format">DateTime.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this IDataReader reader, int i, string format)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetDateTimeConverter(format)).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为DateTime。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="format">DateTime.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this IDataReader reader, int i, string format, DateTime defaultValue)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetDateTimeConverter(format), defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为DateTime
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="format">DateTime.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this IDataReader reader, string name, string format)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetDateTimeConverter(format)).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为DateTime。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="format">DateTime.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this IDataReader reader, string name, string format, DateTime defaultValue)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetDateTimeConverter(format), defaultValue).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空DateTime
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="format">DateTime.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <returns></returns>
    public static DateTime? ToDateTimeN(this IDataReader reader, int i, string format)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetDateTimeConverter(format)).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空DateTime。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="format">DateTime.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static DateTime? ToDateTimeN(this IDataReader reader, int i, string format, DateTime defaultValue)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetDateTimeConverter(format), defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空DateTime
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="format">DateTime.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <returns></returns>
    public static DateTime? ToDateTimeN(this IDataReader reader, string name, string format)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetDateTimeConverter(format)).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空DateTime。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="format">DateTime.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static DateTime? ToDateTimeN(this IDataReader reader, string name, string format, DateTime defaultValue)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetDateTimeConverter(format), defaultValue).Invoke(reader[name]);
    #endregion

    #endregion

    #region TimeSpan

    #region ConverterBuilder转换
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为TimeSpan
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static TimeSpan ToTimeSpan(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetTimeSpanConverter()).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为TimeSpan。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static TimeSpan ToTimeSpan(this IDataReader reader, int i, TimeSpan defaultValue)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetTimeSpanConverter(), defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为TimeSpan
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static TimeSpan ToTimeSpan(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetTimeSpanConverter()).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为TimeSpan。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static TimeSpan ToTimeSpan(this IDataReader reader, string name, TimeSpan defaultValue)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetTimeSpanConverter(), defaultValue).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空TimeSpan
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static TimeSpan? ToTimeSpanN(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetTimeSpanConverter()).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空TimeSpan。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static TimeSpan? ToTimeSpanN(this IDataReader reader, int i, TimeSpan defaultValue)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetTimeSpanConverter(), defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空TimeSpan
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static TimeSpan? ToTimeSpanN(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetTimeSpanConverter()).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空TimeSpan。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static TimeSpan? ToTimeSpanN(this IDataReader reader, string name, TimeSpan defaultValue)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetTimeSpanConverter(), defaultValue).Invoke(reader[name]);
    #endregion

    #region ConverterBuilder Format转换

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为TimeSpan
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="format">TimeSpan.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <returns></returns>
    public static TimeSpan ToTimeSpan(this IDataReader reader, int i, string format)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetTimeSpanConverter(format)).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为TimeSpan。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="format">TimeSpan.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static TimeSpan ToTimeSpan(this IDataReader reader, int i, string format, TimeSpan defaultValue)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetTimeSpanConverter(format), defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为TimeSpan
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="format">TimeSpan.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <returns></returns>
    public static TimeSpan ToTimeSpan(this IDataReader reader, string name, string format)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetTimeSpanConverter(format)).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为TimeSpan。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="format">TimeSpan.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static TimeSpan ToTimeSpan(this IDataReader reader, string name, string format, TimeSpan defaultValue)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetTimeSpanConverter(format), defaultValue).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空TimeSpan
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="format">TimeSpan.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <returns></returns>
    public static TimeSpan? ToTimeSpanN(this IDataReader reader, int i, string format)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetTimeSpanConverter(format)).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空TimeSpan。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="format">TimeSpan.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static TimeSpan? ToTimeSpanN(this IDataReader reader, int i, string format, TimeSpan defaultValue)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetTimeSpanConverter(format), defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空TimeSpan
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="format">TimeSpan.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <returns></returns>
    public static TimeSpan? ToTimeSpanN(this IDataReader reader, string name, string format)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetTimeSpanConverter(format)).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空TimeSpan。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="format">TimeSpan.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static TimeSpan? ToTimeSpanN(this IDataReader reader, string name, string format, TimeSpan defaultValue)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetTimeSpanConverter(format), defaultValue).Invoke(reader[name]);
    #endregion

    #endregion

    #region Guid

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Guid
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static Guid ToGuid(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetGuidConverter()).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Guid。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static Guid ToGuid(this IDataReader reader, int i, Guid defaultValue)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetGuidConverter(), defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Guid
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static Guid ToGuid(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetGuidConverter()).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Guid。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static Guid ToGuid(this IDataReader reader, string name, Guid defaultValue)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetGuidConverter(), defaultValue).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Guid
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static Guid? ToGuidN(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetGuidConverter()).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Guid。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static Guid? ToGuidN(this IDataReader reader, int i, Guid defaultValue)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetGuidConverter(), defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Guid
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static Guid? ToGuidN(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetGuidConverter()).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Guid。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static Guid? ToGuidN(this IDataReader reader, string name, Guid defaultValue)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetGuidConverter(), defaultValue).Invoke(reader[name]);
    #endregion

    #region Byte[]
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Byte[]
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static byte[] ToBytes(this IDataReader reader, int i)
        => ConverterBuilder.GetConverterR(ConverterBuilder.GetBytesConverter()).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Byte[]。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static byte[] ToBytes(this IDataReader reader, int i, byte[] defaultValue)
        => ConverterBuilder.GetConverterR(ConverterBuilder.GetBytesConverter(), defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Byte[]
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static byte[] ToBytes(this IDataReader reader, string name)
        => ConverterBuilder.GetConverterR(ConverterBuilder.GetBytesConverter()).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Byte[]。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static byte[] ToBytes(this IDataReader reader, string name, byte[] defaultValue)
        => ConverterBuilder.GetConverterR(ConverterBuilder.GetBytesConverter(), defaultValue).Invoke(reader[name]);

    #endregion

    #region Image
    /*
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Image
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static Image ToImage(this IDataReader reader, int i)
    {
        return ConverterBuilder.GetConverterR(ConverterBuilder.GetImageConverter()).Invoke(reader[i]);
    }
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Image。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static Image ToImage(this IDataReader reader, int i, Image defaultValue)
    {
        return ConverterBuilder.GetConverterR(ConverterBuilder.GetImageConverter(), defaultValue).Invoke(reader[i]);
    }
    /// <summary>
    /// 获取位于指定名称的列的值并转换为Image
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static Image ToImage(this IDataReader reader, string name)
    {
        return ConverterBuilder.GetConverterR(ConverterBuilder.GetImageConverter()).Invoke(reader[name]);
    }
    /// <summary>
    /// 获取位于指定名称的列的值并转换为Image。如转换失败则使用默认值
    /// </summary>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static Image ToImage(this IDataReader reader, string name, Image defaultValue)
    {
        return ConverterBuilder.GetConverterR(ConverterBuilder.GetImageConverter(), defaultValue).Invoke(reader[name]);
    }
    */
    #endregion

    #region Enum
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Enum
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static T ToEnum<T>(this IDataReader reader, int i)
        where T : struct
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetEnumConverter<T>()).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Enum。如转换失败则使用默认值
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static T ToEnum<T>(this IDataReader reader, int i, T defaultValue)
        where T : struct
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetEnumConverter<T>(), defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Enum
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static T ToEnum<T>(this IDataReader reader, string name)
        where T : struct
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetEnumConverter<T>()).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Enum。如转换失败则使用默认值
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static T ToEnum<T>(this IDataReader reader, string name, T defaultValue)
        where T : struct
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetEnumConverter<T>(), defaultValue).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Enum
    /// </summary>
    /// <typeparam name="T">可空枚举类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static T? ToEnumN<T>(this IDataReader reader, int i)
        where T : struct
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetEnumConverter<T>()).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Enum。如转换失败则使用默认值
    /// </summary>
    /// <typeparam name="T">可空枚举类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static T? ToEnumN<T>(this IDataReader reader, int i, T defaultValue)
        where T : struct
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetEnumConverter<T>(), defaultValue).Invoke(reader[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Enum
    /// </summary>
    /// <typeparam name="T">可空枚举类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static T? ToEnumN<T>(this IDataReader reader, string name)
        where T : struct
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetEnumConverter<T>()).Invoke(reader[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Enum。如转换失败则使用默认值
    /// </summary>
    /// <typeparam name="T">可空枚举类型</typeparam>
    /// <param name="reader">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static T? ToEnumN<T>(this IDataReader reader, string name, T defaultValue)
        where T : struct
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetEnumConverter<T>(), defaultValue).Invoke(reader[name]);
    #endregion

    #endregion IDataReader字段值转换
}