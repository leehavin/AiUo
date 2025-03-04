using System;

namespace AiUo.Text.Pinyin;

/// <summary>
/// 拼音信息实体类，包含拼音（包含拼写和声调），声调，拼写 
/// </summary>
public class PinyinEntity : IComparable<PinyinEntity>
{
    /// <summary>
    /// 获得或设置拼音（包含拼写和声调）
    /// </summary>
    public string Pinyin { get; set; }

    /// <summary>
    /// 获得或设置声调
    /// </summary>
    public string Tone { get; set; }
    /// <summary>
    /// 获得或设置拼写
    /// </summary>
    public string Spell { get; set; }

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public PinyinEntity() { }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="pinyin">拼音</param>
    /// <param name="tone">声调</param>
    /// <param name="spell">拼写</param>
    public PinyinEntity(string pinyin, string tone, string spell)
    {
        this.Pinyin = pinyin;
        this.Tone = tone;
        this.Spell = spell;
    }

    /// <summary>
    /// 重写ToString()
    /// </summary>
    /// <returns></returns>
    public override string ToString()
        => string.Format("拼音：{0} 拼写：{1} 声调：{2}", Pinyin, Spell, Tone);

    #region IComparable<PinyinEntity> Members
    /// <summary>
    /// 实现IComparable接口，提供按照拼写排序的支持
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(PinyinEntity other)
    {
        return string.Compare(this.Spell, other.Spell);
    }

    #endregion
}