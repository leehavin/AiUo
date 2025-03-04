using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AiUo.Text;

/// <summary>
/// 注册时用户名验证类(username, email, mobile)，必须缓存对象
/// 标准规则：
///     长度6-30
///     数字字母和.
///     不能.开头
///     不能连续.
/// </summary>
public class AccountStringValidator
{
    private int _min = 6;
    private int _max = 30;

    private bool _allowUsername = false;
    private bool _standardRule = false;
    private LimitedCharFilter _charFilter = null;

    private bool _allowEmail = false;
    private bool _allowMobile = false;
    private string _mobileRegex = null;

    #region Allow
    public AccountStringValidator AllowLengh(int length)
        => AllowLengh(length, length);
    /// <summary>
    /// 长度限制
    /// </summary>
    /// <param name="min">最小长度（包含）</param>
    /// <param name="max">最大长度（包含）</param>
    /// <returns></returns>
    public AccountStringValidator AllowLengh(int min, int max)
    {
        _min = min;
        _max = max;
        return this;
    }
    public AccountStringValidator AllowUsername(bool standardRule, string allowChars)
        => AllowUsername(standardRule, allowChars.ToArray());
    public AccountStringValidator AllowUsername(bool standardRule = true, IEnumerable<char> allowChars = null)
    {
        _allowUsername = true;
        _standardRule = standardRule;
        _charFilter = new LimitedCharFilter(true);
        if (standardRule)
            _charFilter.AddChars(StringUtil.NumberLetterChars).AddChar('.');
        if (allowChars != null)
            _charFilter.AddChars(allowChars);
        return this;
    }
    public AccountStringValidator AllowEmail()
    {
        _allowEmail = true;
        return this;
    }
    public AccountStringValidator AllowMobile(string mobileRegex = null)
    {
        if (string.IsNullOrEmpty(mobileRegex))
            throw new Exception("允许mobile时正则表达式不能为空");
        _allowMobile = true;
        _mobileRegex = mobileRegex;
        return this;
    }
    #endregion

    public bool IsValid(string accountString)
    {
        var ret = false;
        // email
        if (_allowEmail)
            ret = StringUtil.IsEmail(accountString);
        if (ret) return true;

        // username
        if (_allowUsername)
        {
            ret = accountString.Length >= _min
                  && accountString.Length <= _max
                  && _charFilter.IsValid(accountString);
            if (ret && _standardRule)
                ret = !accountString.StartsWith('.') && accountString.IndexOf("..") < 0;
        }
        if (ret) return true;

        // mobile
        if (_allowMobile)
        {
            ret = Regex.IsMatch(accountString, _mobileRegex);
        }
        return ret;
    }
}