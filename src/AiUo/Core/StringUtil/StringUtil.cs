using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AiUo;

/// <summary>
/// 字符串操作静态辅助类
/// </summary>
public static partial class StringUtil
{
    private static readonly object _locker = new object();

    public static bool IsEmpty(this string str)
        => string.IsNullOrEmpty(str);

    /// <summary>
    /// 获取不包含'-'的GUID字符串
    /// </summary>
    /// <param name="removeSymbol">是否替换间隔符号-</param>
    /// <returns></returns>
    public static string GetGuidString(bool removeSymbol = true)
    {
        var ret = Convert.ToString(Guid.NewGuid());
        return removeSymbol ? ret.Replace("-", "") : ret;
    }

    /// <summary>
    /// 编辑距离（Levenshtein Distance），计算字符串相似度
    /// </summary>
    /// <param name="source">源字符串</param>
    /// <param name="target">目标字符串</param>
    /// <param name="ignoreCase">是否忽略大小写</param>
    /// <returns></returns>
    public static int LevenshteinDistance(string source, string target, bool ignoreCase = true)
    {
        int n = source.Length;
        int m = target.Length;
        int[,] d = new int[n + 1, m + 1];

        // Step 1
        if (n == 0) return m;
        if (m == 0) return n;

        if (ignoreCase)
        {
            source = source.ToLower();
            target = target.ToLower();
        }

        // Step 2
        for (int i = 0; i <= n; d[i, 0] = i++) { }

        for (int j = 0; j <= m; d[0, j] = j++) { }

        // Step 3
        for (int i = 1; i <= n; i++)
        {
            //Step 4
            for (int j = 1; j <= m; j++)
            {
                // Step 5
                int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                // Step 6
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        // Step 7
        return d[n, m];
    }

    /// <summary>
    /// 使用camel命名法
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string CamelCase(this string name)
        => char.ToLower(name[0]) + name.Substring(1);

    /// <summary>
    /// 使用Pascal命名法
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string PascalCase(this string name)
        => char.ToUpper(name[0]) + name.Substring(1);

    /// <summary>
    /// 将字符串按NewLine进行Split
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static string[] SplitNewLine(this string src)
        => src.Trim().Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
    /// <summary>
    /// 将字符串按空格进行Split
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static string[] SplitSpace(this string src)
        => src.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
    // C#保留字
    private static Regex CSharpReserved = new Regex("^(ABSTRACT|AS|BASE|BOOL|BREAK|BYTE|CASE|CATCH|CHAR|CHECKED|CLASS|CONST|CONTINUE|DECIMAL|DEFAULT|DELEGATE|DO|DOUBLE|ELSE|ENUM|EVENT|EXPLICIT|EXTERN|FALSE|FINALLY|FIXED|FLOAT|FOR|FOREACH|GET|GOTO|IF|IMPLICIT|IN|INT|INTERFACE|INTERNAL|IS|LOCK|LONG|NAMESPACE|NEW|NULL|OBJECT|OPERATOR|OUT|OVERRIDE|PARAMS|PARTIAL|PRIVATE|PROTECTED|PUBLIC|READONLY|REF|RETURN|SBYTE|SEALED|SET|SHORT|SIZEOF|STACKALLOC|STATIC|STRING|STRUCT|SWITCH|THIS|THROW|TRUE|TRY|TYPEOF|UINT|ULONG|UNCHECKED|UNSAFE|USHORT|USING|VALUE|VIRTUAL|VOID|VOLATILE|WHERE|WHILE|YIELD)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// 是否是.NET保留字
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static bool IsCSharpReserved(this string src)
        => CSharpReserved.IsMatch(src);

    /// <summary>
    /// 获取连续TAB输入字符串
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    public static string Tab(int n)
        => (n == 0) ? string.Empty : new string('\t', n);

    /// <summary>
    /// 获取字符串第一行，没有返回string.Empty
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static string GetFirstLine(string src)
    {
        var strs = SplitNewLine(src);
        return (strs != null && strs.Length > 0) ? strs[0] : string.Empty;
    }

    /// <summary>
    /// 查找字符串中指定字符匹配索引（跳过指定次数）
    /// 如：IndexOf("192.12.0.0", '.', 1); 返回6
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <param name="skipNum">跳过的次数</param>
    /// <returns></returns>
    public static int IndexOf(string str, char value, int skipNum)
    {
        int currSkip = 0;
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == value)
            {
                if (currSkip == skipNum)
                    return i;
                currSkip++;
            }
        }
        return -1;
    }
    /// <summary>
    /// 格式化字符串固定长度显示
    /// </summary>
    /// <param name="src"></param>
    /// <param name="fixedLen"></param>
    /// <param name="leftAlign">true:左对齐</param>
    /// <param name="paddingChar">填充字符</param>
    /// <returns></returns>
    public static string FormatFixedLength(object src, int fixedLen, bool leftAlign = true, char paddingChar = ' ')
    {
        var str = Convert.ToString(src);
        return leftAlign ? str.PadRight(fixedLen, paddingChar) : str.PadLeft(fixedLen, paddingChar);
    }

    /// <summary>
    /// 根据标识提取字符串中的内容
    /// 如: ExtractMatches("abcd:123->efg", ":", "->") => 123
    /// </summary>
    /// <param name="src"></param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static List<string> ExtractMatches(this string src, string begin, string end)
    {
        if (string.IsNullOrEmpty(src))
            return null;
        var ret = new List<string>();
        var r = new Regex($@"\{begin}(\w+)\{end}");
        var ms = r.Matches(src);
        foreach (Match m in ms)
        {
            ret.Add(Convert.ToString(m.Groups[1]));
        }
        return ret;
    }

    /// <summary>
    /// 字符串固定长度显示，指定位置用*隐藏
    /// </summary>
    /// <param name="src"></param>
    /// <param name="toFixedLength">转换后需要显示的字符串总长度</param>
    /// <param name="beginReservedLen">起始保留的字符串长度。0-不保留</param>
    /// <param name="endReservedLen">结尾保留的字符串长度。0-不保留</param>
    /// <param name="paddingChar">隐藏时显示的字符</param>
    /// <returns></returns>
    public static string HideFixedLength(this string src, int toFixedLength, int beginReservedLen, int endReservedLen, char paddingChar = '*')
    {
        if (string.IsNullOrEmpty(src) || (beginReservedLen == 0 && endReservedLen == 0))
            return string.Empty.PadRight(toFixedLength, paddingChar);
        if (src.Length < toFixedLength)
            return src.PadRight(toFixedLength, paddingChar);
        if (beginReservedLen == 0)
            return src.Substring(src.Length - endReservedLen).PadLeft(toFixedLength, paddingChar);
        if (endReservedLen == 0)
            return src.Substring(0, beginReservedLen).PadRight(toFixedLength, paddingChar);
        var start = src.Substring(0, beginReservedLen);
        var end = src.Substring(src.Length - endReservedLen);
        return start.PadRight(toFixedLength - endReservedLen, paddingChar) + end;
    }

    public static string Max(string s1, string s2)
    {
        return s1.CompareTo(s2) > 0 ? s1 : s2;
    }
    public static string Min(string s1, string s2)
    {
        return s1.CompareTo(s2) < 0 ? s1 : s2;
    }
}