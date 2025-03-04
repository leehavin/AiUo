using AiUo.BIZ.RabbitMQ.DAL;
using AiUo.Data.SqlSugar;
using AiUo.Extensions.RabbitMQ;
using AiUo.Logging;
using EasyNetQ.Topology;
using System.Text;

namespace AiUo.BIZ.RabbitMQ
{
    public static class MQBizUtil
    {
        public static async Task RepublishAsync(List<S_mq_sub_errorPO> items = null)
        {
            items ??= await DbUtil.SelectAsync<S_mq_sub_errorPO>(it => it.Status == 0 && it.MessageMode != 0);
            items.ForEach(async item => await RepublishAsync(item));
        }
        /// <summary>
        /// 重新发送
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="Exception"></exception>
        public static async Task RepublishAsync(S_mq_sub_errorPO item)
        {
            if (item.MessageMode != 1 && item.MessageMode != 2)
                throw new Exception("MQBizUtil.RepublishAsync()MQ消息重新发布时，不支持没有MQMeta属性的消息类型");

            var meta = SerializerUtil.DeserializeJson<MQMessageMeta>(item.MessageMeta);
            if (meta.Topic == null)
                throw new Exception("MQBizUtil.RepublishAsync()MQ消息重新发布时，Topic不能为null");
            if (meta.Properties == null)
                throw new Exception("MQBizUtil.RepublishAsync()MQ消息重新发布时，Properties不能为null");

            var body = Encoding.UTF8.GetBytes(meta.MessageBody);
            var bus = MQUtil.GetBus(item.MQConnection).Advanced;
            var exchange = await bus.ExchangeDeclareAsync(item.MQExchange, config =>
            {
                config.WithType(ExchangeType.Topic);
            });
            try
            {
                await bus.PublishAsync(exchange
                    , meta.Topic
                    , meta.Mandatory
                    , meta.Properties
                    , new ReadOnlyMemory<byte>(body));
                await DbUtil.UpdateAsync<S_mq_sub_errorPO>(it => new S_mq_sub_errorPO
                {
                    Status = 1,
                    RepublishDate = DateTime.UtcNow
                }, it => it.MQLogID == item.MQLogID);
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex, "重新发送MQ消息失败");
                throw;
            }
        }
        /*
        public SerializedMessage SerializeMessage(IMessage message)
        {
            var typeName = typeNameSerializer.Serialize(message.MessageType);
            var messageBody = serializer.MessageToBytes(message.MessageType, message.GetBody());
            var messageProperties = message.Properties;

            messageProperties.Type = typeName;
            if (string.IsNullOrEmpty(messageProperties.CorrelationId))
                messageProperties.CorrelationId = correlationIdGenerator.GetCorrelationId();

            return new SerializedMessage(messageProperties, messageBody);
        }
        */

        /*
        private static ConcurrentDictionary<Type, MethodInfo> _publishMethodDict = new();
         /// <summary>
         /// 重新发送
         /// </summary>
         /// <param name="item"></param>
         /// <param name="configAction"></param>
         /// <exception cref="Exception"></exception>
         public static async Task RepublishAsync(S_mq_sub_errorPO item, Action<IPublishConfiguration> configAction = null)
         {
             if (item.MessageMode == 0)
                 throw new Exception("MQ消息重新发布时，不支持没有MQMeta属性的消息类型");
             var tvs = item.MessageType.Split(',', StringSplitOptions.TrimEntries);
             var asm = Assembly.LoadFrom(tvs[1]);
             var msgType = asm.GetType(tvs[0]);
             var msgObj = SerializerUtil.DeserializeJson(item.MessageData, msgType);
             if (!_publishMethodDict.TryGetValue(msgType, out var method))
             {
                 method = GetGenericMethod(typeof(PubSubExtensions), "Publish", BindingFlags.Static | BindingFlags.Public
                     , new Type[] { typeof(IPubSub), msgType, typeof(Action<IPublishConfiguration>), typeof(CancellationToken) }
                     , msgType);
                 _publishMethodDict.TryAdd(msgType, method);
             }
             Action<IPublishConfiguration> action = (config) =>
             {
                 configAction?.Invoke(config);
                 if (!string.IsNullOrEmpty(item.MessageTopic))
                     config.WithTopic(item.MessageTopic);
             };
             try
             {
                 var pub = MQUtil.GetBus(item.MQConnName).PubSub;
                 method.Invoke(null, new object[] { pub, msgObj, action, CancellationToken.None });
                 await DbUtil.UpdateAsync<S_mq_sub_errorPO>(it => new S_mq_sub_errorPO
                 {
                     Status = 1,
                     RepublishDate = DateTime.UtcNow
                 }, it => it.MQLogID == item.MQLogID);
             }
             catch (Exception ex)
             {
                 LogUtil.Error(ex, "重新发送MQ消息失败");
                 throw;
             }
         }
         private static MethodInfo GetGenericMethod(Type targetType, string name, BindingFlags flags, Type[] parameterTypes, params Type[] typeArguments)
         {
             var methods = targetType.GetMethods(flags).Where(m => m.Name == name && m.IsGenericMethod);
             foreach (MethodInfo method in methods)
             {
                 var parameters = method.GetParameters();
                 if (parameters.Length != parameterTypes.Length)
                     continue;

                 for (var i = 0; i < parameters.Length; i++)
                 {
                     if (parameters[i].ParameterType != parameterTypes[i])
                         break;
                 }
                 return method.MakeGenericMethod(typeArguments);
             }
             return null;
         }
         */
    }
}
