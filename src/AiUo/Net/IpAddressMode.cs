namespace AiUo.Net;

/// <summary>
/// IP地址类型
/// </summary>
public enum IpAddressMode
{
    /// <summary>
    /// 未知
    /// </summary>
    Unknown,

    /// <summary>
    /// 环回地址 127.0.0.1 - 127.255.255.254
    /// </summary>
    Loopback,

    /// <summary>
    /// 本机IP
    /// </summary>
    Local,

    /// <summary>
    /// 内网地址
    /// 10.0.0.0 - 10.255.255.255 
    /// 172.16.0.0 - 172.31.255.255 
    /// 192.168.0.0 - 192.168.255.555  
    /// </summary>
    Intranet,

    /// <summary>
    /// 公网地址
    /// </summary>
    External,

    /// <summary>
    /// 多播地址 224.0.0.0到239.255.255.255
    /// </summary>
    Multicast
}