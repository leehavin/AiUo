using System;
using System.Data;

namespace AiUo.Data;

public static partial class DbHelper
{
    #region DataRow字段值转换

    #region To<T>
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为指定类型
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static T To<T>(this DataRow row, int i)
        => AiUoUtil.ConvertTo<T>(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为指定类型
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="converter">自定义转换器</param>
    /// <returns></returns>
    public static T To<T>(this DataRow row, int i, Func<object, T> converter)
        => converter(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为指定类型，如转换失败则使用默认值
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static T To<T>(this DataRow row, int i, T defaultValue)
    {
        T ret;
        try
        {
            ret = AiUoUtil.ConvertTo<T>(row[i]);
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
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <param name="converter">自定义转换器</param>
    /// <returns></returns>
    public static T To<T>(this DataRow row, int i, T defaultValue, Func<object, T> converter)
    {
        T ret;
        try
        {
            ret = converter(row[i]);
        }
        catch
        {
            ret = defaultValue;
        }
        return ret;
    }

    /// <summary>
    /// 获取位于指定名称的列的值并转换为指定类型
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static T To<T>(this DataRow row, string name)
        => AiUoUtil.ConvertTo<T>(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为指定类型
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="converter">自定义转换器</param>
    /// <returns></returns>
    public static T To<T>(this DataRow row, string name, Func<object, T> converter)
        => converter(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为指定类型，如转换失败则使用默认值
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static T To<T>(this DataRow row, string name, T defaultValue)
    {
        T ret;
        try
        {
            ret = AiUoUtil.ConvertTo<T>(row[name]);
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
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <param name="converter">自定义转换器</param>
    /// <returns></returns>
    public static T To<T>(this DataRow row, string name, T defaultValue, Func<object, T> converter)
    {
        T ret;
        try
        {
            ret = converter(row[name]);
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
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static string ToString(this DataRow row, int i)
        => ConverterBuilder.GetConverterR(Convert.ToString).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为String。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static string ToString(this DataRow row, int i, string defaultValue)
        => ConverterBuilder.GetConverterR(Convert.ToString, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为String
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static string ToString(this DataRow row, string name)
        => ConverterBuilder.GetConverterR(Convert.ToString).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为String。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static string ToString(this DataRow row, string name, string defaultValue)
        => ConverterBuilder.GetConverterR(Convert.ToString, defaultValue).Invoke(row[name]);
    #endregion

    #region 数值类型

    #region Byte
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Byte
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static byte ToByte(this DataRow row, int i)
        => ConverterBuilder.GetConverterV(Convert.ToByte).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Byte。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static byte ToByte(this DataRow row, int i, byte defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToByte, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Byte
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static byte ToByte(this DataRow row, string name)
        => ConverterBuilder.GetConverterV(Convert.ToByte).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Byte。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static byte ToByte(this DataRow row, string name, byte defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToByte, defaultValue).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Byte
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static byte? ToByteN(this DataRow row, int i)
        => ConverterBuilder.GetConverterN(Convert.ToByte).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Byte。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static byte? ToByteN(this DataRow row, int i, byte defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToByte, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Byte
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static byte? ToByteN(this DataRow row, string name)
        => ConverterBuilder.GetConverterN(Convert.ToByte).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Byte。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static byte? ToByteN(this DataRow row, string name, byte defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToByte, defaultValue).Invoke(row[name]);

    #endregion

    #region SByte
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为SByte
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static sbyte ToSByte(this DataRow row, int i)
        => ConverterBuilder.GetConverterV(Convert.ToSByte).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为SByte。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static sbyte ToSByte(this DataRow row, int i, sbyte defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToSByte, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为SByte
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static sbyte ToSByte(this DataRow row, string name)
        => ConverterBuilder.GetConverterV(Convert.ToSByte).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为SByte。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static sbyte ToSByte(this DataRow row, string name, sbyte defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToSByte, defaultValue).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Byte
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static sbyte? ToSByteN(this DataRow row, int i)
        => ConverterBuilder.GetConverterN(Convert.ToSByte).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Byte。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static sbyte? ToSByteN(this DataRow row, int i, sbyte defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToSByte, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Byte
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static sbyte? ToSByteN(this DataRow row, string name)
        => ConverterBuilder.GetConverterN(Convert.ToSByte).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Byte。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static sbyte? ToSByteN(this DataRow row, string name, sbyte defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToSByte, defaultValue).Invoke(row[name]);
    #endregion

    #region Int16
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Int16
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static short ToInt16(this DataRow row, int i)
        => ConverterBuilder.GetConverterV(Convert.ToInt16).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Int16。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static short ToInt16(this DataRow row, int i, short defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToInt16, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Int16
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static short ToInt16(this DataRow row, string name)
        => ConverterBuilder.GetConverterV(Convert.ToInt16).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Int16。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static short ToInt16(this DataRow row, string name, short defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToInt16, defaultValue).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Int16
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static short? ToInt16N(this DataRow row, int i)
        => ConverterBuilder.GetConverterN(Convert.ToInt16).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Int16。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static short? ToInt16N(this DataRow row, int i, short defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToInt16, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Int16
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static short? ToInt16N(this DataRow row, string name)
        => ConverterBuilder.GetConverterN(Convert.ToInt16).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Int16。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static short? ToInt16N(this DataRow row, string name, short defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToInt16, defaultValue).Invoke(row[name]);
    #endregion

    #region UInt16
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为UInt16
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static ushort ToUInt16(this DataRow row, int i)
        => ConverterBuilder.GetConverterV(Convert.ToUInt16).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为UInt16。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static ushort ToUInt16(this DataRow row, int i, ushort defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToUInt16, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为UInt16
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static ushort ToUInt16(this DataRow row, string name)
        => ConverterBuilder.GetConverterV(Convert.ToUInt16).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为UInt16。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static ushort ToUInt16(this DataRow row, string name, ushort defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToUInt16, defaultValue).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空UInt16
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static ushort? ToUInt16N(this DataRow row, int i)
        => ConverterBuilder.GetConverterN(Convert.ToUInt16).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空UInt16。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static ushort? ToUInt16N(this DataRow row, int i, ushort defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToUInt16, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空UInt16
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static ushort? ToUInt16N(this DataRow row, string name)
        => ConverterBuilder.GetConverterN(Convert.ToUInt16).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空UInt16。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static ushort? ToUInt16N(this DataRow row, string name, ushort defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToUInt16, defaultValue).Invoke(row[name]);
    #endregion

    #region Int32
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Int32
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static int ToInt32(this DataRow row, int i)
        => ConverterBuilder.GetConverterV(Convert.ToInt32).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Int32。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static int ToInt32(this DataRow row, int i, int defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToInt32, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Int32
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static int ToInt32(this DataRow row, string name)
        => ConverterBuilder.GetConverterV(Convert.ToInt32).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Int32。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static int ToInt32(this DataRow row, string name, int defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToInt32, defaultValue).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Int32
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static int? ToInt32N(this DataRow row, int i)
        => ConverterBuilder.GetConverterN(Convert.ToInt32).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Int32。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static int? ToInt32N(this DataRow row, int i, int defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToInt32, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Int32
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static int? ToInt32N(this DataRow row, string name)
        => ConverterBuilder.GetConverterN(Convert.ToInt32).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Int32。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static int? ToInt32N(this DataRow row, string name, int defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToInt32, defaultValue).Invoke(row[name]);
    #endregion

    #region UInt32
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为UInt32
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static uint ToUInt32(this DataRow row, int i)
        => ConverterBuilder.GetConverterV(Convert.ToUInt32).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为UInt32。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static uint ToUInt32(this DataRow row, int i, uint defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToUInt32, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为UInt32
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static uint ToUInt32(this DataRow row, string name)
        => ConverterBuilder.GetConverterV(Convert.ToUInt32).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为UInt32。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static uint ToUInt32(this DataRow row, string name, uint defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToUInt32, defaultValue).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空UInt32
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static uint? ToUInt32N(this DataRow row, int i)
        => ConverterBuilder.GetConverterN(Convert.ToUInt32).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空UInt32。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static uint? ToUInt32N(this DataRow row, int i, uint defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToUInt32, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空UInt32
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static uint? ToUInt32N(this DataRow row, string name)
        => ConverterBuilder.GetConverterN(Convert.ToUInt32).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空UInt32。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static uint? ToUInt32N(this DataRow row, string name, uint defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToUInt32, defaultValue).Invoke(row[name]);
    #endregion

    #region Int64
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Int64
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static long ToInt64(this DataRow row, int i)
        => ConverterBuilder.GetConverterV(Convert.ToInt64).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Int64。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static long ToInt64(this DataRow row, int i, long defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToInt64, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Int64
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static long ToInt64(this DataRow row, string name)
        => ConverterBuilder.GetConverterV(Convert.ToInt64).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Int64。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static long ToInt64(this DataRow row, string name, long defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToInt64, defaultValue).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Int64
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static long? ToInt64N(this DataRow row, int i)
        => ConverterBuilder.GetConverterN(Convert.ToInt64).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Int64。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static long? ToInt64N(this DataRow row, int i, long defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToInt64, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Int64
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static long? ToInt64N(this DataRow row, string name)
        => ConverterBuilder.GetConverterN(Convert.ToInt64).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Int64。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static long? ToInt64N(this DataRow row, string name, long defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToInt64, defaultValue).Invoke(row[name]);
    #endregion

    #region UInt64
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为UInt64
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static ulong ToUInt64(this DataRow row, int i)
        => ConverterBuilder.GetConverterV(Convert.ToUInt64).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为UInt64。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static ulong ToUInt64(this DataRow row, int i, ulong defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToUInt64, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为UInt64
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static ulong ToUInt64(this DataRow row, string name)
        => ConverterBuilder.GetConverterV(Convert.ToUInt64).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为UInt64。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static ulong ToUInt64(this DataRow row, string name, ulong defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToUInt64, defaultValue).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空UInt64
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static ulong? ToUInt64N(this DataRow row, int i)
        => ConverterBuilder.GetConverterN(Convert.ToUInt64).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空UInt64。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static ulong? ToUInt64N(this DataRow row, int i, ulong defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToUInt64, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空UInt64
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static ulong? ToUInt64N(this DataRow row, string name)
        => ConverterBuilder.GetConverterN(Convert.ToUInt64).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空UInt64。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static ulong? ToUInt64N(this DataRow row, string name, ulong defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToUInt64, defaultValue).Invoke(row[name]);
    #endregion

    #region Single
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Single
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static float ToSingle(this DataRow row, int i)
        => ConverterBuilder.GetConverterV(Convert.ToSingle).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Single。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static float ToSingle(this DataRow row, int i, float defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToSingle, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Single
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static float ToSingle(this DataRow row, string name)
        => ConverterBuilder.GetConverterV(Convert.ToSingle).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Single。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static float ToSingle(this DataRow row, string name, float defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToSingle, defaultValue).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Single
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static float? ToSingleN(this DataRow row, int i)
        => ConverterBuilder.GetConverterN(Convert.ToSingle).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Single。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static float? ToSingleN(this DataRow row, int i, float defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToSingle, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Single
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static float? ToSingleN(this DataRow row, string name)
        => ConverterBuilder.GetConverterN(Convert.ToSingle).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Single。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static float? ToSingleN(this DataRow row, string name, float defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToSingle, defaultValue).Invoke(row[name]);
    #endregion

    #region Double
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Double
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static double ToDouble(this DataRow row, int i)
        => ConverterBuilder.GetConverterV(Convert.ToDouble).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Double。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static double ToDouble(this DataRow row, int i, double defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToDouble, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Double
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static double ToDouble(this DataRow row, string name)
        => ConverterBuilder.GetConverterV(Convert.ToDouble).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Double。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static double ToDouble(this DataRow row, string name, double defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToDouble, defaultValue).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Double
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static double? ToDoubleN(this DataRow row, int i)
        => ConverterBuilder.GetConverterN(Convert.ToDouble).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Double。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static double? ToDoubleN(this DataRow row, int i, double defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToDouble, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Double
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static double? ToDoubleN(this DataRow row, string name)
        => ConverterBuilder.GetConverterN(Convert.ToDouble).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Double。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static double? ToDoubleN(this DataRow row, string name, double defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToDouble, defaultValue).Invoke(row[name]);
    #endregion

    #region Decimal
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Decimal
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static decimal ToDecimal(this DataRow row, int i)
        => ConverterBuilder.GetConverterV(Convert.ToDecimal).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Decimal。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static decimal ToDecimal(this DataRow row, int i, decimal defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToDecimal, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Decimal
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static decimal ToDecimal(this DataRow row, string name)
        => ConverterBuilder.GetConverterV(Convert.ToDecimal).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Decimal。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static decimal ToDecimal(this DataRow row, string name, decimal defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToDecimal, defaultValue).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Decimal
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static decimal? ToDecimalN(this DataRow row, int i)
        => ConverterBuilder.GetConverterN(Convert.ToDecimal).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Decimal。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static decimal? ToDecimalN(this DataRow row, int i, decimal defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToDecimal, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Decimal
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static decimal? ToDecimalN(this DataRow row, string name)
        => ConverterBuilder.GetConverterN(Convert.ToDecimal).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Decimal。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static decimal? ToDecimalN(this DataRow row, string name, decimal defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToDecimal, defaultValue).Invoke(row[name]);
    #endregion

    #endregion //数值类型

    #region Char
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Char
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static char ToChar(this DataRow row, int i)
        => ConverterBuilder.GetConverterV(Convert.ToChar).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Char。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static char ToChar(this DataRow row, int i, char defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToChar, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Char
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static char ToChar(this DataRow row, string name)
        => ConverterBuilder.GetConverterV(Convert.ToChar).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Char。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static char ToChar(this DataRow row, string name, char defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToChar, defaultValue).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Char
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static char? ToCharN(this DataRow row, int i)
        => ConverterBuilder.GetConverterN(Convert.ToChar).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Char。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static char? ToCharN(this DataRow row, int i, char defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToChar, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Byte
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static char? ToCharN(this DataRow row, string name)
        => ConverterBuilder.GetConverterN(Convert.ToChar).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Byte。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static char? ToCharN(this DataRow row, string name, char defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToChar, defaultValue).Invoke(row[name]);
    #endregion

    #region Boolean
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Boolean
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static bool ToBoolean(this DataRow row, int i)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetBooleanConverter()).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Boolean。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static bool ToBoolean(this DataRow row, int i, bool defaultValue)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetBooleanConverter(), defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Boolean
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static bool ToBoolean(this DataRow row, string name)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetBooleanConverter()).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Boolean。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static bool ToBoolean(this DataRow row, string name, bool defaultValue)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetBooleanConverter(), defaultValue).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Boolean
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static bool? ToBooleanN(this DataRow row, int i)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetBooleanConverter()).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Boolean。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static bool? ToBooleanN(this DataRow row, int i, bool defaultValue)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetBooleanConverter(), defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Boolean
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static bool? ToBooleanN(this DataRow row, string name)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetBooleanConverter()).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Boolean。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static bool? ToBooleanN(this DataRow row, string name, bool defaultValue)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetBooleanConverter(), defaultValue).Invoke(row[name]);
    #endregion

    #region DateTime

    #region Convert.ToDateTime转换
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为DateTime
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this DataRow row, int i)
        => ConverterBuilder.GetConverterV(Convert.ToDateTime).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为DateTime。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this DataRow row, int i, DateTime defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToDateTime, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为DateTime
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this DataRow row, string name)
        => ConverterBuilder.GetConverterV(Convert.ToDateTime).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为DateTime。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this DataRow row, string name, DateTime defaultValue)
        => ConverterBuilder.GetConverterV(Convert.ToDateTime, defaultValue).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空DateTime
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static DateTime? ToDateTimeN(this DataRow row, int i)
        => ConverterBuilder.GetConverterN(Convert.ToDateTime).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空DateTime。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static DateTime? ToDateTimeN(this DataRow row, int i, DateTime defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToDateTime, defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空DateTime
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static DateTime? ToDateTimeN(this DataRow row, string name)
        => ConverterBuilder.GetConverterN(Convert.ToDateTime).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空DateTime。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static DateTime? ToDateTimeN(this DataRow row, string name, DateTime defaultValue)
        => ConverterBuilder.GetConverterN(Convert.ToDateTime, defaultValue).Invoke(row[name]);
    #endregion

    #region DateTime.ParseExact转换

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为DateTime
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="format">DateTime.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this DataRow row, int i, string format)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetDateTimeConverter(format)).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为DateTime。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="format">DateTime.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this DataRow row, int i, string format, DateTime defaultValue)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetDateTimeConverter(format), defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为DateTime
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="format">DateTime.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this DataRow row, string name, string format)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetDateTimeConverter(format)).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为DateTime。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="format">DateTime.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this DataRow row, string name, string format, DateTime defaultValue)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetDateTimeConverter(format), defaultValue).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空DateTime
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="format">DateTime.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <returns></returns>
    public static DateTime? ToDateTimeN(this DataRow row, int i, string format)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetDateTimeConverter(format)).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空DateTime。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="format">DateTime.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static DateTime? ToDateTimeN(this DataRow row, int i, string format, DateTime defaultValue)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetDateTimeConverter(format), defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空DateTime
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="format">DateTime.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <returns></returns>
    public static DateTime? ToDateTimeN(this DataRow row, string name, string format)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetDateTimeConverter(format)).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空DateTime。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="format">DateTime.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static DateTime? ToDateTimeN(this DataRow row, string name, string format, DateTime defaultValue)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetDateTimeConverter(format), defaultValue).Invoke(row[name]);
    #endregion

    #endregion

    #region TimeSpan

    #region ConverterBuilder转换
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为TimeSpan
    /// </summary>
    /// <param name="row">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static TimeSpan ToTimeSpan(this DataRow row, int i)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetTimeSpanConverter()).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为TimeSpan。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static TimeSpan ToTimeSpan(this DataRow row, int i, TimeSpan defaultValue)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetTimeSpanConverter(), defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为TimeSpan
    /// </summary>
    /// <param name="row">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static TimeSpan ToTimeSpan(this DataRow row, string name)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetTimeSpanConverter()).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为TimeSpan。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static TimeSpan ToTimeSpan(this DataRow row, string name, TimeSpan defaultValue)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetTimeSpanConverter(), defaultValue).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空TimeSpan
    /// </summary>
    /// <param name="row">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static TimeSpan? ToTimeSpanN(this DataRow row, int i)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetTimeSpanConverter()).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空TimeSpan。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static TimeSpan? ToTimeSpanN(this DataRow row, int i, TimeSpan defaultValue)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetTimeSpanConverter(), defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空TimeSpan
    /// </summary>
    /// <param name="row">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static TimeSpan? ToTimeSpanN(this DataRow row, string name)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetTimeSpanConverter()).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空TimeSpan。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static TimeSpan? ToTimeSpanN(this DataRow row, string name, TimeSpan defaultValue)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetTimeSpanConverter(), defaultValue).Invoke(row[name]);
    #endregion

    #region ConverterBuilder Format转换

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为TimeSpan
    /// </summary>
    /// <param name="row">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="format">TimeSpan.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <returns></returns>
    public static TimeSpan ToTimeSpan(this DataRow row, int i, string format)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetTimeSpanConverter(format)).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为TimeSpan。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="format">TimeSpan.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static TimeSpan ToTimeSpan(this DataRow row, int i, string format, TimeSpan defaultValue)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetTimeSpanConverter(format), defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为TimeSpan
    /// </summary>
    /// <param name="row">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="format">TimeSpan.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <returns></returns>
    public static TimeSpan ToTimeSpan(this DataRow row, string name, string format)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetTimeSpanConverter(format)).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为TimeSpan。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="format">TimeSpan.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static TimeSpan ToTimeSpan(this DataRow row, string name, string format, TimeSpan defaultValue)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetTimeSpanConverter(format), defaultValue).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空TimeSpan
    /// </summary>
    /// <param name="row">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="format">TimeSpan.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <returns></returns>
    public static TimeSpan? ToTimeSpanN(this DataRow row, int i, string format)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetTimeSpanConverter(format)).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空TimeSpan。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">IDataReader对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="format">TimeSpan.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static TimeSpan? ToTimeSpanN(this DataRow row, int i, string format, TimeSpan defaultValue)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetTimeSpanConverter(format), defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空TimeSpan
    /// </summary>
    /// <param name="row">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="format">TimeSpan.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <returns></returns>
    public static TimeSpan? ToTimeSpanN(this DataRow row, string name, string format)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetTimeSpanConverter(format)).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空TimeSpan。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">IDataReader对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="format">TimeSpan.ParseExact所用到的日期格式说明符，如：yyyy-MM-dd HH:mm:ss</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static TimeSpan? ToTimeSpanN(this DataRow row, string name, string format, TimeSpan defaultValue)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetTimeSpanConverter(format), defaultValue).Invoke(row[name]);
    #endregion

    #endregion

    #region Guid

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Guid
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static Guid ToGuid(this DataRow row, int i)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetGuidConverter()).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Guid。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static Guid ToGuid(this DataRow row, int i, Guid defaultValue)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetGuidConverter(), defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Guid
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static Guid ToGuid(this DataRow row, string name)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetGuidConverter()).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Guid。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static Guid ToGuid(this DataRow row, string name, Guid defaultValue)
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetGuidConverter(), defaultValue).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Guid
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static Guid? ToGuidN(this DataRow row, int i)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetGuidConverter()).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Guid。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static Guid? ToGuidN(this DataRow row, int i, Guid defaultValue)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetGuidConverter(), defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Guid
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static Guid? ToGuidN(this DataRow row, string name)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetGuidConverter()).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Guid。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static Guid? ToGuidN(this DataRow row, string name, Guid defaultValue)
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetGuidConverter(), defaultValue).Invoke(row[name]);
    #endregion

    #region Byte[]
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Byte[]
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static byte[] ToBytes(this DataRow row, int i)
        => ConverterBuilder.GetConverterR(ConverterBuilder.GetBytesConverter()).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Byte[]。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static byte[] ToBytes(this DataRow row, int i, byte[] defaultValue)
        => ConverterBuilder.GetConverterR(ConverterBuilder.GetBytesConverter(), defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Byte[]
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static byte[] ToBytes(this DataRow row, string name)
        => ConverterBuilder.GetConverterR(ConverterBuilder.GetBytesConverter()).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Byte[]。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static byte[] ToBytes(this DataRow row, string name, byte[] defaultValue)
        => ConverterBuilder.GetConverterR(ConverterBuilder.GetBytesConverter(), defaultValue).Invoke(row[name]);
    #endregion

    #region Image
    /*
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Image
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static Image ToImage(this DataRow row, int i)
    {
        return ConverterBuilder.GetConverterR(ConverterBuilder.GetImageConverter()).Invoke(row[i]);
    }
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Image。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static Image ToImage(this DataRow row, int i, Image defaultValue)
    {
        return ConverterBuilder.GetConverterR(ConverterBuilder.GetImageConverter(), defaultValue).Invoke(row[i]);
    }
    /// <summary>
    /// 获取位于指定名称的列的值并转换为Image
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static Image ToImage(this DataRow row, string name)
    {
        return ConverterBuilder.GetConverterR(ConverterBuilder.GetImageConverter()).Invoke(row[name]);
    }
    /// <summary>
    /// 获取位于指定名称的列的值并转换为Image。如转换失败则使用默认值
    /// </summary>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static Image ToImage(this DataRow row, string name, Image defaultValue)
    {
        return ConverterBuilder.GetConverterR(ConverterBuilder.GetImageConverter(), defaultValue).Invoke(row[name]);
    }
    */
    #endregion

    #region Enum
    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Enum
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static T ToEnum<T>(this DataRow row, int i)
        where T : struct
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetEnumConverter<T>()).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为Enum。如转换失败则使用默认值
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static T ToEnum<T>(this DataRow row, int i, T defaultValue)
        where T : struct
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetEnumConverter<T>(), defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Enum
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static T ToEnum<T>(this DataRow row, string name)
        where T : struct
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetEnumConverter<T>()).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为Enum。如转换失败则使用默认值
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static T ToEnum<T>(this DataRow row, string name, T defaultValue)
        where T : struct
        => ConverterBuilder.GetConverterV(ConverterBuilder.GetEnumConverter<T>(), defaultValue).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Enum
    /// </summary>
    /// <typeparam name="T">可空枚举类型</typeparam>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <returns></returns>
    public static T? ToEnumN<T>(this DataRow row, int i)
        where T : struct
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetEnumConverter<T>()).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定索引处的列的值并转换为可空Enum。如转换失败则使用默认值
    /// </summary>
    /// <typeparam name="T">可空枚举类型</typeparam>
    /// <param name="row">DataRow对象</param>
    /// <param name="i">要获取的列的从零开始的索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static T? ToEnumN<T>(this DataRow row, int i, T defaultValue)
        where T : struct
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetEnumConverter<T>(), defaultValue).Invoke(row[i]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Enum
    /// </summary>
    /// <typeparam name="T">可空枚举类型</typeparam>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <returns></returns>
    public static T? ToEnumN<T>(this DataRow row, string name)
        where T : struct
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetEnumConverter<T>()).Invoke(row[name]);

    /// <summary>
    /// 获取位于指定名称的列的值并转换为可空Enum。如转换失败则使用默认值
    /// </summary>
    /// <typeparam name="T">可空枚举类型</typeparam>
    /// <param name="row">DataRow对象</param>
    /// <param name="name">要查找的列的名称</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static T? ToEnumN<T>(this DataRow row, string name, T defaultValue)
        where T : struct
        => ConverterBuilder.GetConverterN(ConverterBuilder.GetEnumConverter<T>(), defaultValue).Invoke(row[name]);
    #endregion

    #endregion DataRow字段值转换
}