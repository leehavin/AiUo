using EasyNetQ;

namespace AiUo.Extensions.RabbitMQ;

/// <summary>
/// 发送接收(Send=>Receive)模式的Receive基类(命令管道，可处理多个不同消息类型)
/// 用于接收使用MQUtil.Send方法发出的MQ消息
/// 继承的子类名建议使用MQRcv结尾
/// </summary>
public abstract class MQReceiveConsumer : BaseMQConsumer
{
    private IDisposable _dispos;
    /// <summary>
    /// QueueName
    /// </summary>
    public abstract string QueueName { get; }
    /// <summary>
    /// 是否启用高可用(仅MQ群集使用)
    /// </summary>
    protected virtual bool UseQuorum { get; set; }
    protected override string GetConnectionStringName()
    {
        return null;
    }
    public override async Task Register()
    {
        Action<IReceiveConfiguration> configAction = (config) =>
        {
            if (UseQuorum && MQUseQuorum)
                config.WithQueueType(QueueType.Quorum);
            Configuration(config);
        };
        _dispos = await Bus.SendReceive.ReceiveAsync(QueueName, AddHandlers, configAction);
    }

    /// <summary>
    /// 添加多个异步消息不同的消息处理程序：reg.Add...
    /// </summary>
    /// <param name="reg"></param>
    protected abstract void AddHandlers(IReceiveRegistration reg);
    protected abstract void Configuration(IReceiveConfiguration config);

    public override void Dispose()
    {
        _dispos?.Dispose();
        base.Dispose();
    }
}