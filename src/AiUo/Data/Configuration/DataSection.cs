using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;
using AiUo.Data;
using AiUo.Data.Instrumentation;
using AiUo.Data.ORM.Router;
using AiUo.Reflection;

namespace AiUo.Configuration;

/// <summary>
/// 数据库配置
/// </summary>
public class DataSection : ConfigSection
{
    #region Section
    /// <summary>
    /// Section名称
    /// </summary>
    public override string SectionName => "Data";
    /// <summary>
    /// 默认连接字符串名
    /// </summary>
    public string DefaultConnectionStringName { get; set; }

    public string OrmConnectionRouterType { get; set; }
    /// <summary>
    /// ORM连接路由（表名）
    /// </summary>
    public IOrmConnectionRouter OrmConnectionRouter { get; set; }

    /// <summary>
    /// 跟踪服务提供程序
    /// </summary>
    public string InstProvider { get; set; }
    /// <summary>
    /// 连接字符串集合
    /// </summary>
    public Dictionary<string, ConnectionStringElement> ConnectionStrings = new Dictionary<string, ConnectionStringElement>();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    public override void Bind(IConfiguration configuration)
    {
        base.Bind(configuration);
        ConnectionStrings = configuration.GetSection("ConnectionStrings")
            .Get<Dictionary<string, ConnectionStringElement>>() ?? new();
        LoadConnectionStringConfigs();
        if (!string.IsNullOrEmpty(OrmConnectionRouterType))
            OrmConnectionRouter = ReflectionUtil.CreateInstance<IOrmConnectionRouter>(OrmConnectionRouterType);
    }
    #endregion 

    #region ConnectionStringConfigs
    /// <summary>
    /// 数据库连接字符串配置集合。
    /// key: connectionStringName
    /// </summary>
    public ConcurrentDictionary<string, ConnectionStringConfig> ConnectionStringConfigs = new ConcurrentDictionary<string, ConnectionStringConfig>();
    /// <summary>
    /// 命名空间与数据库连接字符串映射集合
    /// key: namespace value: connectionStringName
    /// </summary>
    public ConcurrentDictionary<string, ConnectionStringConfig> ConnectionStringNamespaces = new ConcurrentDictionary<string, ConnectionStringConfig>();
    private void LoadConnectionStringConfigs()
    {
        ConnectionStringConfigs.Clear();
        ConnectionStringNamespaces.Clear();
        foreach (var sett in ConnectionStrings)
        {
            if(string.IsNullOrEmpty(sett.Key))
                throw new Exception("配置中Data:ConnectionStrings:Name不能为空");
            if(string.IsNullOrEmpty(sett.Value.ConnectionString))
                throw new Exception($"配置中Data:ConnectionStrings:Name[{sett.Key}].ConnectionString不能为空");
            if(string.IsNullOrEmpty(sett.Value.ProviderName))
                throw new Exception($"配置中Data:ConnectionStrings:Name[{sett.Key}].ProviderName不能为空");
            string instType = !string.IsNullOrEmpty(sett.Value.InstProvider) ? sett.Value.InstProvider : InstProvider;
            var config = new ConnectionStringConfig
            {
                ConnectionStringName = sett.Key,
                Provider = DbDataProviderUtil.GetProvider(sett.Value.ProviderName),
                ConnectionString = sett.Value.ConnectionString,
                ReadConnectionString = sett.Value.ReadConnectionString,
                CommandTimeout = sett.Value.CommandTimeout == 0 ? 30 : sett.Value.CommandTimeout,
                InstProvider = string.IsNullOrEmpty(instType) ? DefaultDataInstProvider.Instance
                    : (IDataInstProvider)ReflectionUtil.CreateInstance(Type.GetType(instType))
            };
            if (!ConnectionStringConfigs.TryAdd(config.ConnectionStringName, config))
                throw new Exception("配置中Data:ConnectionStrings:Name重复。Name: " + config.ConnectionStringName);

            if (!string.IsNullOrEmpty(sett.Value.OrmMap))
            {
                foreach (var ns in sett.Value.OrmMap.Split(';'))
                {
                    if (!ConnectionStringNamespaces.TryAdd(ns, config))
                        throw new Exception($"AiUo配置中Data:ConnectionStrings:OrmMap配置重复。name: {sett.Key} ormMap: {ns}");
                }
            }
        }
        if (string.IsNullOrEmpty(DefaultConnectionStringName) && ConnectionStringConfigs.Count == 1)
            DefaultConnectionStringName = ConnectionStringConfigs.First().Key;
    }
    #endregion
}