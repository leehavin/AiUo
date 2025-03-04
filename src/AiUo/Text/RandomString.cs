using System;
using System.Collections.Generic;
using System.Text;

namespace AiUo.Text;

/// <summary>
/// 随机字符串生成器,包括随机生成汉字，字母和数字
/// </summary>
public class RandomString
{
    #region Members
    private static Dictionary<CharsScope, char[]> _cache = new Dictionary<CharsScope, char[]>();
    private static object _locker = new object();

    /// <summary>
    /// 获取或设置可选取的字符范围
    /// </summary>
    public char[] Scope { get; set; }

    /// <summary> 
    /// 获取或设置随机String的长度 
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="scope">可选取的字符范围</param>
    /// <param name="length">随机字符串的长度</param>
    public RandomString(CharsScope scope, int length)
    {
        this.Scope = GetCacheChars(scope);
        this.Length = length;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="scope">可选取的字符范围</param>
    /// <param name="length">随机字符串的长度</param>
    public RandomString(char[] scope, int length)
    {
        this.Scope = scope;
        this.Length = length;
    }
    #endregion

    /// <summary> 
    /// 获得随机字符串 
    /// </summary>
    /// <param name="scope">可选取的字符范围</param>
    /// <param name="length">生成随机String的长度</param>
    /// <returns>返回随机字符串</returns>
    public static string Next(CharsScope scope, int length)
        => Next(GetCacheChars(scope), length);

    /// <summary>
    /// 获得随机字符串 
    /// </summary>
    /// <param name="scope">可选取的字符范围</param>
    /// <param name="length">生成随机String的长度</param>
    /// <returns></returns>
    public static string Next(char[] scope, int length)
    {
        Random rnd = new Random();
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            int idx = rnd.Next(0, scope.Length);
            sb.Append(scope[idx]);
        }
        return sb.ToString();
    }

    /// <summary>
    /// 获得随机字符串
    /// </summary>
    /// <returns></returns>
    public string Next()
        => Next(Scope, Length);

    //获得目标字符数组
    private static char[] GetCacheChars(CharsScope scope)
    {
        if (!_cache.ContainsKey(scope))
        {
            lock (_locker)
            {
                if (!_cache.ContainsKey(scope))
                {
                    _cache.Add(scope, StringUtil.GetCharArray(scope));
                }
            }
        }
        return _cache[scope];
    }
}