using AiUo.Configuration;
using AiUo.Logging;
using EasyNetQ;
using System.Reflection;

namespace AiUo.Extensions.RabbitMQ;

/// <summary>
/// MQ消息订阅(Publish => Subscribe)模式的Subscribe基类
/// 用于接收使用MQUtil.Publish方法发布的MQ消息
/// 继承的子类名建议使用MQSub结尾
/// </summary>
/// <typeparam name="TMessage">接收的消息类型</typeparam>
public abstract partial class MQSubscribeConsumer<TMessage> : BaseMQConsumer
    where TMessage : class, new()
{
    #region Properties
    /// <summary>
    /// 订阅模式
    /// </summary>
    public abstract MQSubscribeMode SubscribeMode { get; }
    public Type MQMessageType => typeof(TMessage);

    /// <summary>
    /// 是否启用高可用(仅MQ群集使用)
    /// </summary>
    protected virtual bool UseQuorum { get; }

    protected SubscriptionResult? _subResult;
    protected override string GetConnectionStringName()
    {
        return MQUtil.GetMessageAttribute<MQPublishMessageAttribute>(typeof(TMessage))
            ?.ConnectionStringName;
    }
    #endregion

    public override async Task Register()
    {
        if (GetType().GetCustomAttribute<MQConsumerIgnoreAttribute>() == null)
            LogUtil.Debug($"注册 => MQSubscribeConsumer: {GetType().FullName}");
        var subId = GetSubscriptionId();
        var configure = GetConfigure();
        Func<TMessage, CancellationToken, Task> onMessage = (message, token) =>
        {
            return HandlerMessage(message, token);
        };

        if (SubscribeMode == MQSubscribeMode.Broadcast)
        {
            _subResult = await Bus.PubSub.SubscribeAsync(subId, onMessage, configure);
            Bus.Advanced.Connected += (_, e) =>
            {
                _subResult = Bus.PubSub.Subscribe(subId, onMessage, configure);
            };
        }
        else
        {
            _subResult = await Bus.PubSub.SubscribeAsync(subId, onMessage, configure);
        }
    }

    protected string GetSubscriptionId()
    {
        switch (SubscribeMode)
        {
            case MQSubscribeMode.OneQueue:
                return $"{GetType().FullName}";
            case MQSubscribeMode.Broadcast:
                return $"{GetType().FullName}-{ConfigUtil.Service.SID.Replace('.', '_').Replace('-', '_')}";
            case MQSubscribeMode.SAC:
                return $"{GetType().FullName}";
        }
        throw new NotImplementedException();
    }
    private Action<ISubscriptionConfiguration> GetConfigure()
    {
        return (config) =>
        {
            switch (SubscribeMode)
            {
                case MQSubscribeMode.OneQueue:
                    break;
                case MQSubscribeMode.Broadcast:
                    config.WithDurable(false); //非持久Queue
                    config.WithAutoDelete(true);
                    break;
                case MQSubscribeMode.SAC:
                    config.WithSingleActiveConsumer();//单一消费者
                    break;
            }
            if (UseQuorum && MQUseQuorum)
            {
                if (SubscribeMode == MQSubscribeMode.Broadcast)
                    throw new Exception($"MQSubscribeConsumer的Broadcast时无法启用Quorum.type:{GetType().FullName}");
                config.WithQueueType(QueueType.Quorum);
            }
            Configuration(config);
        };
    }
    private ILogBuilder GetLogger()
    {
        var logger = new LogBuilder()
            .AddField("MQConsumerType", GetType().FullName)
            .AddField("MQMessageType", MQMessageType.FullName)
            .AddField("MQSubscribeMode", SubscribeMode)
            .AddField("MQExchange", _subResult?.Exchange.Name)
            .AddField("MQQueue", _subResult?.Queue.Name);
        return logger;
    }
    /// <summary>
    /// 配置设置（topic等）
    /// </summary>
    /// <param name="config"></param>
    protected abstract void Configuration(ISubscriptionConfiguration config);

    public override void Dispose()
    {
        _subResult?.Dispose();
        base.Dispose();
    }
}