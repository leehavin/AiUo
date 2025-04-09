using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace AiUo.Extensions.DotNetty;

/// <summary>
/// MQTT主题匹配器，用于支持通配符订阅
/// </summary>
public class MQTTTopicMatcher
{
    // 单层通配符: +
    // 多层通配符: #
    
    // 正则表达式缓存，提高性能
    private static readonly ConcurrentDictionary<string, Regex> _regexCache = new ConcurrentDictionary<string, Regex>();
    
    /// <summary>
    /// 检查主题是否匹配过滤器
    /// </summary>
    /// <param name="topic">发布的主题</param>
    /// <param name="topicFilter">订阅的主题过滤器</param>
    /// <returns>是否匹配</returns>
    public static bool IsMatch(string topic, string topicFilter)
    {
        // 空值检查
        if (string.IsNullOrEmpty(topic) || string.IsNullOrEmpty(topicFilter))
            return false;
            
        // 如果没有通配符，直接比较
        if (!topicFilter.Contains('+') && !topicFilter.Contains('#'))
            return topic == topicFilter;
        
        // 优化：使用缓存的正则表达式
        var regex = _regexCache.GetOrAdd(topicFilter, filter => 
        {
            string pattern = ConvertTopicFilterToRegex(filter);
            return new Regex(pattern, RegexOptions.Compiled);
        });
        
        // 使用正则表达式匹配
        return regex.IsMatch(topic);
    }
    
    /// <summary>
    /// 将主题过滤器转换为正则表达式
    /// </summary>
    /// <param name="topicFilter">主题过滤器</param>
    /// <returns>正则表达式</returns>
    private static string ConvertTopicFilterToRegex(string topicFilter)
    {
        // 转义正则表达式特殊字符
        string pattern = Regex.Escape(topicFilter);
        
        // 替换单层通配符 '+'
        // '+' 匹配一个层级，不包含 '/'
        pattern = pattern.Replace("\\+", "[^/]+");
        
        // 替换多层通配符 '#'
        // '#' 必须在最后，匹配零个或多个层级
        if (pattern.EndsWith("\\#"))
        {
            // 如果 '#' 是唯一的字符，它匹配所有主题
            if (pattern == "\\#")
                return "^.*$";
                
            // 否则，它必须前面有 '/'
            pattern = pattern.Substring(0, pattern.Length - 2) + "(/.*)?$";
        }
        
        // 添加开始和结束锚点
        return "^" + pattern + "$";
    }
    
    /// <summary>
    /// 验证主题过滤器是否有效
    /// </summary>
    /// <param name="topicFilter">主题过滤器</param>
    /// <returns>是否有效</returns>
    public static bool IsValidTopicFilter(string topicFilter)
    {
        // 空值检查
        if (string.IsNullOrEmpty(topicFilter))
            return false;
        
        // 主题过滤器长度限制 (MQTT v3.1.1规范)
        if (topicFilter.Length > 65535)
            return false;
            
        // 主题过滤器不能包含 '#' 字符，除非它是最后一个字符且前面是 '/'
        int indexOfHash = topicFilter.IndexOf('#');
        if (indexOfHash != -1 && (indexOfHash != topicFilter.Length - 1 || 
            (indexOfHash > 0 && topicFilter[indexOfHash - 1] != '/')))
            return false;
            
        // '+' 字符只能单独作为一个层级，不能是层级的一部分
        string[] levels = topicFilter.Split('/');
        foreach (var level in levels)
        {
            if (level.Contains('+') && level.Length > 1)
                return false;
        }
        
        // 检查主题过滤器是否包含空层级
        if (topicFilter.Contains("//"))
            return false;
        
        return true;
    }
    
    /// <summary>
    /// 验证主题是否有效
    /// </summary>
    /// <param name="topic">主题</param>
    /// <returns>是否有效</returns>
    public static bool IsValidTopic(string topic)
    {
        // 空值检查
        if (string.IsNullOrEmpty(topic))
            return false;
        
        // 主题长度限制 (MQTT v3.1.1规范)
        if (topic.Length > 65535)
            return false;
            
        // 主题不能包含通配符
        if (topic.Contains('+') || topic.Contains('#'))
            return false;
        
        // 检查主题是否包含空层级
        if (topic.Contains("//"))
            return false;
        
        return true;
    }
    
    /// <summary>
    /// 清除正则表达式缓存
    /// </summary>
    public static void ClearRegexCache()
    {
        _regexCache.Clear();
    }
}