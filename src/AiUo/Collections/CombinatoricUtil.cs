using System;
using System.Collections.Generic;
using System.Linq;
using AiUo.Collections.Combinatorics;

namespace AiUo.Collections;

/// <summary>
/// 排列组合辅助类 Kaos.Combinatorics
/// </summary>
public static class CombinatoricUtil
{
    /// <summary>
    /// 实现Javascript Splice函数的功能。例如：int[] a = { 1, 2, 3, 4, 5, 6 };
    ///     删除: Splice(ref a, 2, 2)       => a=1-2-5-6         =>返回: 3-4
    ///     插入: Splice(ref a, 2, 0, 9, 9) => a=1-2-9-9-3-4-5-6 => 
    ///     替换: Splice(ref a, 2, 1, 9, 9) => a=1-2-9-9-4-5-6   =>返回：3
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sourceArray">原数组</param>
    /// <param name="sourceIndex">起始索引位</param>
    /// <param name="length">长度。=0:插入 >0:被替换或删除的个数</param>
    /// <param name="insertedElements">插入的元素</param>
    /// <returns>返回变动的元素数组</returns>
    public static T[] Splice<T>(ref T[] sourceArray, int sourceIndex, int length, params T[] insertedElements)
    {
        if (sourceArray == null)
        {
            throw new ArgumentNullException(nameof(sourceArray));
        }
        if (sourceIndex < 0 || sourceIndex >= sourceArray.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceIndex));
        }
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        // 如果要截取的元素个数大于从sourceIndex开始到数组结束的元素个数
        int acturalLength = (sourceIndex + length) > sourceArray.Length ? (sourceArray.Length - sourceIndex) : length;

        var deletedItems = new T[acturalLength];
        Array.ConstrainedCopy(sourceArray, sourceIndex, deletedItems, 0, acturalLength);

        int arrayLengthDifference = insertedElements.Length - acturalLength;
        int newArrayLength = sourceArray.Length + arrayLengthDifference;
        var newArray = new T[newArrayLength];

        int newArrayCopyedElementsIndex = sourceIndex;
        Array.Copy(sourceArray, newArray, newArrayCopyedElementsIndex);


        Array.ConstrainedCopy(insertedElements, 0, newArray, newArrayCopyedElementsIndex, insertedElements.Length);
        newArrayCopyedElementsIndex += insertedElements.Length;

        int remainedElementsIndex = sourceIndex + acturalLength;
        // 源数组末尾还有元素才进行拷贝
        if (remainedElementsIndex < sourceArray.Length)
        {
            Array.ConstrainedCopy(sourceArray, remainedElementsIndex, newArray, newArrayCopyedElementsIndex, sourceArray.Length - remainedElementsIndex);
        }

        sourceArray = newArray;

        return deletedItems;
    }

    /// <summary>
    /// [不重复,指定个数,从小到大排列组合]从choices个数字中获取picks个不重复的数字组成从小到大的排列组合
    /// 例：choices=3，picks=2 ==> 01,02,12
    /// </summary>
    /// <param name="choices">要选择的值的数量</param>
    /// <param name="picks">序列中的元素数</param>
    /// <returns></returns>
    public static IEnumerable<CombinatoricItem> GetCombination(int choices, int picks)
        => new Combination(choices, picks).GetRows().Select(src=> new CombinatoricItem { Rank = src.Rank, Value=src.ToList()});

    /// <summary>
    /// [不重复,任意个数,从小到大排列组合]从choices个数字中获取任意个不重复的数字组成从小到大的排列组合
    /// 例：choices=3 ==> 0,1,2,01,02,12,012
    /// </summary>
    /// <param name="choices"></param>
    /// <returns></returns>
    public static IEnumerable<CombinatoricItem> GetCombinationForAllPicks(int choices)
        => new Combination(choices).GetRowsForAllPicks().Select(src => new CombinatoricItem { Rank = src.Rank, Value = src.ToList() });

    /// <summary>
    /// [不重复,指定个数,无序排列组合]从choices个数字中获取picks个不重复的数字组成无序的排列组合
    /// 例：choices=3，picks=2 ==> 01,02,10,12,20,21
    /// </summary>
    /// <param name="choices">要选择的值的数量</param>
    /// <param name="picks">序列中的元素数</param>
    /// <returns></returns>
    public static IEnumerable<CombinatoricItem> GetPermutation(int choices, int picks)
        => new Permutation(choices, picks).GetRows().Select(src => new CombinatoricItem { Rank = src.Rank, Value = src.ToList() });

    /// <summary>
    /// [不重复,任意个数,无序排列组合]从choices个数字中获取任意个不重复的数字组成无序的排列组合
    /// 例：choices=3，picks=2 ==> 0,01,10,012,021,102,120,201,210
    /// </summary>
    /// <param name="choices">要选择的值的数量</param>
    /// <returns></returns>
    public static IEnumerable<CombinatoricItem> GetPermutationForAllChoices(int choices)
        => new Permutation(choices).GetRowsForAllChoices().Select(src => new CombinatoricItem { Rank = src.Rank, Value = src.ToList() });

    /// <summary>
    /// [可重复,指定个数,从小到大排列组合]从choices个数字中获取picks个可重复的数字组成从小到大的排列组合
    /// 例：choices=3，picks=2 ==>  00,01,02,11,12,22
    /// </summary>
    /// <param name="choices"></param>
    /// <param name="picks"></param>
    /// <returns></returns>
    public static IEnumerable<CombinatoricItem> GetMulticombination(int choices, int picks)
        => new Multicombination(choices, picks).GetRows().Select(src => new CombinatoricItem { Rank = src.Rank, Value = src.ToList() });

    /// <summary>
    /// [可重复指定个小到大]从choices个数字中获取指定多个可重复的数字组成从小到大的排列组合
    /// 例：choices=3，startPicks=1, stopPicks=2 ==> 0,1,2,00,01,02,11,12,22
    /// </summary>
    /// <param name="choices"></param>
    /// <param name="startPicks"></param>
    /// <param name="stopPicks"></param>
    /// <returns></returns>
    public static IEnumerable<CombinatoricItem> GetMulticombinationForPicks(int choices, int startPicks, int stopPicks)
        => new Multicombination(choices).GetRowsForPicks(startPicks, stopPicks).Select(src => new CombinatoricItem { Rank = src.Rank, Value = src.ToList() });

    /// <summary>
    /// [可重复指定个无序排]通过sizes指定每一列的范围，获取sizes.Length个可重复的数字组成无序的排列组合
    /// 例：int[] sizes = { 2, 2 } ==> 00,01,10,11
    /// </summary>
    /// <param name="sizes"></param>
    /// <returns></returns>
    public static IEnumerable<CombinatoricItem> GetProduct(int[] sizes)
        => new Product(sizes).GetRows().Select(src => new CombinatoricItem { Rank = src.Rank, Value = src.ToList() });
}

/// <summary>
/// 排列组合项
/// </summary>
public class CombinatoricItem
{
    /// <summary>
    /// 排名
    /// </summary>
    public long Rank { get; set; }
    /// <summary>
    /// 组合中元素个数
    /// </summary>
    public int Count => Value.Count;
    /// <summary>
    /// 当前排列组合元素
    /// </summary>
    public List<int> Value { get; set; }
        
    /// <summary>
    /// Item
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public int this[int index]
    {
        get => Value[index];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{Rank,8}: {{{string.Join(",", Value)}}}";
    }
}