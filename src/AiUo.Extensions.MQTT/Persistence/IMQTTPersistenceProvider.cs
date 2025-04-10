namespace AiUo.Extensions.MQTT.Persistence;

/// <summary>
/// MQTT消息持久化提供程序接口
/// </summary>
public interface IMQTTPersistenceProvider
{
    /// <summary>
    /// 保存消息
    /// </summary>
    /// <param name="topic">主题</param>
    /// <param name="payload">消息内容</param>
    /// <param name="qosLevel">服务质量等级</param>
    /// <param name="retain">是否保留</param>
    /// <param name="messageId">消息ID</param>
    /// <returns>操作是否成功</returns>
    Task<bool> SaveMessageAsync(string topic, byte[] payload, int qosLevel, bool retain, string messageId);

    /// <summary>
    /// 标记消息为已确认
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <returns>操作是否成功</returns>
    Task<bool> MarkMessageAcknowledgedAsync(string messageId);

    /// <summary>
    /// 获取未确认的消息
    /// </summary>
    /// <returns>未确认的消息列表</returns>
    Task<IEnumerable<MQTTPersistedMessage>> GetPendingMessagesAsync();

    /// <summary>
    /// 清理已确认的消息
    /// </summary>
    /// <param name="olderThan">清理早于此时间的消息</param>
    /// <returns>清理的消息数量</returns>
    Task<int> CleanAcknowledgedMessagesAsync(DateTime olderThan);
}

/// <summary>
/// MQTT持久化消息
/// </summary>
public class MQTTPersistedMessage
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public string MessageId { get; set; }

    /// <summary>
    /// 主题
    /// </summary>
    public string Topic { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    public byte[] Payload { get; set; }

    /// <summary>
    /// 服务质量等级
    /// </summary>
    public int QosLevel { get; set; }

    /// <summary>
    /// 是否保留
    /// </summary>
    public bool Retain { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 是否已确认
    /// </summary>
    public bool IsAcknowledged { get; set; }

    /// <summary>
    /// 确认时间
    /// </summary>
    public DateTime? AcknowledgedAt { get; set; }
}