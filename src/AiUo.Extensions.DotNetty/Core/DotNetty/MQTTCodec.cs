using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using System.Text;

namespace AiUo.Extensions.DotNetty.Core.DotNetty;

/// <summary>
/// MQTT消息类型
/// </summary>
public enum MQTTMessageType
{
    CONNECT = 1,
    CONNACK = 2,
    PUBLISH = 3,
    PUBACK = 4,
    PUBREC = 5,
    PUBREL = 6,
    PUBCOMP = 7,
    SUBSCRIBE = 8,
    SUBACK = 9,
    UNSUBSCRIBE = 10,
    UNSUBACK = 11,
    PINGREQ = 12,
    PINGRESP = 13,
    DISCONNECT = 14
}

/// <summary>
/// MQTT消息
/// </summary>
public class MQTTMessage
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public MQTTMessageType MessageType { get; set; }
    
    /// <summary>
    /// 是否是重复消息
    /// </summary>
    public bool IsDuplicate { get; set; }
    
    /// <summary>
    /// 服务质量级别
    /// </summary>
    public MQTTQualityOfServiceLevel QoS { get; set; }
    
    /// <summary>
    /// 是否保留消息
    /// </summary>
    public bool IsRetain { get; set; }
    
    /// <summary>
    /// 剩余长度
    /// </summary>
    public int RemainingLength { get; set; }
    
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
}

/// <summary>
/// MQTT编码器
/// </summary>
public class MQTTEncoder : MessageToByteEncoder<MQTTMessage>
{
    protected override void Encode(IChannelHandlerContext context, MQTTMessage message, IByteBuffer output)
    {
        // 固定头部第一个字节
        byte fixedHeaderFirstByte = (byte)((byte)message.MessageType << 4);
        if (message.IsDuplicate)
            fixedHeaderFirstByte |= 0x08;
        fixedHeaderFirstByte |= (byte)((byte)message.QoS << 1);
        if (message.IsRetain)
            fixedHeaderFirstByte |= 0x01;
        
        output.WriteByte(fixedHeaderFirstByte);
        
        // 计算可变头部和有效载荷的长度
        int remainingLength = 0;
        
        // 对于PUBLISH消息，计算主题长度和有效载荷长度
        if (message.MessageType == MQTTMessageType.PUBLISH)
        {
            byte[] topicBytes = Encoding.UTF8.GetBytes(message.Topic);
            remainingLength += 2 + topicBytes.Length; // 主题长度字段(2字节) + 主题
            
            // QoS > 0 时需要消息ID
            if (message.QoS > MQTTQualityOfServiceLevel.AtMostOnce)
                remainingLength += 2; // 消息ID(2字节)
            
            // 有效载荷
            if (message.Payload != null)
                remainingLength += message.Payload.Length;
        }
        else
        {
            // 其他消息类型的可变头部长度
            // 这里简化处理，实际实现需要根据不同消息类型计算
            remainingLength = message.RemainingLength;
        }
        
        // 编码剩余长度
        EncodeRemainingLength(remainingLength, output);
        
        // 编码可变头部和有效载荷
        if (message.MessageType == MQTTMessageType.PUBLISH)
        {
            // 编码主题
            byte[] topicBytes = Encoding.UTF8.GetBytes(message.Topic);
            output.WriteShort(topicBytes.Length);
            output.WriteBytes(topicBytes);
            
            // QoS > 0 时编码消息ID
            if (message.QoS > MQTTQualityOfServiceLevel.AtMostOnce)
                output.WriteShort(message.MessageId);
            
            // 编码有效载荷
            if (message.Payload != null)
                output.WriteBytes(message.Payload);
        }
        // 其他消息类型的编码在此省略，实际实现需要根据不同消息类型处理
    }
    
    private void EncodeRemainingLength(int length, IByteBuffer buffer)
    {
        do
        {
            byte digit = (byte)(length % 128);
            length /= 128;
            if (length > 0)
                digit |= 0x80;
            buffer.WriteByte(digit);
        } while (length > 0);
    }
}

/// <summary>
/// MQTT解码器
/// </summary>
public class MQTTDecoder : ByteToMessageDecoder
{
    protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
    {
        if (input.ReadableBytes < 2) // 至少需要固定头部的第一个字节和一个剩余长度字节
            return;
        
        input.MarkReaderIndex();
        
        // 读取固定头部第一个字节
        byte fixedHeaderFirstByte = input.ReadByte();
        
        // 解析消息类型
        MQTTMessageType messageType = (MQTTMessageType)((fixedHeaderFirstByte >> 4) & 0x0F);
        
        // 解析标志位
        bool isDuplicate = (fixedHeaderFirstByte & 0x08) > 0;
        MQTTQualityOfServiceLevel qos = (MQTTQualityOfServiceLevel)((fixedHeaderFirstByte >> 1) & 0x03);
        bool isRetain = (fixedHeaderFirstByte & 0x01) > 0;
        
        // 解析剩余长度
        int remainingLength = 0;
        int multiplier = 1;
        byte digit;
        int bytesRead = 0;
        
        do
        {
            if (input.ReadableBytes < 1)
            {
                input.ResetReaderIndex();
                return; // 需要更多数据
            }
            
            digit = input.ReadByte();
            bytesRead++;
            remainingLength += (digit & 127) * multiplier;
            multiplier *= 128;
            
            if (multiplier > 128 * 128 * 128)
                throw new DecoderException("MQTT剩余长度字段无效");
                
        } while ((digit & 128) != 0);
        
        // 检查是否有足够的字节可读
        if (input.ReadableBytes < remainingLength)
        {
            input.ResetReaderIndex();
            return; // 需要更多数据
        }
        
        // 创建MQTT消息对象
        MQTTMessage message = new MQTTMessage
        {
            MessageType = messageType,
            IsDuplicate = isDuplicate,
            QoS = qos,
            IsRetain = isRetain,
            RemainingLength = remainingLength
        };
        
        // 解析可变头部和有效载荷
        if (messageType == MQTTMessageType.PUBLISH)
        {
            // 解析主题
            int topicLength = input.ReadUnsignedShort();
            if (input.ReadableBytes < topicLength)
            {
                input.ResetReaderIndex();
                return; // 需要更多数据
            }
            
            byte[] topicBytes = new byte[topicLength];
            input.ReadBytes(topicBytes);
            message.Topic = Encoding.UTF8.GetString(topicBytes);
            
            // QoS > 0 时解析消息ID
            if (qos > MQTTQualityOfServiceLevel.AtMostOnce)
            {
                if (input.ReadableBytes < 2)
                {
                    input.ResetReaderIndex();
                    return; // 需要更多数据
                }
                message.MessageId = input.ReadUnsignedShort();
            }
            
            // 解析有效载荷
            int payloadLength = remainingLength - (2 + topicLength + (qos > MQTTQualityOfServiceLevel.AtMostOnce ? 2 : 0));
            if (payloadLength > 0)
            {
                message.Payload = new byte[payloadLength];
                input.ReadBytes(message.Payload);
            }
        }
        // 其他消息类型的解析在此省略，实际实现需要根据不同消息类型处理
        
        output.Add(message);
    }
}