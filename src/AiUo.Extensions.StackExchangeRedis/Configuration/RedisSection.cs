using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using AiUo.Configuration;
using AiUo.Extensions.StackExchangeRedis;
using AiUo.Collections;

namespace AiUo.Configuration;

/// <summary>
/// Redis配置文件
/// </summary>
public class RedisSection : ConfigSection
{
    /// <summary>
    /// Section名称
    /// </summary>
    public override string SectionName => "Redis";
    /// <summary>
    /// 默认redis连接
    /// </summary>
    public string DefaultConnectionStringName { get; set; }

    /// <summary>
    /// redis连接集合
    /// </summary>
    public Dictionary<string, ConnectionStringElement> ConnectionStrings = new Dictionary<string, ConnectionStringElement>();
    /// <summary>
    /// 命名空间与Redis连接字符串映射集合
    /// key: namespace value: connectionStringName
    /// </summary>
    public ConcurrentDictionary<string, string> ConnectionStringNamespaces = new ConcurrentDictionary<string, string>();

    /// <summary>
    /// 是否自动加载
    /// </summary>
    public bool AutoLoad { get; set; }
    /// <summary>
    /// 发布订阅DLL
    /// </summary>
    public List<string> ConsumerAssemblies { get; set; } = new List<string>();

    /// <summary>
    /// 配置绑定
    /// </summary>
    /// <param name="configuration"></param>
    public override void Bind(IConfiguration configuration)
    {
        base.Bind(configuration);
        ConnectionStrings = configuration.GetSection("ConnectionStrings")
            .Get<Dictionary<string, ConnectionStringElement>>() ?? new();
        ConnectionStrings.ForEach(x => x.Value.Name = x.Key);

        ConnectionStringNamespaces.Clear();
        foreach (var conn in ConnectionStrings)
        {
            if (!string.IsNullOrEmpty(conn.Value.NamespaceMap))
            {
                foreach (var ns in conn.Value.NamespaceMap.Split(';'))
                {
                    if (!ConnectionStringNamespaces.TryAdd(ns, conn.Key))
                        throw new Exception($"AiUo配置中Redis:ConnectionStrings:NamespaceMap配置重复。name: {conn.Key} NamespaceMap: {ns}");
                }
            }
        }

        ConsumerAssemblies.Clear();
        ConsumerAssemblies = configuration?.GetSection("ConsumerAssemblies")
            .Get<List<string>>() ?? new List<string>();

        if (string.IsNullOrEmpty(DefaultConnectionStringName) && ConnectionStrings.Count == 1)
            DefaultConnectionStringName = ConnectionStrings.First().Key;
    }

    public ConnectionStringElement GetConnectionStringElement(string connectionStringName = null, Type type = null)
    {
        if (string.IsNullOrEmpty(connectionStringName))
        {
            connectionStringName = (type == null)
                                   || !ConnectionStringNamespaces.TryGetValue(type.Namespace, out string name)
                ? DefaultConnectionStringName
                : name;
        }
        if (string.IsNullOrEmpty(connectionStringName) || !ConnectionStrings.TryGetValue(connectionStringName, out var ret))
            throw new Exception($"Redis配置Redis:ConnectionStrings:Name不存在。Name:{connectionStringName}");
        if (string.IsNullOrEmpty(ret.ConnectionString))
            throw new Exception($"Redis配置Redis:ConnectionStrings:ConnectionString不能为空。Name:{ret.Name}");
        return ret;
    }
}