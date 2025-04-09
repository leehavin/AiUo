using AiUo.Logging;
using System.Collections.Concurrent;

namespace AiUo.Extensions.DotNetty;

/// <summary>
/// MQTT主题路由器，负责管理主题订阅和消息路由
/// </summary>
public class MQTTTopicRouter : IDisposable
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, MQTTSubscription>> _subscriptions = new();
    
    /// <summary>
    /// 保留消息字典，键为主题，值为消息内容
    /// </summary>
    private readonly ConcurrentDictionary<string, RetainedMessage> _retainedMessages = new();
    
    /// <summary>
    /// 主题统计信息
    /// </summary>
    private readonly ConcurrentDictionary<string, TopicStatistics> _topicStatistics = new();
    
    /// <summary>
    /// 添加订阅
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <param name="topicFilter">主题过滤器</param>
    /// <param name="qos">服务质量级别</param>
    /// <returns>是否成功添加</returns>
    public bool AddSubscription(string clientId, string topicFilter, MQTTQualityOfServiceLevel qos)
    {
        // 验证主题过滤器
        if (!MQTTTopicMatcher.IsValidTopicFilter(topicFilter))
        {
            LogUtil.Warning("无效的MQTT主题过滤器: {TopicFilter}", topicFilter);
            return false;
        }
        
        // 获取或创建客户端的订阅字典
        var clientSubscriptions = _subscriptions.GetOrAdd(clientId, _ => new ConcurrentDictionary<string, MQTTSubscription>());
        
        // 添加或更新订阅
        clientSubscriptions[topicFilter] = new MQTTSubscription
        {
            ClientId = clientId,
            TopicFilter = topicFilter,
            QoS = qos,
            SubscribeTime = DateTime.UtcNow
        };
        
        LogUtil.Debug("添加MQTT订阅: ClientId={ClientId}, TopicFilter={TopicFilter}, QoS={QoS}", 
            clientId, topicFilter, qos);
            
        return true;
    }
    
    /// <summary>
    /// 移除订阅
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <param name="topicFilter">主题过滤器</param>
    /// <returns>是否成功移除</returns>
    public bool RemoveSubscription(string clientId, string topicFilter)
    {
        // 检查客户端是否存在
        if (!_subscriptions.TryGetValue(clientId, out var clientSubscriptions))
            return false;
            
        // 移除订阅
        if (clientSubscriptions.TryRemove(topicFilter, out _))
        {
            LogUtil.Debug("移除MQTT订阅: ClientId={ClientId}, TopicFilter={TopicFilter}", 
                clientId, topicFilter);
                
            // 如果客户端没有任何订阅，移除客户端
            if (clientSubscriptions.IsEmpty)
                _subscriptions.TryRemove(clientId, out _);
                
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 移除客户端的所有订阅
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <returns>是否成功移除</returns>
    public bool RemoveClientSubscriptions(string clientId)
    {
        return _subscriptions.TryRemove(clientId, out _);
    }
    
    /// <summary>
    /// 获取客户端的所有订阅
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <returns>订阅列表</returns>
    public IEnumerable<MQTTSubscription> GetClientSubscriptions(string clientId)
    {
        if (_subscriptions.TryGetValue(clientId, out var clientSubscriptions))
            return clientSubscriptions.Values;
            
        return Enumerable.Empty<MQTTSubscription>();
    }
    
    /// <summary>
    /// 查找匹配主题的所有订阅
    /// </summary>
    /// <param name="topic">发布的主题</param>
    /// <returns>匹配的订阅列表</returns>
    public IEnumerable<MQTTSubscription> FindMatchingSubscriptions(string topic)
    {
        // 验证主题
        if (!MQTTTopicMatcher.IsValidTopic(topic))
            return Enumerable.Empty<MQTTSubscription>();
            
        var result = new List<MQTTSubscription>();
        
        // 更新主题统计信息
        UpdateTopicStatistics(topic);
        
        // 遍历所有客户端的订阅
        foreach (var clientSubscriptions in _subscriptions.Values)
        {
            // 遍历客户端的所有订阅
            foreach (var subscription in clientSubscriptions.Values)
            {
                // 检查主题是否匹配
                if (MQTTTopicMatcher.IsMatch(topic, subscription.TopicFilter))
                {
                    result.Add(subscription);
                }
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// 更新主题统计信息
    /// </summary>
    /// <param name="topic">主题</param>
    private void UpdateTopicStatistics(string topic)
    {
        var stats = _topicStatistics.GetOrAdd(topic, _ => new TopicStatistics { Topic = topic });
        stats.MessageCount++;
        stats.LastMessageTime = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 清除所有订阅
    /// </summary>
    public void ClearAllSubscriptions()
    {
        _subscriptions.Clear();
        LogUtil.Debug("已清除所有MQTT订阅");
    }
    
    /// <summary>
    /// 存储保留消息
    /// </summary>
    /// <param name="topic">主题</param>
    /// <param name="payload">消息内容</param>
    /// <param name="qos">服务质量级别</param>
    /// <returns>是否成功存储</returns>
    public bool StoreRetainedMessage(string topic, byte[] payload, MQTTQualityOfServiceLevel qos)
    {
        // 验证主题
        if (!MQTTTopicMatcher.IsValidTopic(topic))
            return false;
            
        // 如果有效载荷为空或长度为0，则删除保留消息
        if (payload == null || payload.Length == 0)
        {
            _retainedMessages.TryRemove(topic, out _);
            LogUtil.Debug("已删除MQTT保留消息: Topic={Topic}", topic);
            return true;
        }
        
        // 存储保留消息
        var retainedMessage = new RetainedMessage
        {
            Topic = topic,
            Payload = payload,
            QoS = qos,
            Timestamp = DateTime.UtcNow
        };
        
        _retainedMessages[topic] = retainedMessage;
        LogUtil.Debug("已存储MQTT保留消息: Topic={Topic}, QoS={QoS}", topic, qos);
        return true;
    }
    
    /// <summary>
    /// 获取匹配主题过滤器的所有保留消息
    /// </summary>
    /// <param name="topicFilter">主题过滤器</param>
    /// <returns>匹配的保留消息列表</returns>
    public IEnumerable<RetainedMessage> GetMatchingRetainedMessages(string topicFilter)
    {
        // 验证主题过滤器
        if (!MQTTTopicMatcher.IsValidTopicFilter(topicFilter))
            return Enumerable.Empty<RetainedMessage>();
            
        var result = new List<RetainedMessage>();
        
        // 如果主题过滤器不包含通配符，直接查找
        if (!topicFilter.Contains('+') && !topicFilter.Contains('#'))
        {
            if (_retainedMessages.TryGetValue(topicFilter, out var message))
                result.Add(message);
                
            return result;
        }
        
        // 对于包含通配符的主题过滤器，需要遍历所有保留消息
        foreach (var message in _retainedMessages.Values)
        {
            if (MQTTTopicMatcher.IsMatch(message.Topic, topicFilter))
                result.Add(message);
        }
        
        return result;
    }
    
    /// <summary>
    /// 获取主题统计信息
    /// </summary>
    /// <param name="topic">主题，如果为null则返回所有主题的统计信息</param>
    /// <returns>主题统计信息列表</returns>
    public IEnumerable<TopicStatistics> GetTopicStatistics(string topic = null)
    {
        if (string.IsNullOrEmpty(topic))
            return _topicStatistics.Values;
            
        if (_topicStatistics.TryGetValue(topic, out var stats))
            return new[] { stats };
            
        return Enumerable.Empty<TopicStatistics>();
    }
    
    /// <summary>
    /// 清除所有保留消息
    /// </summary>
    public void ClearAllRetainedMessages()
    {
        _retainedMessages.Clear();
        LogUtil.Debug("已清除所有MQTT保留消息");
    }
    
    /// <summary>
    /// 清除所有主题统计信息
    /// </summary>
    public void ClearAllTopicStatistics()
    {
        _topicStatistics.Clear();
        LogUtil.Debug("已清除所有MQTT主题统计信息");
    }
    
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        ClearAllSubscriptions();
        ClearAllRetainedMessages();
        ClearAllTopicStatistics();
    }
}

/// <summary>
/// MQTT订阅信息
/// </summary>
public class MQTTSubscription
{
    /// <summary>
    /// 客户端ID
    /// </summary>
    public string ClientId { get; set; }
    
    /// <summary>
    /// 主题过滤器
    /// </summary>
    public string TopicFilter { get; set; }
    
    /// <summary>
    /// 服务质量级别
    /// </summary>
    public MQTTQualityOfServiceLevel QoS { get; set; }
    
    /// <summary>
    /// 订阅时间
    /// </summary>
    public DateTime SubscribeTime { get; set; }
    
    /// <summary>
    /// 最后活动时间
    /// </summary>
    public DateTime LastActivityTime { get; set; }
    
    /// <summary>
    /// 收到的消息数量
    /// </summary>
    public long ReceivedMessageCount { get; set; }
}

/// <summary>
/// MQTT保留消息
/// </summary>
 public class RetainedMessage
{
    /// <summary>
    /// 主题
    /// </summary>
    public string Topic { get; set; }
    
    /// <summary>
    /// 有效载荷
    /// </summary>
    public byte[] Payload { get; set; }
    
    /// <summary>
    /// 服务质量级别
    /// </summary>
    public MQTTQualityOfServiceLevel QoS { get; set; }
    
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// 主题统计信息
/// </summary>
public class TopicStatistics
{
    /// <summary>
    /// 主题
    /// </summary>
    public string Topic { get; set; }
    
    /// <summary>
    /// 消息数量
    /// </summary>
    public long MessageCount { get; set; }
    
    /// <summary>
    /// 最后消息时间
    /// </summary>
    public DateTime LastMessageTime { get; set; }
}