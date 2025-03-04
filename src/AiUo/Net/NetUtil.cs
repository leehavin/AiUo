using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace AiUo.Net;

/// <summary>
/// 网络通用类
/// </summary>
public static class NetUtil
{
    #region IP 操作
    /// <summary>
    /// 转换long类型的IP值为字符串类型的IP地址
    /// </summary>
    /// <param name="ip">long类型的IP值</param>
    /// <returns></returns>
    public static string GetIpString(long ip)
        => new IPAddress(ip).ToString();

    /// <summary>
    /// 转换字符串类型的IP地址为long类型的值
    /// </summary>
    /// <param name="ip">IP地址，如：127.0.0.1</param>
    /// <returns></returns>
    public static long GetIpLong(string ip)
    {
        long ret = 0;
        byte[] data = IPAddress.Parse(ip).GetAddressBytes();
        for (int i = 0; i < data.Length; i++)
        {
            ret += data[i] * (long)Math.Pow(256, i);
        }
        return ret;
    }

    /// <summary>
    /// 获取IP地址类型
    /// </summary>
    /// <param name="ip">IP地址</param>
    /// <returns></returns>
    public static IpAddressMode GetIpMode(string ip)
    {
        if (ip == "::1" || ip == "0.0.0.1" || GetLocalIPs().Contains(ip))
            return IpAddressMode.Local;
        if (!IPAddress.TryParse(ip, out var address))
            return IpAddressMode.Unknown;
        var bytes = address.GetAddressBytes();
        if (bytes[0] == 127)
            return IpAddressMode.Loopback;
        return (bytes switch
        {
            var x when x[0] == 10 => true,
            var x when x[0] == 172 && x[1] >= 16 && x[1] <= 31 => true,
            var x when x[0] == 192 && x[1] == 168 => true,
            _ => false
        }) ? IpAddressMode.Intranet : IpAddressMode.External;
    }

    private static HashSet<string> _localIps;
    /// <summary>
    /// 获取本机 IPV4 集合
    /// </summary>
    /// <returns></returns>
    public static List<string> GetLocalIPs()
    {
        if(_localIps != null)
            return _localIps.ToList();
        var ret = new List<string>();
        var ipEntry = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in ipEntry.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                ret.Add(ip.ToString());
        }
        _localIps =  ret.ToHashSet();
        return ret;
    }

    private static string _localIp;
    /// <summary>
    /// 获取一个IPv4 地址
    /// </summary>
    /// <returns></returns>
    public static string GetLocalIP()
    {
        if (!string.IsNullOrEmpty(_localIp))
            return _localIp;
        string ret = null;
        //获取所有网卡
        var networks = NetworkInterface.GetAllNetworkInterfaces();
        //遍历数组
        foreach (var network in networks)
        {
            if (network.OperationalStatus != OperationalStatus.Up)
                continue;
            //单个网卡的IP对象
            var props = network.GetIPProperties();
            if (props.GatewayAddresses.Count == 0) continue;
            foreach(var ip in props.UnicastAddresses)
            {
                if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                    && !System.Net.IPAddress.IsLoopback(ip.Address))
                {
                    ret = ip.Address.ToString();
                }
            }
        }
        _localIp = ret;
        return ret;
    }
        
    /// <summary>
    /// 获得本地IP地址集合
    /// </summary>
    /// <returns></returns>
    public static IPAddress[] GetHostAddresses()
        => Dns.GetHostAddresses(Dns.GetHostName());
        
    /// <summary>
    /// 是否是局域网地址
    /// </summary>
    /// <param name="ipv4Address"></param>
    /// <returns></returns>
    public static bool IsPrivateNetwork(string ipv4Address)
    {
        if (IPAddress.TryParse(ipv4Address, out _))
        {
            if (ipv4Address.StartsWith("192.168.") || ipv4Address.StartsWith("10."))
            {
                return true;
            }

            if (ipv4Address.StartsWith("172."))
            {
                string seg2 = ipv4Address[4..7];
                if (seg2.EndsWith('.') &&
                    String.Compare(seg2, "16.") >= 0 &&
                    String.Compare(seg2, "31.") <= 0)
                {
                    return true;
                }
            }
        }

        return false;
    }
    #endregion

    /// <summary>
    /// 检测监听的端口通讯是否正常
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <param name="port"></param>
    /// <returns></returns>
    public static bool TestListenPort(string ipAddress, int port)
    {
        IPAddress ip = IPAddress.Parse(ipAddress);
        try
        {
            var point = new IPEndPoint(ip, port);
            using (var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                sock.Connect(point);
                sock.Close();
                return true;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 判断文件类型是否为WEB格式图片(注：JPG,GIF,BMP,PNG)
    /// </summary>
    /// <param name="contentType">HTTP MIME 类型</param>
    /// <returns></returns>
    public static bool IsWebImage(string contentType)
        => contentType == "image/pjpeg" || contentType == "image/jpeg" || contentType == "image/gif"
           || contentType == "image/bmp" || contentType == "image/png" || contentType == "image/x-png";

    /// <summary>
    /// StatusCode是否成功
    ///     1xx - 信息输出
    ///     2xx - 成功输出
    ///     3xx - 跳转
    ///     4xx - 客户端错误
    ///     5xx - 服务端异常
    /// </summary>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static bool IsSuccessStatusCode(int statusCode)
        => statusCode >= 200 && statusCode < 300;

    /// <summary>
    /// 文件扩展名映射的HTTP content-type
    /// </summary>
    /// <param name="fileExt"></param>
    /// <returns></returns>
    public static string GetContentTypeByFileExt(string fileExt) 
        => FileExtContentTypeMapper.GetContentType(fileExt);
    /// <summary>
    /// HTTP content-type映射的文件扩展名
    /// </summary>
    /// <param name="contentType"></param>
    /// <returns></returns>
    public static string GetFileExtByContentType(string contentType) 
        => FileExtContentTypeMapper.GetFileExtension(contentType);
}