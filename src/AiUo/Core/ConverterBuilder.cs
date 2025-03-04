using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

namespace AiUo;

/// <summary>
/// 常用类型转换器构建类
/// </summary>
public static class ConverterBuilder
{
    #region 类型转换器（值类型，引用类型，可空类型），主要处理null、DBNull和异常时提供默认值
    /// <summary>
    /// 获取值类型转换器，无法转换Null或DBNull，将抛出异常。
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="converter">转换器</param>
    /// <returns></returns>
    public static Func<object, T> GetConverterV<T>(Converter<object, T> converter)
        where T : struct
    {
        return (value) =>
        {
            if (value == null || value == DBNull.Value)
                throw new Exception("Null和DBNull不能转换为值类型。");
            return value is T ret ? ret : converter.Invoke(value);
        };
    }

    /// <summary>
    /// 获取值类型转换器，Null或DBNull，或者转换发生异常时，则使用默认值。
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="converter">转换器</param>
    /// <param name="defaultValue">异常时的默认值</param>
    /// <returns></returns>
    public static Func<object, T> GetConverterV<T>(Converter<object, T> converter, T defaultValue)
        where T : struct
    {
        return (value) =>
        {
            if (value == null || value == DBNull.Value) return defaultValue;
            T ret = default;
            try
            {
                ret = (value is T t) ? t : converter.Invoke(value);
            }
            catch
            {
                ret = defaultValue;
            }
            return ret;
        };
    }

    /// <summary>
    /// 获取引用类型转换器，Null或DBNull转换为null。
    /// </summary>
    /// <typeparam name="T">引用类型信息</typeparam>
    /// <param name="converter"></param>
    /// <returns></returns>
    public static Func<object, T> GetConverterR<T>(Converter<object, T> converter)
        where T : class
    {
        return (value) =>
        {
            if (value == null || value == DBNull.Value) return null;
            if (value is T ret) return ret;
            return converter.Invoke(value);
        };
    }

    /// <summary>
    /// 获取引用类型转换器，Null或DBNull转换为null，转换失败则使用默认值。
    /// </summary>
    /// <typeparam name="T">引用类型信息</typeparam>
    /// <param name="converter"></param>
    /// <param name="defaultValue">转换失败后的默认值</param>
    /// <returns></returns>
    public static Func<object, T> GetConverterR<T>(Converter<object, T> converter, T defaultValue)
        where T : class
    {
        return (value) =>
        {
            T ret = default;
            try
            {
                ret = GetConverterR(converter).Invoke(value);
            }
            catch
            {
                ret = defaultValue;
            }
            return ret;
        };
    }

    /// <summary>
    /// 获取可空类型转换器，Null或DBNull转换为null。
    /// </summary>
    /// <typeparam name="T">可空类型的基类型</typeparam>
    /// <param name="convert"></param>
    /// <returns></returns>
    public static Func<object, T?> GetConverterN<T>(Converter<object, T> convert)
        where T : struct
    {
        return (value) =>
        {
            if (value == null || value == DBNull.Value) return null;
            return value is T t ? t : convert.Invoke(value);
        };
    }

    /// <summary>
    /// 获取可空类型转换器，Null或DBNull转换为null，转换失败则使用默认值。
    /// </summary>
    /// <typeparam name="T">可空类型的基类型</typeparam>
    /// <param name="convert"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static Func<object, T?> GetConverterN<T>(Converter<object, T> convert, T defaultValue)
        where T : struct
    {
        return (value) =>
        {
            T? ret = null;
            try
            {
                ret = GetConverterN(convert).Invoke(value);
            }
            catch
            {
                ret = defaultValue;
            }
            return ret;
        };
    }
    #endregion

    #region Date
    /// <summary>
    /// TimeSpan转换器，采用将对象转换成字符串后再转换成类型对象
    /// </summary>
    /// <param name="formart"></param>
    /// <returns></returns>
    internal static Converter<object, TimeSpan> GetTimeSpanConverter(string formart = null)
    {
        return (value) =>
        {
            if (value == null) throw new Exception("要转换成TimeSpan的数据对象不能NULL。");
            if (value is TimeSpan span) return span;
            //if (typeof(TimeSpan).IsInstanceOfType(value)) return (TimeSpan)value;
            if (string.IsNullOrEmpty(formart))
                return TimeSpan.Parse(Convert.ToString(value), System.Globalization.CultureInfo.InvariantCulture);
            return TimeSpan.ParseExact(Convert.ToString(value), formart, System.Globalization.CultureInfo.InvariantCulture);
        };
    }
    /// <summary>
    /// DateTime转换器，采用将对象转换成字符串后再转换成类型对象
    /// </summary>
    /// <param name="formart">仅用于字符串转换时所需的格式说明符，如"yyyy-MM-dd"</param>
    /// <returns></returns>
    internal static Converter<object, DateTime> GetDateTimeConverter(string formart = null)
    {
        return (value) =>
        {
            return value is DateTime time ? time
                : GetDateTimeStringConverter(formart).Invoke(Convert.ToString(value));
        };
    }
    internal static Func<string, DateTime> GetDateTimeStringConverter(string format = null)
    {
        return (value) =>
        {
            if (string.IsNullOrEmpty(value))
                throw new Exception("要转换成DateTime的数据对象不能NULL或空。");
            if (string.IsNullOrEmpty(format))
            {
                try
                {
                    return DateTimeUtil.ParseFormatString(value);
                }
                catch
                { }
                return DateTime.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            return DateTime.ParseExact(value, format, System.Globalization.CultureInfo.InvariantCulture);
        };
    }
    /// <summary>
    /// DateTimeOffset转换器，采用将对象转换成字符串后再转换成类型对象
    /// </summary>
    /// <param name="formart">仅用于字符串转换时所需的格式说明符，如"yyyy-MM-dd"</param>
    /// <returns></returns>
    internal static Converter<object, DateTimeOffset> GetDateTimeOffsetConverter(string formart = null)
    {
        return (value) =>
        {
            if (value == null) throw new Exception("要转换成DateTimeOffset的数据对象不能NULL。");
            if (value is DateTimeOffset offset) return offset;
            if (string.IsNullOrEmpty(formart))
                return DateTimeOffset.Parse(Convert.ToString(value), System.Globalization.CultureInfo.InvariantCulture);
            return DateTimeOffset.ParseExact(Convert.ToString(value), formart, System.Globalization.CultureInfo.InvariantCulture);
        };
    }
    #endregion

    #region bool Guid byte[] Image Enum
    private static readonly Dictionary<string, bool> _booleanCache = new Dictionary<string, bool>() {
        { "TRUE",true}, { "FALSE",false},
        { "1",true},{ "0",false},
        { "YES",true},{ "NO",false},
        { "真",true},{ "假",false},
        { "是",true},{ "否",false}
    };
    /// <summary>
    /// String => Boolean转换器
    /// </summary>
    /// <returns></returns>
    internal static Func<string, bool> GetBooleanStringConverter()
    {
        return str =>
        {
            if (bool.TryParse(str, out bool ret))
                return ret;
            str = str.ToUpper().Trim();
            if (!_booleanCache.ContainsKey(str))
                throw new Exception(str + "值无法转换为Boolean。");
            return _booleanCache[str];
        };
    }
    /// <summary>
    /// Boolean转换器
    /// </summary>
    /// <returns></returns>
    internal static Converter<object, bool> GetBooleanConverter()
    {
        return (value) =>
        {
            if (value == null) throw new Exception("要转换成bool的数据对象不能NULL。");
            // 同类型
            if (value is bool boolean) return boolean;
            string strValue = Convert.ToString(value);
            return GetBooleanStringConverter().Invoke(strValue);
        };
    }

    /// <summary>
    /// Guid转换器
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    internal static Converter<object, Guid> GetGuidConverter(string format = null)
    {
        return (value) =>
        {
            if (value == null) throw new Exception("要转换成Guid的数据对象不能NULL。");
            // 同类型
            if (value is Guid) return (Guid)value;
            string strValue = Convert.ToString(value);
            return string.IsNullOrEmpty(format)
                ? Guid.Parse(strValue)
                : Guid.ParseExact(strValue, format);
        };
    }

    /// <summary>
    /// Byte[]转换器，采用将对象BinaryFormatter成byte[]
    /// </summary>
    /// <returns></returns>
    internal static Converter<object, byte[]> GetBytesConverter()
    {
        return (value) =>
        {
            if (value == null) return null;
            if (value is byte[] ret) return ret;
            var options = new JsonSerializerOptions
            {
                WriteIndented = false
            };
            return JsonSerializer.SerializeToUtf8Bytes(value, options);
        };

    }

    //internal static Converter<object, Image> GetImageConverter()
    //{
    //    return (value) =>
    //    {
    //        if (value == null) return null;
    //        Image ret = value as Image;
    //        if (ret != null) return ret;
    //        using (var ms = new MemoryStream((byte[])value))
    //        {
    //            ret = Image.FromStream(ms);
    //        }
    //        return ret;
    //    };
    //}

    /// <summary>
    /// Enum转换器，提供将对象值（字符串或值）转换成对应的枚举类型的值
    /// </summary>
    /// <typeparam name="T">需要转换的枚举类型</typeparam>
    /// <returns></returns>
    internal static Converter<object, T> GetEnumConverter<T>()
        => (value) => (T)GetEnumConverter(typeof(T)).Invoke(value);

    internal static Converter<object, object> GetEnumConverter(Type enumType)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException($"{enumType.Name} 必须是一个Enum类型。");
        return (value) =>
        {
            if (value == null)
                throw new Exception($"转换Enum类型不能为null: {enumType.Name}");
            object ret = null;
            if (value.GetType() == typeof(string))
                ret = Enum.Parse(enumType, Convert.ToString(value), true);
            else if (Enum.IsDefined(enumType, value))
                ret = value;
            else
                throw new Exception("Enum类型转换失败。");
            return ret;
        };
    }
    #endregion
}