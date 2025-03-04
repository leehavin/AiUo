using EasyNetQ;
using System.Collections.Concurrent;
using AiUo.Configuration;

namespace AiUo.Extensions.RabbitMQ;

public abstract partial class MQSubscribeConsumer<TMessage>
{
    private List<MQSubHandler<TMessage>> HandlerList = new();
    private ConcurrentDictionary<string, MQSubHandler<TMessage>> HandlerDict = new();
    /// <summary>
    /// 0-没有MQMeta属性 1-IMQMessage 2- object MQMeta属性
    /// </summary>
    protected int MQMessageMode = 0;
    public MQSubscribeConsumer()
    {
        MQMessageMode = GetMQMessageMode(MQMessageType);
        AddHandler(OnMessage);
    }
    protected abstract Task OnMessage(TMessage message, CancellationToken cancellationToken);

    #region AddHandler
    protected void AddHandler(Func<TMessage, CancellationToken, Task> method, string desc = null)
    {
        var name = method?.Method?.Name;
        AddHandler(name, method, desc);
    }
    protected void AddHandler(IMQSubscribeHandler<TMessage> handler, string desc = null)
    {
        var name = handler.GetType().FullName;
        AddHandler(name, handler.OnMessage, desc);
    }
    protected void AddHandler<THandler>(string desc = null)
        where THandler : IMQSubscribeHandler<TMessage>, new()
    {
        var name = typeof(THandler).FullName;
        var handler = new THandler();
        AddHandler(name, handler.OnMessage, desc);
    }
    private void AddHandler(string name, Func<TMessage, CancellationToken, Task> method, string desc)
    {
        if (string.IsNullOrEmpty(name))
            throw new Exception($"MQSubscribeConsumer添加Handler时无法获取方法名: consumer:{GetType().FullName} message:{MQMessageType.FullName}");
        var item = new MQSubHandler<TMessage>
        {
            Name = name,
            Description = desc,
            Method = method,

        };
        if (!HandlerDict.TryAdd(name, item))
            throw new Exception($"MQSubscribeConsumer添加Handler时重复: method:{name} consumer:{GetType().FullName} message:{MQMessageType.FullName}");
        HandlerList.Add(item);
    }
    #endregion

    private async Task HandlerMessage(TMessage message, CancellationToken cancellationToken)
    {
        var meta = GetMQMessageMeta(message, MQMessageMode);
        var err = new MQSubError<TMessage>()
        {
            Message = message,
            Meta = meta,
            Errors = new()
        };
        List<MQSubHandler<TMessage>> list = null;
        if (meta?.ErrorHandlers?.Count > 0)
        {
            err.IsRepublic = true;
            list = new();
            meta.RepublishCount++;
            meta.ErrorHandlers.ForEach(name =>
            {
                if (HandlerDict.TryGetValue(name, out var item))
                    list.Add(item);
            });
            meta.ErrorHandlers.Clear();
        }
        else
            list = HandlerList;
        foreach (var item in list)
        {
            var begin = DateTime.UtcNow.ToTimestamp(true);
            try
            {
                await item.Method(message, cancellationToken);
                if (DIUtil.GetService<RabbitMQSection>()?.MessageLogEnabled ?? false)
                    SaveLogger(message, meta, item, begin, null);
            }
            catch (Exception ex)
            {
                SaveLogger(message, meta, item, begin, ex);
                meta?.ErrorHandlers?.Add(item.Name);
                err.Errors.Add((item, ex));
            }
        }
        if (err.Errors.Count > 0)
        {
            SetMQMessageMeta(message, meta, MQMessageMode);
            try
            {
                await OnError(err);
            }
            catch (Exception ex)
            {
                GetLogger()
                    .AddMessage("[MQ] MQSubscribeConsumer.OnError()时出现异常")
                    .AddField("MQMeta", meta)
                    .AddField("MQMessageBody", message)
                    .AddException(ex)
                    .Save();
            }
            // 不要catch，此异常将导致被发送到默认错误代理队列 error queue (broker)
            throw new EasyNetQException($"MQSubscribeConsumer消费异常。ConsumerType:{GetType().FullName} MessageId:{meta?.MessageId}");
        }
    }
    protected abstract Task OnError(MQSubError<TMessage> error);
    private void SaveLogger(TMessage msg, MQMessageMeta meta, MQSubHandler<TMessage> item, long begin, Exception ex)
    {
        GetLogger().AddMessage($"[MQ] MQSubscribeConsumer消费{(ex == null ? "成功" : "失败")}。")
            .AddField("MQMessageId", meta?.MessageId)
            .AddField("MQMessageBody", SerializerUtil.SerializeJson(msg))
            .AddField("MQElaspedTime", GetElaspedTime(begin))
            .AddField("MQMeta", meta != null ? SerializerUtil.SerializeJson(meta) : null)
            .AddField("MQActionName", item.Name)
            .AddField("MQActionDesc", item.Description)
            .AddException(ex)
            .Save();
    }
}
public class MQSubHandler<TMessage>
    where TMessage : class, new()
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Func<TMessage, CancellationToken, Task> Method { get; set; }
}

public class MQSubError<TMessage>
    where TMessage : class, new()
{
    public bool IsRepublic { get; set; }
    public TMessage Message { get; set; }
    public MQMessageMeta Meta { get; set; }
    public List<(MQSubHandler<TMessage> Handler, Exception Exception)> Errors { get; set; }
}