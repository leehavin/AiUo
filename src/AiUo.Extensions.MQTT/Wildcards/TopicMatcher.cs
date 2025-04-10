namespace AiUo.Extensions.MQTT.Wildcards;

/// <summary>
/// MQTT主题匹配器，支持通配符
/// </summary>
public static class TopicMatcher
{
    /// <summary>
    /// 检查主题是否匹配通配符模式
    /// </summary>
    /// <param name="topic">实际主题</param>
    /// <param name="pattern">通配符模式</param>
    /// <returns>是否匹配</returns>
    public static bool IsMatch(string topic, string pattern)
    {
        if (string.IsNullOrEmpty(topic) || string.IsNullOrEmpty(pattern))
            return false;

        // 如果模式是 #，匹配所有主题
        if (pattern == "#")
            return true;

        // 分割主题和模式为层级
        var topicLevels = topic.Split('/');
        var patternLevels = pattern.Split('/');

        // 如果模式以 # 结尾，只需匹配前面的部分
        bool endsWithMultiLevelWildcard = patternLevels[patternLevels.Length - 1] == "#";
        if (endsWithMultiLevelWildcard)
        {
            // 如果主题层级少于模式层级（不包括 #），则不匹配
            if (topicLevels.Length < patternLevels.Length - 1)
                return false;
        }
        else
        {
            // 如果主题层级不等于模式层级，则不匹配
            if (topicLevels.Length != patternLevels.Length)
                return false;
        }

        // 逐层比较
        for (int i = 0; i < patternLevels.Length; i++)
        {
            // 如果是多级通配符 #，匹配剩余所有层级
            if (patternLevels[i] == "#")
                return true;

            // 如果已经超出主题层级范围，不匹配
            if (i >= topicLevels.Length)
                return false;

            // 如果不是单级通配符 + 且不相等，则不匹配
            if (patternLevels[i] != "+" && patternLevels[i] != topicLevels[i])
                return false;
        }

        return true;
    }

    /// <summary>
    /// 从一组模式中找出匹配给定主题的所有模式
    /// </summary>
    /// <param name="topic">实际主题</param>
    /// <param name="patterns">通配符模式集合</param>
    /// <returns>匹配的模式列表</returns>
    public static IEnumerable<string> FindMatchingPatterns(string topic, IEnumerable<string> patterns)
    {
        return patterns.Where(pattern => IsMatch(topic, pattern));
    }
}