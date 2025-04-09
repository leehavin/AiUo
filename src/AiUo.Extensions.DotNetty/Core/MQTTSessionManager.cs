using AiUo.Logging;
using System.Collections.Concurrent;

namespace AiUo.Extensions.DotNetty;

/// <summary>
/// MQTT会话管理器
/// </summary>
public class MQTTSessionManager : IDisposable
{
    private readonly ConcurrentDictionary<string, MQTTSession> _sessions = new();
    private readonly Timer _sessionExpiryTimer;
    private readonly TimeSpan _sessionExpiryCheckInterval = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _defaultSessionExpiryInterval = TimeSpan.FromDays(1);
    private readonly bool _persistSessions;
    private readonly string _sessionStorePath;
    
    /// <summary>
    /// 创建MQTT会话管理器
    /// </summary>
    /// <param name="persistSessions">是否持久化会话</param>
    /// <param name="sessionStorePath">会话存储路径，如果为null则使用默认路径</param>
    /// <param name="sessionExpiryInterval">会话过期间隔，如果为null则使用默认值（1天）</param>
    public MQTTSessionManager(bool persistSessions = false, string sessionStorePath = null, TimeSpan? sessionExpiryInterval = null)
    {
        _persistSessions = persistSessions;
        _sessionStorePath = sessionStorePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mqtt_sessions");
        _defaultSessionExpiryInterval = sessionExpiryInterval ?? TimeSpan.FromDays(1);
        
        // 创建会话过期检查定时器
        _sessionExpiryTimer = new Timer(CheckSessionExpiry, null, _sessionExpiryCheckInterval, _sessionExpiryCheckInterval);
        
        // 如果启用会话持久化，则加载持久化的会话
        if (_persistSessions)
        {
            LoadSessions();
        }
    }
    
    /// <summary>
    /// 创建或获取会话
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <param name="cleanSession">是否清理会话</param>
    /// <param name="sessionExpiryInterval">会话过期间隔，如果为null则使用默认值</param>
    /// <returns>MQTT会话</returns>
    public MQTTSession GetOrCreateSession(string clientId, bool cleanSession = true, TimeSpan? sessionExpiryInterval = null)
    {
        // 如果请求清理会话，则移除现有会话
        if (cleanSession && _sessions.TryRemove(clientId, out var existingSession))
        {
            existingSession.Dispose();
            LogUtil.Debug("已清理MQTT会话: {ClientId}", clientId);
        }
        
        // 创建或获取会话
        return _sessions.GetOrAdd(clientId, id => 
        {
            var session = new MQTTSession(id)
            {
                CleanSession = cleanSession,
                ExpiryInterval = sessionExpiryInterval ?? _defaultSessionExpiryInterval,
                ExpiryTime = DateTime.UtcNow.Add(sessionExpiryInterval ?? _defaultSessionExpiryInterval)
            };
            LogUtil.Debug("已创建MQTT会话: {ClientId}, CleanSession={CleanSession}, ExpiryInterval={ExpiryInterval}", 
                id, cleanSession, session.ExpiryInterval);
            return session;
        });
    }
    
    /// <summary>
    /// 获取会话
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <returns>MQTT会话，如果不存在则返回null</returns>
    public MQTTSession GetSession(string clientId)
    {
        if (_sessions.TryGetValue(clientId, out var session))
        {
            // 更新会话过期时间
            session.ExpiryTime = DateTime.UtcNow.Add(session.ExpiryInterval);
            return session;
        }
        return null;
    }
    
    /// <summary>
    /// 移除会话
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <returns>是否成功移除</returns>
    public bool RemoveSession(string clientId)
    {
        if (_sessions.TryRemove(clientId, out var session))
        {
            session.Dispose();
            LogUtil.Debug("已移除MQTT会话: {ClientId}", clientId);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 检查会话过期
    /// </summary>
    private void CheckSessionExpiry(object state)
    {
        var now = DateTime.UtcNow;
        var expiredSessions = new List<string>();
        
        // 查找过期的会话
        foreach (var session in _sessions.Values)
        {
            if (session.ExpiryTime <= now)
            {
                expiredSessions.Add(session.ClientId);
            }
        }
        
        // 移除过期的会话
        foreach (var clientId in expiredSessions)
        {
            if (_sessions.TryRemove(clientId, out var session))
            {
                session.Dispose();
                LogUtil.Debug("MQTT会话已过期: {ClientId}", clientId);
            }
        }
        
        // 如果启用会话持久化，则保存会话
        if (_persistSessions)
        {
            SaveSessions();
        }
    }
    
    /// <summary>
    /// 保存会话到磁盘
    /// </summary>
    private void SaveSessions()
    {
        try
        {
            // 确保目录存在
            Directory.CreateDirectory(_sessionStorePath);
            
            // 保存每个会话
            foreach (var session in _sessions.Values)
            {
                if (!session.CleanSession) // 只保存非清理会话
                {
                    var filePath = Path.Combine(_sessionStorePath, $"{session.ClientId}.session");
                    using var fileStream = File.Create(filePath);
                    using var writer = new BinaryWriter(fileStream);
                    
                    // 序列化会话数据
                    // 这里简化处理，实际实现需要更完善的序列化逻辑
                    writer.Write(session.ClientId);
                    writer.Write(session.CleanSession);
                    writer.Write(session.ExpiryInterval.Ticks);
                    writer.Write(session.ExpiryTime.Ticks);
                    writer.Write(session.State.ToString());
                    
                    // 序列化订阅信息
                    writer.Write(session.Subscriptions.Count);
                    foreach (var subscription in session.Subscriptions)
                    {
                        writer.Write(subscription.Key);
                        writer.Write((int)subscription.Value);
                    }
                    
                    // 序列化待确认消息
                    writer.Write(session.PendingMessages.Count);
                    foreach (var pendingMessage in session.PendingMessages.Values)
                    {
                        writer.Write(pendingMessage.MessageId);
                        writer.Write(pendingMessage.Topic);
                        writer.Write(pendingMessage.Payload.Length);
                        writer.Write(pendingMessage.Payload);
                        writer.Write((int)pendingMessage.QoS);
                        writer.Write(pendingMessage.Retain);
                        writer.Write(pendingMessage.CreatedTime.Ticks);
                        writer.Write(pendingMessage.RetryCount);
                    }
                }
            }
            
            LogUtil.Debug("已保存MQTT会话到磁盘: {Path}", _sessionStorePath);
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "保存MQTT会话失败: {Path}", _sessionStorePath);
        }
    }
    
    /// <summary>
    /// 从磁盘加载会话
    /// </summary>
    private void LoadSessions()
    {
        try
        {
            // 确保目录存在
            if (!Directory.Exists(_sessionStorePath))
                return;
                
            // 加载每个会话文件
            foreach (var filePath in Directory.GetFiles(_sessionStorePath, "*.session"))
            {
                try
                {
                    using var fileStream = File.OpenRead(filePath);
                    using var reader = new BinaryReader(fileStream);
                    
                    // 反序列化会话数据
                    var clientId = reader.ReadString();
                    var cleanSession = reader.ReadBoolean();
                    var expiryIntervalTicks = reader.ReadInt64();
                    var expiryTimeTicks = reader.ReadInt64();
                    var stateStr = reader.ReadString();
                    
                    // 创建会话
                    var session = new MQTTSession(clientId)
                    {
                        CleanSession = cleanSession,
                        ExpiryInterval = TimeSpan.FromTicks(expiryIntervalTicks),
                        ExpiryTime = new DateTime(expiryTimeTicks, DateTimeKind.Utc),
                        State = Enum.Parse<MQTTSessionState>(stateStr)
                    };
                    
                    // 反序列化订阅信息
                    int subscriptionCount = reader.ReadInt32();
                    for (int i = 0; i < subscriptionCount; i++)
                    {
                        var topic = reader.ReadString();
                        var qos = (MQTTQualityOfServiceLevel)reader.ReadInt32();
                        session.Subscriptions[topic] = qos;
                    }
                    
                    // 反序列化待确认消息
                    int pendingMessageCount = reader.ReadInt32();
                    for (int i = 0; i < pendingMessageCount; i++)
                    {
                        var messageId = reader.ReadUInt16();
                        var topic = reader.ReadString();
                        var payloadLength = reader.ReadInt32();
                        var payload = reader.ReadBytes(payloadLength);
                        var qos = (MQTTQualityOfServiceLevel)reader.ReadInt32();
                        var retain = reader.ReadBoolean();
                        var createdTimeTicks = reader.ReadInt64();
                        var retryCount = reader.ReadInt32();
                        
                        var pendingMessage = new PendingMessage
                        {
                            MessageId = messageId,
                            Topic = topic,
                            Payload = payload,
                            QoS = qos,
                            Retain = retain,
                            CreatedTime = new DateTime(createdTimeTicks, DateTimeKind.Utc),
                            RetryCount = retryCount
                        };
                        
                        session.PendingMessages[messageId] = pendingMessage;
                    }
                    
                    // 添加会话到管理器
                    _sessions[clientId] = session;
                    
                    LogUtil.Debug("已从磁盘加载MQTT会话: {ClientId}", clientId);
                }
                catch (Exception ex)
                {
                    LogUtil.Error(ex, "加载MQTT会话文件失败: {FilePath}", filePath);
                }
            }
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "加载MQTT会话失败: {Path}", _sessionStorePath);
        }
    }
    
    /// <summary>
    /// 获取所有会话
    /// </summary>
    /// <returns>会话列表</returns>
    public IEnumerable<MQTTSession> GetAllSessions()
    {
        return _sessions.Values;
    }
    
    /// <summary>
    /// 获取会话统计信息
    /// </summary>
    /// <returns>会话统计信息</returns>
    public SessionStatistics GetStatistics()
    {
        var stats = new SessionStatistics
        {
            TotalSessions = _sessions.Count,
            ActiveSessions = _sessions.Values.Count(s => s.State == MQTTSessionState.Connected),
            PersistentSessions = _sessions.Values.Count(s => !s.CleanSession),
            ExpiringSessionsCount = _sessions.Values.Count(s => s.ExpiryTime <= DateTime.UtcNow.AddHours(1))
        };
        
        return stats;
    }
    
    /// <summary>
    /// 清理所有会话
    /// </summary>
    public void ClearSessions()
    {
        foreach (var session in _sessions.Values)
        {
            try
            {
                session.Dispose();
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "清理MQTT会话失败: {ClientId}", session.ClientId);
            }
        }
        _sessions.Clear();
        LogUtil.Debug("已清理所有MQTT会话");
        
        // 如果启用会话持久化，则清理会话文件
        if (_persistSessions && Directory.Exists(_sessionStorePath))
        {
            try
            {
                foreach (var file in Directory.GetFiles(_sessionStorePath, "*.session"))
                {
                    File.Delete(file);
                }
                LogUtil.Debug("已清理所有MQTT会话文件");
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "清理MQTT会话文件失败");
            }
        }
    }
    
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _sessionExpiryTimer?.Dispose();
        
        // 如果启用会话持久化，则保存会话
        if (_persistSessions)
        {
            SaveSessions();
        }
        
        ClearSessions();
    }
}

/// <summary>
/// MQTT会话
/// </summary>
public class MQTTSession : IDisposable
{
    /// <summary>
    /// 客户端ID
    /// </summary>
    public string ClientId { get; }
    
    /// <summary>
    /// 会话创建时间
    /// </summary>
    public DateTime CreatedTime { get; }
    
    /// <summary>
    /// 最后活动时间
    /// </summary>
    public DateTime LastActivityTime { get; private set; }
    
    /// <summary>
    /// 是否已清理会话
    /// </summary>
    public bool CleanSession { get; set; }
    
    /// <summary>
    /// 会话状态
    /// </summary>
    public MQTTSessionState State { get; set; }
    
    /// <summary>
    /// 会话过期间隔
    /// </summary>
    public TimeSpan ExpiryInterval { get; set; }
    
    /// <summary>
    /// 会话过期时间
    /// </summary>
    public DateTime ExpiryTime { get; set; }
    
    /// <summary>
    /// 连接时间
    /// </summary>
    public DateTime? ConnectTime { get; set; }
    
    /// <summary>
    /// 断开连接时间
    /// </summary>
    public DateTime? DisconnectTime { get; set; }
    
    /// <summary>
    /// 客户端地址
    /// </summary>
    public string ClientAddress { get; set; }
    
    /// <summary>
    /// 订阅主题集合
    /// </summary>
    public ConcurrentDictionary<string, MQTTQualityOfServiceLevel> Subscriptions { get; }
    
    /// <summary>
    /// 未确认的消息集合
    /// </summary>
    public ConcurrentDictionary<ushort, PendingMessage> PendingMessages { get; }
    
    /// <summary>
    /// 遗嘱消息
    /// </summary>
    public WillMessage WillMessage { get; set; }
    
    /// <summary>
    /// 创建MQTT会话
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    public MQTTSession(string clientId)
    {
        ClientId = clientId;
        CreatedTime = DateTime.UtcNow;
        LastActivityTime = DateTime.UtcNow;
        State = MQTTSessionState.Disconnected;
        Subscriptions = new ConcurrentDictionary<string, MQTTQualityOfServiceLevel>();
        PendingMessages = new ConcurrentDictionary<ushort, PendingMessage>();
    }
    
    /// <summary>
    /// 更新活动时间
    /// </summary>
    public void UpdateActivity()
    {
        LastActivityTime = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 设置会话状态
    /// </summary>
    /// <param name="state">会话状态</param>
    public void SetState(MQTTSessionState state)
    {
        State = state;
        UpdateActivity();
    }
    
    /// <summary>
    /// 添加订阅
    /// </summary>
    /// <param name="topic">主题</param>
    /// <param name="qos">服务质量级别</param>
    public void AddSubscription(string topic, MQTTQualityOfServiceLevel qos)
    {
        Subscriptions[topic] = qos;
        UpdateActivity();
    }
    
    /// <summary>
    /// 移除订阅
    /// </summary>
    /// <param name="topic">主题</param>
    /// <returns>是否成功移除</returns>
    public bool RemoveSubscription(string topic)
    {
        var result = Subscriptions.TryRemove(topic, out _);
        if (result)
        {
            UpdateActivity();
        }
        return result;
    }
    
    /// <summary>
    /// 添加待确认消息
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="message">待确认消息</param>
    public void AddPendingMessage(ushort messageId, PendingMessage message)
    {
        PendingMessages[messageId] = message;
        UpdateActivity();
    }
    
    /// <summary>
    /// 移除待确认消息
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <returns>是否成功移除</returns>
    public bool RemovePendingMessage(ushort messageId)
    {
        var result = PendingMessages.TryRemove(messageId, out _);
        if (result)
        {
            UpdateActivity();
        }
        return result;
    }
    
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        PendingMessages.Clear();
        Subscriptions.Clear();
        WillMessage = null;
    }
}

/// <summary>
/// MQTT会话状态
/// </summary>
public enum MQTTSessionState
{
    /// <summary>
    /// 已断开连接
    /// </summary>
    Disconnected,
    
    /// <summary>
    /// 正在连接
    /// </summary>
    Connecting,
    
    /// <summary>
    /// 已连接
    /// </summary>
    Connected,
    
    /// <summary>
    /// 正在断开连接
    /// </summary>
    Disconnecting
}

/// <summary>
/// 待确认消息
/// </summary>
public class PendingMessage
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public ushort MessageId { get; set; }
    
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
    /// 是否保留消息
    /// </summary>
    public bool Retain { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
    
    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryCount { get; set; }
    
    /// <summary>
    /// 创建待确认消息
    /// </summary>
    public PendingMessage()
    {
        CreatedTime = DateTime.UtcNow;
    }
}

/// <summary>
/// 遗嘱消息
/// </summary>
public class WillMessage
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
    /// 是否保留消息
    /// </summary>
    public bool Retain { get; set; }
}