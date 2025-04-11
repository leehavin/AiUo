using System;
using System.Collections.Generic;

namespace AiUo.Extensions.MQTT.Server;

/// <summary>
/// MQTT服务器配置选项
/// </summary>
public class MQTTServerOptions
{
    /// <summary>
    /// 服务器监听端口
    /// </summary>
    public int Port { get; set; } = 1883;

    /// <summary>
    /// 是否使用TLS/SSL
    /// </summary>
    public bool UseTls { get; set; } = false;

    /// <summary>
    /// TLS/SSL端口
    /// </summary>
    public int TlsPort { get; set; } = 8883;

    /// <summary>
    /// 证书文件路径
    /// </summary>
    public string CertificatePath { get; set; }

    /// <summary>
    /// 证书密码
    /// </summary>
    public string CertificatePassword { get; set; }

    /// <summary>
    /// 是否启用认证
    /// </summary>
    public bool EnableAuthentication { get; set; } = false;

    /// <summary>
    /// 是否启用订阅授权
    /// </summary>
    public bool EnableSubscriptionAuthorization { get; set; } = false;

    /// <summary>
    /// 是否启用发布授权
    /// </summary>
    public bool EnablePublishAuthorization { get; set; } = false;

    /// <summary>
    /// 未授权订阅时是否关闭连接
    /// </summary>
    public bool CloseConnectionOnUnauthorizedSubscription { get; set; } = false;

    /// <summary>
    /// 未授权发布时是否关闭连接
    /// </summary>
    public bool CloseConnectionOnUnauthorizedPublish { get; set; } = false;

    /// <summary>
    /// 最大并发客户端连接数
    /// </summary>
    public int MaximumConnections { get; set; } = 100;

    /// <summary>
    /// 用户名和密码字典，用于简单认证
    /// </summary>
    public Dictionary<string, string> Credentials { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// 主题访问控制列表，用于简单授权
    /// </summary>
    public Dictionary<string, List<string>> TopicAccessControl { get; set; } = new Dictionary<string, List<string>>();

    /// <summary>
    /// 验证凭据的委托
    /// </summary>
    public Func<string, string, bool> ValidateCredentials { get; set; } = (username, password) => true;

    /// <summary>
    /// 授权订阅的委托
    /// </summary>
    public Func<string, string, bool> AuthorizeSubscription { get; set; } = (clientId, topic) => true;

    /// <summary>
    /// 授权发布的委托
    /// </summary>
    public Func<string, string, bool> AuthorizePublish { get; set; } = (clientId, topic) => true;

    /// <summary>
    /// 创建一个新的MQTT服务器配置选项实例
    /// </summary>
    public MQTTServerOptions()
    {
        // 设置默认的凭据验证逻辑
        ValidateCredentials = (username, password) =>
        {
            if (!EnableAuthentication)
                return true;

            if (string.IsNullOrEmpty(username))
                return false;

            return Credentials.TryGetValue(username, out var storedPassword) && storedPassword == password;
        };

        // 设置默认的订阅授权逻辑
        AuthorizeSubscription = (clientId, topic) =>
        {
            if (!EnableSubscriptionAuthorization)
                return true;

            if (string.IsNullOrEmpty(clientId))
                return false;

            return TopicAccessControl.TryGetValue(clientId, out var allowedTopics) && 
                   (allowedTopics.Contains("#") || allowedTopics.Contains(topic) || 
                    allowedTopics.Any(t => IsTopicMatch(t, topic)));
        };

        // 设置默认的发布授权逻辑
        AuthorizePublish = (clientId, topic) =>
        {
            if (!EnablePublishAuthorization)
                return true;

            if (string.IsNullOrEmpty(clientId))
                return false;

            return TopicAccessControl.TryGetValue(clientId, out var allowedTopics) && 
                   (allowedTopics.Contains("#") || allowedTopics.Contains(topic) || 
                    allowedTopics.Any(t => IsTopicMatch(t, topic)));
        };
    }

    /// <summary>
    /// 检查主题是否匹配通配符模式
    /// </summary>
    /// <param name="pattern">通配符模式</param>
    /// <param name="topic">要检查的主题</param>
    /// <returns>是否匹配</returns>
    private bool IsTopicMatch(string pattern, string topic)
    {
        // 处理多层级通配符 #
        if (pattern == "#")
            return true;

        // 处理单层级通配符 +
        var patternSegments = pattern.Split('/');
        var topicSegments = topic.Split('/');

        // 如果模式以 # 结尾，则只需要匹配前面的部分
        if (patternSegments[patternSegments.Length - 1] == "#")
        {
            if (topicSegments.Length < patternSegments.Length - 1)
                return false;

            for (int i = 0; i < patternSegments.Length - 1; i++)
            {
                if (patternSegments[i] != "+" && patternSegments[i] != topicSegments[i])
                    return false;
            }

            return true;
        }

        // 否则，段数必须相同
        if (patternSegments.Length != topicSegments.Length)
            return false;

        // 逐段比较
        for (int i = 0; i < patternSegments.Length; i++)
        {
            if (patternSegments[i] != "+" && patternSegments[i] != topicSegments[i])
                return false;
        }

        return true;
    }
}