using AiUo.Configuration;
using AiUo.Security;
using EasyNetQ;
using EasyNetQ.Internals;
using System.Collections.Concurrent;

namespace AiUo.Extensions.RabbitMQ;

internal class MQConventions : Conventions
{
    private ConcurrentDictionary<Type, string> _exchangeDict = new();
    private ConcurrentDictionary<string, string> _queueDict = new();
    public MQConventions(ITypeNameSerializer typeNameSerializer, bool useShortNaming)
        : base(typeNameSerializer)
    {
        ExchangeNamingConvention = (msgType) =>
        {
            if (_exchangeDict.TryGetValue(msgType, out var ret))
                return ret;
            ret = typeNameSerializer.Serialize(msgType);
            if (useShortNaming)
            {
                ret = $"{GetMessageName(msgType)}=>[{GetHash(ret)}]";
            }
            _exchangeDict.TryAdd(msgType, ret);
            return ret;
        };
        QueueNamingConvention = (msgType, subId) =>
        {
            var key = GetDefaultQueueName(typeNameSerializer, msgType, subId);
            if (_queueDict.TryGetValue(key, out var ret))
                return ret;
            if (useShortNaming)
            {
                // 获取consumerName
                var idx = subId.LastIndexOf('.');
                var consumerName = idx >= 0
                    ? subId.Substring(idx + 1)
                    : subId;

                var projectId = ConfigUtil.Project.ProjectId;
                ret = $"{GetMessageName(msgType)}=>[{projectId}]{consumerName}=>[{GetHash(key)}]";
            }
            else
                ret = key;
            _queueDict.TryAdd(key, ret);
            return ret;
        };
    }
    private string GetMessageName(Type type)
    {
        var dll = type.AssemblyQualifiedName.Split(',')[1].Trim();
        return $"[{dll}]{type.Name}";
    }
    private string GetHash(string src)
    {
        var hash = MurmurHash3.Hash32(src);
        return NumberSystemUtil.DecToBase(hash, 32);
    }
    private string GetDefaultQueueName(ITypeNameSerializer typeNameSerializer, Type msgType, string subId)
    {
        var attr = msgType.GetAttribute<QueueAttribute>();
        if (!string.IsNullOrEmpty(attr?.QueueName))
        {
            return string.IsNullOrEmpty(subId)
                ? attr.QueueName
                : $"{attr.QueueName}_{subId}";
        }

        var typeName = typeNameSerializer.Serialize(msgType);
        return string.IsNullOrEmpty(subId)
            ? typeName
            : $"{typeName}_{subId}";
    }
}