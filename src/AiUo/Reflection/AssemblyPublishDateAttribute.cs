using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AiUo.Reflection;

/// <summary>
/// Assembly发布日期
/// [assembly: AssemblyPublishDate("2016-06-20")]
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
[ComVisible(true)]
public sealed class AssemblyPublishDateAttribute : Attribute
{
    /// <summary>
    /// 发布日期
    /// </summary>
    public string PublishDate { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="publishDate"></param>
    public AssemblyPublishDateAttribute(string publishDate)
    {
        PublishDate = publishDate;
    }

    /// <summary>
    /// 获取指定Assembly中定义的AssemblyPublishDateAttribute
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static AssemblyPublishDateAttribute GetAttribute(Assembly assembly)
        => assembly.GetCustomAttribute<AssemblyPublishDateAttribute>();
}