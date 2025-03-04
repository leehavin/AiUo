using System;

namespace AiUo;

/// <summary>
/// 字符范围枚举，可使用StringUtil类获取相应的字符集合
/// </summary>
[Flags]
public enum CharsScope
{
    /// <summary>
    /// 数字 = 1
    /// </summary>
    Numbers = 1,

    /// <summary>
    /// 小写字母 = 2
    /// </summary>
    LowerLetters = 2,

    /// <summary>
    /// 大写字母 = 4
    /// </summary>
    UpperLetters = 4,

    /// <summary>
    /// 字母 = 6
    /// </summary>
    Letters = LowerLetters | UpperLetters,

    /// <summary>
    /// 数字和小写字母 = 3
    /// </summary>
    NumbersAndLowerLetters = Numbers | LowerLetters,

    /// <summary>
    /// 数字和大写字母 = 5
    /// </summary>
    NumbersAndUpperLetters = Numbers | UpperLetters,

    /// <summary>
    /// 数字字母 = 7
    /// </summary>
    NumbersAndLetters = Numbers | Letters,

    /// <summary>
    /// 常用汉字 = 8
    /// </summary>
    UsualChinese = 8,

    /// <summary>
    /// 数字，字母和汉字 = 15
    /// </summary>
    All = Numbers | Letters | UsualChinese
}