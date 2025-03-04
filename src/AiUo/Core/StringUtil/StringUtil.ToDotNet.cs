using System;
using System.Globalization;

namespace AiUo;

/// <summary>
/// 字符串操作静态辅助类
/// TODO: 应考虑重写To(T)方法，使用ConverterBuilder做转换器，支持万能转换！20160707
/// </summary>
public static partial class StringUtil
{

    #region DefaultIfNull
    /// <summary>
    /// 如果String为Null则使用指定的默认值
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static string DefaultIfNull(this string src, string defaultValue)
        => src ?? defaultValue;

    /// <summary>
    /// 如果String为Null或Empty则使用指定的默认值
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static string DefaultIfNullOrEmpty(this string src, string defaultValue)
        => string.IsNullOrEmpty(src) ? defaultValue : src;

    #endregion

    #region To<T>
    public static object To(this string src, Type destType)
    {
        if (src == null && AiUoUtil.CanNullType(destType))
            return null;
        object ret = null;
        switch (destType.FullName)
        {
            case "System.Object":
                ret = src;
                break;
            case SimpleTypeNames.String:
                ret = src;
                break;
            case SimpleTypeNames.Byte:
                ret = src.ToByte();
                break;
            case SimpleTypeNames.SByte:
                ret = src.ToSByte();
                break;
            case SimpleTypeNames.Int16:
                ret = src.ToInt16();
                break;
            case SimpleTypeNames.UInt16:
                ret = src.ToUInt16();
                break;
            case SimpleTypeNames.Int32:
                ret = src.ToInt32();
                break;
            case SimpleTypeNames.UInt32:
                ret = src.ToUInt32();
                break;
            case SimpleTypeNames.Int64:
                ret = src.ToInt64();
                break;
            case SimpleTypeNames.UInt64:
                ret = src.ToUInt64();
                break;
            case SimpleTypeNames.Single:
                ret = src.ToSingle();
                break;
            case SimpleTypeNames.Double:
                ret = src.ToDouble();
                break;
            case SimpleTypeNames.Boolean:
                ret = src.ToBoolean();
                break;
            case SimpleTypeNames.Char:
                ret = src.ToChar();
                break;
            case SimpleTypeNames.Decimal:
                ret = src.ToDecimal();
                break;
            case SimpleTypeNames.TimeSpan:
                ret = TimeSpan.Parse(src);
                break;
            case SimpleTypeNames.DateTime:
            case SimpleTypeNames.DateTimeOffset:
                ret = src.ToDateTime();
                break;
            case SimpleTypeNames.Guid:
                ret = Guid.Parse(src);
                break;
            default:
                throw new Exception($"不支持字符串到此类型的转换{destType.FullName}");
        }
        return ret;
    }

    /// <summary>
    /// 将String转换成公共语言运行时类型。
    /// 公共语言运行时类型: SByte、Byte、Int16、UInt16、Int32、UInt32、Int64、UInt64、Single、Double、Decimal、Boolean、DateTime、Char、String
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="src">将转换的字符串</param>
    /// <returns></returns>
    public static T To<T>(this string src)
        => (T)Convert.ChangeType(src, typeof(T), null);

    /// <summary>
    /// 将String转换成公共语言运行时类型。如转换失败则使用默认值
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="src">将转换的字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static T To<T>(this string src, T defaultValue)
    {
        T ret = default(T);
        try
        {
            ret = To<T>(src);
        }
        catch
        {
            ret = defaultValue;
        }
        return ret;
    }

    /// <summary>
    /// 将String转换成公共语言运行时类型。
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="src">将转换的字符串</param>
    /// <param name="provider">一个提供区域性特定的格式设置信息的对象</param>
    /// <returns></returns>
    public static T To<T>(this string src, IFormatProvider provider)
        => (T)Convert.ChangeType(src, typeof(T), provider);

    /// <summary>
    /// 将String转换成公共语言运行时类型。如转换失败则使用默认值
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="src">将转换的字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <param name="provider">一个提供区域性特定的格式设置信息的对象</param>
    /// <returns></returns>
    public static T To<T>(this string src, T defaultValue, IFormatProvider provider)
    {
        T ret = default(T);
        try
        {
            ret = To<T>(src, provider);
        }
        catch
        {
            ret = defaultValue;
        }
        return ret;
    }

    /// <summary>
    /// 将String转换成指定类型
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="src">将转换的字符串</param>
    /// <param name="converter">自定义转换器</param>
    /// <returns></returns>
    public static T To<T>(this string src, Func<string, T> converter)
        => (converter ?? To<T>)(src);

    /// <summary>
    /// 将String转换成指定类型。如转换失败则使用默认值
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="src">将转换的字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <param name="converter">自定义转换器</param>
    /// <returns></returns>
    public static T To<T>(this string src, T defaultValue, Func<string, T> converter)
    {
        T ret = default(T);
        try
        {
            ret = To(src, converter);
        }
        catch
        {
            ret = defaultValue;
        }
        return ret;
    }

    /// <summary>
    /// 将String转换成可空类型
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="src">将转换的字符串</param>
    /// <returns></returns>
    public static T? ToN<T>(this string src)
        where T : struct
    {
        T? ret = null;
        if (!string.IsNullOrEmpty(src))
            ret = To<T>(src);
        return ret;
    }

    /// <summary>
    /// 将String转换成可空类型
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="src">将转换的字符串</param>
    /// <param name="provider">一个提供区域性特定的格式设置信息的对象</param>
    /// <returns></returns>
    public static T? ToN<T>(this string src, IFormatProvider provider)
        where T : struct
        => (!string.IsNullOrEmpty(src)) ? To<T>(src, provider) : (T?)null;

    /// <summary>
    /// 将String转换成可空类型。如转换失败则使用默认值
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="src">将转换的字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static T? ToN<T>(this string src, T defaultValue)
        where T : struct
        => (!string.IsNullOrEmpty(src)) ? To(src, defaultValue) : (T?)null;

    /// <summary>
    /// 将String转换成可空类型。如转换失败则使用默认值
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="src">将转换的字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <param name="provider">一个提供区域性特定的格式设置信息的对象</param>
    /// <returns></returns>
    public static T? ToN<T>(this string src, T defaultValue, IFormatProvider provider)
        where T : struct
        => (!string.IsNullOrEmpty(src)) ? To(src, defaultValue, provider) : (T?)null;

    /// <summary>
    /// 将String转换成可空类型
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="src">目标类型</param>
    /// <param name="converter">自定义转换器</param>
    /// <returns></returns>
    public static T? ToN<T>(this string src, Func<string, T> converter)
        where T : struct
        => (!string.IsNullOrEmpty(src)) ? To(src, converter) : (T?)null;

    /// <summary>
    /// 将String转换成可空类型。如转换失败则使用默认值
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="src">将转换的字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <param name="converter">自定义转换器</param>
    /// <returns></returns>
    public static T? ToN<T>(this string src, T defaultValue, Func<string, T> converter)
        where T : struct
        => (!string.IsNullOrEmpty(src)) ? To(src, defaultValue, converter) : (T?)null;
    #endregion //To<T>

    #region 数值类型

    #region SByte
    /// <summary>
    /// 将String转换成SByte
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static sbyte ToSByte(this string src)
        => To(src, (str) => sbyte.Parse(str, NumberStyles.Any));

    /// <summary>
    /// 将String转换成SByte，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static sbyte ToSByte(this string src, sbyte defaultValue)
        => string.IsNullOrEmpty(src) ? defaultValue : To(src, defaultValue, ToSByte);

    /// <summary>
    /// 将String转换成可空SByte
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static sbyte? ToSByteN(this string src)
        => ToN(src, ToSByte);

    /// <summary>
    /// 将String转换成可空SByte，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static sbyte? ToSByteN(this string src, sbyte defaultValue)
        => ToN(src, defaultValue, ToSByte);
    #endregion

    #region Byte
    /// <summary>
    /// 将String转换成Byte
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static byte ToByte(this string src)
        => To(src, (str) => byte.Parse(str, NumberStyles.Any));

    /// <summary>
    /// 将String转换成Byte，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static byte ToByte(this string src, byte defaultValue)
        => string.IsNullOrEmpty(src) ? defaultValue : To(src, defaultValue, ToByte);

    /// <summary>
    /// 将String转换成可空Byte
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static byte? ToByteN(this string src)
        => ToN(src, ToByte);

    /// <summary>
    /// 将String转换成可空Byte，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static byte? ToByteN(this string src, byte defaultValue)
        => ToN(src, defaultValue, ToByte);
    #endregion

    #region Int16
    /// <summary>
    /// 将String转换成Int16(short)
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static short ToInt16(this string src)
        => To(src, (str) => short.Parse(str, NumberStyles.Any));

    /// <summary>
    /// 将String转换成Int16(short)，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static short ToInt16(this string src, short defaultValue)
        => string.IsNullOrEmpty(src) ? defaultValue : To(src, defaultValue, ToInt16);

    /// <summary>
    /// 将String转换成可空Int16(short?)
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static short? ToInt16N(this string src)
        => ToN(src, ToInt16);

    /// <summary>
    /// 将String转换成可空Int16(short?)，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static short? ToInt16N(this string src, short defaultValue)
        => ToN(src, defaultValue, ToInt16);
    #endregion

    #region UInt16
    /// <summary>
    /// 将String转换成UInt16(ushort)
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static ushort ToUInt16(this string src)
        => To(src, (str) => ushort.Parse(str, NumberStyles.Any));

    /// <summary>
    /// 将String转换成UInt16(ushort)，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static ushort ToUInt16(this string src, ushort defaultValue)
        => string.IsNullOrEmpty(src) ? defaultValue : To(src, defaultValue, ToUInt16);

    /// <summary>
    /// 将String转换成可空UInt16(ushort?)
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static ushort? ToUInt16N(this string src)
        => ToN(src, ToUInt16);

    /// <summary>
    /// 将String转换成可空UInt16(ushort?)，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static ushort? ToUInt16N(this string src, ushort defaultValue)
        => ToN(src, defaultValue, ToUInt16);
    #endregion

    #region Int32
    /// <summary>
    /// 将String转换成Int32(int)
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static int ToInt32(this string src)
        => To(src, (str) => int.Parse(str, NumberStyles.Any));

    /// <summary>
    /// 将String转换成Int32，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static int ToInt32(this string src, int defaultValue)
        => string.IsNullOrEmpty(src) ? defaultValue : To(src, defaultValue, ToInt32);

    /// <summary>
    /// 将String转换成可空Int32
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static int? ToInt32N(this string src)
        => ToN(src, ToInt32);

    /// <summary>
    /// 将String转换成可空Int32，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static int? ToInt32N(this string src, int defaultValue)
        => ToN(src, defaultValue, ToInt32);
    #endregion

    #region UInt32
    /// <summary>
    /// 将String转换成UInt32(uint)
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static uint ToUInt32(this string src)
        => To(src, (str) => uint.Parse(str, NumberStyles.Any));

    /// <summary>
    /// 将String转换成UInt32(uint)，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static uint ToUInt32(this string src, uint defaultValue)
        => string.IsNullOrEmpty(src) ? defaultValue : To(src, defaultValue, ToUInt32);

    /// <summary>
    /// 将String转换成可空UInt32(uint)
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static uint? ToUInt32N(this string src)
        => ToN(src, ToUInt32);

    /// <summary>
    /// 将String转换成可空UInt32(uint)，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static uint? ToUInt32N(this string src, uint defaultValue)
        => ToN(src, defaultValue, ToUInt32);
    #endregion

    #region Int64
    /// <summary>
    /// 将String转换成Int64(long)
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static long ToInt64(this string src)
        => To(src, (str) => long.Parse(str, NumberStyles.Any));

    /// <summary>
    /// 将String转换成Int64(long)，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static long ToInt64(this string src, long defaultValue)
        => string.IsNullOrEmpty(src) ? defaultValue : To(src, defaultValue, ToInt64);

    /// <summary>
    /// 将String转换成可空Int64(long?)
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static long? ToInt64N(this string src)
        => ToN(src, ToInt64);

    /// <summary>
    /// 将String转换成可空Int64(long?)，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static long? ToInt64N(this string src, long defaultValue)
        => ToN(src, defaultValue, ToInt64);
    #endregion

    #region UInt64
    /// <summary>
    /// 将String转换成UInt64(ulong)
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static ulong ToUInt64(this string src)
        => To(src, (str) => ulong.Parse(str, NumberStyles.Any));

    /// <summary>
    /// 将String转换成UInt64(ulong)，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static ulong ToUInt64(this string src, ulong defaultValue)
        => string.IsNullOrEmpty(src) ? defaultValue : To(src, defaultValue, ToUInt64);

    /// <summary>
    /// 将String转换成可空UInt64(ulong)
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static ulong? ToUInt64N(this string src)
        => ToN(src, ToUInt64);

    /// <summary>
    /// 将String转换成可空UInt64(ulong)，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static ulong? ToUInt64N(this string src, ulong defaultValue)
        => ToN(src, defaultValue, ToUInt64);
    #endregion

    #region Single
    /// <summary>
    /// 将String转换成Single(float)
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static float ToSingle(this string src)
        => To(src, (str) => float.Parse(str, NumberStyles.Any));

    /// <summary>
    /// 将String转换成Single(float)，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static float ToSingle(this string src, float defaultValue)
        => string.IsNullOrEmpty(src) ? defaultValue : To(src, defaultValue, ToSingle);

    /// <summary>
    /// 将String转换成可空Single(float?)
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static float? ToSingleN(this string src)
        => ToN(src, ToSingle);

    /// <summary>
    /// 将String转换成可空Single(float?)，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static float? ToSingleN(this string src, float defaultValue)
        => ToN(src, defaultValue, ToSingle);
    #endregion

    #region Double
    /// <summary>
    /// 将String转换成Double
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static double ToDouble(this string src)
        => To(src, (str) => double.Parse(str, NumberStyles.Any));

    /// <summary>
    /// 将String转换成Double，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static double ToDouble(this string src, double defaultValue)
        => string.IsNullOrEmpty(src) ? defaultValue : To(src, defaultValue, ToDouble);

    /// <summary>
    /// 将String转换成可空Double
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static double? ToDoubleN(this string src)
        => ToN(src, ToDouble);

    /// <summary>
    /// 将String转换成可空Double
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static double? ToDoubleN(this string src, double defaultValue)
        => ToN(src, defaultValue, ToDouble);
    #endregion

    #region Decimal
    /// <summary>
    /// 将String转换成Decimal
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static decimal ToDecimal(this string src)
        => To(src, (str) => decimal.Parse(str, NumberStyles.Any));

    /// <summary>
    /// 将String转换成Decimal，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static decimal ToDecimal(this string src, decimal defaultValue)
        => string.IsNullOrEmpty(src) ? defaultValue : To(src, defaultValue, ToDecimal);

    /// <summary>
    /// 将String转换成可空Decimal
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static decimal? ToDecimalN(this string src)
        => ToN(src, ToDecimal);

    /// <summary>
    /// 将String转换成可空Decimal，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static decimal? ToDecimalN(this string src, decimal defaultValue)
        => ToN(src, defaultValue, ToDecimal);
    #endregion // Decimal

    #endregion // 数值类型

    #region Boolean
    /// <summary>
    /// 将String转换成Boolean
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static bool ToBoolean(this string src)
        => To(src, ConverterBuilder.GetBooleanStringConverter());

    /// <summary>
    /// 将String转换成Boolean，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static bool ToBoolean(this string src, bool defaultValue)
        => string.IsNullOrEmpty(src) ? defaultValue : To(src, defaultValue, ToBoolean);

    /// <summary>
    /// 将String转换成可空Boolean
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static bool? ToBooleanN(this string src)
        => ToN(src, ToBoolean);
    /// <summary>
    /// 将String转换成可空Boolean
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static bool? ToBooleanN(this string src, bool defaultValue)
        => ToN(src, defaultValue, ToBoolean);

    #endregion

    #region Char

    /// <summary>
    /// 将String转换成Char
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static char ToChar(this string src)
        => To(src, char.Parse);

    /// <summary>
    /// 将String转换成Char，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static char ToChar(this string src, char defaultValue)
        => string.IsNullOrEmpty(src) ? defaultValue : To(src, defaultValue, ToChar);

    /// <summary>
    /// 将String转换成可空Char
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static char? ToCharN(this string src)
        => ToN(src, ToChar);

    /// <summary>
    /// 将String转换成可空Char，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static char? ToCharN(this string src, char defaultValue)
        => ToN(src, defaultValue, ToChar);
    #endregion

    #region DateTime
    /// <summary>
    /// 将String转换成DateTime
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this string src)
        => To(src, ConverterBuilder.GetDateTimeStringConverter(null));

    /// <summary>
    /// 将String转换成DateTime，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this string src, DateTime defaultValue)
        => string.IsNullOrEmpty(src) ? defaultValue : To(src, defaultValue, ToDateTime);

    /// <summary>
    /// 将String转换成可空DateTime
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <returns></returns>
    public static DateTime? ToDateTimeN(this string src)
        => ToN(src, ToDateTime);

    /// <summary>
    /// 将String转换成可空DateTime，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static DateTime? ToDateTimeN(this string src, DateTime defaultValue)
        => ToN(src, defaultValue, ToDateTime);
    /// <summary>
    /// 将String转换成DateTime
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="format">日期格式说明符：yyyyMMdd HH:mm:ss</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this string src, string format)
        => To(src, ConverterBuilder.GetDateTimeStringConverter(format));

    /// <summary>
    /// 将String转换成DateTime，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="format">日期格式说明符：yyyyMMdd HH:mm:ss</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static DateTime ToDateTime(this string src, string format, DateTime defaultValue)
    {
        DateTime ret;
        try
        {
            ret = string.IsNullOrEmpty(src) ? defaultValue : ToDateTime(src, format);
        }
        catch
        {
            ret = defaultValue;
        }
        return ret;
    }

    /// <summary>
    /// 将String转换成可空DateTime
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="format">日期格式说明符：yyyyMMdd HH:mm:ss</param>
    /// <returns></returns>
    public static DateTime? ToDateTimeN(this string src, string format)
        => (!string.IsNullOrEmpty(src)) ? ToDateTime(src, format) : (DateTime?)null;

    /// <summary>
    /// 将String转换成可空DateTime，如果转换失败则使用默认值。
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="format">日期格式说明符：yyyyMMdd HH:mm:ss</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static DateTime? ToDateTimeN(this string src, string format, DateTime defaultValue)
        => (!string.IsNullOrEmpty(src)) ? ToDateTime(src, format, defaultValue) : (DateTime?)null;

    #endregion
}