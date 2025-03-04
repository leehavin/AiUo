using System;

namespace AiUo.Serialization;

/// <summary>
/// 序列化器
/// </summary>
public interface ISerializer
{
    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    byte[] Serialize(object value);

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="utf8Bytes">序列化对象的UTF8字节流</param>
    /// <param name="returnType"></param>
    /// <returns></returns>
    object Deserialize(byte[] utf8Bytes, Type returnType);
    /// <summary>
    /// 反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="utf8Bytes">序列化对象的UTF8字节流</param>
    /// <returns>
    /// The instance of the specified Item
    /// </returns>
    T Deserialize<T>(byte[] utf8Bytes);
}