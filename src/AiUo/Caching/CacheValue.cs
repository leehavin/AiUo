namespace AiUo.Caching;

/// <summary>
/// 返回的缓存值
/// </summary>
/// <typeparam name="T"></typeparam>
public class CacheValue<T>
{
    /// <summary>
    /// 是否存在缓存值
    /// </summary>
    public bool HasValue { get; set; }
    public T Value { get; set; }

    public CacheValue()
    { }
    public CacheValue(T value)
    {
        HasValue = true;
        Value = value;
    }
    public CacheValue(bool hasValue, T value = default)
    {
        HasValue = hasValue;
        Value = value;
    }
}