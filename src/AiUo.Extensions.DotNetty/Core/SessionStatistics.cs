namespace AiUo.Extensions.DotNetty;

/// <summary>
/// MQTT会话统计信息
/// </summary>
public class SessionStatistics
{
    /// <summary>
    /// 总会话数
    /// </summary>
    public int TotalSessions { get; set; }
    
    /// <summary>
    /// 活跃会话数
    /// </summary>
    public int ActiveSessions { get; set; }
    
    /// <summary>
    /// 持久会话数（非清理会话）
    /// </summary>
    public int PersistentSessions { get; set; }
    
    /// <summary>
    /// 即将过期的会话数（1小时内）
    /// </summary>
    public int ExpiringSessionsCount { get; set; }
    
    /// <summary>
    /// 统计时间
    /// </summary>
    public DateTime StatisticsTime { get; set; } = DateTime.UtcNow;
}