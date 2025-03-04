using System;
using AiUo.Configuration;
using AiUo.Data.Instrumentation;

namespace AiUo.Data;

/// <summary>
/// AiUo.config中的数据库配置信息
/// </summary>
public static class DbConfigManager
{
    /// <summary>
    /// DataSection
    /// </summary>
    public static DataSection Section
    {
        get
        {
            var section = ConfigUtil.GetSection<DataSection>();
            if (section == null)
                throw new Exception($"AiUo配置中没有Data配置节");
            return section;
        }
    }

    /// <summary>
    /// 判断是否存在指定名称的ConnectionStringName
    /// </summary>
    /// <param name="connectionStringName"></param>
    /// <returns></returns>
    public static bool ExistConnectionStringName(string connectionStringName)
        => Section.ConnectionStringConfigs.ContainsKey(connectionStringName);

    /// <summary>
    /// 根据配置文件中数据库连接字符串名称获得ConnectionStringConfig
    /// </summary>
    /// <param name="connectionStringName">数据库连接字符串名称</param>
    /// <returns></returns>
    public static ConnectionStringConfig GetConnectionStringConfig(string connectionStringName = null)
    {
        connectionStringName = connectionStringName ?? Section.DefaultConnectionStringName;
        if (string.IsNullOrEmpty(connectionStringName))
            throw new ArgumentNullException("connectionStringName");
        if (!Section.ConnectionStringConfigs.TryGetValue(connectionStringName, out ConnectionStringConfig config))
            throw new Exception("配置文件中Data:ConnectionStrings:Name不存在。Name: " + connectionStringName);
        return config;
    }

    /// <summary>
    /// 获得ConnectionStringConfig
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="connectionString"></param>
    /// <param name="readConnectionString"></param>
    /// <param name="commandTimeout"></param>
    /// <param name="inst"></param>
    /// <returns></returns>
    public static ConnectionStringConfig GetConnectionStringConfig(DbDataProvider provider, string connectionString, string readConnectionString, int commandTimeout = 30, IDataInstProvider inst = null)
    {
        return new ConnectionStringConfig() {
            Provider = provider,
            ConnectionString = connectionString,
            ReadConnectionString = readConnectionString,
            CommandTimeout = commandTimeout,
            InstProvider = inst ?? DefaultDataInstProvider.Instance
        };
    }

    /// <summary>
    /// 尝试获取连接配置信息
    /// </summary>
    /// <param name="connectionStringName"></param>
    /// <param name="provider"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static bool TryGetConnectionStringConfig(string connectionStringName, DbDataProvider provider, out ConnectionStringConfig config)
    {
        config = null;
        connectionStringName = connectionStringName ?? Section.DefaultConnectionStringName;
        if (string.IsNullOrEmpty(connectionStringName)) // 空并且没有默认值
            return false;
        if (connectionStringName.Length > 30) // 长度大于30当做连接字符串处理
        {
            config = GetConnectionStringConfig(provider, connectionStringName, null, 30, null);
            return true;
        }
        if (!Section.ConnectionStringConfigs.TryGetValue(connectionStringName, out config)) // 配置文件中不存在
            return false;
        return true;
    }

    /// <summary>
    /// 根据命名空间获取配置信息,不存在映射取默认数据库连接
    /// </summary>
    /// <param name="ns"></param>
    /// <returns></returns>
    public static ConnectionStringConfig GetOrmConnectionStringConfig(string ns)
    {
        if (!Section.ConnectionStringNamespaces.TryGetValue(ns, out ConnectionStringConfig config))
            config = GetConnectionStringConfig();
        //throw new Exception($"AiUo配置中Data:ConnectionStrings:OrmMap不包含此namespace。Namespace:{ns}");
        return config;
    }
}