using AiUo.Extensions.DotNetty.Core.DotNetty;
using AiUo.Logging;
using System.Collections.Concurrent;

namespace AiUo.Extensions.DotNetty;

/// <summary>
/// MQTT消息重传管理器，用于处理QoS 1和QoS 2级别的消息重传
/// </summary>
public class MQTTRetransmissionManager : IDisposable
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<ushort, RetransmissionItem>> _pendingMessages = new();
    private readonly Timer _retransmissionTimer;
    private TimeSpan _retransmissionInterval; // 重传间隔
    private int _maxRetransmissionCount; // 最大重传次数
    private TimeSpan _initialRetransmissionDelay; // 初始重传延迟
    private bool _useExponentialBackoff; // 是否使用指数退避策略
    private readonly ConcurrentDictionary<string, RetransmissionStatistics> _statistics = new(); // 重传统计信息
    
    /// <summary>
    /// 获取或设置重传间隔
    /// </summary>
    public TimeSpan RetransmissionInterval
    {
        get => _retransmissionInterval;
        set => _retransmissionInterval = value;
    }
    
    /// <summary>
    /// 获取或设置最大重传次数
    /// </summary>
    public int MaxRetransmissionCount
    {
        get => _maxRetransmissionCount;
        set => _maxRetransmissionCount = value;
    }
    
    /// <summary>
    /// 获取或设置是否使用指数退避策略
    /// </summary>
    public bool UseExponentialBackoff
    {
        get => _useExponentialBackoff;
        set => _useExponentialBackoff = value;
    }
    
    /// <summary>
    /// 创建MQTT消息重传管理器
    /// </summary>
    /// <param name="retransmissionInterval">重传间隔，默认为10秒</param>
    /// <param name="maxRetransmissionCount">最大重传次数，默认为5次</param>
    /// <param name="initialRetransmissionDelay">初始重传延迟，默认为5秒</param>
    /// <param name="useExponentialBackoff">是否使用指数退避策略，默认为true</param>
    public MQTTRetransmissionManager(
        TimeSpan? retransmissionInterval = null,
        int maxRetransmissionCount = 5,
        TimeSpan? initialRetransmissionDelay = null,
        bool useExponentialBackoff = true)
    {
        _retransmissionInterval = retransmissionInterval ?? TimeSpan.FromSeconds(10);
        _maxRetransmissionCount = maxRetransmissionCount;
        _initialRetransmissionDelay = initialRetransmissionDelay ?? TimeSpan.FromSeconds(5);
        _useExponentialBackoff = useExponentialBackoff;
        
        // 创建重传定时器，定期检查需要重传的消息
        _retransmissionTimer = new Timer(CheckRetransmissions, null, _retransmissionInterval, _retransmissionInterval);
    }
    
    /// <summary>
    /// 添加待确认消息
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <param name="messageId">消息ID</param>
    /// <param name="message">消息内容</param>
    /// <param name="messageType">消息类型</param>
    /// <param name="qos">服务质量级别</param>
    /// <param name="retransmitAction">重传动作</param>
    public void AddPendingMessage(string clientId, ushort messageId, object message, MQTTMessageType messageType, 
        MQTTQualityOfServiceLevel qos, Func<RetransmissionItem, Task> retransmitAction)
    {
        // 只处理QoS > 0的消息
        if (qos == MQTTQualityOfServiceLevel.AtMostOnce)
            return;
            
        // 获取或创建客户端的待确认消息字典
        var clientPendingMessages = _pendingMessages.GetOrAdd(clientId, _ => new ConcurrentDictionary<ushort, RetransmissionItem>());
        
        // 创建重传项
        var item = new RetransmissionItem
        {
            ClientId = clientId,
            MessageId = messageId,
            Message = message,
            MessageType = messageType,
            QoS = qos,
            CreatedTime = DateTime.UtcNow,
            LastRetransmitTime = DateTime.UtcNow,
            RetransmitCount = 0,
            RetransmitAction = retransmitAction
        };
        
        // 添加到待确认消息字典
        clientPendingMessages[messageId] = item;
        
        LogUtil.Debug("添加MQTT待确认消息: ClientId={ClientId}, MessageId={MessageId}, MessageType={MessageType}, QoS={QoS}", 
            clientId, messageId, messageType, qos);
    }
    
    /// <summary>
    /// 确认消息已收到
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <param name="messageId">消息ID</param>
    /// <returns>是否成功确认</returns>
    public bool AcknowledgeMessage(string clientId, ushort messageId)
    {
        // 检查客户端是否存在
        if (!_pendingMessages.TryGetValue(clientId, out var clientPendingMessages))
            return false;
            
        // 移除待确认消息
        if (clientPendingMessages.TryRemove(messageId, out var item))
        {
            LogUtil.Debug("确认MQTT消息: ClientId={ClientId}, MessageId={MessageId}, MessageType={MessageType}", 
                clientId, messageId, item.MessageType);
                
            // 如果客户端没有任何待确认消息，移除客户端
            if (clientPendingMessages.IsEmpty)
                _pendingMessages.TryRemove(clientId, out _);
                
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 移除客户端的所有待确认消息
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <returns>是否成功移除</returns>
    public bool RemoveClientPendingMessages(string clientId)
    {
        return _pendingMessages.TryRemove(clientId, out _);
    }
    
    /// <summary>
    /// 获取客户端的所有待确认消息
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <returns>待确认消息列表</returns>
    public IEnumerable<RetransmissionItem> GetClientPendingMessages(string clientId)
    {
        if (_pendingMessages.TryGetValue(clientId, out var clientPendingMessages))
            return clientPendingMessages.Values;
            
        return Enumerable.Empty<RetransmissionItem>();
    }
    
    /// <summary>
    /// 检查需要重传的消息
    /// </summary>
    private async void CheckRetransmissions(object state)
    {
        var now = DateTime.UtcNow;
        var retransmitTasks = new List<Task>();
        
        // 遍历所有客户端的待确认消息
        foreach (var clientEntry in _pendingMessages)
        {
            string clientId = clientEntry.Key;
            var clientPendingMessages = clientEntry.Value;
            var clientStats = _statistics.GetOrAdd(clientId, _ => new RetransmissionStatistics { ClientId = clientId });
            
            // 遍历客户端的所有待确认消息
            foreach (var item in clientPendingMessages.Values)
            {
                // 计算当前消息的重传间隔
                TimeSpan currentInterval = _retransmissionInterval;
                
                // 如果使用指数退避策略，则根据重传次数调整间隔
                if (_useExponentialBackoff && item.RetransmitCount > 0)
                {
                    // 使用指数退避算法：初始延迟 * 2^(重传次数-1)，但不超过重传间隔的5倍
                    double backoffFactor = Math.Min(Math.Pow(2, item.RetransmitCount - 1), 5);
                    currentInterval = TimeSpan.FromMilliseconds(_initialRetransmissionDelay.TotalMilliseconds * backoffFactor);
                }
                
                // 检查是否需要重传
                if ((now - item.LastRetransmitTime) >= currentInterval)
                {
                    // 增加重传次数
                    item.RetransmitCount++;
                    item.LastRetransmitTime = now;
                    
                    // 更新统计信息
                    clientStats.TotalRetransmissions++;
                    clientStats.LastRetransmissionTime = now;
                    
                    // 检查是否超过最大重传次数
                    if (item.RetransmitCount > _maxRetransmissionCount)
                    {
                        LogUtil.Warning("MQTT消息重传次数超过最大值，放弃重传: ClientId={ClientId}, MessageId={MessageId}, MessageType={MessageType}", 
                            item.ClientId, item.MessageId, item.MessageType);
                            
                        // 移除待确认消息
                        clientPendingMessages.TryRemove(item.MessageId, out _);
                        
                        // 更新统计信息
                        clientStats.FailedRetransmissions++;
                        continue;
                    }
                    
                    // 执行重传动作
                    try
                    {
                        LogUtil.Debug("重传MQTT消息: ClientId={ClientId}, MessageId={MessageId}, MessageType={MessageType}, RetransmitCount={RetransmitCount}", 
                            item.ClientId, item.MessageId, item.MessageType, item.RetransmitCount);
                            
                        retransmitTasks.Add(item.RetransmitAction(item));
                    }
                    catch (Exception ex)
                    {
                        LogUtil.Error(ex, "MQTT消息重传失败: ClientId={ClientId}, MessageId={MessageId}, MessageType={MessageType}", 
                            item.ClientId, item.MessageId, item.MessageType);
                        
                        // 更新统计信息
                        clientStats.FailedRetransmissions++;
                    }
                }
            }
        }
        
        // 等待所有重传任务完成
        if (retransmitTasks.Count > 0)
        {
            try
            {
                await Task.WhenAll(retransmitTasks);
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "MQTT消息重传任务执行失败");
            }
        }
    }
    
    /// <summary>
    /// 清除所有待确认消息
    /// </summary>
    public void ClearAllPendingMessages()
    {
        _pendingMessages.Clear();
        LogUtil.Debug("已清除所有MQTT待确认消息");
    }
    
    /// <summary>
    /// 获取重传统计信息
    /// </summary>
    /// <param name="clientId">客户端ID，如果为null则返回所有客户端的统计信息</param>
    /// <returns>重传统计信息列表</returns>
    public IEnumerable<RetransmissionStatistics> GetStatistics(string clientId = null)
    {
        if (string.IsNullOrEmpty(clientId))
            return _statistics.Values;
            
        if (_statistics.TryGetValue(clientId, out var stats))
            return new[] { stats };
            
        return Enumerable.Empty<RetransmissionStatistics>();
    }
    
    /// <summary>
    /// 重置重传统计信息
    /// </summary>
    /// <param name="clientId">客户端ID，如果为null则重置所有客户端的统计信息</param>
    public void ResetStatistics(string clientId = null)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            _statistics.Clear();
            LogUtil.Debug("已重置所有MQTT重传统计信息");
        }
        else if (_statistics.TryRemove(clientId, out _))
        {
            LogUtil.Debug("已重置MQTT重传统计信息: ClientId={ClientId}", clientId);
        }
    }
    
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _retransmissionTimer?.Dispose();
        ClearAllPendingMessages();
        ResetStatistics();
    }
}

/// <summary>
/// 重传项
/// </summary>
public class RetransmissionItem
{
    /// <summary>
    /// 客户端ID
    /// </summary>
    public string ClientId { get; set; }
    
    /// <summary>
    /// 消息ID
    /// </summary>
    public ushort MessageId { get; set; }
    
    /// <summary>
    /// 消息内容
    /// </summary>
    public object Message { get; set; }
    
    /// <summary>
    /// 消息类型
    /// </summary>
    public MQTTMessageType MessageType { get; set; }
    
    /// <summary>
    /// 服务质量级别
    /// </summary>
    public MQTTQualityOfServiceLevel QoS { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
    
    /// <summary>
    /// 最后重传时间
    /// </summary>
    public DateTime LastRetransmitTime { get; set; }
    
    /// <summary>
    /// 重传次数
    /// </summary>
    public int RetransmitCount { get; set; }
    
    /// <summary>
    /// 重传动作
    /// </summary>
    public Func<RetransmissionItem, Task> RetransmitAction { get; set; }
}

/// <summary>
/// 重传统计信息
/// </summary>
public class RetransmissionStatistics
{
    /// <summary>
    /// 客户端ID
    /// </summary>
    public string ClientId { get; set; }
    
    /// <summary>
    /// 总重传次数
    /// </summary>
    public long TotalRetransmissions { get; set; }
    
    /// <summary>
    /// 失败的重传次数
    /// </summary>
    public long FailedRetransmissions { get; set; }
    
    /// <summary>
    /// 最后重传时间
    /// </summary>
    public DateTime LastRetransmissionTime { get; set; }
}