using System;

namespace AiUo.Caching;

/// <summary>
/// 缓存项未找到
/// </summary>
public class CacheNotFound : Exception
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message"></param>
    public CacheNotFound(string message) : base(message) { }
}