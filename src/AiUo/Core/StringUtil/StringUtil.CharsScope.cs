using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace AiUo;

/// <summary>
/// 字符串操作静态辅助类
/// </summary>
public static partial class StringUtil
{
    #region 大小写字母数字的字符数组

    /// <summary>
    /// 数字字符数组
    /// </summary>
    public static readonly char[] NumberChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

    /// <summary>
    /// 小写字母字符数组
    /// </summary>
    public static readonly char[] LowerLetterChars = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

    /// <summary>
    /// 大写字母字符数组
    /// </summary>
    public static readonly char[] UpperLetterChars = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

    /// <summary>
    /// 字母字符数组
    /// </summary>
    public static readonly char[] LetterChars = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

    /// <summary>
    /// 进位制使用的字符，支持64进制以下
    /// </summary>
    public static readonly char[] NumeralRadixChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '+', '/' };
    /// <summary>
    /// 常用英文标点符号数组
    /// </summary>
    public static readonly char[] CommonPunctuation = { '`', '~', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '=', '+', '[', ']', '{', '}', '\\', ';', ':', '\'', '"', '"', ',', '.', '?', '/' };

    // 8,16,32,64进制字母所代表的10进制值缓存
    private static Dictionary<char, int> _numeralRadixCache = null;
        
    /// <summary>
    /// 进位制使用的字符对应的值
    /// </summary>
    public static Dictionary<char, int> NumeralRadixCache
    {
        get
        {
            if (_numeralRadixCache == null)
            {
                lock (_locker)
                {
                    if (_numeralRadixCache == null)
                    {
                        _numeralRadixCache = new Dictionary<char, int>(64);
                        for (int i = 0; i < NumeralRadixChars.Length; i++)
                        {
                            _numeralRadixCache.Add(NumeralRadixChars[i], i);
                        }
                    }
                }
            }
            return _numeralRadixCache;
        }
    }

    /// <summary>
    /// 数字字母字符数组
    /// </summary>
    public static readonly char[] NumberLetterChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

    /// <summary>
    /// 300个常用汉字
    /// </summary>
    public static readonly char[] UsualChineseChars = "的一是在不了有和人这中大为上个国我以要他时来用们生到作地于出就分对成会可主发年动同工也能下过子说产种面而方后多定行学法所民得经十三之进着等部度家电力里如水化高自二理起小物现实加量都两体制机当使点从业本去把性好应开它合还因由其些然前外天政四日那社义事平形相全表间样与关各重新线内数正心反你明看原又么利比或但质气第向道命此变条只没结解问意建月公无系军很情者最立代想已通并提直题党程展五果料象员革位入常文总次品式活设及管特件长求老头基资边流路级少图山统接知较将组见计别她手角期根论运农指几九区强放决西被干做必战先回则任取据处队南给色光门即保治北造百规热领七海口东导器压志世金增争济阶油思术极交受联什认六共权收证改清己美再采转更单风切打白教速花带安场身车例真务具万每目至达走积示议声报斗完类八离华名确才科张信马节话米整空元况今集温传土许步群广石记需段研界拉林律叫且究观越织装影算低持音众书布复容儿须际商非验连断深难近矿千周委素技备半办青省列习响约支般史感劳便团往酸历市克何除消构府称太准精值号率族维划选标写存候毛亲快效斯院查江型眼王按格养易置派层片始却专状育厂京识适属圆包火住调满县局照参红细引听该铁价严".ToCharArray();

    private static ConcurrentDictionary<CharsScope, char[]> _charArrayCache = new ConcurrentDictionary<CharsScope, char[]>();
    /// <summary>
    /// 获取指定字符选取范围的字符数组
    /// </summary>
    /// <param name="scope"></param>
    /// <returns></returns>
    public static char[] GetCharArray(CharsScope scope)
    {
        char[] ret = null;
        switch (scope)
        {
            case CharsScope.Numbers:
                ret = NumberChars;
                break;
            case CharsScope.LowerLetters:
                ret = LowerLetterChars;
                break;
            case CharsScope.UpperLetters:
                ret = UpperLetterChars;
                break;
            case CharsScope.Letters:
                ret = LetterChars;
                break;
            case CharsScope.NumbersAndLowerLetters:
                if (!_charArrayCache.TryGetValue(CharsScope.NumbersAndLowerLetters, out ret))
                {
                    ret = new char[NumberChars.Length + LowerLetterChars.Length];
                    Array.Copy(NumberChars, 0, ret, 0, NumberChars.Length);
                    Array.Copy(LowerLetterChars, 0, ret, NumberChars.Length, LowerLetterChars.Length);
                    _charArrayCache.TryAdd(CharsScope.NumbersAndLowerLetters, ret);
                }
                break;
            case CharsScope.NumbersAndUpperLetters:
                if (!_charArrayCache.TryGetValue(CharsScope.NumbersAndUpperLetters, out ret))
                {
                    ret = new char[NumberChars.Length + UpperLetterChars.Length];
                    Array.Copy(NumberChars, 0, ret, 0, NumberChars.Length);
                    Array.Copy(UpperLetterChars, 0, ret, NumberChars.Length, UpperLetterChars.Length);
                    _charArrayCache.TryAdd(CharsScope.NumbersAndUpperLetters, ret);
                }
                break;
            case CharsScope.NumbersAndLetters:
                if (!_charArrayCache.TryGetValue(CharsScope.NumbersAndLetters, out ret))
                {
                    ret = new char[NumberChars.Length + LetterChars.Length];
                    Array.Copy(NumberChars, 0, ret, 0, NumberChars.Length);
                    Array.Copy(LetterChars, 0, ret, NumberChars.Length, LetterChars.Length);
                    _charArrayCache.TryAdd(CharsScope.NumbersAndLetters, ret);
                }
                break;
            case CharsScope.UsualChinese:
                ret = UsualChineseChars;
                break;
            case CharsScope.All:
                if (!_charArrayCache.TryGetValue(CharsScope.All, out ret))
                {
                    ret = new char[NumberChars.Length + LetterChars.Length + UsualChineseChars.Length];
                    Array.Copy(NumberChars, 0, ret, 0, NumberChars.Length);
                    Array.Copy(LetterChars, 0, ret, NumberChars.Length, LetterChars.Length);
                    Array.Copy(UsualChineseChars, 0, ret, NumberChars.Length + LetterChars.Length, UsualChineseChars.Length);
                    _charArrayCache.TryAdd(CharsScope.All, ret);
                }
                break;
        }
        return ret;
    }
    #endregion//大小写字母数字的字符数组
}