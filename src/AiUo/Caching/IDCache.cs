using System;

namespace AiUo.Caching;

public interface IDCache
{
    bool TryGet<T>(object key, out T value);
    T GetOrAdd<T>(object key, Func<object, CacheItem<T>> valueFactory);
    T GetOrDefault<T>(object key, T defaultValue);
    T GetOrException<T>(object key);
    void Remove(object key);
    void Set<T>(object key, T value, TimeSpan? expire=null);
}