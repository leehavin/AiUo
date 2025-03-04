using BloomFilter;
using BloomFilter.Redis;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using AiUo.Collections;
using AiUo.Configuration;
using AiUo.Extensions.StackExchangeRedis.Clients;
using AiUo.Extensions.StackExchangeRedis.Serializers;
using AiUo.Serialization;
using static StackExchange.Redis.RedisChannel;
using Type = System.Type;

namespace AiUo.Extensions.StackExchangeRedis;

/// <summary>
/// Redis辅助类，获取：
///     IDatabase:      操作Redis底层类，包含全部操作
///     RedisClient:    操作Redis高层类，针对Redis类型进行了封装
/// </summary>
public static class RedisUtil
{
    #region Constructors
    private static ConcurrentDictionary<string, ConnectionStringElement> _elementDic = new();
    // key: ConfigString
    private static readonly ConcurrentDictionary<string, Lazy<ConnectionMultiplexer>> _redisDict = new();

    private static ISerializer _jsonSerializer { get; }
    public static JsonSerializerSettings JsonSettings { get; }
    private static readonly ISerializer _bytesSerializer = new RedisBytesSerializer();
    //private static readonly ISerializer _memoryPackSerializer = new RedisMemoryPackSerializer();

    static RedisUtil()
    {
        var serializer = new RedisJsonSerializer();
        _jsonSerializer = serializer;
        JsonSettings = serializer.JsonSettings;
    }
    #endregion

    #region Redis & Database
    public static ConnectionMultiplexer GetRedis(string connectionStringName = null, string flag = null)
    {
        var element = GetConfigElement(connectionStringName);
        return GetRedisByConnectionString(element.ConnectionString, flag);
    }
    public static ConnectionMultiplexer GetRedisByConnectionString(string connectionString, string flag = null)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException("connectionString");
        var key = $"{connectionString}|{flag}";
        return _redisDict.GetOrAdd(key, (k) =>
        {
            return new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect(connectionString);
            });
        }).Value;
    }
    private static IDatabase _defaultDatabase;
    /// <summary>
    /// 默认Database
    /// </summary>
    public static IDatabase DefaultDatabase
        => _defaultDatabase ??= GetDatabase();

    /// <summary>
    /// 获得基础Redis操作类IDatabase
    /// </summary>
    /// <param name="connectionStringName"></param>
    /// <param name="databaseIndex"></param>
    /// <returns></returns>
    public static IDatabase GetDatabase(string connectionStringName = null, int databaseIndex = -1)
    {
        var element = GetConfigElement(connectionStringName);
        return GetRedisByConnectionString(element.ConnectionString).GetDatabase(databaseIndex);
    }
    internal static ConnectionStringElement GetConfigElement(string connectionStringName = null, Type type = null)
    {
        var key = $"{type?.FullName}|{connectionStringName}";
        if (!_elementDic.TryGetValue(key, out var ret))
        {
            var section = ConfigUtil.GetSection<RedisSection>();
            if (section == null)
                throw new Exception($"Redis配置不存在");
            ret = section.GetConnectionStringElement(connectionStringName, type);
            _elementDic.TryAdd(key, ret);
        }
        return ret;
    }
    #endregion

    #region CreateClient
    public static RedisListClient<T> CreateListClient<T>(string redisKey, string connectionStringName = null, bool isGlobal = false)
        => CreateClient<RedisListClient<T>>(redisKey, connectionStringName, isGlobal);
    public static RedisSetClient<T> CreateSetClient<T>(string redisKey, string connectionStringName = null, bool isGlobal = false)
        => CreateClient<RedisSetClient<T>>(redisKey, connectionStringName, isGlobal);
    public static RedisSortedSetClient<T> CreateSortedSetClient<T>(string redisKey, string connectionStringName = null, bool isGlobal = false)
        => CreateClient<RedisSortedSetClient<T>>(redisKey, connectionStringName, isGlobal);
    public static RedisStringClient<T> CreateStringClient<T>(string redisKey, string connectionStringName = null, bool isGlobal = false)
        => CreateClient<RedisStringClient<T>>(redisKey, connectionStringName, isGlobal);
    public static RedisBitClient CreateBitClient(string redisKey, string connectionStringName = null, bool isGlobal = false)
        => CreateClient<RedisBitClient>(redisKey, connectionStringName, isGlobal);
    public static RedisHashClient<T> CreateHashClient<T>(string redisKey, string connectionStringName = null, bool isGlobal = false)
        => CreateClient<RedisHashClient<T>>(redisKey, connectionStringName, isGlobal);
    public static RedisHashClient CreateHashClient(string redisKey, string connectionStringName = null, bool isGlobal = false)
        => CreateClient<RedisHashClient>(redisKey, connectionStringName, isGlobal);
    public static RedisHashExpireClient<T> CreateHashExpireClient<T>(string redisKey, string connectionStringName = null, bool isGlobal = false)
        => CreateClient<RedisHashExpireClient<T>>(redisKey, connectionStringName, isGlobal);
    public static RedisHashExpireClient CreateHashExpireClient(string redisKey, string connectionStringName = null, bool isGlobal = false)
        => CreateClient<RedisHashExpireClient>(redisKey, connectionStringName, isGlobal);
    private static T CreateClient<T>(string redisKey, string connectionStringName = null, bool isGlobal = false)
        where T : IRedisClient, new()
    {
        T ret = new T();
        ret.RedisKey = isGlobal ? redisKey : $"{ConfigUtil.Project.ProjectId}:_CUSTOM:{redisKey}";
        ret.Options = GetOptions(connectionStringName, null);
        return ret;
    }
    #endregion

    #region GetRedisKey

    public static string GetGlobalGroupRedisKey(string group, Type type, object id = null)
    {
        return id == null
            ? $"{RedisPrefixConst.GLOBAL}:{group}:{GetTypeName(type)}"
            : $"{RedisPrefixConst.GLOBAL}:{group}:{GetTypeName(type)}:{Convert.ToString(id)}";
    }
    /// <summary>
    /// 全局默认RedisKey格式:GLOBAL:TypeName:Id
    /// </summary>
    /// <param name="type"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string GetGlobalRedisKey(Type type, object id = null)
    {
        return id == null
            ? $"{RedisPrefixConst.GLOBAL}:{GetTypeName(type)}"
            : $"{RedisPrefixConst.GLOBAL}:{GetTypeName(type)}:{Convert.ToString(id)}";
    }
    /// <summary>
    /// 全局默认RedisKey格式:Global:TypeName:Id
    /// </summary>
    /// <param name="type"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string GetGlobalRedisKey(string type, object id = null)
    {
        return id == null
            ? $"{RedisPrefixConst.GLOBAL}:{type}"
            : $"{RedisPrefixConst.GLOBAL}:{type}:{Convert.ToString(id)}";
    }
    public static string GetProjectGroupRedisKey(string group, Type type, object id = null)
    {
        return id == null
            ? $"{ConfigUtil.Project.ProjectId}:{group}:{GetTypeName(type)}"
            : $"{ConfigUtil.Project.ProjectId}:{group}:{GetTypeName(type)}:{Convert.ToString(id)}";
    }
    /// <summary>
    /// 项目默认RedisKey格式:ProjectId:TypeName:Id
    /// </summary>
    /// <param name="type"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string GetProjectRedisKey(Type type, object id = null)
    {
        return id == null
            ? $"{ConfigUtil.Project.ProjectId}:{GetTypeName(type)}"
            : $"{ConfigUtil.Project.ProjectId}:{GetTypeName(type)}:{Convert.ToString(id)}";
    }
    /// <summary>
    /// 项目默认RedisKey格式:ProjectId:TypeName:Id
    /// </summary>
    /// <param name="type"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string GetProjectRedisKey(string type, object id = null)
    {
        return id == null
            ? $"{ConfigUtil.Project.ProjectId}:{type}"
            : $"{ConfigUtil.Project.ProjectId}:{type}:{Convert.ToString(id)}";
    }
    private static ConcurrentDictionary<Type, string> _typeNameCache = new ConcurrentDictionary<Type, string>();
    private static string GetTypeName(Type type)
    {
        if (_typeNameCache.TryGetValue(type, out string typeName))
            return typeName;
        typeName = type.Name;
        var idx = typeName.IndexOf('`');
        if (idx > 0)
            typeName = typeName.Substring(0, idx);
        typeName = typeName.TrimEnd("DCache", false);
        _typeNameCache.TryAdd(type, typeName);
        return typeName;
    }
    #endregion

    #region Lock
    /// <summary>
    /// 申请分布式事务锁，直到等待时间到期，申请到锁后锁自动延期
    /// using(var redLock = await LockAsync(key, 20))
    /// {
    ///     if(redLock.IsLocked) //成功上锁
    ///     { }
    ///     else
    ///     { }
    /// }
    /// </summary>
    /// <param name="lockKey">要锁定资源的键值（一般指业务范围）</param>
    /// <param name="waitSeconds">等待申请锁超时时间</param>
    /// <param name="retryInterval">申请锁间隔</param>
    /// <param name="connectionStringName"></param>
    /// <returns></returns>
    public static async Task<RedLock> LockAsync(string lockKey, int waitSeconds = 30, int retryInterval = 500, string connectionStringName = null)
    {
        var waitSpan = TimeSpan.FromSeconds(waitSeconds);
        var interval = TimeSpan.FromMilliseconds(retryInterval);
        return await LockAsync(lockKey, waitSpan, interval, connectionStringName);
    }
    public static async Task<RedLock> LockAsync(string lockKey, TimeSpan waitSpan, TimeSpan? retryInterval = null, string connectionStringName = null)
    {
        var database = !string.IsNullOrEmpty(connectionStringName)
            ? GetRedis(connectionStringName).GetDatabase()
            : DefaultDatabase;
        var ret = new RedLock(database, lockKey, waitSpan, retryInterval);
        await ret.StartAsync();
        return ret;
    }
    #endregion

    #region Publish
    /// <summary>
    /// 发布广播消息（RedisSubscribeConsumer子类消费）
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message"></param>
    /// <param name="connectionStringName"></param>
    /// <returns></returns>
    public static async Task PublishAsync<TMessage>(TMessage message, string connectionStringName = null)
    {
        var attr = typeof(TMessage).GetCustomAttribute<RedisPublishMessageAttribute>();
        var channel = GetPublishChannel(message, attr?.PatternMode ?? PatternMode.Auto);
        var msg = GetSerializer(RedisSerializeMode.Json).Serialize(message);
        await GetRedis(connectionStringName ?? attr?.ConnectionStringName)
            .GetSubscriber()
            .PublishAsync(channel, msg);
    }
    /// <summary>
    /// 发布队列消息,队列消息将被阻塞且单一执行（RedisQueueConsumer子类消费）
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message"></param>
    /// <param name="connectionStringName"></param>
    /// <returns></returns>
    public static async Task PublishQueueAsync<TMessage>(TMessage message, string connectionStringName = null)
    {
        var attr = typeof(TMessage).GetCustomAttribute<RedisPublishMessageAttribute>();
        var channel = GetPublishChannel(message, attr?.PatternMode ?? PatternMode.Auto);
        var msg = GetSerializer(RedisSerializeMode.Json).Serialize(message);
        var redis = GetRedis(connectionStringName ?? attr?.ConnectionStringName);
        var key = GetQueueKey<TMessage>();
        await redis.GetDatabase().ListLeftPushAsync(key, msg, flags: CommandFlags.FireAndForget);
        await redis.GetSubscriber().PublishAsync(channel, string.Empty);
    }
    private static RedisChannel GetPublishChannel<TMessage>(TMessage msg, PatternMode mode = PatternMode.Auto)
    {
        var key = (msg is IRedisPublishMessage)
            ? ((IRedisPublishMessage)msg).PatternKey
            : null;
        return GetChannel<TMessage>(key, mode);
    }
    internal static RedisChannel GetChannel<TMessage>(string key, PatternMode mode = PatternMode.Auto)
    {
        var name = GetBaseChannelName<TMessage>();
        if (!string.IsNullOrEmpty(key))
            name = $"{name}:{key}";
        return new RedisChannel(name, mode);
    }
    internal static string GetBaseChannelName<TMessage>()
        => $"_PubSub:{typeof(TMessage).FullName}";
    internal static string GetQueueKey<TMessage>()
    {
        return $"_Queue:{typeof(TMessage).FullName}";
    }
    #endregion

    #region BloomFilter
    /// <summary>
    /// 布隆过滤器(Contains返回false则一定不存在，true则有可能存在)
    /// </summary>
    /// <param name="redisKey">业务标识</param>
    /// <param name="expectedElements">预期总元素数</param>
    /// <param name="method">哈希算法</param>
    /// <param name="connectionStringName"></param>
    /// <returns></returns>
    public static IBloomFilter CreateBloomFilter(string redisKey, long expectedElements, HashMethod method = HashMethod.Murmur3, string connectionStringName = null)
    {
        var key = $"{RedisPrefixConst.BLOOM_FILTER}:{redisKey}";
        var conn = GetRedis(connectionStringName);
        return FilterRedisBuilder.Build(conn, key, expectedElements, method, redisKey);
    }
    #endregion

    #region Utils
    /// <summary>
    /// 查询指定redis指定database指定pattern的keys
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="connectionStringName"></param>
    /// <param name="database"></param>
    /// <returns></returns>
    public static async Task<List<string>> ScanKeysAsync(RedisValue pattern = default, string connectionStringName = null, int database = -1)
    {
        var ret = new List<string>();
        foreach (var endPoint in GetEndPoints(connectionStringName))
        {
            var server = GetRedis(connectionStringName).GetServer(endPoint);
            var keys = server.KeysAsync(database, pattern);
            await foreach (var key in keys)
                ret.Add(key.ToString());
        }
        return ret;
    }

    /// <summary>
    /// 获得服务器节点信息
    /// </summary>
    /// <param name="connectionStringName"></param>
    /// <returns></returns>
    public static EndPoint[] GetEndPoints(string connectionStringName = null)
        => GetRedis(connectionStringName).GetEndPoints();
    public static EndPoint[] GetEndPointsByConnectionString(string connectionString)
        => GetRedisByConnectionString(connectionString).GetEndPoints();
    /// <summary>
    /// 获取redis server，以便于使用服务器命令
    /// </summary>
    /// <param name="hostAndPort"></param>
    /// <param name="asyncState"></param>
    /// <param name="connectionStringName"></param>
    /// <returns></returns>
    public static IServer GetServer(string hostAndPort, object asyncState = null, string connectionStringName = null)
        => GetRedis(connectionStringName).GetServer(hostAndPort, asyncState);
    public static IServer GetServerByConnectionString(string connectionString, string hostAndPort, object asyncState = null)
        => GetRedisByConnectionString(connectionString).GetServer(hostAndPort, asyncState);

    internal static RedisClientOptions GetOptions(string connectionStringName = null, Type type = null)
    {
        var element = GetConfigElement(connectionStringName, type);
        return new RedisClientOptions
        {
            ConnectionStringName = element.Name,
            ConnectionString = element.ConnectionString,
            SerializeMode = element.SerializeMode
        };
    }

    internal static ISerializer GetSerializer(RedisSerializeMode serializer)
    {
        ISerializer ret = null;
        switch (serializer)
        {
            case RedisSerializeMode.Json:
                ret = _jsonSerializer;
                break;
            case RedisSerializeMode.Bytes:
                ret = _bytesSerializer;
                break;
            //case RedisSerializeMode.MemoryPack:
            //    ret = _memoryPackSerializer;
            //    break;
            default:
                throw new Exception("仅支持json序列化");
        }
        return ret;
    }

    internal static void ReleaseAllRedis()
    {
        _redisDict.ForEach(x => x.Value.Value.Dispose());
        //LogUtil.Info("Redis资源已释放");
    }
    #endregion 
}