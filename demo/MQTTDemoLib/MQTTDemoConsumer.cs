using AiUo.Extensions.MQTT;
using AiUo.Logging;
using System.Threading.Tasks;

namespace MQTTDemoLib;

/// <summary>
/// MQTT消息订阅消费者示例
/// </summary>
public class MQTTDemoConsumer : MQTTSubscribeConsumer<MQTTSubscribeMsg>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public MQTTDemoConsumer() : base("mqtt/demo/#")
    {
    }

    protected override Task ProcessMessageAsync(MQTTSubscribeMsg message)
    {

        LogUtil.Info("收到MQTT消息: {Content}, 值: {Value}, 主题: {Topic}",
            message.Content,
            message.Value,
            message.MQTTMeta.Topic);

        return Task.FromResult(true);
    }
}