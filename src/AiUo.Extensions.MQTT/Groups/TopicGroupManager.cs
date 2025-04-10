using AiUo.Extensions.MQTT.Wildcards;
using System.Collections.Concurrent;

namespace AiUo.Extensions.MQTT.Groups;

/// <summary>
/// MQTT主题分组管理器
/// </summary>
public class TopicGroupManager
{
    private readonly ConcurrentDictionary<string, TopicGroup> _groups = new();

    /// <summary>
    /// 获取所有分组
    /// </summary>
    public IEnumerable<TopicGroup> Groups => _groups.Values;

    /// <summary>
    /// 创建一个新的主题分组
    /// </summary>
    /// <param name="name">分组名称</param>
    /// <param name="description">分组描述</param>
    /// <param name="topics">初始主题列表</param>
    /// <returns>创建的分组</returns>
    public TopicGroup CreateGroup(string name, string description = null, IEnumerable<string> topics = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        if (_groups.ContainsKey(name))
            throw new ArgumentException($"分组名称 '{name}' 已存在", nameof(name));

        var group = new TopicGroup(name, description, topics);
        _groups[name] = group;
        return group;
    }

    /// <summary>
    /// 获取指定名称的分组
    /// </summary>
    /// <param name="name">分组名称</param>
    /// <returns>分组，如果不存在则返回null</returns>
    public TopicGroup GetGroup(string name)
    {
        _groups.TryGetValue(name, out var group);
        return group;
    }

    /// <summary>
    /// 移除指定名称的分组
    /// </summary>
    /// <param name="name">分组名称</param>
    /// <returns>是否成功移除</returns>
    public bool RemoveGroup(string name)
    {
        return _groups.TryRemove(name, out _);
    }

    /// <summary>
    /// 查找包含指定主题的所有分组
    /// </summary>
    /// <param name="topic">主题</param>
    /// <returns>包含该主题的分组列表</returns>
    public IEnumerable<TopicGroup> FindGroupsContainingTopic(string topic)
    {
        return _groups.Values.Where(g => g.ContainsTopic(topic));
    }

    /// <summary>
    /// 查找与指定主题匹配的所有分组（使用通配符匹配）
    /// </summary>
    /// <param name="topic">主题</param>
    /// <returns>匹配该主题的分组列表</returns>
    public IEnumerable<TopicGroup> FindMatchingGroups(string topic)
    {
        var result = new List<TopicGroup>();
        foreach (var group in _groups.Values)
        {
            foreach (var pattern in group.Topics)
            {
                if (TopicMatcher.IsMatch(topic, pattern))
                {
                    result.Add(group);
                    break;
                }
            }
        }
        return result;
    }
}