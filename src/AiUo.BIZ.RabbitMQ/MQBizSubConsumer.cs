using AiUo.BIZ.RabbitMQ.DAL;
using AiUo.Configuration;
using AiUo.Data.SqlSugar;
using AiUo.Extensions.RabbitMQ;
using AiUo.Text;
using System.Text;
using System.Text.Json;

namespace AiUo.BIZ.RabbitMQ
{
    public abstract class MQBizSubConsumer<TMessage> : MQSubscribeConsumer<TMessage>
        where TMessage : class, new()
    {
        private JsonSerializerOptions _jsonOptions;
        public MQBizSubConsumer() 
        {
            _jsonOptions = SerializerUtil.GetJsonOptions();
            _jsonOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        }

        protected override async Task OnError(MQSubError<TMessage> error)
        {
            // 广播不重试
            if (SubscribeMode == MQSubscribeMode.Broadcast)
                return;

            var handlers = new List<string>();
            var excs = new StringBuilder();
            foreach (var item in error.Errors)
            {
                handlers.Add(item.Handler.Name);
                excs.AppendLine($"===> {item.Handler.Name} [{item.Handler.Description}]");
                excs.AppendLine(SerializerUtil.SerializeJsonNet(item.Exception));
                excs.AppendLine();
            }

            if (error.Meta != null)
            {
                var buffer = Bus.Advanced.Container.Resolve<EasyNetQ.ISerializer>()
                    .MessageToBytes(MQMessageType, error.Message);
                error.Meta.MessageBody = Encoding.UTF8.GetString(buffer.Memory.ToArray());
            }
            ClearMQMessageMeta(error.Message, MQMessageMode);
            var eo = new S_mq_sub_errorPO
            {
                MQLogID = ObjectId.NewId(),
                MessageType = $"{MQMessageType.FullName}, {MQMessageType.Assembly.ManifestModule.Name}",
                MessageMode = MQMessageMode,
                ProjectId = ConfigUtil.Project.ProjectId,
                ConsumerType = $"{GetType().FullName}, {GetType().Assembly.ManifestModule.Name}",
                SubscribeMode = SubscribeMode.ToString(),
                MessageId = error.Meta?.MessageId,
                MessageTime = error.Meta?.Timestamp > 0
                    ? DateTimeUtil.ParseTimestamp(error.Meta.Timestamp)
                    : null,
                MessageTopic = error.Meta?.Topic,
                MessageData = SerializerUtil.SerializeJson(error.Message, _jsonOptions),
                MessageMeta = SerializerUtil.SerializeJson(error.Meta),
                MQConnection = MQConnectionStringName,
                MQExchange = _subResult?.Exchange.Name,
                ErrorHandlers = string.Join('|', handlers),
                Exception = excs.ToString(),
                RepublishCount = error.Meta?.RepublishCount ?? 0,
                Status = 0,
                RecDate = DateTime.UtcNow,
            };
            await DbUtil.InsertAsync(eo);
        }
    }
}
