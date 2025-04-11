using AiUo.Configuration;
using AiUo.Logging;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AiUo.Extensions.MQTT.Server;

/// <summary>
/// MQTT服务器工具类，提供服务器相关的便捷方法
/// </summary>
public static class MQTTServerUtil
{
    /// <summary>
    /// 获取MQTT服务器实例
    /// </summary>
    /// <returns>MQTT服务器实例</returns>
    public static MQTTServer GetServer()
    {
        var container = DIUtil.GetRequiredService<MQTTServerContainer>();
        if (!container.IsInitialized || container.Server == null)
            throw new InvalidOperationException("MQTT服务器未初始化或未启动");
            
        return container.Server;
    }

    /// <summary>
    /// 检查MQTT服务器是否已启动
    /// </summary>
    /// <returns>是否已启动</returns>
    public static bool IsServerStarted()
    {
        try
        {
            var container = DIUtil.GetService<MQTTServerContainer>();
            return container != null && container.IsInitialized && container.Server != null && container.Server.IsStarted;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 从服务器发布消息到指定主题
    /// </summary>
    /// <param name="topic">主题</param>
    /// <param name="payload">消息内容</param>
    /// <param name="qosLevel">服务质量等级(0,1,2)</param>
    /// <param name="retain">是否保留消息</param>
    public static async Task PublishFromServerAsync(string topic, byte[] payload, int qosLevel = 0, bool retain = false)
    {
        try
        {
            var server = GetServer();
            await server.PublishAsync(topic, payload, qosLevel, retain);
            
            if (ConfigUtil.GetSection<MQTTSection>()?.MessageLogEnabled == true)
            {
                LogUtil.Info("[MQTT Server] 发布消息: 主题={Topic}, QoS={QosLevel}, Retain={Retain}, 大小={Size}字节", 
                    topic, qosLevel, retain, payload.Length);
            }
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "[MQTT Server] 发布消息失败: 主题={Topic}", topic);
            throw;
        }
    }

    /// <summary>
    /// 从服务器发布消息到指定主题
    /// </summary>
    /// <typeparam name="TMessage">消息类型</typeparam>
    /// <param name="message">消息对象</param>
    /// <param name="topic">主题</param>
    /// <param name="qosLevel">服务质量等级(0,1,2)</param>
    /// <param name="retain">是否保留消息</param>
    public static async Task PublishFromServerAsync<TMessage>(TMessage message, string topic, int qosLevel = 0, bool retain = false)
        where TMessage : class, new()
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            var payload = Encoding.UTF8.GetBytes(json);
            
            await PublishFromServerAsync(topic, payload, qosLevel, retain);
            
            if (ConfigUtil.GetSection<MQTTSection>()?.DebugLogEnabled == true)
            {
                LogUtil.Debug("[MQTT Server] 发布消息内容: {Content}", json);
            }
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "[MQTT Server] 发布消息失败: 主题={Topic}", topic);
            throw;
        }
    }

    /// <summary>
    /// 从服务器发布MQTT消息
    /// </summary>
    /// <typeparam name="TMessage">消息类型</typeparam>
    /// <param name="message">消息对象</param>
    /// <param name="topic">消息主题，如果为null则使用消息类型上的MQTTPublishMessageAttribute中的Topic</param>
    /// <param name="qosLevel">服务质量等级(0,1,2)，如果为-1则使用消息类型上的MQTTPublishMessageAttribute中的QosLevel</param>
    /// <param name="retain">是否保留消息，如果为null则使用消息类型上的MQTTPublishMessageAttribute中的Retain</param>
    public static async Task PublishFromServerAsync<TMessage>(TMessage message, string topic = null, int qosLevel = -1, bool? retain = null)
        where TMessage : class, new()
    {
        // 获取消息属性
        var attr = MQTTUtil.GetMessageAttribute<MQTTPublishMessageAttribute>(message);
        if (attr == null && string.IsNullOrEmpty(topic))
            throw new Exception("消息类型上没有MQTTPublishMessageAttribute特性，必须指定topic参数");

        // 确定主题
        var actualTopic = topic ?? attr.Topic;
        if (string.IsNullOrEmpty(actualTopic))
            throw new Exception("主题不能为空");

        // 确定QoS级别
        var actualQosLevel = qosLevel >= 0 ? qosLevel : (attr?.QosLevel ?? 0);
        if (actualQosLevel < 0 || actualQosLevel > 2)
            throw new Exception($"QosLevel必须为0-2之间的值: {actualQosLevel}");

        // 确定是否保留消息
        var actualRetain = retain ?? attr?.Retain ?? false;

        // 发布消息
        await PublishFromServerAsync(message, actualTopic, actualQosLevel, actualRetain);
    }
}