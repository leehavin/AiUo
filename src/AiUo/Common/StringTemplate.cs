using System;
using System.Linq;
using System.Text.RegularExpressions;
using AiUo.Reflection;

namespace AiUo.Common;

/// <summary>
/// 字符串模板渲染器
/// 用于模板字符串替换，变量格式如: {{key}}
/// 如：字符串模板{{key1}}替换
/// </summary>
public class StringTemplateReplacer
{
    /// <summary>
    /// 源模板数据，如: TEST{userId}
    /// </summary>
    public string TemplateContent { get; }
    /// <summary>
    /// 是否允许检查未使用
    /// </summary>
    public bool IsAllowNotUsed { get; }
    private string _content { get; set; }

    /// <summary>
    /// 模版引擎
    /// </summary>
    /// <param name="templateContent"></param>
    /// <param name="isAllowNotUsed"></param>
    public StringTemplateReplacer(string templateContent, bool isAllowNotUsed = false)
    {
        TemplateContent = _content = templateContent;
        IsAllowNotUsed = isAllowNotUsed;
    }
    /// <summary>
    /// 重置
    /// </summary>
    /// <returns></returns>
    public StringTemplateReplacer Reset()
    {
        _content = TemplateContent;
        return this;
    }
    /// <summary>
    /// 设置变量
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public StringTemplateReplacer Set(string key, string value)
    {
        _content = _content.Replace("{{" + key + "}}", value);
        return this;
    }
    public StringTemplateReplacer Set<T>(T templateData)
    {
        var mc = Regex.Matches(_content, @"\{\{.+?\}\}");
        foreach (Match m in mc)
        {
            var name = m.Value.Trim('{', '}');
            if (!ReflectionUtil.TryGetPropertyValue(templateData, name, out var value))
                continue;
            Set(name, Convert.ToString(value));
        }
        return this;
    }

    /// <summary>
    /// 渲染模板
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (!IsAllowNotUsed)
        {
            var mc = Regex.Matches(_content, @"\{\{.+?\}\}");
            if (mc.Count > 0)
            {
                string keys = string.Join('|', mc.Select(x => x.Value));
                throw new ArgumentException($"模版变量未被使用。keys: {keys}");
            }
        }

        return _content;
    }
}