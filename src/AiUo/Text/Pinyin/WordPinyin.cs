using System;
using System.Collections.Generic;
using System.Text;

namespace AiUo.Text.Pinyin;

/// <summary>
/// 汉字拼音相关信息类，通过PinYinUtil类获取 
/// </summary>
public class WordPinyin : IComparable<WordPinyin>
{
    private string _hex;    // 字符编码的十六进制表示

    /// <summary>
    /// 获得或设置汉字
    /// </summary>
    public char Word { get; set; }

    /// <summary>
    /// 获得拼音信息集合
    /// </summary>
    public List<PinyinEntity> Pinyins { get; } = new List<PinyinEntity>(3);

    /// <summary>
    /// 获得字符编码的十六进制表示
    /// </summary>
    public string Hex
    {
        get
        {
            if (string.IsNullOrEmpty(_hex))
            {
                byte[] buffer = Encoding.UTF8.GetBytes(new char[] { Word });
                for (int i = 0; i < buffer.Length; i++)
                    _hex += Convert.ToString(buffer[i], 16).ToUpper();
            }
            return _hex;
        }
    }

    /// <summary>
    /// 获得是否多音字
    /// </summary>
    public bool IsMutiPinyin => Pinyins.Count > 1;
    /// <summary>
    /// 获得汉字的多音字数量
    /// </summary>
    public int PinyinCount => Pinyins.Count;

    /// <summary>
    /// 显示汉字信息
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        foreach (PinyinEntity info in Pinyins)
        {
            builder.AppendLine(Word + " " + info.ToString());
        }
        return builder.ToString();
    }

    #region IComparable<WordEntity> Members
    /// <summary>
    /// 实现IComparable接口，提供按照字符排序的支持
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(WordPinyin other)
        => Word.CompareTo(other.Word);

    #endregion
}