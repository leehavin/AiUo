using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text;

namespace AiUo.Caching;

internal class DistributedCacheEx : IDCache
{
    private IDistributedCache _cache;
    public DistributedCacheEx(IDistributedCache cache)
    {
        _cache = cache;
    }

    public void Remove(object key)
    {
        _cache.Remove(GetKey(key));
    }

    public void Set<T>(object key, T value, TimeSpan? expire = null)
    {
        var k = GetKey(key);
        var data = Serialize(value);
        var opts = new DistributedCacheEntryOptions();
        if (expire.HasValue)
            opts.SlidingExpiration = expire.Value;
        _cache.Set(k, data, opts);
    }

    public bool TryGet<T>(object key, out T value)
    {
        var k = GetKey(key);
        var byts = _cache.Get(k);
        if (byts != null)
        {
            value = Deserialize<T>(byts);
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }
    public T GetOrAdd<T>(object key, Func<object, CacheItem<T>> valueFactory)
    {
        if (!TryGet(key, out T ret))
        {
            var value = valueFactory(key);
            Set(key, value.Value, value.GetExpireSpan());
            return value.Value;
        }
        return ret;
    }

    public T GetOrDefault<T>(object key, T defaultValue)
    {
        return TryGet(key, out T ret) ? ret : defaultValue;
    }

    public T GetOrException<T>(object key)
    {
        if (!TryGet(key, out T ret))
            throw new CacheNotFound("IDistributedCache");
        return ret;
    }
    private string GetKey(object key)
    {
        var ret = Convert.ToString(key);
        if (string.IsNullOrEmpty(ret))
            throw new Exception($"DistributedCacheEx的key不能为空");
        return ret;
    }
    private byte[] Serialize<T>(T value)
        => Encoding.UTF8.GetBytes(SerializerUtil.SerializeJson(value));
    private T Deserialize<T>(byte[] data)
        => SerializerUtil.DeserializeJson<T>(Encoding.UTF8.GetString(data));
}