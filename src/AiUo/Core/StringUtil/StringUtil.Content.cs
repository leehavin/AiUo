using System;
using System.Text;
using System.Linq;

namespace AiUo;

/// <summary>
/// 字符串操作静态辅助类
/// </summary>
public static partial class StringUtil
{
    #region 字符串宽度处理
    /// <summary>
    /// 获取字符串的实际宽度
    /// 全角符号汉字等占2个宽度
    /// </summary>
    /// <param name="str">字符串</param>
    /// <returns></returns>
    public static int GetStringWidth(this string str)
        => str.Sum(chr => GetCharWidth(chr));

    /// <summary>
    /// 获取字符的实际宽度
    /// 全角符号汉字等占2个宽度
    /// </summary>
    /// <param name="chr">字符</param>
    /// <returns></returns>
    public static int GetCharWidth(char chr)
        => (chr < 255) ? 1 : 2;

    /// <summary>
    /// 截取字符串不超过指定宽度
    /// </summary>
    /// <param name="str">字符串</param>
    /// <param name="width">指定宽度，全角符号汉字等占2个宽度</param>
    /// <returns></returns>
    public static string TrimWidth(this string str, int width)
    {
        string ret = null;
        if (width >= str.Length * 2) return str;
        for (int i = 0; i < str.Length; i++)
        {
            width -= GetCharWidth(str[i]);
            if (width <= 0)
            {
                if (width == 0)
                    ret = str.Substring(0, i + 1);
                else if (width == -1)
                    ret = str.Substring(0, i);
                break;
            }
        }
        return ret ?? str;
    }

    /// <summary>
    /// 截取字符串不超过指定宽度，如超过使用..或...结尾
    /// </summary>
    /// <param name="str"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    public static string TrimWidthSuffix(this string str, int width)
    {
        if (width >= str.Length * 2) return str;
        string ret = null;
        int currWidth = 0;
        for (int i = 0; i < str.Length; i++)
        {
            currWidth += GetCharWidth(str[i]);
            if (currWidth < width) continue;
            if (currWidth == width)
            {
                if (i < str.Length - 1) // 不是最后一个字符
                {
                    str = (GetCharWidth(str[i]) == 2) ? str.Substring(0, i) + ".."
                        : str.Substring(0, i - 1) + new string('.', GetCharWidth(str[i - 1]) + 1);
                }
            }
            else
            {
                str = (GetCharWidth(str[i - 1]) == 2) ? str.Substring(0, i - 1) + "..."
                    : str.Substring(0, i - 1) + "..";
            }
            break;
        }
        return ret ?? str;
    }

    #endregion

    #region 字符串内容处理 Replace Trim 
    /// <summary>
    /// 替换空白字符
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="newValue">使用此字符串替换空白字符</param>
    /// <param name="repeated">如果出现连续的空白字符是否重复替换</param>
    /// <returns></returns>
    public static string ReplaceWhiteSpace(this string src, string newValue, bool repeated = false)
    {
        if (string.IsNullOrEmpty(src)) return src;

        int lastReplace = int.MinValue;
        StringBuilder ret = new StringBuilder();
        for (int i = 0; i < src.Length; i++)
        {
            char chr = src[i];
            if (char.IsWhiteSpace(chr))
            {
                if (repeated)
                    ret.Append(newValue);
                else
                {
                    if (lastReplace != i - 1)
                        ret.Append(newValue);
                    lastReplace = i;
                }
            }
            else
                ret.Append(chr);
        }
        return ret.ToString();
    }

    /// <summary>
    /// 替换空白字符
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="newChar">使用此字符替换空白字符</param>
    /// <param name="repeated">如果出现连续的空白字符是否重复替换</param>
    /// <returns></returns>
    public static string ReplaceWhiteSpace(this string src, char newChar, bool repeated = true)
        => ReplaceWhiteSpace(src, newChar.ToString(), repeated);

    /// <summary>
    /// 使用指定char替换指定位置长度的字符
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="newChar">使用此字符替换指定位置</param>
    /// <param name="beginIdx">替换起始位置，0开始</param>
    /// <param name="length">替换长度，必须大于1</param>
    /// <returns></returns>
    public static string Replace(this string src, char newChar, int beginIdx, int length)
    {
        if (src.Length < beginIdx + length)
            throw new Exception("起始位置加上替换长度大于源字符串长度。");
        if (beginIdx < 0)
            throw new Exception("参数beginIdx必须大于0");
        if (length < 1)
            throw new Exception("参数length必须大于1");

        string ret = string.Empty;
        if (beginIdx > 0)
            ret = src.Substring(0, beginIdx);
        ret += new string(newChar, length);
        if (src.Length - beginIdx - length > 0)
            ret += src.Substring(beginIdx + length);
        return ret;
    }

    /// <summary>
    /// 从当前 System.String 对象移除指定字符的所有前导匹配项
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="start">要删除的起始字符串</param>
    /// <param name="ignoreCase">是否忽略大小写</param>
    /// <returns></returns>
    public static string TrimStart(this string src, string start, bool ignoreCase = true)
    {
        string ret = src;
        while (true)
        {
            if (ret.StartsWith(start, ignoreCase, null))
            {
                ret = ret.Substring(start.Length);
                continue;
            }
            break;
        }
        return ret;
    }

    /// <summary>
    /// 从当前 System.String 对象移除指定字符的所有尾部匹配项
    /// </summary>
    /// <param name="src">源字符串</param>
    /// <param name="end">要删除的结束字符串</param>
    /// <param name="ignoreCase">是否忽略大小写</param>
    /// <returns></returns>
    public static string TrimEnd(this string src, string end, bool ignoreCase = true)
    {
        string ret = src;
        while (true)
        {
            if (ret.EndsWith(end, ignoreCase, null))
            {
                ret = ret.Substring(0, ret.Length - end.Length);
                continue;
            }
            break;
        }
        return ret;
    }

    /// <summary>
    /// 从当前 System.String 对象移除指定字符的所有前导和尾部匹配项
    /// </summary>
    /// <param name="src"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public static string Trim(this string src, string start, string end, bool ignoreCase = true)
        => src.TrimStart(start, ignoreCase).TrimEnd(end, ignoreCase);
    #endregion
}