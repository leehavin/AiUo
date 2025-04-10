using MQTTnet;

namespace AiUo.Extensions.MQTT.Exceptions;

/// <summary>
/// MQTT连接异常
/// </summary>
public class MQTTConnectionException : MQTTException
{
    /// <summary>
    /// 连接结果
    /// </summary>
    public MqttClientConnectResultCode ResultCode { get; }

    /// <summary>
    /// 服务器地址
    /// </summary>
    public string Server { get; }

    /// <summary>
    /// 端口
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// 创建一个新的MQTT连接异常
    /// </summary>
    /// <param name="server">服务器地址</param>
    /// <param name="port">端口</param>
    /// <param name="resultCode">连接结果代码</param>
    public MQTTConnectionException(string server, int port, MqttClientConnectResultCode resultCode)
        : base($"MQTT连接失败: {server}:{port}, 错误代码: {resultCode}")
    {
        Server = server;
        Port = port;
        ResultCode = resultCode;
    }

    /// <summary>
    /// 使用指定错误消息创建一个新的MQTT连接异常
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="server">服务器地址</param>
    /// <param name="port">端口</param>
    /// <param name="innerException">内部异常</param>
    public MQTTConnectionException(string message, string server, int port, Exception innerException = null)
        : base(message, innerException)
    {
        Server = server;
        Port = port;
    }
}