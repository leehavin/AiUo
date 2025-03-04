using System.Collections.Generic;
using System.Reflection;

namespace AiUo;

/// <summary>
/// 枚举类型的值的描述信息
/// </summary>
public class EnumItem:IComparer<EnumItem>
{
    /// <summary>
    /// 枚举int值
    /// </summary>
    public int Value { get; set; }
    /// <summary>
    /// 枚举名称
    /// </summary>
    public string Name { get; set; }
    public string MapName { get; set; }
    /// <summary>
    /// 枚举描述，通过DescriptionAttribute定义的描述
    /// </summary>
    public string Description { get; set; }
    public FieldInfo FieldInfo { get; set; }

    public int Compare(EnumItem x, EnumItem y)
    {
        return x.Value.CompareTo(y.Value);
    }
}