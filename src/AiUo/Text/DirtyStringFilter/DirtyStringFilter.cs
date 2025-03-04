using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using AiUo.IO;

namespace AiUo.Text;

/// <summary>
/// 脏字过滤器，用于验证是否存在脏字和替换脏字
/// </summary>
public class DirtyStringFilter
{
    /// <summary>
    /// 过滤器单例
    /// </summary>
    public static readonly DirtyStringFilter Instance = new DirtyStringFilter();

    private const string RESOURCE_NAME = "AiUo.Text.DirtyStringFilter.DirtyStringFilter.dic.20160817.7z";

    #region Members
    private HashSet<string> _hash = new HashSet<string>(); //禁止的字符串
    private byte[] _fastCheck = new byte[char.MaxValue];
    private byte[] _fastLength = new byte[char.MaxValue];
    private BitArray _charCheck = new BitArray(char.MaxValue);
    private BitArray _endCheck = new BitArray(char.MaxValue);
    private int _maxWordLength = 0;
    private int _minWordLength = int.MaxValue;

    private List<string> _dirtyStrings = new List<string>();
    private object _locker = new object();

    #endregion

    #region Constructors
    /// <summary>
    /// 构造函数
    /// </summary>
    public DirtyStringFilter()
    {
        var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RESOURCE_NAME);
        var words = Encoding.UTF8.GetString(CompressionUtil.UnSevenZipToBytes(resourceStream));
        AddDirtyWords(words, '、');
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dirty">脏字集合</param>
    public DirtyStringFilter(IEnumerable<string> dirty)
        => AddDirtyWords(dirty);

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dirty">字符串，包含脏字的定义</param>
    /// <param name="delimiter">分隔符</param>
    public DirtyStringFilter(string dirty, char delimiter = '|')
        => AddDirtyWords(dirty, delimiter);

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="filename">文件名，文件包含脏字定义</param>
    /// <param name="delimiter">分隔符</param>
    public DirtyStringFilter(string filename, string delimiter)
        => AddDirtyWords(filename, delimiter);
    #endregion

    #region AddDirty 添加脏字（词）定义
    /// <summary>
    /// 添加脏字词
    /// </summary>
    /// <param name="word"></param>
    public void AddDirtyWord(string word)
    {
        if (!_dirtyStrings.Contains(word))
        {
            lock (_locker)
            {
                if (!_dirtyStrings.Contains(word))
                {
                    _maxWordLength = Math.Max(_maxWordLength, word.Length);
                    _minWordLength = Math.Min(_minWordLength, word.Length);

                    for (int i = 0; i < 7 && i < word.Length; i++)
                    {
                        _fastCheck[word[i]] |= (byte)(1 << i);
                    }

                    for (int i = 7; i < word.Length; i++)
                    {
                        _fastCheck[word[i]] |= 0x80;
                    }

                    if (word.Length == 1)
                    {
                        _charCheck[word[0]] = true;
                    }
                    else
                    {
                        _hash.Add(word);
                    }
                    _dirtyStrings.Add(word);
                }
            }
        }
    }
    /// <summary>
    /// 添加脏字定义
    /// </summary>
    /// <param name="dirty">脏字定义列表</param>
    public void AddDirtyWords(IEnumerable<string> dirty)
    {
        foreach (string word in dirty)
        {
            AddDirtyWord(word);
        }
    }

    /// <summary>
    /// 添加脏字定义
    /// </summary>
    /// <param name="dirty">字符串，包含脏字的定义</param>
    /// <param name="delimiter">分隔符</param>
    public void AddDirtyWords(string dirty, char delimiter = '|')
    {
        AddDirtyWords(dirty.Split(new char[] { delimiter }, StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// 添加脏字定义通过读取文件
    /// </summary>
    /// <param name="filename">文件名</param>
    /// <param name="delimiter">分隔符</param>
    public void AddDirtyWords(string filename, string delimiter)
    {
        var words = File.ReadAllText(filename);
        AddDirtyWords(words.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries));
    }
    #endregion

    #region Methods
    /// <summary>
    /// 用指定字符串替换脏字
    /// </summary>
    /// <param name="input">输入的字符串</param>
    /// <param name="mask">替换的字符串</param>
    /// <returns></returns>
    public string Replace(string input, string mask)
    {
        StringBuilder ret = new StringBuilder(input.Length);
        int index = 0;
        int currIdx = 0;
        while (index < input.Length)
        {
            currIdx = index;
            if ((_fastCheck[input[index]] & 1) == 0)
            {
                while (index < input.Length - 1 && (_fastCheck[input[++index]] & 1) == 0) ;
            }

            if (_minWordLength == 1 && _charCheck[input[index]])
            {
                ret.Append(input.Substring(currIdx, index - currIdx));
                ret.Append(mask);
                index++;
                currIdx = index;
                continue;
            }

            for (int j = 1; j <= Math.Min(_maxWordLength, input.Length - index - 1); j++)
            {
                if ((_fastCheck[input[index + j]] & (1 << Math.Min(j, 7))) == 0)
                {
                    ret.Append(input[index]);
                    break;
                }

                if (j + 1 >= _minWordLength)
                {
                    string sub = input.Substring(index, j + 1);

                    if (_hash.Contains(sub))
                    {
                        if (index - currIdx > 0)
                            ret.Append(input.Substring(currIdx, index - currIdx));
                        ret.Append(mask);
                        index += sub.Length - 1;
                        if (index + 1 == input.Length)
                            currIdx = input.Length;
                        break;
                    }
                }
            }
            index++;
        }
        if (currIdx < input.Length)
            ret.Append(input.Substring(currIdx));
        return ret.ToString();
    }

    /// <summary>
    /// 用指定字符替换脏字，长度相同
    /// </summary>
    /// <param name="input"></param>
    /// <param name="mask"></param>
    /// <returns></returns>
    public string Replace(string input, char mask)
    {
        StringBuilder ret = new StringBuilder(input.Length);
        int index = 0;
        int currIdx = 0;
        while (index < input.Length)
        {
            currIdx = index;
            if ((_fastCheck[input[index]] & 1) == 0)
            {
                while (index < input.Length - 1 && (_fastCheck[input[++index]] & 1) == 0) ;
            }

            if (_minWordLength == 1 && _charCheck[input[index]])
            {
                ret.Append(input.Substring(currIdx, index - currIdx));
                ret.Append(mask);
                index++;
                currIdx = index;
                continue;
            }

            for (int j = 1; j <= Math.Min(_maxWordLength, input.Length - index - 1); j++)
            {
                if ((_fastCheck[input[index + j]] & (1 << Math.Min(j, 7))) == 0)
                {
                    ret.Append(input[index]);
                    break;
                }

                if (j + 1 >= _minWordLength)
                {
                    string sub = input.Substring(index, j + 1);

                    if (_hash.Contains(sub))
                    {
                        if (index - currIdx > 0)
                            ret.Append(input.Substring(currIdx, index - currIdx));
                        ret.Append(new string(mask, sub.Length));
                        index += sub.Length - 1;
                        if (index + 1 == input.Length)
                            currIdx = input.Length;
                        break;
                    }
                }
            }
            index++;
        }
        if (currIdx < input.Length)
            ret.Append(input.Substring(currIdx));
        return ret.ToString();
    }

    /// <summary>
    /// 验证是否存在脏字，存在返回true
    /// </summary>
    /// <param name="input">输入的字符串</param>
    /// <returns></returns>
    public bool HasDirty(string input)
    {
        int index = 0;

        while (index < input.Length)
        {
            if ((_fastCheck[input[index]] & 1) == 0)
            {
                while (index < input.Length - 1 && (_fastCheck[input[++index]] & 1) == 0) ;
            }

            if (_minWordLength == 1 && _charCheck[input[index]])
            {
                return true;
            }

            for (int j = 1; j <= Math.Min(_maxWordLength, input.Length - index - 1); j++)
            {
                if ((_fastCheck[input[index + j]] & (1 << Math.Min(j, 7))) == 0)
                {
                    break;
                }

                if (j + 1 >= _minWordLength)
                {
                    string sub = input.Substring(index, j + 1);

                    if (_hash.Contains(sub))
                    {
                        return true;
                    }
                }
            }

            index++;
        }

        return false;
    }

    /// <summary>
    /// 获得定义的脏字集合
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetDirtyStrings()
    {
        foreach (string curr in _dirtyStrings)
        {
            yield return curr;
        }
    }
    #endregion
}