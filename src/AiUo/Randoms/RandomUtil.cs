using System;
using System.Collections.Generic;
using System.Linq;

namespace AiUo.Randoms;

public static class RandomUtil
{
    private static RandomProvider _defaultProvider;
    public static RandomProvider DefaultProvider
    {
        get
        {
            if (_defaultProvider == null)
                _defaultProvider = RandomProviderFactory.CreateDefaultProvider();
            return _defaultProvider;
        }
        set
        {
            _defaultProvider = value;
        }
    }

    #region NextByte

    /// <summary>
    /// 生成一个byte随机数
    /// </summary>
    /// <returns></returns>
    public static byte NextByte() 
        => DefaultProvider.NextByte();

    /// <summary>
    /// [0, max) 返回大于等于0且小于max的非负随机数
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    public static byte NextByte(byte max) 
        => DefaultProvider.NextByte(max);

    /// <summary>
    /// [min, max) 返回大于等于min且小于max的随机数
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static byte NextByte(byte min, byte max) 
        => DefaultProvider.NextByte(min, max);

    /// <summary>
    ///  产生一个随机数组
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public static byte[] NextBytes(int size) 
        => DefaultProvider.NextBytes(size);

    /// <summary>
    /// [0, max) 返回大于等于0且小于max的非负随机数组
    /// </summary>
    /// <param name="size"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static byte[] NextBytes(int size, byte max) 
        => DefaultProvider.NextBytes(size, max);

    /// <summary>
    /// [min, max) 返回大于等于min且小于max的随机数组
    /// </summary>
    /// <param name="size"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static byte[] NextBytes(int size, byte min, byte max) 
        => DefaultProvider.NextBytes(size, min, max);

    #endregion

    #region NextInt

    /// <summary>
    /// 生成一个int随机数
    /// </summary>
    /// <returns></returns>
    public static int NextInt() 
        => DefaultProvider.NextInt();

    /// <summary>
    /// [0, max) 返回大于等于0且小于max的非负随机数
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    public static int NextInt(int max)
        => DefaultProvider.NextInt(max);

    /// <summary>
    /// [min, max) 返回大于等于min且小于max的随机数
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static int NextInt(int min, int max)
        => DefaultProvider.NextInt(min, max);

    /// <summary>
    ///  产生一个随机数组
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public static int[] NextInts(int size)
        => DefaultProvider.NextInts(size);

    /// <summary>
    /// [0, max) 返回大于等于0且小于max的非负随机数组
    /// </summary>
    /// <param name="size"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static int[] NextInts(int size, int max)
        => DefaultProvider.NextInts(size, max);

    /// <summary>
    /// [min, max) 返回大于等于min且小于max的随机数组
    /// </summary>
    /// <param name="size"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static int[] NextInts(int size, int min, int max)
        => DefaultProvider.NextInts(size, min, max);

    #endregion

    #region RandomNotRepeat
    /// <summary>
    /// 产生指定范围 range 内 size 个随机数数组（使用 NextInt 获取随机数）
    /// </summary>
    /// <param name="range">随机数范围</param>
    /// <param name="size">抽取多少个元素</param>
    /// <returns></returns>
    public static int[] RandomNotRepeat(int range, int size)
        => DefaultProvider.RandomNotRepeat(range, size);

    /// <summary>
    /// 产生指定范围 range 内 size 个随机数数组（使用 NextByte 获取随机数）
    /// </summary>
    /// <param name="range"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static int[] RandomNotRepeat(byte range, byte size)
        => DefaultProvider.RandomNotRepeat(range, size);

    /// <summary>
    /// 从指定源数组中按照不重复随机数组rnds顺序抽取元素生成新的不重复随机数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="src"></param>
    /// <param name="rnds"></param>
    /// <returns></returns>
    public static T[] RandomNotRepeat<T>(T[] src, int[] rnds)
        => DefaultProvider.RandomNotRepeat(src, rnds);

    /// <summary>
    /// 从指定数组中获取size个不重复随机数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="src"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static T[] RandomNotRepeat<T>(T[] src, byte size)
        => DefaultProvider.RandomNotRepeat(src, size);

    /// <summary>
    /// 从指定数组中获取size个不重复随机数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="src"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static T[] RandomNotRepeat<T>(T[] src, int size)
        => DefaultProvider.RandomNotRepeat(src, size);

    /// <summary>
    /// 数组重新随机排列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static List<T> Random<T>(List<T> list)
        => list?.OrderBy(x => Guid.NewGuid()).ToList();
    #endregion

    #region Utils
    /// <summary>
    /// 随机判定指定概率下是否命中
    /// </summary>
    /// <param name="ratio">概率，如 0.3</param>
    /// <param name="precision">精度，如 100</param>
    /// <returns></returns>
    public static bool IsOdds(decimal ratio, int precision = 100)
    {
        var rnd = NextInt(precision);
        var value = (int)(ratio * precision);
        return value > rnd;
    }
    #endregion
}