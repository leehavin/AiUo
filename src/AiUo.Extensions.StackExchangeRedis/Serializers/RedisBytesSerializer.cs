using System.Text;
using AiUo.Serialization;

namespace AiUo.Extensions.StackExchangeRedis.Serializers;

/// <summary>
/// 简单类型用bytes，复杂类型用json
/// </summary>
public class RedisBytesSerializer : ISerializer
{
    public byte[] Serialize(object value)
    {
        var type = value.GetType();
        switch (type.FullName)
        {
            case SimpleTypeNames.Boolean:
                return BitConverter.GetBytes((bool)value);
            case SimpleTypeNames.Char:
                return BitConverter.GetBytes((char)value);
            case SimpleTypeNames.Double:
                return BitConverter.GetBytes((double)value);
            case SimpleTypeNames.Int16:
                return BitConverter.GetBytes((short)value);
            case SimpleTypeNames.Int32:
                return BitConverter.GetBytes((int)value);
            case SimpleTypeNames.Int64:
                return BitConverter.GetBytes((long)value);
            case SimpleTypeNames.Single:
                return BitConverter.GetBytes((float)value);
            case SimpleTypeNames.UInt16:
                return BitConverter.GetBytes((ushort)value);
            case SimpleTypeNames.UInt32:
                return BitConverter.GetBytes((uint)value);
            case SimpleTypeNames.UInt64:
                return BitConverter.GetBytes((ulong)value);
            case SimpleTypeNames.String:
                return Encoding.UTF8.GetBytes((string)value);
            case SimpleTypeNames.Guid:
                return Encoding.UTF8.GetBytes(((Guid)value).ToString());
            case SimpleTypeNames.Bytes:
                return (byte[])value;
        }
        var json = Encoding.UTF8.GetBytes(SerializerUtil.SerializeJson(value));
        return json;
    }
    public object Deserialize(byte[] utf8Bytes, Type returnType)
    {
        switch (returnType.FullName)
        {
            case SimpleTypeNames.Boolean:
                return BitConverter.ToBoolean(utf8Bytes, 0);
            case SimpleTypeNames.Char:
                return BitConverter.ToChar(utf8Bytes, 0);
            case SimpleTypeNames.Double:
                return BitConverter.ToDouble(utf8Bytes, 0);
            case SimpleTypeNames.Int16:
                return BitConverter.ToInt16(utf8Bytes, 0);
            case SimpleTypeNames.Int32:
                return BitConverter.ToInt32(utf8Bytes, 0);
            case SimpleTypeNames.Int64:
                return BitConverter.ToInt64(utf8Bytes, 0);
            case SimpleTypeNames.Single:
                return BitConverter.ToSingle(utf8Bytes, 0);
            case SimpleTypeNames.UInt16:
                return BitConverter.ToUInt16(utf8Bytes, 0);
            case SimpleTypeNames.UInt32:
                return BitConverter.ToUInt32(utf8Bytes, 0);
            case SimpleTypeNames.UInt64:
                return BitConverter.ToUInt64(utf8Bytes, 0);
            case SimpleTypeNames.String:
                return Encoding.UTF8.GetString(utf8Bytes);
            case SimpleTypeNames.Guid:
                return new Guid(Encoding.UTF8.GetString(utf8Bytes));
            case SimpleTypeNames.Bytes:
                return utf8Bytes;
        }
        var ret = SerializerUtil.DeserializeJson(Encoding.UTF8.GetString(utf8Bytes), returnType);
        return ret;
    }

    public T Deserialize<T>(byte[] utf8Bytes)
    {
        return (T)Deserialize(utf8Bytes, typeof(T));
    }
}