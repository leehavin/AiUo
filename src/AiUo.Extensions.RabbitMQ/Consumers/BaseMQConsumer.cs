using AiUo.Reflection;
using EasyNetQ;

namespace AiUo.Extensions.RabbitMQ;

public abstract class BaseMQConsumer : IMQConsumer
{
    protected string MQConnectionStringName { get; }
    protected string MQConnectionString { get; }
    protected bool MQUseQuorum { get; }
    protected IBus Bus { get; }

    public BaseMQConsumer()
    {
        MQConnectionStringName = GetConnectionStringName();
        var conn = MQUtil.GetConnectionStringElement(MQConnectionStringName);
        MQConnectionString = conn.ConnectionString;
        MQUseQuorum = conn.UseQuorum;

        Bus = DIUtil.GetRequiredService<MQContainer>()
            .GetSubBus(MQConnectionStringName);
    }

    /// <summary>
    /// MQ链接字符串,不使用默认则子类需重写
    /// </summary>
    /// <returns></returns>
    protected virtual string GetConnectionStringName() { return null; }
    public abstract Task Register();

    public virtual void Dispose()
    {
        Bus?.Dispose();
    }

    protected int GetMQMessageMode(Type type)
    {
        var ret = 0;
        if (typeof(IMQMessage).IsAssignableFrom(type))
        {
            ret = 1;
        }
        else
        {
            var prop = type.GetProperty("MQMeta");
            if (prop != null && (prop.PropertyType == typeof(object) || prop.PropertyType == typeof(MQMessageMeta)))
            {
                ret = 2;
            }
        }
        return ret;
    }
    protected MQMessageMeta GetMQMessageMeta<TMessage>(TMessage msg, int mode)
    {
        MQMessageMeta ret = null;
        switch (mode)
        {
            //case 0:
            //    var now = DateTime.UtcNow;
            //    ret = new MQMessageMeta
            //    {
            //        MessageId = ObjectId.NewId(now),
            //        Timestamp = now.ToTimestamp()
            //    };
            //    break;
            case 1:
                ret = (msg as IMQMessage)?.MQMeta;
                break;
            case 2:
                var json = Convert.ToString(ReflectionUtil.GetPropertyValue<object>(msg, "MQMeta"));
                ret = SerializerUtil.DeserializeJson<MQMessageMeta>(json);
                break;
        }
        return ret;
    }
    protected void SetMQMessageMeta<TMessage>(TMessage msg, MQMessageMeta meta, int mode)
    {
        switch (mode)
        {
            //case 1:
            //    (msg as IMQMessage).MQMeta = meta;
            //    break;
            case 2:
                ReflectionUtil.SetPropertyValue(msg, "MQMeta", meta);
                break;
        }
    }
    protected void ClearMQMessageMeta<TMessage>(TMessage msg, int mode)
    {
        switch (mode)
        {
            case 1:
                (msg as IMQMessage).MQMeta = null;
                break;
            case 2:
                ReflectionUtil.SetPropertyValue(msg, "MQMeta", null);
                break;
        }
    }

    protected long GetElaspedTime(long? beginTimestamp)
        => beginTimestamp.HasValue ? DateTime.UtcNow.ToTimestamp(true) - beginTimestamp.Value : 0;
}
public interface IMQConsumer : IDisposable
{
    Task Register();
}