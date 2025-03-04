using EasyNetQ;

namespace AiUo.Extensions.RabbitMQ;

internal class MQPublishConfiguration : IPublishConfiguration
{
    public MQPublishConfiguration(string defaultTopic)
    {
        Topic = defaultTopic;
    }

    public IPublishConfiguration WithPriority(byte priority)
    {
        Priority = priority;
        return this;
    }

    public IPublishConfiguration WithTopic(string topic)
    {
        Topic = topic;
        return this;
    }

    public IPublishConfiguration WithExpires(TimeSpan expires)
    {
        Expires = expires;
        return this;
    }

    public IPublishConfiguration WithHeaders(IDictionary<string, object> headers)
    {
        foreach (var kvp in headers)
            (MessageHeaders ??= new Dictionary<string, object>()).Add(kvp.Key, kvp.Value);
        return this;
    }

    public IPublishConfiguration WithPublisherConfirms(bool publisherConfirms)
    {
        PublisherConfirms = publisherConfirms;
        return this;
    }

    public byte? Priority { get; private set; }
    public string Topic { get; private set; }
    public TimeSpan? Expires { get; private set; }
    public IDictionary<string, object> MessageHeaders { get; private set; }
    public bool PublisherConfirms { get; private set; }
}