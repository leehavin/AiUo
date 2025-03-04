using System;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.IO;

namespace AiUo;

/// <summary>
/// 提供AiUo辅助方法
/// </summary>
public static partial class AiUoUtil
{
    #region ConvertTo 类型转换

    /// <summary>
    ///  通用类型转换函数，函数会将DBNull类型转换成null或可空类型
    /// </summary>
    /// <typeparam name="T">转换的类型</typeparam>
    /// <param name="value">需转换的值</param>
    /// <returns></returns>
    public static T ConvertTo<T>(this object value)
        => (T)ConvertTo(value, typeof(T));

    private static readonly ConcurrentDictionary<Type, bool> _typeCanNullCache = new ConcurrentDictionary<Type, bool>();
    /// <summary>
    /// 类型是否可null
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool CanNullType(Type type)
    {
        if (!_typeCanNullCache.TryGetValue(type, out bool ret))
        {
            ret = (!type.IsValueType || IsNullableType(type));
            _typeCanNullCache.TryAdd(type, ret);
        }
        return ret;
    }
    /// <summary>
    /// 通用类型转换函数，函数会将DBNull类型转换成null或可空类型
    /// </summary>
    /// <param name="value"></param>
    /// <param name="toType"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public static object ConvertTo(this object value, Type toType, string format=null)
    {
        if (toType.IsInstanceOfType(value))
            return value;
        // DBNull ==> 引用类型/可空类型
        if (value == null || value == DBNull.Value)
        {
            if (CanNullType(toType))
                return null;
            else
                throw new Exception($"无法转换null或DBNull为类型 {toType.FullName}");
        }
        // 基类型
        Type baseType = Nullable.GetUnderlyingType(toType) ?? toType;
        // 同类型
        if (baseType.IsInstanceOfType(value))
            return value;
        // 简单类型
        switch (baseType.FullName)
        {
            #region 简单类型转换
            case SimpleTypeNames.Byte:
                return Convert.ToByte(value);
            case SimpleTypeNames.SByte:
                return Convert.ToSByte(value);
            case SimpleTypeNames.Int16:
                return Convert.ToInt16(value);
            case SimpleTypeNames.UInt16:
                return Convert.ToUInt16(value);
            case SimpleTypeNames.Int32:
                return Convert.ToInt32(value);
            case SimpleTypeNames.UInt32:
                return Convert.ToUInt32(value);
            case SimpleTypeNames.Int64:
                return Convert.ToInt64(value);
            case SimpleTypeNames.UInt64:
                return Convert.ToUInt64(value);
            case SimpleTypeNames.Single:
                return Convert.ToSingle(value);
            case SimpleTypeNames.Double:
                return Convert.ToDouble(value);
            case SimpleTypeNames.Decimal:
                return Convert.ToDecimal(value);
            case SimpleTypeNames.String:
                return (value is DateTime) ? ((DateTime)value).ToString(format) : Convert.ToString(value);
            case SimpleTypeNames.Char:
                return Convert.ToChar(value);
            case SimpleTypeNames.Boolean:
                return ConverterBuilder.GetBooleanConverter().Invoke(value);
            case SimpleTypeNames.DateTime:
                return ConverterBuilder.GetDateTimeConverter(format).Invoke(value);
            case SimpleTypeNames.TimeSpan:
                return ConverterBuilder.GetTimeSpanConverter(format).Invoke(value);
            case SimpleTypeNames.DateTimeOffset:
                return ConverterBuilder.GetDateTimeOffsetConverter(format).Invoke(value);
            case SimpleTypeNames.Guid:
                return ConverterBuilder.GetGuidConverter().Invoke(value);
            case SimpleTypeNames.Bytes:
                return ConverterBuilder.GetBytesConverter().Invoke(value);
            #endregion
        }
        if (baseType.IsEnum)
            return ConverterBuilder.GetEnumConverter(baseType)(value);

        #region IConvertible && TypeConverter
        // IConvertible
        if (value is IConvertible)
            return Convert.ChangeType(value, baseType);

        // TypeConverter
        TypeConverter converter = TypeDescriptor.GetConverter(baseType);
        bool flag = converter.CanConvertFrom(value.GetType());
        if (!flag)
            converter = TypeDescriptor.GetConverter(value.GetType());
        if (!flag && !converter.CanConvertTo(baseType))
            throw new InvalidOperationException($"无法转换成类型：{Convert.ToString(value)} ==> {baseType.FullName}");

        try
        {
            return flag ? converter.ConvertFrom(value) : converter.ConvertTo(value, baseType);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"类型转换出错：{Convert.ToString(value)} ==> {baseType.FullName}", ex);
        }
        #endregion
    }
    /// <summary>
    /// byte[] => decimal
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static decimal ByteArrayToDecimal(byte[] src)
    {
        using (MemoryStream stream = new MemoryStream(src))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                return reader.ReadDecimal();
            }
        }
    }
    /// <summary>
    /// decimal => byte[]
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static byte[] DecimalToByteArray(decimal src)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(src);
                return stream.ToArray();
            }
        }
    }

    /// <summary>
    /// 转换矩形数组为交错数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <returns></returns>
    public static T[][] ConvertArray<T>(T[,] array)
    {
        T[][] ret = new T[array.GetLength(0)][];
        for (int i = array.GetLowerBound(0); i <= array.GetUpperBound(0); i++)
        {
            T[] item = new T[array.GetLength(1)];
            for (int j = array.GetLowerBound(1); j <= array.GetUpperBound(1); j++)
            {
                item[j] = array[i, j];
            }
            ret[i] = item;
        }
        return ret;
    }

    #endregion
}