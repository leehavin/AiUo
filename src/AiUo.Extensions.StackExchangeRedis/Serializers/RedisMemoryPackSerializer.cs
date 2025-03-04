/*
using MemoryPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AiUo.Serialization;

namespace AiUo.Extensions.StackExchangeRedis.Serializers
{
    /// <summary>
    /// MemoryPack序列化
    /// 序列化的类必须添加MemoryPackableAttribute和partial
    /// </summary>
    public class RedisMemoryPackSerializer : SerializerBase
    {
        private static ConcurrentDictionary<Type, bool> _typeCache = new ConcurrentDictionary<Type, bool>();
        public override byte[] Serialize(object value)
        {
            var type = value.GetType();
            if (!_typeCache.ContainsKey(type))
            {
                var attr = type.GetCustomAttribute<MemoryPackableAttribute>();
                if (attr == null)
                    throw new Exception($"使用MemoryPack序列化时，类必须添加MemoryPackableAttribute和partial。type:{type.FullName}");
                _typeCache.TryAdd(type, true);
            }
            return MemoryPackSerializer.Serialize(type, value);
        }

        public override object Deserialize(byte[] utf8Bytes, Type returnType)
        {
            return MemoryPackSerializer.Deserialize(returnType, utf8Bytes);
        }

    }
}
*/
