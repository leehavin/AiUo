using StackExchange.Redis;
using AiUo.Caching;

namespace AiUo.Extensions.StackExchangeRedis;

public partial class RedisHashExpireBase<TField> 
{
    #region Serializer
    protected RedisValue SerializeExpire<T>(T value, DateTime? expireAt)
    {
        var cacheItem = new CacheItem<T>(value, expireAt);
        return Serializer.Serialize(cacheItem);
    }
    protected RedisValue SerializeExpire(TField value, DateTime? expireAt)
        => SerializeExpire<TField>(value, expireAt);
    protected RedisValue SerializeExpire<T>(T value, TimeSpan? expire)
    {
        var cacheItem = new CacheItem<T>(value, expire);
        return Serializer.Serialize(cacheItem);
    }
    protected RedisValue SerializeExpire(TField value, TimeSpan? expire)
        => SerializeExpire<TField>(value, expire);
    protected RedisValue SerializeExpire<T>(CacheItem<T> cacheItem)
        => Serializer.Serialize(cacheItem);
    protected RedisValue SerializeExpire(CacheItem<TField> cacheItem)
        => Serializer.Serialize(cacheItem);

    protected bool TryDeserializeExpire(string field, RedisValue redisValue, out TField value)
    {
        return TryDeserializeExpire<TField>(field, redisValue, out value);
    }
    protected bool TryDeserializeExpire<T>(string field, RedisValue redisValue, out T value)
    {
        value = default;
        if (redisValue.IsNull)// redis不存在此key
        {
            return false;
        }
        else // 正常有值
        {
            var cacheItem = (CacheItem<T>)Serializer.Deserialize(redisValue, typeof(CacheItem<T>));
            if (cacheItem.IsExpired)
            {
                DeleteAsync(field).GetTaskResult();
                return false;
            }
            else
            {
                value = cacheItem.Value;
            }
        }
        return true;
    }
    protected bool TryDeserializeExpire(string field, RedisValue redisValue, out CacheItem<TField> cacheItem)
    {
        cacheItem = default;
        if (redisValue.IsNull)// redis不存在此key
        {
            return false;
        }
        else // 正常有值
        {
            cacheItem = (CacheItem<TField>)Serializer.Deserialize(redisValue, typeof(CacheItem<TField>));
            if (cacheItem.IsExpired)
            {
                DeleteAsync(field).GetTaskResult();
                return false;
            }
        }
        return true;
    }
    #endregion
}