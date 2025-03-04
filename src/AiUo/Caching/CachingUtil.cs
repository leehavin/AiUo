using Microsoft.Extensions.Caching.Distributed;
using System;

namespace AiUo.Caching;

public static class CachingUtil
{
    private static IDCache _dcache;
    private static IDCache GetCache()
    {
        if (_dcache == null)
        {
            var dcache = DIUtil.GetService<IDistributedCache>();
            if (dcache != null)
            {
                _dcache = new DistributedCacheEx(dcache);
            }
            else
            {
                _dcache = new MemoryCacheEx();
            }
        }
        return _dcache;
    }
    public static bool TryGet<T>(object key, out T value)
        => GetCache().TryGet(key, out value);

    public static T GetOrAdd<T>(object key, Func<object, CacheItem<T>> valueFactory)
        => GetCache().GetOrAdd(key, valueFactory);
    public static T GetOrDefault<T>(object key, T defaultValue)
        => GetCache().GetOrDefault(key, defaultValue);
    public static T GetOrException<T>(object key)
        => GetCache().GetOrException<T>(key);
    public static void Remove(object key)
        => GetCache().Remove(key);
    public static void Set<T>(object key, T value, TimeSpan? expire = null)
        => GetCache().Set(key, value, expire);
}