namespace AiUo.Extensions.MQTT.Groups;

/// <summary>
/// MQTT主题分组
/// </summary>
public class TopicGroup
{
    /// <summary>
    /// 分组名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 分组描述
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// 分组中的主题列表
    /// </summary>
    public List<string> Topics { get; }

    /// <summary>
    /// 创建一个新的主题分组
    /// </summary>
    /// <param name="name">分组名称</param>
    /// <param name="description">分组描述</param>
    /// <param name="topics">初始主题列表</param>
    public TopicGroup(string name, string description = null, IEnumerable<string> topics = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        Topics = topics?.ToList() ?? new List<string>();
    }

    /// <summary>
    /// 添加主题到分组
    /// </summary>
    /// <param name="topic">主题</param>
    public void AddTopic(string topic)
    {
        if (string.IsNullOrEmpty(topic))
            throw new ArgumentNullException(nameof(topic));

        if (!Topics.Contains(topic))
            Topics.Add(topic);
    }

    /// <summary>
    /// 从分组中移除主题
    /// </summary>
    /// <param name="topic">主题</param>
    /// <returns>是否成功移除</returns>
    public bool RemoveTopic(string topic)
    {
        return Topics.Remove(topic);
    }

    /// <summary>
    /// 检查分组是否包含指定主题
    /// </summary>
    /// <param name="topic">主题</param>
    /// <returns>是否包含</returns>
    public bool ContainsTopic(string topic)
    {
        return Topics.Contains(topic);
    }
}