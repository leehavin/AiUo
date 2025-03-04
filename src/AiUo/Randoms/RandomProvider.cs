using System;

namespace AiUo.Randoms;

/// <summary>
/// 随机数生成提供程序
/// </summary>
public class RandomProvider : IDisposable
{
    public IRandomReader Reader;
    public ISamplingContainer Sampling;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="reader">随机数读取器</param>
    /// <param name="sampling">样本检测参数</param>
    public RandomProvider(IRandomReader reader = null, ISamplingContainer sampling = null)
    {
        Reader = reader ?? new RNGReader();
        Sampling = sampling;
    }

    #region NextByte

    /// <summary>
    /// 生成一个byte随机数
    /// </summary>
    /// <returns></returns>
    public byte NextByte()
    {
        var ret = NextByteBase();
        Sampling?.AddRandom(0, byte.MaxValue, ret);
        return ret;
    }
    /// <summary>
    /// [0, max) 返回大于等于0且小于max的非负随机数
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    public byte NextByte(byte max)
    {
        var ret = NextByteBase(max);
        Sampling?.AddRandom(0, max, ret);
        return ret;
    }
    /// <summary>
    /// [min, max) 返回大于等于min且小于max的随机数
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public byte NextByte(byte min, byte max)
    {
        var ret = NextByteBase(min, max);
        Sampling?.AddRandom(min, max, ret);
        return ret;
    }
    private static bool IsFairRoll(byte value, byte numSides)
    {
        int fullSetsOfValues = byte.MaxValue / numSides;
        return value < numSides * fullSetsOfValues;
    }

    /// <summary>
    ///  产生一个随机数组
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public byte[] NextBytes(int size)
    {
        var ret = NextBytesBase(size);
        Sampling?.AddRandom(0, byte.MaxValue, ret);
        return ret;
    }

    /// <summary>
    /// [0, max) 返回大于等于0且小于max的非负随机数组
    /// </summary>
    /// <param name="size"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public byte[] NextBytes(int size, byte max)
    {
        var ret = GetByteArray(size, () => NextByteBase(max));
        Sampling?.AddRandom(0, max, ret);
        return ret;
    }


    /// <summary>
    /// [min, max) 返回大于等于min且小于max的随机数组
    /// </summary>
    /// <param name="size"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public byte[] NextBytes(int size, byte min, byte max)
    {
        var ret = GetByteArray(size, () => NextByteBase(min, max));
        Sampling?.AddRandom(min, max, ret);
        return ret;
    }

    private static byte[] GetByteArray(int size, Func<byte> func)
    {
        var ret = new byte[size];
        for (int i = 0; i < size; i++)
        {
            ret[i] = func();
        }
        return ret;
    }
    private byte[] NextBytesBase(int size)
        => Reader.ReadBytes(size);
    private byte NextByteBase() => NextBytesBase(1)[0];
    private byte NextByteBase(byte max)
    {
        byte ret;
        do
        {
            ret = NextByteBase();
        }
        while (!IsFairRoll(ret, max));
        return (byte)(ret % max);
    }
    private byte NextByteBase(byte min, byte max)
    {
        var sides = (byte)(max - min);
        return (byte)(NextByteBase(sides) + min);
    }
    #endregion

    #region NextInt

    /// <summary>
    /// 生成一个int随机数
    /// </summary>
    /// <returns></returns>
    public int NextInt()
    {
        var ret = NextIntBase();
        Sampling?.AddRandom(0, int.MaxValue, ret);
        return ret;
    }

    /// <summary>
    /// [0, max) 返回大于等于0且小于max的非负随机数
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    public int NextInt(int max)
    {
        int ret = NextIntBase(max);
        Sampling?.AddRandom(0, max, ret);
        return ret;
    }

    /// <summary>
    /// [min, max) 返回大于等于min且小于max的随机数
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public int NextInt(int min, int max)
    {
        int ret = NextIntBase(min, max);
        Sampling?.AddRandom(min, max, ret);
        return ret;
    }
    /// <summary>
    ///  产生一个随机数组
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public int[] NextInts(int size)
    {
        var ret = NextIntsBase(size);
        Sampling?.AddRandom(0, int.MaxValue, ret);
        return ret;
    }
    /// <summary>
    /// [0, max) 返回大于等于0且小于max的非负随机数组
    /// </summary>
    /// <param name="size"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public int[] NextInts(int size, int max)
        => GetIntArray(size, () => NextIntBase(max));

    /// <summary>
    /// [min, max) 返回大于等于min且小于max的随机数组
    /// </summary>
    /// <param name="size"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public int[] NextInts(int size, int min, int max)
        => GetIntArray(size, () => NextIntBase(min, max));

    private int[] NextIntsBase(int size) => Reader.ReadInts(size);
    private int NextIntBase() => NextIntsBase(1)[0];
    private int NextIntBase(int max)
    {
        int ret;
        do
        {
            ret = NextIntBase();
        }
        while (!IsFairRoll(ret, max));
        return Math.Abs((ret % max));
    }
    private static bool IsFairRoll(int value, int numSides)
    {
        int fullSetsOfValues = int.MaxValue / numSides;
        return value < numSides * fullSetsOfValues;
    }
    private int NextIntBase(int min, int max)
    {
        if (min < 0)
            throw new Exception("只支持大于等于0的随机数");
        var sides = (max - min);
        return NextIntBase(sides) + min;
    }
    private static int[] GetIntArray(int size, Func<int> func)
    {
        var ret = new int[size];
        for (int i = 0; i < size; i++)
        {
            ret[i] = func();
        }
        return ret;
    }
    #endregion

    #region RandomNotRepeat
    /// <summary>
    /// 产生指定范围 range 内 size 个随机数数组（使用 NextInt 获取随机数）
    /// </summary>
    /// <param name="range">随机数范围</param>
    /// <param name="size">抽取多少个元素</param>
    /// <returns></returns>
    public int[] RandomNotRepeat(int range, int size)
    {
        var ret = RandomNotRepeat(range, size, (max) => NextIntBase(max));
        Sampling?.AddNotRepeat(range, size, ret);
        return ret;
    }

    /// <summary>
    /// 产生指定范围 range 内 size 个随机数数组（使用 NextByte 获取随机数）
    /// </summary>
    /// <param name="range"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public int[] RandomNotRepeat(byte range, byte size)
    {
        var ret = RandomNotRepeat(range, size, (max) => NextByteBase((byte)max));
        Sampling?.AddNotRepeat(range, size, ret);
        return ret;
    }

    private static int[] RandomNotRepeat(int range, int size, Func<int, int> nextFunc)
    {
        int[] ret = new int[size];
        int[] numbers = new int[range];
        for (int i = 0; i < range; i++)
            numbers[i] = i;
        int site = range;//设置上限 
        int idx;
        for (int i = 0; i < size; i++)
        {
            idx = nextFunc(site);
            ret[i] = numbers[idx]; //随机位取出并保存
            numbers[idx] = numbers[site - 1]; //最后一个数赋值到当前位
            site--; //去除最后一位
        }
        return ret;
    }

    /// <summary>
    /// 从指定源数组中按照不重复随机数组rnds顺序抽取元素生成新的不重复随机数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="src"></param>
    /// <param name="rnds"></param>
    /// <returns></returns>
    public T[] RandomNotRepeat<T>(T[] src, int[] rnds)
    {
        T[] ret = new T[rnds.Length];
        for (int i = 0; i < rnds.Length; i++)
        {
            ret[i] = src[rnds[i]];
        }
        return ret;
    }

    /// <summary>
    /// 从指定数组中获取size个不重复随机数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="src"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public T[] RandomNotRepeat<T>(T[] src, byte size)
        => RandomNotRepeat(src, size, (min, max) => NextByte((byte)min, (byte)max));

    /// <summary>
    /// 从指定数组中获取size个不重复随机数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="src"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public T[] RandomNotRepeat<T>(T[] src, int size)
        => RandomNotRepeat(src, size, NextInt);
    private static T[] RandomNotRepeat<T>(T[] src, int size, Func<int, int, int> nextFunc)
    {
        T[] ret = new T[size];
        T[] srcClone = (T[])src.Clone();
        int site = srcClone.Length;//设置上限 
        int idx;
        for (int i = 0; i < size; i++)
        {
            idx = nextFunc(0, site);
            ret[i] = srcClone[idx]; //随机位取出并保存
            srcClone[idx] = srcClone[site - 1]; //最后一个数赋值到当前位
            site--; //去除最后一位
        }
        return ret;
    }

    #endregion
    public void Dispose()
    {
        Reader?.Dispose();
        Sampling?.Dispose();
    }
}