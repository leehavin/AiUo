using Newtonsoft.Json;
using System.Text;
using AiUo.Serialization;

namespace AiUo.Extensions.StackExchangeRedis.Serializers;

internal class RedisJsonSerializer : ISerializer
{
    public JsonSerializerSettings JsonSettings { get; }
    public RedisJsonSerializer()
    {
        // 不能使用System.Text.Json
        JsonSettings = SerializerUtil.GetJsonNetSettings();
    }
    public byte[] Serialize(object value)
    {
        var json = SerializerUtil.SerializeJsonNet(value, JsonSettings);
        return Encoding.UTF8.GetBytes(json);
    }
    public object Deserialize(byte[] utf8Bytes, Type returnType)
    {
        var json = Encoding.UTF8.GetString(utf8Bytes);
        return SerializerUtil.DeserializeJsonNet(json, returnType, JsonSettings);
    }

    public T Deserialize<T>(byte[] utf8Bytes)
    {
        return (T)Deserialize(utf8Bytes, typeof(T));
    }

}