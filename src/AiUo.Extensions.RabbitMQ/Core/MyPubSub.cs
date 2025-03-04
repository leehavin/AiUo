using AiUo.Reflection;
using AiUo.Text;
using EasyNetQ;
using EasyNetQ.Internals;
using EasyNetQ.Producer;
using EasyNetQ.Topology;

namespace AiUo.Extensions.RabbitMQ;

public class MyPubSub : DefaultPubSub
{
    #region Constructors
    private readonly IAdvancedBus advancedBus;
    private readonly ConnectionConfiguration configuration;
    private readonly IConventions conventions;
    private readonly IMessageDeliveryModeStrategy messageDeliveryModeStrategy;
    private readonly IExchangeDeclareStrategy exchangeDeclareStrategy;
    private readonly IMessageSerializationStrategy messageSerializationStrategy;
    public readonly ITypeNameSerializer typeNameSerializer;
    public readonly ISerializer serializer;
    public readonly ICorrelationIdGenerationStrategy correlationIdGenerator;

    public MyPubSub(ConnectionConfiguration configuration, IConventions conventions, IExchangeDeclareStrategy exchangeDeclareStrategy, IMessageDeliveryModeStrategy messageDeliveryModeStrategy, IAdvancedBus advancedBus, IMessageSerializationStrategy messageSerializationStrategy, ITypeNameSerializer typeNameSerializer, ISerializer serializer, ICorrelationIdGenerationStrategy correlationIdGenerator)
        : base(configuration, conventions, exchangeDeclareStrategy, messageDeliveryModeStrategy, advancedBus)
    {
        this.configuration = configuration;
        this.conventions = conventions;
        this.exchangeDeclareStrategy = exchangeDeclareStrategy;
        this.messageDeliveryModeStrategy = messageDeliveryModeStrategy;
        this.advancedBus = advancedBus;
        this.messageSerializationStrategy = messageSerializationStrategy;
        this.typeNameSerializer = typeNameSerializer;
        this.serializer = serializer;
        this.correlationIdGenerator = correlationIdGenerator;
    }

    #endregion
    public override async Task PublishAsync<T>(T message, Action<IPublishConfiguration> configure, CancellationToken cancellationToken)
    {
        using var cts = cancellationToken.WithTimeout(configuration.Timeout);

        var publishConfiguration = new PublishConfiguration(conventions.TopicNamingConvention(typeof(T)));
        configure(publishConfiguration);

        var messageType = typeof(T);
        var properties = new EasyNetQ.MessageProperties();
        if (publishConfiguration.Priority != null)
            properties.Priority = publishConfiguration.Priority.Value;
        if (publishConfiguration.Expires != null)
            properties.Expiration = publishConfiguration.Expires.Value;
        if (publishConfiguration.Headers?.Count > 0)
            properties.Headers.UnionWith(publishConfiguration.Headers);
        properties.DeliveryMode = messageDeliveryModeStrategy.GetDeliveryMode(messageType);

        var exchange = await exchangeDeclareStrategy.DeclareExchangeAsync(
            messageType, ExchangeType.Topic, cts.Token
        ).ConfigureAwait(false);
        //
        messageType = message != null ? message.GetType() : typeof(T);
        properties.Type = typeNameSerializer.Serialize(messageType);
        if (string.IsNullOrEmpty(properties.CorrelationId))
            properties.CorrelationId = correlationIdGenerator.GetCorrelationId();

        // 设置MessageMeta
        SetMessageMeta(message, publishConfiguration, properties);
        var messageBody = serializer.MessageToBytes(messageType, message);
        using var serializedMessage = new SerializedMessage(properties, messageBody);
        //var body = Encoding.UTF8.GetString(serializedMessage.Body.ToArray());
        await advancedBus.PublishAsync(exchange
            , publishConfiguration.Topic
            , configuration.MandatoryPublish
            , serializedMessage.Properties
            , serializedMessage.Body
            , cancellationToken
        ).ConfigureAwait(false);
    }
    private MQMessageMeta SetMessageMeta<TMessage>(TMessage message, PublishConfiguration config, EasyNetQ.MessageProperties properties)
    {
        MQMessageMeta ret = null;
        if (message is IMQMessage msg)
        {
            if (msg.MQMeta != null)
                throw new Exception("MQUtil.Publish时,Message属性MQMeta必须为null");
            ret = msg.MQMeta = CreateMeta(config, properties);
        }
        else
        {
            var prop = message.GetType().GetProperty("MQMeta");
            if (prop != null && (prop.PropertyType == typeof(object) || prop.PropertyType == typeof(MQMessageMeta)))
            {
                if (ReflectionUtil.GetPropertyValue(message, prop) != null)
                    throw new Exception($"MQUtil.Publish时,Message的MQMeta属性必须为null");
                ret = CreateMeta(config, properties);
                ReflectionUtil.SetPropertyValue(message, prop, ret);
            }
        }
        return ret;
    }
    private MQMessageMeta CreateMeta(PublishConfiguration config, EasyNetQ.MessageProperties properties)
    {
        var now = DateTime.UtcNow;
        return new MQMessageMeta
        {
            MessageId = ObjectId.NewId(now),
            Timestamp = now.ToTimestamp(),
            Topic = config.Topic,
            Mandatory = configuration.MandatoryPublish,
            Properties = properties,
        };
    }
}
internal class PublishConfiguration : IPublishConfiguration
{
    public PublishConfiguration(string defaultTopic)
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
        Headers = headers;
        return this;
    }

    public byte? Priority { get; private set; }
    public string Topic { get; private set; }
    public TimeSpan? Expires { get; private set; }
    public IDictionary<string, object> Headers { get; private set; }
}