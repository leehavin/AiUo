using System;

namespace AiUo.Caching;

/// <summary>
/// 保存在缓存中的项（支持过期）
/// </summary>
/// <typeparam name="T"></typeparam>
public class CacheItem<T>
{
    public T Value { get; set; }
    public long? ExpireTime { get; set; }

    public CacheItem()
    {
    }

    public CacheItem(T value)
    {
        Value = value;
    }
    public CacheItem(T value, DateTime? expireAt)
    {
        Value = value;
        SetExpire(expireAt);
    }
    public CacheItem(T value, TimeSpan? expireSpan)
    {
        Value = value;
        SetExpire(expireSpan);
    }


    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public bool IsExpired => ExpireTime.HasValue
        ? (DateTime.UtcNow.ToTimestamp() - ExpireTime) > 0
        : false;
    public void SetExpire(DateTime? expireAt)
    {
        if (expireAt.HasValue)
            ExpireTime = expireAt.Value.ToTimestamp();
    }
    public void SetExpire(TimeSpan? expireSpan)
    {
        if (expireSpan.HasValue)
            ExpireTime = DateTime.UtcNow.Add(expireSpan.Value).ToTimestamp();
    }
    public TimeSpan? GetExpireSpan()
    {
        if (!ExpireTime.HasValue)
            return null;
        return DateTimeUtil.ParseTimestamp(ExpireTime.Value, true) - DateTime.UtcNow;
    }
    public DateTime? GetExpireUtcTime()
    {
        if (!ExpireTime.HasValue)
            return null;
        return DateTimeUtil.ParseTimestamp(ExpireTime.Value, true);
    }
    public DateTime? GetExpireTime()
    {
        if (!ExpireTime.HasValue)
            return null;
        return DateTimeUtil.ParseTimestamp(ExpireTime.Value);
    }
}