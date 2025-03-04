using StackExchange.Redis;
using System.Reflection;
using AiUo.Logging;
using static StackExchange.Redis.RedisChannel;

namespace AiUo.Extensions.StackExchangeRedis;

/// <summary>
/// redis发布订阅的消费基类(队列消息将多播发送)
/// </summary>
/// <typeparam name="TMessage"></typeparam>
public abstract class RedisSubscribeConsumer<TMessage> : IRedisConsumer, IDisposable
    where TMessage : class
{
    private ISubscriber _sub;
    private RedisChannel _channel;
    private ChannelMessageQueue _queue;

    public virtual string ConnectionStringName { get; set; }
    /// <summary>
    /// 模式匹配，如: *
    /// </summary>
    public virtual string PatternKey { get; set; }
    public virtual PatternMode? PatternMode { get; set; }
    /// <summary>
    /// 消息是并发处理
    /// </summary>
    public virtual bool IsConcurrentProcess { get; set; } = true;

    public RedisSubscribeConsumer()
    {
        var attr = typeof(TMessage).GetCustomAttribute<RedisPublishMessageAttribute>();
        if (string.IsNullOrEmpty(ConnectionStringName))
            ConnectionStringName = attr?.ConnectionStringName;
        if (!PatternMode.HasValue)
            PatternMode = attr?.PatternMode ?? RedisChannel.PatternMode.Auto;

    }
    public void Register()
    {
        if (GetType().GetCustomAttribute<RedisConsumerRegisterIgnoreAttribute>() == null)
            LogUtil.Info($"注册 => RedisSubscribeConsumer: {GetType().FullName}");
        _sub = RedisUtil.GetRedis(ConnectionStringName, "PUBSUB").GetSubscriber();
        //_sub = RedisUtil.GetRedis(ConnectionStringName).GetSubscriber();
        _channel = RedisUtil.GetChannel<TMessage>(PatternKey, PatternMode.Value);
        if (IsConcurrentProcess)
        {
            _sub.Subscribe(_channel, async (c, m) =>
            {
                await ExecMessage(m);
            });
        }
        else
        {
            _queue = _sub.Subscribe(_channel);
            _queue.OnMessage((m) => ExecMessage(m.Message));
        }
    }
    private async Task ExecMessage(RedisValue channelMessage)
    {
        var msg = RedisUtil.GetSerializer(RedisSerializeMode.Json)
            .Deserialize<TMessage>(channelMessage);
        try
        {
            await OnMessage(msg);
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, $"RedisSubscribeConsumer.OnMessage异常。type:{this.GetType().FullName}");
            await OnError(msg, ex);
        }
    }

    protected abstract Task OnMessage(TMessage message);
    protected abstract Task OnError(TMessage message, Exception ex);

    public void Dispose()
    {

    }
}