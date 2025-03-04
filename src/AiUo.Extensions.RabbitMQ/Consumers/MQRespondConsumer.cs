using EasyNetQ;
using AiUo.Configuration;
using AiUo.Logging;

namespace AiUo.Extensions.RabbitMQ;

/// <summary>
/// MQ请求响应(Request => Respond)模式的Respond基类
/// 用于接收使用MQUtil.Request方法发出的MQ消息
/// 继承的子类名建议使用MQRsp结尾
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public abstract class MQRespondConsumer<TRequest, TResponse> : BaseMQConsumer
    where TRequest : new()
{
    private IDisposable _dispos;

    protected int MQMessageMode = 0;
    public MQRespondConsumer()
    {
        MQMessageMode = GetMQMessageMode(typeof(TRequest));
    }
    protected override string GetConnectionStringName()
    {
        return MQUtil.GetMessageAttribute<MQRequestMessageAttribute>(typeof(TRequest))
            ?.ConnectionStringName;
    }
    public override async Task Register()
    {
        Func<TRequest, CancellationToken, Task<MQResponseResult<TResponse>>> responder = async (request, cancellationToken) =>
        {
            var ret = new MQResponseResult<TResponse>();
            var begin = DateTime.UtcNow.ToTimestamp(true);
            var meta = GetMQMessageMeta(request, MQMessageMode);
            ret.MessageId = meta?.MessageId;
            ret.Timestamp = meta?.Timestamp ?? 0;
            try
            {
                ret.Result = await Respond(request, cancellationToken);
                ret.Success = true;
                ret.MessageElasped = GetElaspedTime(begin);
                if (DIUtil.GetService<RabbitMQSection>()?.MessageLogEnabled ?? false)
                {
                    GetLogger().AddMessage("[MQ] MQRespondConsumer消费成功。")
                        .AddField("MQMessageId", meta?.MessageId)
                        .AddField("MQRequestBody", SerializerUtil.SerializeJson(request))
                        .AddField("MQElaspedTime", ret.MessageElasped)
                        .Save();
                }
            }
            catch (Exception ex)
            {
                ret.Success = false;
                ret.MessageElasped = GetElaspedTime(begin);
                var exc = ExceptionUtil.GetException<CustomException>(ex);
                if (exc != null)
                {
                    ret.Code = exc.Code;
                    ret.Message = exc.Message;
                }
                else
                {
                    ret.Message = ex.Message;
                    ret.Exception = ex;
                    GetLogger().AddMessage("[MQ] MQRespondConsumer消费异常。")
                        .AddException(ex)
                        .AddField("MQMessageId", meta?.MessageId)
                        .AddField("MQRequestBody", SerializerUtil.SerializeJson(request))
                        .AddField("MQResponseBody", SerializerUtil.SerializeJson(ret))
                        .AddField("MQElaspedTime", ret.MessageElasped)
                        .Save();
                }
            }
            return ret;
        };
        Action<IResponderConfiguration> configAction = (config) =>
        {
            Configuration(config);
        };
        _dispos = await Bus.Rpc.RespondAsync(responder, configAction);
    }
    protected ILogBuilder GetLogger()
    {
        var logger = new LogBuilder()
            .AddField("MQConsumerType", GetType().FullName)
            .AddField("MQRequestType", typeof(TRequest).FullName)
            .AddField("MQResponseType", typeof(TResponse).FullName);
        return logger;
    }

    /// <summary>
    /// 配置设置（主要考虑设置QueueName）
    /// </summary>
    /// <param name="config"></param>
    protected abstract void Configuration(IResponderConfiguration config);
    protected abstract Task<TResponse> Respond(TRequest request, CancellationToken cancellationToken);
    public override void Dispose()
    {
        _dispos?.Dispose();
        base.Dispose();
    }
}