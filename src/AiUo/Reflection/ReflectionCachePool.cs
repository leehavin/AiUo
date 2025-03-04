using System;
using System.Collections.Generic;

namespace AiUo.Reflection;

/// <summary>
/// 如果获取 <typeparamref name="TSource"/> 对应信息的过程比较耗时（例如反射），
/// 则可使用 <see cref="ReflectionCachePool{TSource,TCache}"/> 对此过程进行缓存。
/// </summary>
/// <typeparam name="TSource">为了获取信息所需的源对象。</typeparam>
/// <typeparam name="TCache">获取的信息将会储存在此类型的缓存对象中。</typeparam>
public sealed class ReflectionCachePool<TSource, TCache>
{
    /// <summary>
    /// 使用特定的转换器创建 <see cref="ReflectionCachePool{TSource,TCache}"/> 的新实例。
    /// </summary>
    /// <param name="conversion">从源对象到目标对象的转换方法，此方法仅执行一次。</param>
    /// <param name="threadSafe">如果获取缓存的过程可能在不同线程，则设置此值为 true，以便让缓存过程是线程安全的。</param>
    public ReflectionCachePool(Func<TSource, TCache> conversion, bool threadSafe = false)
    {
        _convert = conversion ?? throw new ArgumentNullException(nameof(conversion));
        _locker = threadSafe ? new object() : null;
    }

    /// <summary>
    /// 从缓存池中获取缓存的信息，如果从未获取过信息，则将会执行一次
    /// 从 <typeparamref name="TSource"/> 到 <typeparamref name="TCache"/> 的转换。
    /// </summary>
    /// <param name="source">为了获取信息所需的源对象。</param>
    /// <returns>缓存的对象。</returns>
    public TCache this[TSource source] => GetOrCacheValue(source);

    /// <summary>
    /// 获取锁，如果此值为 null，说明无需加锁。
    /// </summary>
    private readonly object _locker;

    /// <summary>
    /// 获取转换对象的方法。
    /// </summary>
    private readonly Func<TSource, TCache> _convert;

    /// <summary>
    /// 获取缓存了 <typeparamref name="TCache"/> 的字典。
    /// </summary>
    private readonly Dictionary<TSource, TCache> _cacheDictionary =
        new Dictionary<TSource, TCache>();

    /// <summary>
    /// 从缓存池中获取缓存的信息，如果从未获取过信息，则将会执行一次
    /// 从 <typeparamref name="TSource"/> 到 <typeparamref name="TCache"/> 的转换。
    /// </summary>
    private TCache GetOrCacheValue(TSource source)
    {
        // 如果不需要加锁，则直接返回值。
        if (_locker == null)
        {
            return GetOrCacheValue();
        }

        // 如果需要加锁，则加锁后返回值。
        lock (_locker)
        {
            return GetOrCacheValue();
        }

        // 如果存在缓存，则获取缓存；否则从源值转换成缓存。
        TCache GetOrCacheValue()
        {
            if (!_cacheDictionary.TryGetValue(source, out var cache))
            {
                cache = _convert(source);
                _cacheDictionary[source] = cache;
            }

            return cache;
        }
    }
}