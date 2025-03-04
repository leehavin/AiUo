using StackExchange.Redis;
using System.Collections.Concurrent;
using AiUo.Caching;
using AiUo.Serialization;

namespace AiUo.Extensions.StackExchangeRedis;

public interface IRedisClient
{
    IDatabase Database { get; }
    RedisClientOptions Options { get; set; }
    string RedisKey { get; set; }
    RedisType RedisType { get; }

    Task<bool> KeyDeleteAsync(CommandFlags flags = CommandFlags.None);
    Task<byte[]> KeyDumpAsync(CommandFlags flags = CommandFlags.None);
    Task<bool> KeyExistsAsync(CommandFlags flags = CommandFlags.None);
    Task<bool> KeyExpireAsync(TimeSpan expirySpan, CommandFlags flags = CommandFlags.None);
    Task<bool> KeyExpireAtAsync(DateTime expiryAt, CommandFlags flags = CommandFlags.None);
    Task<bool> KeyExpireDaysAsync(int days, CommandFlags flags = CommandFlags.None);
    Task<bool> KeyExpireHoursAsync(int hours, CommandFlags flags = CommandFlags.None);
    Task<bool> KeyExpireMillisecondsAsync(int milliseconds, CommandFlags flags = CommandFlags.None);
    Task<bool> KeyExpireMinutesAsync(int minutes, CommandFlags flags = CommandFlags.None);
    Task<bool> KeyExpireSecondsAsync(int seconds, CommandFlags flags = CommandFlags.None);
    Task<TimeSpan?> KeyIdleTimeAsync(CommandFlags flags = CommandFlags.None);
    Task<bool> KeyRenameAsync(RedisKey newKey, When when = When.Always, CommandFlags flags = CommandFlags.None);
    Task<TimeSpan?> KeyTimeToLiveAsync(CommandFlags flags = CommandFlags.None);
    Task<RedisType> KeyTypeAsync(CommandFlags flags = CommandFlags.None);
}

/// <summary>
/// Redis对象基类：hash, list, set, zset(sortedset),string
///     RedisKey = GetGlobalRedisKey() 或者 GetGlobalRedisKey()
/// </summary>
public abstract class RedisClientBase : IRedisClient
{
    #region Properties & Constructors
    /// <summary>
    /// Redis类型
    /// </summary>
    public abstract RedisType RedisType { get; }

    /// <summary>
    /// 当前Redis缓存key
    ///     GetProjectRedisKey(id) --默认
    ///     GetGlobalRedisKey(id)
    /// </summary>
    public string RedisKey { get; set; }

    /// <summary>
    /// Redis设置
    /// </summary>
    public RedisClientOptions Options { get; set; }

    private ISerializer _serializer;
    /// <summary>
    /// 缓存对象序列化器
    /// </summary>
    protected ISerializer Serializer
    {
        get
        {
            if (_serializer == null)
                _serializer = RedisUtil.GetSerializer(Options.SerializeMode);
            return _serializer;
        }
        set { _serializer = value; }
    }

    private static ConcurrentDictionary<Type, ConnectionMultiplexer> _connDict = new();
    private ConnectionMultiplexer GetConnection()
    {
        var type = GetType();
        if (_connDict.TryGetValue(type, out var ret))
            return ret;
        ret = !string.IsNullOrEmpty(Options.ConnectionString)
            ? RedisUtil.GetRedisByConnectionString(Options.ConnectionString)
            : RedisUtil.GetRedis(Options.ConnectionStringName);
        if (!type.Namespace.StartsWith("AiUo.Extensions.StackExchangeRedis"))
            _connDict.TryAdd(type, ret);
        return ret;
    }
    /// <summary>
    /// 操作缓存数据的Database
    /// </summary>
    public IDatabase Database
    {
        get
        {
            if (string.IsNullOrEmpty(RedisKey))
                throw new Exception($"Redis操作时，子类必须设置RedisKey。可在构造函数设置如:GetGlobalRedisKey(id),GetGlobalGroupRedisKey(group,id),或者GetProjectRedisKey(id),GetProjectGroupRedisKey(group,id)! type:{GetType().FullName}");
            return GetConnection()
                .GetDatabase(Options.DatabaseIndex);
        }
    }

    public RedisClientBase()
    {
        Options = new RedisClientOptions();
        RedisKey = GetProjectRedisKey();
    }
    protected async Task SetSlidingExpirationAsync(TimeSpan? expire = null)
    {
        if (expire.HasValue)
        {
            await KeyExpireAsync(expire.Value);
        }
        else
        {
            if (Options.SlidingExpiration.HasValue)
            {
                await KeyExpireAsync(Options.SlidingExpiration.Value);
            }
        }
    }
    #endregion

    #region Methods
    public Task<bool> KeyDeleteAsync(CommandFlags flags = CommandFlags.None)
        => Database.KeyDeleteAsync(RedisKey, flags);

    public Task<byte[]> KeyDumpAsync(CommandFlags flags = CommandFlags.None)
        => Database.KeyDumpAsync(RedisKey, flags);

    public Task<bool> KeyExistsAsync(CommandFlags flags = CommandFlags.None)
        => Database.KeyExistsAsync(RedisKey, flags);

    /// <summary>
    /// 设置当前缓存的确切到期时间
    /// </summary>
    /// <param name="expiryAt">要设置的确切到期日期</param>
    /// <param name="flags"></param>
    /// <returns>如果设置了超时，则为true。 如果密钥不存在或无法设置超时，则为false</returns>
    public Task<bool> KeyExpireAtAsync(DateTime expiryAt, CommandFlags flags = CommandFlags.None)
        => Database.KeyExpireAsync(RedisKey, expiryAt, flags);

    /// <summary>
    /// 设置当前缓存从添加时算起，缓存多长时间到期
    /// </summary>
    /// <param name="expirySpan">要设置的超时时间</param>
    /// <param name="flags"></param>
    /// <returns>如果设置了超时，则为true。 如果密钥不存在或无法设置超时，则为false</returns>
    public Task<bool> KeyExpireAsync(TimeSpan expirySpan, CommandFlags flags = CommandFlags.None)
        => Database.KeyExpireAsync(RedisKey, expirySpan, flags);
    public Task<bool> KeyExpireMillisecondsAsync(int milliseconds, CommandFlags flags = CommandFlags.None)
        => KeyExpireAsync(TimeSpan.FromMilliseconds(milliseconds), flags);
    public Task<bool> KeyExpireSecondsAsync(int seconds, CommandFlags flags = CommandFlags.None)
        => KeyExpireAsync(TimeSpan.FromSeconds(seconds), flags);
    public Task<bool> KeyExpireMinutesAsync(int minutes, CommandFlags flags = CommandFlags.None)
        => KeyExpireAsync(TimeSpan.FromMinutes(minutes), flags);
    public Task<bool> KeyExpireHoursAsync(int hours, CommandFlags flags = CommandFlags.None)
        => KeyExpireAsync(TimeSpan.FromHours(hours), flags);
    public Task<bool> KeyExpireDaysAsync(int days, CommandFlags flags = CommandFlags.None)
        => KeyExpireAsync(TimeSpan.FromDays(days), flags);

    /// <summary>
    /// 返回自存储在指定键处的对象处于空闲状态以来的时间（未请求读或写操作）
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public Task<TimeSpan?> KeyIdleTimeAsync(CommandFlags flags = CommandFlags.None)
        => Database.KeyIdleTimeAsync(RedisKey, flags);

    /// <summary>
    /// 将键重命名为newkey。 当源名称和目标名称相同或键不存在时，它将返回错误
    /// </summary>
    /// <param name="newKey"></param>
    /// <param name="when"></param>
    /// <param name="flags"></param>
    /// <returns>如果密钥已重命名，则为true，否则为false</returns>
    public Task<bool> KeyRenameAsync(RedisKey newKey, When when = When.Always, CommandFlags flags = CommandFlags.None)
        => Database.KeyRenameAsync(RedisKey, newKey, when, flags);

    /// <summary>
    /// 返回具有超时的键的剩余生存时间
    /// </summary>
    /// <param name="flags"></param>
    /// <returns>TTL，如果键不存在或没有超时，则为null</returns>
    public Task<TimeSpan?> KeyTimeToLiveAsync(CommandFlags flags = CommandFlags.None)
        => Database.KeyTimeToLiveAsync(RedisKey, flags);

    /// <summary>
    /// 当前Redis缓存数据类型
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public Task<RedisType> KeyTypeAsync(CommandFlags flags = CommandFlags.None)
        => Database.KeyTypeAsync(RedisKey, flags);
    #endregion

    #region Utils
    /// <summary>
    /// 存入Redis前将T类型转换为RedisValue，如果值为null则存入EmptyString
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected virtual RedisValue Serialize(object value)
        => (value == null) ? RedisValue.EmptyString
            : (RedisValue)Serializer.Serialize(value);

    /// <summary>
    /// 反序列化，缓存项必须存在
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="redisValue"></param>
    /// <returns></returns>
    protected T Deserialize<T>(RedisValue redisValue)
    {
        if (!TryDeserialize(redisValue, out T ret))
            throw new CacheNotFound($"redis缓存项不存在,此调用非法请检查代码! type:{GetType().FullName}");
        return ret;
    }

    protected bool TryDeserialize<T>(RedisValue redisValue, out T value)
    {
        var ret = TryDeserialize(redisValue, typeof(T), out object objValue);
        value = ret ? (T)objValue : default;
        return ret;
    }
    protected virtual bool TryDeserialize(RedisValue redisValue, Type toType, out object value)
    {
        value = null;
        if (redisValue.IsNull)// redis不存在此key
        {
            return false;
        }
        else if (redisValue.IsNullOrEmpty) // redis值是EmptyString
        {
            if (!AllowNull(toType))
                throw new Exception($"缓存存储的是RedisValue.EmptyString,需转换的类不支持null。type:{toType.FullName}");
            else
                return true;
        }
        else // 正常有值
        {
            value = Serializer.Deserialize(redisValue, toType);
        }
        return true;
    }
    private static ConcurrentDictionary<Type, bool> _typeCache = new ConcurrentDictionary<Type, bool>();
    // 当前类型是否可存null值
    private bool AllowNull(Type toType)
    {
        if (!_typeCache.TryGetValue(toType, out bool ret))
        {
            ret = toType.IsClass || toType.IsNullableType();
            _typeCache.TryAdd(toType, ret);
        }
        return ret;
    }
    #endregion

    #region GetRedisKey
    protected string GetGlobalGroupRedisKey(string group, object id = null)
        => RedisUtil.GetGlobalGroupRedisKey(group, GetType(), id);
    /// <summary>
    /// 全局默认RedisKey格式:GLOBAL:TypeName:Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    protected string GetGlobalRedisKey(object id = null)
        => RedisUtil.GetGlobalRedisKey(GetType(), id);
    protected string GetProjectGroupRedisKey(string group, object id = null)
        => RedisUtil.GetProjectGroupRedisKey(group, GetType(), id);
    /// <summary>
    /// 全局默认RedisKey格式:GLOBAL:TypeName:Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    protected string GetProjectRedisKey(object id = null)
        => RedisUtil.GetProjectRedisKey(GetType(), id);
    #endregion
}