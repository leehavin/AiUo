using System;

namespace AiUo.Extensions.DotNetty;

/// <summary>
/// MQTT连接信息
/// </summary>
public class ConnectionInfo
{
    /// <summary>
    /// 连接ID
    /// </summary>
    public string ConnectionId { get; set; }
    
    /// <summary>
    /// 客户端ID
    /// </summary>
    public string ClientId { get; set; }
    
    /// <summary>
    /// 远程地址
    /// </summary>
    public string RemoteAddress { get; set; }
    
    /// <summary>
    /// 远程端口
    /// </summary>
    public int RemotePort { get; set; }
    
    /// <summary>
    /// 连接时间
    /// </summary>
    public DateTime ConnectTime { get; set; }
    
    /// <summary>
    /// 最后活动时间
    /// </summary>
    public DateTime LastActivityTime { get; set; }
    
    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnected { get; set; }
    
    /// <summary>
    /// 重连尝试次数
    /// </summary>
    public int ReconnectAttempts { get; set; }
    
    /// <summary>
    /// 最后重连时间
    /// </summary>
    public DateTime? LastReconnectTime { get; set; }
    
    /// <summary>
    /// 断开连接时间
    /// </summary>
    public DateTime? DisconnectTime { get; set; }
    
    /// <summary>
    /// 断开连接原因
    /// </summary>
    public string DisconnectReason { get; set; }
    
    /// <summary>
    /// 协议版本
    /// </summary>
    public string ProtocolVersion { get; set; }
    
    /// <summary>
    /// 保持连接时间（秒）
    /// </summary>
    public int KeepAlive { get; set; }
    
    /// <summary>
    /// 是否使用TLS
    /// </summary>
    public bool UseTls { get; set; }
    
    /// <summary>
    /// 已发送的字节数
    /// </summary>
    public long BytesSent { get; set; }
    
    /// <summary>
    /// 已接收的字节数
    /// </summary>
    public long BytesReceived { get; set; }
}