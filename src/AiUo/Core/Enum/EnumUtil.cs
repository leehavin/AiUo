using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace AiUo;

/// <summary>
/// 枚举辅助类
/// </summary>
public static class EnumUtil
{
    #region EnumInfo

    /// <summary>
    /// 枚举描述缓存
    /// </summary>
    private static readonly ConcurrentDictionary<string, EnumInfo> _descsCache = new ConcurrentDictionary<string, EnumInfo>();

    /// <summary>
    /// 获取枚举类型的描述信息
    /// </summary>
    /// <param name="enumType"></param>
    /// <returns></returns>
    public static EnumInfo GetInfo(Type enumType)
    {
        EnumInfo ret = null;
        if (!_descsCache.TryGetValue(enumType.FullName, out ret))
        {
            ret = new EnumInfo(enumType);
            _descsCache.AddOrUpdate(enumType.FullName, ret, (key, value) => ret);
        }
        return ret;
    }

    /// <summary>
    /// 获取枚举类型的描述信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static EnumInfo GetInfo<T>() => GetInfo(typeof(T));

    /// <summary>
    /// 获取枚举项描述信息
    /// </summary>
    /// <param name="enumType"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static EnumItem GetItemInfo(Type enumType, object value) => GetInfo(enumType).GetItem(value);

    /// <summary>
    /// 获取枚举项描述信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static EnumItem GetItemInfo<T>(object value) => GetItemInfo(typeof(T), value);

    /// <summary>
    /// 获取枚举项描述信息
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static EnumItem GetItemInfo(this Enum value) => GetItemInfo(value.GetType(), value);

    /// <summary>
    /// 获取枚举的Description
    /// </summary>
    /// <param name="enumType"></param>
    /// <returns></returns>
    public static string GetDescription(Type enumType) => GetInfo(enumType).Description;

    /// <summary>
    /// 获取枚举的Description
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string GetDescription<T>() => GetInfo<T>().Description;

    /// <summary>
    /// 获取枚举项的Description
    /// </summary>
    /// <param name="enumType"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetItemDescription(Type enumType, object value) => GetItemInfo(enumType, value).Description;

    /// <summary>
    /// 获取枚举项的Description
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetItemDescription<T>(object value) => GetItemInfo<T>(value).Description;

    /// <summary>
    /// 获取枚举项的Description
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetItemDescription(this Enum value)
        => GetItemInfo(value.GetType(), value).Description;

    #endregion // EnumInfo

    /// <summary>
    /// 判断flags枚举值value是否包含flag
    /// </summary>
    /// <param name="value">枚举值，如5</param>
    /// <param name="flag">要判断的值，如3</param>
    /// <returns></returns>
    public static bool HasFlag(int value, int flag)
        => (value & flag) != 0;

    /// <summary>
    /// 判断flags枚举类型variable是否包含flag
    /// </summary>
    /// <param name="variable">要验证的Enum值</param>
    /// <param name="flag">判断value是否包含在variable中</param>
    /// <returns></returns>
    public static bool HasFlag(Enum variable, Enum flag)
    {
        var valueNum = Convert.ToUInt64(flag);
        return (Convert.ToUInt64(variable) & valueNum) == valueNum;
    }

    #region ToEnum
    /// <summary>
    /// 将int转换成Enum，失败抛出异常
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static T ToEnum<T>(this int value)
        where T : Enum
    {
        if (!Enum.IsDefined(typeof(T), value))
            throw new Exception($"int数值{value}在枚举{typeof(T).FullName}中没有定义");
        return (T)(object)value;
    }
    /// <summary>
    /// 将int转换成Enum，失败返回null
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static T? ToEnumN<T>(this int value)
        where T : struct
    {
        if (!Enum.IsDefined(typeof(T), value))
            return null;
        return (T)(object)value;
    }

    /// <summary>
    /// 将int转换成Enum，失败转换成默认值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static T ToEnum<T>(this int value, T defaultValue)
        where T : Enum
        => Enum.IsDefined(typeof(T), value) ? (T)(object)value : defaultValue;


    /// <summary>
    /// 将string转换成Enum，失败抛出异常
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static T ToEnum<T>(this string value)
        where T : Enum
    {
        return Enum.TryParse(typeof(T), value, true, out var ret)
            ? (T)ret 
            : throw new Exception($"string数值{value}在枚举{typeof(T).FullName}中没有定义");
    }

    /// <summary>
    /// 将string转换成Enum，失败返回null
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static T? ToEnumN<T>(this string value)
        where T : struct
    {
        return Enum.TryParse(typeof(T), value, true, out var ret)
            ? (T)ret : null;
    }

    /// <summary>
    /// 将string转换成Enum，失败转换成默认值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static T ToEnum<T>(this string value, T defaultValue)
        where T : Enum
    {
        return Enum.TryParse(typeof(T), value, true, out var ret)
            ? (T)ret : defaultValue;
    }

    /// <summary>
    /// 通过EnumMapAttribute指定枚举值的字符串映射转换
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static T ToEnumByMap<T>(this string value)
        where T : Enum
    {
        try
        {
            var info = GetInfo<T>();
            if (!info.TryGetItemByMap(value, out int v))
                throw new Exception($"Enum类型没有定义EnumMapAttribute进行映射。type: {typeof(T).FullName} string:{value}");
            return v.ToEnum<T>();
        }
        catch
        {
            throw new Exception($"string数值{value}在枚举{typeof(T).FullName}中没有定义");
        }
    }
    #endregion

    /// <summary>
    /// 将多个int项值转换成flags值
    /// </summary>
    /// <param name="values">取值范围0-31</param>
    /// <returns></returns>
    public static long GetFlagsValue(params int[] values)
    {
        if (values == null || values.Length == 0)
            return 0;
        long ret = 0;
        foreach (var value in values)
        {
            if (value < 0 || value > 31)
                throw new Exception("EnumUtil.GetTagsValue时，value值范围0-31");
            ret |= (long)1 << value;
        }
        return ret;
    }
    /// <summary>
    /// 解析flags值为多个项值
    /// </summary>
    /// <param name="flagsValue"></param>
    /// <returns></returns>
    public static List<int> ParseFlagsValue(long flagsValue)
    {
        var ret = new List<int>();
        if (flagsValue >= 0)
        {
            for (int i = 0; i < 32; i++)
            {
                var id = (int)(flagsValue & (1 << i));
                if (id > 0)
                    ret.Add(i);
            }
        }
        return ret;
    }
}