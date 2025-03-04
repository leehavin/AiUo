using Microsoft.Extensions.Caching.Memory;
using System;

namespace AiUo.Caching;

internal class MemoryCacheEx : IDCache
{
    private MemoryCache _cache;
    public MemoryCacheEx()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    public void Remove(object key)
        => _cache.Remove(key);

    public void Set<T>(object key, T value, TimeSpan? expire = null)
    {
        if (expire.HasValue)
            _cache.Set(key, value, expire.Value);
        else
            _cache.Set(key, value);
    }

    public bool TryGet<T>(object key, out T value)
        => _cache.TryGetValue(key, out value);

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
}