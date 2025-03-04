using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;

namespace AiUo.Collections;

/// <summary>
/// 集合类扩展方法
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// 集合对象分页
    /// </summary>
    /// <typeparam name="T">集合泛型类型</typeparam>
    /// <param name="list">集合对象</param>
    /// <param name="size">分页大小</param>
    /// <param name="index">当前页索引，从1开始</param>
    /// <returns></returns>
    public static IEnumerable<T> ToPage<T>(this IEnumerable<T> list, int size, int index)
        => list.Skip((index - 1) * size).Take(size);

    /// <summary>
    /// 集合分页成列表集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public static List<List<T>> ToPage<T>(this List<T> list, int pageSize)
    {
        var ret = new List<List<T>>();
        var pageCount = AiUoUtil.GetPageCount(list.Count, pageSize);
        for (int i = 1; i <= pageCount; i++)
        {
            var page = list.ToPage(pageSize, i).ToList();
            ret.Add(page);
        }
        return ret;
    }

    /// <summary>
    /// 遍历每个元素使用Action操作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="action"></param>
    public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
    {
        if (list == null) return;
        foreach (T item in list)
            action(item);
    }
    /// <summary>
    /// 遍历每个元素使用Action操作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="action"></param>
    public static void ForEach<T>(this ICollection list, Action<T> action)
    {
        if (list == null) return;
        foreach (var item in list)
            action((T)item);
    }

    /// <summary>
    /// 遍历每个元素使用Action异步操作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static async Task ForEachAsync<T>(this IEnumerable<T> list, Func<T, Task> action)
    {
        if (list == null) return;
        foreach (var item in list)
        {
            await action(item);
        }
    }

    /// <summary>
    /// 集合中是否有值
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static bool HasValue(this ICollection list)
        => list != null && list.Count > 0;
    public static bool HasValue<T>(this IEnumerable<T> list)
        => list != null && list.Count() > 0;

    public static List<TDest> ToList<TSource, TDest>(this IEnumerable<TSource> source, Func<TSource, TDest> selector)
    {
        return source.Select(selector).ToList();
    }

    #region Array Exts

    /// <summary>
    /// 向Array中添加一个元素
    /// </summary>
    /// <typeparam name="T">泛型</typeparam>
    /// <param name="array">Array</param>
    /// <param name="item">需要添加项</param>
    /// <returns>返回新的Array</returns>
    public static T[] Add<T>(this T[] array, T item)
    {
        int _count = array.Length;
        Array.Resize(ref array, _count + 1);
        array[_count] = item;
        return array;
    }

    /// <summary>
    /// 向Array中添加一个集合
    /// </summary>
    /// <typeparam name="T">泛型</typeparam>
    /// <param name="sourceArray">Array</param>
    /// <param name="addArray">Array</param>
    /// <returns>返回新的Array</returns>
    public static T[] AddRange<T>(this T[] sourceArray, T[] addArray)
    {
        int _count = sourceArray.Length;
        int _addCount = addArray.Length;
        Array.Resize(ref sourceArray, _count + _addCount);
        addArray.CopyTo(sourceArray, _count);
        return sourceArray;
    }
    #endregion
}