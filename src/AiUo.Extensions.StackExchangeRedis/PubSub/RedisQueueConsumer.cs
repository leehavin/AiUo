using StackExchange.Redis;
using System.Reflection;
using AiUo.Logging;
using static StackExchange.Redis.RedisChannel;

namespace AiUo.Extensions.StackExchangeRedis;

public interface IRedisConsumer
{
    void Register();
}
/// <summary>
/// redis队列消费基类(队列消息将被阻塞且单一执行)
/// </summary>
/// <typeparam name="TMessage"></typeparam>
public abstract class RedisQueueConsumer<TMessage> : IRedisConsumer, IDisposable
    where TMessage : class
{
    private ConnectionMultiplexer _redis;
    private IDatabase _database;
    private ISubscriber _sub;
    private RedisChannel _channel;
    private ChannelMessageQueue _queue;
    private string _queueKey;

    public virtual string ConnectionStringName { get; set; }
    /// <summary>
    /// 模式匹配，如: *
    /// </summary>
    public virtual string PatternKey { get; set; }
    public virtual PatternMode? PatternMode { get; set; }
    /// <summary>
    /// 消息是否并发处理
    /// </summary>
    public virtual bool IsConcurrentProcess { get; set; } = true;

    public RedisQueueConsumer()
    {
        var attr = typeof(TMessage).GetCustomAttribute<RedisPublishMessageAttribute>();
        if (string.IsNullOrEmpty(ConnectionStringName))
            ConnectionStringName = attr?.ConnectionStringName;
        if (!PatternMode.HasValue)
            PatternMode = attr?.PatternMode ?? RedisChannel.PatternMode.Auto;

    }
    public void Register()
    {
        _redis = RedisUtil.GetRedis(ConnectionStringName, "PUBSUB");
        //_redis = RedisUtil.GetRedis(ConnectionStringName);
        _database = _redis.GetDatabase();
        _sub = _redis.GetSubscriber();
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
        _queueKey = RedisUtil.GetQueueKey<TMessage>();
    }
    private async Task ExecMessage(RedisValue channelMessage)
    {
        var value = await _database.ListRightPopAsync(_queueKey);
        if (value.IsNullOrEmpty)
            return;
        var msg = RedisUtil.GetSerializer(RedisSerializeMode.Json)
            .Deserialize<TMessage>(value);
        try
        {
            await OnMessage(msg);
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "RedisQueueConsumer.OnMessage异常。type:{0}", this.GetType().FullName);
            await OnError(msg, ex);
        }
    }

    protected abstract Task OnMessage(TMessage message);
    protected abstract Task OnError(TMessage message, Exception ex);

    public void Dispose()
    {
    }
}