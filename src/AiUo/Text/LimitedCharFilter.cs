using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AiUo.Text;

/// <summary>
/// 字符限制检查器，用于检验或替换字符串，常用于注册
/// 存在有两种模式：
///     1）只允许模式，不存在于已定义的字符集合中的字符都是被禁止的
///     2）只禁止模式，不存在于已定义的字符集合中的字符都是被允许的
/// </summary>
public class LimitedCharFilter
{
    private BitArray _charCheck;

    /// <summary>
    /// 此检查器是只允许模式还是只禁止模式
    /// </summary>
    public bool IsAllow { get; private set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="isAllow">检查器模式:true只允许模式 false只禁止模式</param>
    /// <param name="chars">定义的字符集合</param>
    public LimitedCharFilter(bool isAllow, IEnumerable<char> chars = null)
    {
        IsAllow = isAllow;
        _charCheck = new BitArray(char.MaxValue, !IsAllow);
        if (chars != null) AddChars(chars);
    }

    #region AddChars 添加字符定义
    /// <summary>
    /// 添加字符定义
    /// </summary>
    /// <param name="chr"></param>
    public LimitedCharFilter AddChar(char chr)
    {
        _charCheck[chr] = IsAllow;
        return this;
    }

    /// <summary>
    /// 添加字符集合定义
    /// </summary>
    /// <param name="chars">字符集合</param>
    public LimitedCharFilter AddChars(IEnumerable<char> chars)
    {
        foreach (char chr in chars)
        {
            AddChar(chr);
        }
        return this;
    }

    /// <summary>
    /// 添加字符集合定义
    /// </summary>
    /// <param name="chars">字符集合</param>
    public LimitedCharFilter AddChars(string chars)
    {
        AddChars(chars.ToCharArray());
        return this;
    }

    /// <summary>
    /// 添加数字字符集合定义
    /// </summary>
    public LimitedCharFilter AddNumberChars()
    {
        AddChars(StringUtil.NumberChars);
        return this;
    }

    /// <summary>
    /// 添加小写字符集合定义
    /// </summary>
    public LimitedCharFilter AddLowerChars()
    {
        AddChars(StringUtil.LowerLetterChars);
        return this;
    }

    /// <summary>
    /// 添加大写字符集合定义
    /// </summary>
    public LimitedCharFilter AddUpperChars()
    {
        AddChars(StringUtil.UpperLetterChars);
        return this;
    }

    /// <summary>
    /// 添加字母字符集合定义
    /// </summary>
    public LimitedCharFilter AddLetterChars()
    {
        AddChars(StringUtil.LetterChars);
        return this;
    }

    /// <summary>
    /// 添加常用英文标点符号数组
    /// </summary>
    public LimitedCharFilter AddCommonPunctuationChars()
    {
        AddChars(StringUtil.CommonPunctuation);
        return this;
    }

    /// <summary>
    /// 添加中文字符集合定义
    /// </summary>
    public LimitedCharFilter AddChinese()
    {
        for (int i = 19968; i <= 40869; i++)
        {
            _charCheck[i] = IsAllow;
        }
        return this;
    }
    #endregion

    /// <summary>
    /// 验证是否有效
    /// </summary>
    /// <param name="input">输入的字符串</param>
    /// <returns></returns>
    public bool IsValid(string input)
    {
        foreach (char curr in input)
        {
            if (!_charCheck[curr]) return false;
        }
        return true;
    }

    /// <summary>
    /// 用指定字符串替换限制的字符
    /// </summary>
    /// <param name="input">输入的字符串</param>
    /// <param name="mask">替换的字符串</param>
    /// <returns></returns>
    public string Replace(string input, string mask)
    {
        StringBuilder ret = new StringBuilder(input.Length);
        foreach (char curr in input)
        {
            if (_charCheck[curr]) ret.Append(curr);
            else ret.Append(mask);
        }
        return ret.ToString();
    }

    /// <summary>
    /// 获得定义的字符集合
    /// </summary>
    /// <returns></returns>
    public IEnumerable<char> GetLimitedChars()
    {
        for (int i = 0; i < _charCheck.Length; i++)
        {
            if (IsAllow == _charCheck[i])
                yield return Convert.ToChar(i);
        }
    }

}