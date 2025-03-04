using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AiUo.Collections;

public static class ArrayUtil
{
    /// <summary>
    /// 获取二维数组中指定Point的数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <param name="points"></param>
    /// <returns></returns>
    public static T[] GetTwoDValues<T>(T[,] arr, IEnumerable<Point> points)
    {
        var ps = points.ToList();
        var ret = new T[ps.Count];
        for (int i = 0; i < ps.Count; i++)
        {
            var p = ps[i];
            ret[i] = arr[p.X, p.Y];
        }
        return ret;
    }
    /// <summary>
    /// 二维数组转换成行值数组的List集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static List<T[]> TwoDToListRows<T>(T[,] arr)
    {
        var ret = new List<T[]>();
        for (int i = 0; i < arr.GetLength(0); i++)
        {
            var row = GetTwoDRow(arr, i);
            ret.Add(row);
        }
        return ret;
    }
    /// <summary>
    /// 二维数组转换成列值数组的List集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static List<T[]> TwoDToListColumns<T>(T[,] arr)
    {
        var ret = new List<T[]>();
        for (int i = 0; i < arr.GetLength(1); i++)
        {
            var column = GetTwoDColumn(arr, i);
            ret.Add(column);
        }
        return ret;
    }

    /// <summary>
    /// 获取二维数组中的一行
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <param name="rowIndex">从0开始</param>
    /// <returns></returns>
    public static T[] GetTwoDRow<T>(T[,] arr, int rowIndex)
    {
        var len = arr.GetLongLength(1);
        var ret = new T[len];
        for (int i = 0; i < len; i++)
        {
            ret[i] = arr[rowIndex, i];
        }
        return ret;
    }
    /// <summary>
    /// 获取二维数组中的一列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <param name="columnIndex">从0开始</param>
    /// <returns></returns>
    public static T[] GetTwoDColumn<T>(T[,] arr, int columnIndex)
    {
        var len = arr.GetLongLength(0);
        var ret = new T[len];
        for (int i = 0; i < len; i++)
        {
            ret[i] = arr[i, columnIndex];
        }
        return ret;
    }

    /// <summary>
    /// 一维数组转换成二维数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <param name="len"></param>
    /// <returns></returns>
    public static T[,] OneToTwoD<T>(T[] arr, int len)
    {
        if (arr.Length % len != 0)
            return null;
        int width = arr.Length / len;
        var ret = new T[len, width];
        for (int i = 0; i < arr.Length; i++)
        {
            ret[i / width, i % width] = arr[i];
        }
        return ret;
    }

    /// <summary>
    /// 二维数组转换成一维数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static T[] TwoToOneD<T>(T[,] arr)
    {
        T[] ret = new T[arr.Length];
        for (int i = 0; i < arr.Length; i++)
            ret[i] = arr[i / arr.GetLength(1), i % arr.GetLength(1)];
        return ret;
    }

    #region EnumerateTwoD
    public static IEnumerable<(T value, Point point)> EnumerateTwoD<T>(this T[,] arr)
    {
        for (int x = 0; x < arr.GetLength(0); x++)
        for (int y = 0; y < arr.GetLength(1); y++)
        {
            yield return (arr[x, y], new Point(x, y));
        }
    }
    public static IEnumerable<T> EnumerateTwoDValues<T>(this T[,] arr)
    {
        for (int x = 0; x < arr.GetLength(0); x++)
        for (int y = 0; y < arr.GetLength(1); y++)
        {
            yield return arr[x, y];
        }
    }
    public static IEnumerable<T> EnumerateTwoDValuesByColumn<T>(this T[,] arr)
    {
        for (int y = 0; y < arr.GetLength(1); y++)
        for (int x = 0; x < arr.GetLength(0); x++)
        {
            yield return arr[x, y];
        }
    }
    public static IEnumerable<Point> EnumerateTwoDPoints<T>(this T[,] arr)
    {
        for (int x = 0; x < arr.GetLength(0); x++)
        for (int y = 0; y < arr.GetLength(1); y++)
        {
            yield return new Point(x, y);
        }
    }
    public static IEnumerable<Point> EnumerateTwoDPointsByColumn<T>(this T[,] arr)
    {
        for (int y = 0; y < arr.GetLength(1); y++)
        for (int x = 0; x < arr.GetLength(0); x++)
        {
            yield return new Point(x, y);
        }
    }
    #endregion
}