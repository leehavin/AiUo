using AiUo.Logging;
using DotNetty.Transport.Channels;
using System.Text;

namespace AiUo.Extensions.DotNetty.Core.DotNetty;

/// <summary>
/// MQTT客户端处理器
/// </summary>
public class MQTTClientHandler : SimpleChannelInboundHandler<MQTTMessage>
{
    private readonly string _clientId;
    private readonly string _server;
    private readonly int _port;
    private readonly Dictionary<string, List<Func<MQTTMessage, Task>>> _topicHandlers;
    private readonly TaskCompletionSource<bool> _connectCompletion;
    
    public bool IsConnected { get; private set; }
    
    public MQTTClientHandler(string clientId, string server, int port)
    {
        _clientId = clientId;
        _server = server;
        _port = port;
        _topicHandlers = new Dictionary<string, List<Func<MQTTMessage, Task>>>();
        _connectCompletion = new TaskCompletionSource<bool>();
    }
    
    public override void ChannelActive(IChannelHandlerContext context)
    {
        // 发送CONNECT消息
        SendConnectMessage(context);
    }
    
    public override void ChannelInactive(IChannelHandlerContext context)
    {
        IsConnected = false;
        LogUtil.Warning("MQTT客户端已断开连接 {Server}:{Port}", _server, _port);
    }
    
    protected override void ChannelRead0(IChannelHandlerContext context, MQTTMessage message)
    {
        switch (message.MessageType)
        {
            case MQTTMessageType.CONNACK:
                HandleConnAck(context, message);
                break;
            case MQTTMessageType.PUBLISH:
                HandlePublish(context, message);
                break;
            case MQTTMessageType.PUBACK:
                HandlePubAck(context, message);
                break;
            case MQTTMessageType.PUBREC:
                HandlePubRec(context, message);
                break;
            case MQTTMessageType.PUBREL:
                HandlePubRel(context, message);
                break;
            case MQTTMessageType.PUBCOMP:
                HandlePubComp(context, message);
                break;
            case MQTTMessageType.SUBACK:
                HandleSubAck(context, message);
                break;
            case MQTTMessageType.UNSUBACK:
                HandleUnsubAck(context, message);
                break;
            case MQTTMessageType.PINGRESP:
                HandlePingResp(context, message);
                break;
        }
    }
    
    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        LogUtil.Error(exception, "MQTT客户端异常");
        context.CloseAsync();
    }
    
    /// <summary>
    /// 发送CONNECT消息
    /// </summary>
    /// <param name="context">通道处理上下文</param>
    private void SendConnectMessage(IChannelHandlerContext context)
    {
        // 按照MQTT v3.1.1协议构建CONNECT消息
        // 协议名称为"MQTT"，协议级别为4
        byte[] protocolName = Encoding.UTF8.GetBytes("MQTT");
        byte protocolLevel = 4; // MQTT v3.1.1
        
        // 连接标志
        // bit 7: 用户名标志 (0 - 不存在用户名)
        // bit 6: 密码标志 (0 - 不存在密码)
        // bit 5: 遗嘱保留 (0 - 遗嘱消息不保留)
        // bit 4-3: 遗嘱QoS (00 - QoS 0)
        // bit 2: 遗嘱标志 (0 - 无遗嘱)
        // bit 1: 清理会话 (1 - 清理会话)
        // bit 0: 保留位 (0)
        byte connectFlags = 0x02; // 清理会话
        
        // 保持连接时间（秒）
        ushort keepAlive = 60;
        
        // 计算可变头部长度
        // 协议名长度(2) + 协议名(4) + 协议级别(1) + 连接标志(1) + 保持连接(2)
        int variableHeaderLength = 2 + protocolName.Length + 1 + 1 + 2;
        
        // 计算有效载荷长度
        // 客户端ID长度(2) + 客户端ID
        byte[] clientIdBytes = Encoding.UTF8.GetBytes(_clientId);
        int payloadLength = 2 + clientIdBytes.Length;
        
        // 计算剩余长度
        int remainingLength = variableHeaderLength + payloadLength;
        
        // 构建CONNECT消息
        var message = new MQTTMessage
        {
            MessageType = MQTTMessageType.CONNECT,
            IsDuplicate = false,
            QoS = MQTTQualityOfServiceLevel.AtMostOnce,
            IsRetain = false,
            RemainingLength = remainingLength,
            // 有效载荷将在编码器中处理
            Payload = new byte[payloadLength + variableHeaderLength]
        };
        
        // 填充有效载荷
        int index = 0;
        
        // 协议名
        message.Payload[index++] = 0; // 长度高字节
        message.Payload[index++] = (byte)protocolName.Length; // 长度低字节
        Array.Copy(protocolName, 0, message.Payload, index, protocolName.Length);
        index += protocolName.Length;
        
        // 协议级别
        message.Payload[index++] = protocolLevel;
        
        // 连接标志
        message.Payload[index++] = connectFlags;
        
        // 保持连接
        message.Payload[index++] = (byte)(keepAlive >> 8); // 高字节
        message.Payload[index++] = (byte)(keepAlive & 0xFF); // 低字节
        
        // 客户端ID
        message.Payload[index++] = (byte)(clientIdBytes.Length >> 8); // 长度高字节
        message.Payload[index++] = (byte)(clientIdBytes.Length & 0xFF); // 长度低字节
        Array.Copy(clientIdBytes, 0, message.Payload, index, clientIdBytes.Length);
        
        LogUtil.Debug("发送CONNECT消息到MQTT服务器 {Server}:{Port}", _server, _port);
        context.WriteAndFlushAsync(message);
    }
    
    /// <summary>
    /// 处理CONNACK消息
    /// </summary>
    /// <param name="context">通道处理上下文</param>
    /// <param name="message">MQTT消息</param>
    private void HandleConnAck(IChannelHandlerContext context, MQTTMessage message)
    {
        // CONNACK消息格式：
        // 字节1：连接确认标志 (bit 0: 会话存在标志, bits 1-7: 保留)
        // 字节2：连接返回码
        //   0x00: 连接已接受
        //   0x01: 连接已拒绝，不支持的协议版本
        //   0x02: 连接已拒绝，标识符被拒绝
        //   0x03: 连接已拒绝，服务器不可用
        //   0x04: 连接已拒绝，用户名或密码错误
        //   0x05: 连接已拒绝，未授权
        if (message.Payload.Length < 2)
        {
            LogUtil.Error("CONNACK消息格式错误");
            _connectCompletion.TrySetException(new Exception("CONNACK消息格式错误"));
            return;
        }
        
        byte sessionPresent = (byte)(message.Payload[0] & 0x01);
        byte returnCode = message.Payload[1];
        
        switch (returnCode)
        {
            case 0x00: // 连接已接受
                IsConnected = true;
                _connectCompletion.TrySetResult(true);
                LogUtil.Info("MQTT客户端已连接到服务器 {Server}:{Port}, 会话存在: {SessionPresent}", 
                    _server, _port, sessionPresent == 1);
                break;
            case 0x01:
                LogUtil.Error("MQTT连接被拒绝: 不支持的协议版本");
                _connectCompletion.TrySetException(new Exception("MQTT连接被拒绝: 不支持的协议版本"));
                break;
            case 0x02:
                LogUtil.Error("MQTT连接被拒绝: 标识符被拒绝");
                _connectCompletion.TrySetException(new Exception("MQTT连接被拒绝: 标识符被拒绝"));
                break;
            case 0x03:
                LogUtil.Error("MQTT连接被拒绝: 服务器不可用");
                _connectCompletion.TrySetException(new Exception("MQTT连接被拒绝: 服务器不可用"));
                break;
            case 0x04:
                LogUtil.Error("MQTT连接被拒绝: 用户名或密码错误");
                _connectCompletion.TrySetException(new Exception("MQTT连接被拒绝: 用户名或密码错误"));
                break;
            case 0x05:
                LogUtil.Error("MQTT连接被拒绝: 未授权");
                _connectCompletion.TrySetException(new Exception("MQTT连接被拒绝: 未授权"));
                break;
            default:
                LogUtil.Error("MQTT连接被拒绝: 未知错误码 {ReturnCode}", returnCode);
                _connectCompletion.TrySetException(new Exception($"MQTT连接被拒绝: 未知错误码 {returnCode}"));
                break;
        }
        
        // 如果连接被拒绝，关闭连接
        if (returnCode != 0x00)
        {
            context.CloseAsync();
        }
    }
    
    /// <summary>
    /// 处理PUBLISH消息
    /// </summary>
    /// <param name="context">通道处理上下文</param>
    /// <param name="message">MQTT消息</param>
    private void HandlePublish(IChannelHandlerContext context, MQTTMessage message)
    {
        LogUtil.Debug("收到PUBLISH消息: 主题={Topic}, QoS={QoS}, 消息ID={MessageId}, 有效载荷长度={PayloadLength}", 
            message.Topic, message.QoS, message.MessageId, message.Payload?.Length ?? 0);
        
        // 根据主题查找处理器
        if (_topicHandlers.TryGetValue(message.Topic, out var handlers))
        {
            foreach (var handler in handlers)
            {
                // 异步处理消息
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await handler(message);
                    }
                    catch (Exception ex)
                    {
                        LogUtil.Error(ex, "MQTT消息处理异常: {Topic}", message.Topic);
                    }
                });
            }
        }
        else
        {
            // 没有找到对应主题的处理器
            LogUtil.Warning("未找到主题 {Topic} 的处理器", message.Topic);
        }
        
        // 根据QoS级别发送确认消息
        switch (message.QoS)
        {
            case MQTTQualityOfServiceLevel.AtMostOnce:
                // QoS 0: 最多一次，不需要确认
                break;
                
            case MQTTQualityOfServiceLevel.AtLeastOnce:
                // QoS 1: 至少一次，发送PUBACK
                var puback = new MQTTMessage
                {
                    MessageType = MQTTMessageType.PUBACK,
                    QoS = MQTTQualityOfServiceLevel.AtMostOnce,
                    MessageId = message.MessageId,
                    // PUBACK消息的有效载荷为空
                    Payload = new byte[2] // 消息ID (2字节)
                };
                
                // 设置消息ID (大端序)
                puback.Payload[0] = (byte)(message.MessageId >> 8);
                puback.Payload[1] = (byte)(message.MessageId & 0xFF);
                
                LogUtil.Debug("发送PUBACK消息: 消息ID={MessageId}", message.MessageId);
                context.WriteAndFlushAsync(puback);
                break;
                
            case MQTTQualityOfServiceLevel.ExactlyOnce:
                // QoS 2: 精确一次，发送PUBREC
                var pubrec = new MQTTMessage
                {
                    MessageType = MQTTMessageType.PUBREC,
                    QoS = MQTTQualityOfServiceLevel.AtMostOnce,
                    MessageId = message.MessageId,
                    // PUBREC消息的有效载荷为空
                    Payload = new byte[2] // 消息ID (2字节)
                };
                
                // 设置消息ID (大端序)
                pubrec.Payload[0] = (byte)(message.MessageId >> 8);
                pubrec.Payload[1] = (byte)(message.MessageId & 0xFF);
                
                LogUtil.Debug("发送PUBREC消息: 消息ID={MessageId}", message.MessageId);
                context.WriteAndFlushAsync(pubrec);
                break;
        }
    }
    
    /// <summary>
    /// 处理PUBACK消息 (QoS 1的响应)
    /// </summary>
    /// <param name="context">通道处理上下文</param>
    /// <param name="message">MQTT消息</param>
    private void HandlePubAck(IChannelHandlerContext context, MQTTMessage message)
    {
        // PUBACK消息格式：
        // 字节1-2：消息ID
        if (message.Payload.Length < 2)
        {
            LogUtil.Error("PUBACK消息格式错误");
            return;
        }
        
        // 解析消息ID (大端序)
        ushort messageId = (ushort)((message.Payload[0] << 8) | message.Payload[1]);
        LogUtil.Debug("收到PUBACK消息: 消息ID={MessageId}", messageId);
        
        // 在实际应用中，可以在此处理QoS 1消息的确认逻辑
        // 例如从待确认消息队列中移除已确认的消息
    }
    
    /// <summary>
    /// 处理PUBREC消息 (QoS 2的第一阶段响应)
    /// </summary>
    /// <param name="context">通道处理上下文</param>
    /// <param name="message">MQTT消息</param>
    private void HandlePubRec(IChannelHandlerContext context, MQTTMessage message)
    {
        // PUBREC消息格式：
        // 字节1-2：消息ID
        if (message.Payload.Length < 2)
        {
            LogUtil.Error("PUBREC消息格式错误");
            return;
        }
        
        // 解析消息ID (大端序)
        ushort messageId = (ushort)((message.Payload[0] << 8) | message.Payload[1]);
        LogUtil.Debug("收到PUBREC消息: 消息ID={MessageId}", messageId);
        
        // 发送PUBREL消息 (QoS 2的第二阶段)
        var pubrel = new MQTTMessage
        {
            MessageType = MQTTMessageType.PUBREL,
            QoS = MQTTQualityOfServiceLevel.AtLeastOnce, // PUBREL消息必须使用QoS 1
            MessageId = messageId,
            Payload = new byte[2] // 消息ID (2字节)
        };
        
        // 设置消息ID (大端序)
        pubrel.Payload[0] = (byte)(messageId >> 8);
        pubrel.Payload[1] = (byte)(messageId & 0xFF);
        
        LogUtil.Debug("发送PUBREL消息: 消息ID={MessageId}", messageId);
        context.WriteAndFlushAsync(pubrel);
    }
    
    /// <summary>
    /// 处理PUBREL消息 (QoS 2的第二阶段响应)
    /// </summary>
    /// <param name="context">通道处理上下文</param>
    /// <param name="message">MQTT消息</param>
    private void HandlePubRel(IChannelHandlerContext context, MQTTMessage message)
    {
        // PUBREL消息格式：
        // 字节1-2：消息ID
        if (message.Payload.Length < 2)
        {
            LogUtil.Error("PUBREL消息格式错误");
            return;
        }
        
        // 解析消息ID (大端序)
        ushort messageId = (ushort)((message.Payload[0] << 8) | message.Payload[1]);
        LogUtil.Debug("收到PUBREL消息: 消息ID={MessageId}", messageId);
        
        // 发送PUBCOMP消息 (QoS 2的第三阶段)
        var pubcomp = new MQTTMessage
        {
            MessageType = MQTTMessageType.PUBCOMP,
            QoS = MQTTQualityOfServiceLevel.AtMostOnce,
            MessageId = messageId,
            Payload = new byte[2] // 消息ID (2字节)
        };
        
        // 设置消息ID (大端序)
        pubcomp.Payload[0] = (byte)(messageId >> 8);
        pubcomp.Payload[1] = (byte)(messageId & 0xFF);
        
        LogUtil.Debug("发送PUBCOMP消息: 消息ID={MessageId}", messageId);
        context.WriteAndFlushAsync(pubcomp);
        
        // 在实际应用中，可以在此处理QoS 2消息的最终确认逻辑
        // 例如将消息传递给应用层处理
    }
    
    /// <summary>
    /// 处理PUBCOMP消息 (QoS 2的第三阶段响应)
    /// </summary>
    /// <param name="context">通道处理上下文</param>
    /// <param name="message">MQTT消息</param>
    private void HandlePubComp(IChannelHandlerContext context, MQTTMessage message)
    {
        // PUBCOMP消息格式：
        // 字节1-2：消息ID
        if (message.Payload.Length < 2)
        {
            LogUtil.Error("PUBCOMP消息格式错误");
            return;
        }
        
        // 解析消息ID (大端序)
        ushort messageId = (ushort)((message.Payload[0] << 8) | message.Payload[1]);
        LogUtil.Debug("收到PUBCOMP消息: 消息ID={MessageId}", messageId);
        
        // 在实际应用中，可以在此处理QoS 2消息的最终确认逻辑
        // 例如从待确认消息队列中移除已确认的消息
    }
    
    /// <summary>
    /// 处理SUBACK消息
    /// </summary>
    /// <param name="context">通道处理上下文</param>
    /// <param name="message">MQTT消息</param>
    private void HandleSubAck(IChannelHandlerContext context, MQTTMessage message)
    {
        // SUBACK消息格式：
        // 字节1-2：消息ID
        // 字节3-n：返回码列表 (每个主题一个返回码)
        //   0x00: 成功 - QoS 0
        //   0x01: 成功 - QoS 1
        //   0x02: 成功 - QoS 2
        //   0x80: 失败
        if (message.Payload.Length < 3)
        {
            LogUtil.Error("SUBACK消息格式错误");
            return;
        }
        
        // 解析消息ID (大端序)
        ushort messageId = (ushort)((message.Payload[0] << 8) | message.Payload[1]);
        
        // 解析返回码列表
        byte[] returnCodes = new byte[message.Payload.Length - 2];
        Array.Copy(message.Payload, 2, returnCodes, 0, returnCodes.Length);
        
        // 记录订阅结果
        for (int i = 0; i < returnCodes.Length; i++)
        {
            byte returnCode = returnCodes[i];
            if (returnCode == 0x80)
            {
                LogUtil.Warning("订阅失败: 消息ID={MessageId}, 索引={Index}", messageId, i);
            }
            else
            {
                LogUtil.Debug("订阅成功: 消息ID={MessageId}, 索引={Index}, QoS={QoS}", 
                    messageId, i, returnCode);
            }
        }
    }
    
    /// <summary>
    /// 处理UNSUBACK消息
    /// </summary>
    /// <param name="context">通道处理上下文</param>
    /// <param name="message">MQTT消息</param>
    private void HandleUnsubAck(IChannelHandlerContext context, MQTTMessage message)
    {
        // UNSUBACK消息格式：
        // 字节1-2：消息ID
        if (message.Payload.Length < 2)
        {
            LogUtil.Error("UNSUBACK消息格式错误");
            return;
        }
        
        // 解析消息ID (大端序)
        ushort messageId = (ushort)((message.Payload[0] << 8) | message.Payload[1]);
        LogUtil.Debug("取消订阅成功: 消息ID={MessageId}", messageId);
    }
    
    /// <summary>
    /// 处理PINGRESP消息
    /// </summary>
    /// <param name="context">通道处理上下文</param>
    /// <param name="message">MQTT消息</param>
    private void HandlePingResp(IChannelHandlerContext context, MQTTMessage message)
    {
        // PINGRESP消息没有有效载荷
        LogUtil.Debug("收到PINGRESP消息");
        
        // 在实际应用中，可以在此更新最后一次接收到PINGRESP的时间戳
        // 用于检测连接是否活跃
    }
    
    /// <summary>
    /// 等待连接完成
    /// </summary>
    /// <param name="timeout">超时时间</param>
    /// <returns>连接是否成功</returns>
    public Task<bool> WaitForConnectAsync(TimeSpan timeout)
    {
        return Task.WhenAny(_connectCompletion.Task, Task.Delay(timeout))
            .ContinueWith(t => t.Result == _connectCompletion.Task && _connectCompletion.Task.Result);
    }
    
    /// <summary>
    /// 添加主题处理器
    /// </summary>
    public void AddTopicHandler(string topic, Func<MQTTMessage, Task> handler)
    {
        if (!_topicHandlers.TryGetValue(topic, out var handlers))
        {
            handlers = new List<Func<MQTTMessage, Task>>();
            _topicHandlers[topic] = handlers;
        }
        
        handlers.Add(handler);
    }
    
    /// <summary>
    /// 移除主题处理器
    /// </summary>
    public void RemoveTopicHandlers(string topic)
    {
        _topicHandlers.Remove(topic);
    }
    
    /// <summary>
    /// 发送SUBSCRIBE消息
    /// </summary>
    /// <param name="context">通道处理上下文</param>
    /// <param name="topic">主题</param>
    /// <param name="qos">服务质量级别</param>
    /// <param name="messageId">消息ID</param>
    public void Subscribe(IChannelHandlerContext context, string topic, MQTTQualityOfServiceLevel qos, ushort messageId)
    {
        // SUBSCRIBE消息格式：
        // 可变头部：
        //   字节1-2：消息ID
        // 有效载荷：
        //   主题过滤器1长度 (2字节)
        //   主题过滤器1 (n字节)
        //   请求的QoS1 (1字节)
        //   [更多主题过滤器和QoS...]
        
        // 计算有效载荷长度
        byte[] topicBytes = Encoding.UTF8.GetBytes(topic);
        int payloadLength = 2 + topicBytes.Length + 1; // 主题长度(2) + 主题 + QoS(1)
        
        // 构建SUBSCRIBE消息
        var message = new MQTTMessage
        {
            MessageType = MQTTMessageType.SUBSCRIBE,
            QoS = MQTTQualityOfServiceLevel.AtLeastOnce, // SUBSCRIBE消息必须使用QoS 1
            MessageId = messageId,
            Payload = new byte[2 + payloadLength] // 消息ID(2) + 有效载荷
        };
        
        // 填充有效载荷
        int index = 0;
        
        // 消息ID (大端序)
        message.Payload[index++] = (byte)(messageId >> 8);
        message.Payload[index++] = (byte)(messageId & 0xFF);
        
        // 主题长度 (大端序)
        message.Payload[index++] = (byte)(topicBytes.Length >> 8);
        message.Payload[index++] = (byte)(topicBytes.Length & 0xFF);
        
        // 主题
        Array.Copy(topicBytes, 0, message.Payload, index, topicBytes.Length);
        index += topicBytes.Length;
        
        // 请求的QoS
        message.Payload[index] = (byte)qos;
        
        LogUtil.Debug("发送SUBSCRIBE消息: 主题={Topic}, QoS={QoS}, 消息ID={MessageId}", 
            topic, qos, messageId);
        context.WriteAndFlushAsync(message);
    }
    
    /// <summary>
    /// 发送UNSUBSCRIBE消息
    /// </summary>
    /// <param name="context">通道处理上下文</param>
    /// <param name="topic">主题</param>
    /// <param name="messageId">消息ID</param>
    public void Unsubscribe(IChannelHandlerContext context, string topic, ushort messageId)
    {
        // UNSUBSCRIBE消息格式：
        // 可变头部：
        //   字节1-2：消息ID
        // 有效载荷：
        //   主题过滤器1长度 (2字节)
        //   主题过滤器1 (n字节)
        //   [更多主题过滤器...]
        
        // 计算有效载荷长度
        byte[] topicBytes = Encoding.UTF8.GetBytes(topic);
        int payloadLength = 2 + topicBytes.Length; // 主题长度(2) + 主题
        
        // 构建UNSUBSCRIBE消息
        var message = new MQTTMessage
        {
            MessageType = MQTTMessageType.UNSUBSCRIBE,
            QoS = MQTTQualityOfServiceLevel.AtLeastOnce, // UNSUBSCRIBE消息必须使用QoS 1
            MessageId = messageId,
            Payload = new byte[2 + payloadLength] // 消息ID(2) + 有效载荷
        };
        
        // 填充有效载荷
        int index = 0;
        
        // 消息ID (大端序)
        message.Payload[index++] = (byte)(messageId >> 8);
        message.Payload[index++] = (byte)(messageId & 0xFF);
        
        // 主题长度 (大端序)
        message.Payload[index++] = (byte)(topicBytes.Length >> 8);
        message.Payload[index++] = (byte)(topicBytes.Length & 0xFF);
        
        // 主题
        Array.Copy(topicBytes, 0, message.Payload, index, topicBytes.Length);
        
        LogUtil.Debug("发送UNSUBSCRIBE消息: 主题={Topic}, 消息ID={MessageId}", 
            topic, messageId);
        context.WriteAndFlushAsync(message);
    }
    /// <summary>
    /// 发送PUBLISH消息
    /// </summary>
    /// <param name="context">通道处理上下文</param>
    /// <param name="topic">主题</param>
    /// <param name="payload">有效载荷</param>
    /// <param name="qos">服务质量级别</param>
    /// <param name="retain">是否保留消息</param>
    /// <param name="messageId">消息ID (QoS > 0时必须提供)</param>
    public void Publish(IChannelHandlerContext context, string topic, byte[] payload, MQTTQualityOfServiceLevel qos, bool retain, ushort messageId = 0)
    {
        // PUBLISH消息格式：
        // 可变头部：
        //   主题名长度 (2字节)
        //   主题名 (n字节)
        //   消息ID (2字节, 仅当QoS > 0时)
        // 有效载荷：
        //   消息内容 (n字节)
        
        // 验证参数
        if (string.IsNullOrEmpty(topic))
            throw new ArgumentException("主题不能为空", nameof(topic));
            
        if (qos > MQTTQualityOfServiceLevel.AtMostOnce && messageId == 0)
            throw new ArgumentException("QoS > 0时必须提供消息ID", nameof(messageId));
        
        // 计算可变头部长度
        byte[] topicBytes = Encoding.UTF8.GetBytes(topic);
        int variableHeaderLength = 2 + topicBytes.Length; // 主题长度(2) + 主题
        
        // QoS > 0 时需要消息ID
        if (qos > MQTTQualityOfServiceLevel.AtMostOnce)
            variableHeaderLength += 2; // 消息ID(2)
        
        // 计算有效载荷长度
        int payloadLength = payload?.Length ?? 0;
        
        // 计算剩余长度
        int remainingLength = variableHeaderLength + payloadLength;
        
        // 构建PUBLISH消息
        var message = new MQTTMessage
        {
            MessageType = MQTTMessageType.PUBLISH,
            IsDuplicate = false,
            QoS = qos,
            IsRetain = retain,
            MessageId = messageId,
            Topic = topic,
            RemainingLength = remainingLength,
            Payload = new byte[variableHeaderLength + payloadLength]
        };
        
        // 填充可变头部和有效载荷
        int index = 0;
        
        // 主题长度 (大端序)
        message.Payload[index++] = (byte)(topicBytes.Length >> 8);
        message.Payload[index++] = (byte)(topicBytes.Length & 0xFF);
        
        // 主题
        Array.Copy(topicBytes, 0, message.Payload, index, topicBytes.Length);
        index += topicBytes.Length;
        
        // QoS > 0 时设置消息ID (大端序)
        if (qos > MQTTQualityOfServiceLevel.AtMostOnce)
        {
            message.Payload[index++] = (byte)(messageId >> 8);
            message.Payload[index++] = (byte)(messageId & 0xFF);
        }
        
        // 有效载荷
        if (payload != null && payload.Length > 0)
            Array.Copy(payload, 0, message.Payload, index, payload.Length);
        
        LogUtil.Debug("发送PUBLISH消息: 主题={Topic}, QoS={QoS}, 保留={Retain}, 消息ID={MessageId}, 有效载荷长度={PayloadLength}", 
            topic, qos, retain, messageId, payloadLength);
        context.WriteAndFlushAsync(message);
    }
    
    /// <summary>
    /// 发送PINGREQ消息
    /// </summary>
    /// <param name="context">通道处理上下文</param>
    public void SendPingReq(IChannelHandlerContext context)
    {
        // PINGREQ消息格式：
        // 固定头部：
        //   字节1：消息类型(12) << 4
        //   字节2：剩余长度(0)
        // 无可变头部和有效载荷
        
        var message = new MQTTMessage
        {
            MessageType = MQTTMessageType.PINGREQ,
            IsDuplicate = false,
            QoS = MQTTQualityOfServiceLevel.AtMostOnce,
            IsRetain = false,
            RemainingLength = 0,
            Payload = new byte[0]
        };
        
        LogUtil.Debug("发送PINGREQ消息");
        context.WriteAndFlushAsync(message);
    }
    
    /// <summary>
    /// 发送DISCONNECT消息
    /// </summary>
    /// <param name="context">通道处理上下文</param>
    public void SendDisconnect(IChannelHandlerContext context)
    {
        // DISCONNECT消息格式：
        // 固定头部：
        //   字节1：消息类型(14) << 4
        //   字节2：剩余长度(0)
        // 无可变头部和有效载荷
        
        var message = new MQTTMessage
        {
            MessageType = MQTTMessageType.DISCONNECT,
            IsDuplicate = false,
            QoS = MQTTQualityOfServiceLevel.AtMostOnce,
            IsRetain = false,
            RemainingLength = 0,
            Payload = new byte[0]
        };
        
        LogUtil.Debug("发送DISCONNECT消息");
        context.WriteAndFlushAsync(message);
        
        // 标记连接已断开
        IsConnected = false;
    }
}