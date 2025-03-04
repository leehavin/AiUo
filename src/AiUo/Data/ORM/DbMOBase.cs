using System;
using System.Data.Common;
using System.Collections.Concurrent;
using AiUo.Configuration;

namespace AiUo.Data.ORM;

/// <summary>
/// 数据库MO基类
///     DbMOBase => DbObjectMO => DbTableMO/DbViewMO => XXTableMO/XXViewMO
///              => DbProcMO => XXProcMO
/// </summary>
public abstract class DbMOBase<TDatabase, TParameter, TDbType>
    where TDatabase : Database<TParameter, TDbType>
    where TParameter : DbParameter
    where TDbType : struct
{
    // key: Type.FullName value: ConnectionStringConfig
    private static ConcurrentDictionary<string, ConnectionStringConfig> TypeNameDict = new();
    static DbMOBase()
    {
        ConfigUtil.RegisterChangedCallback(() => 
        {
            TypeNameDict.Clear();
        });
    }
    /// <summary>
    /// 当前对象初始化,如果不指定connectionStringName，则使用命名空间过滤查询，而不是用默认
    /// </summary>
    /// <param name="connectionStringName"></param>
    /// <param name="builder"></param>
    public void Init(string connectionStringName, Func<ConnectionStringConfig, TDatabase> builder)
    {
        ConnectionStringConfig config = null;
        // 用户指定 connectionStringName
        if (!string.IsNullOrEmpty(connectionStringName))
        {
            config = DbConfigManager.GetConnectionStringConfig(connectionStringName);
            Database = builder(config);
            return;
        }
        // Cache
        var thisType = GetType();
        var key = thisType.FullName;
        if (TypeNameDict.TryGetValue(key, out config))
        {
            Database = builder(config);
            return;
        }

        // IOrmConnectionRouter
        var section = ConfigUtil.GetSection<DataSection>();
        if (section.OrmConnectionRouter != null)
        {
            var connName = section.OrmConnectionRouter.Route(thisType, SourceName);
            config = DbConfigManager.GetConnectionStringConfig(connName);
            if (config != null)
            {
                TypeNameDict.TryAdd(key, config);
                Database = builder(config);
                return;
            }
        }

        // 命名空间配置
        var ns = thisType.Namespace;
        config = DbConfigManager.GetOrmConnectionStringConfig(ns);
        if (config != null)
        {
            TypeNameDict.TryAdd(key, config);
            Database = builder(config);
            return;
        }

        // 默认数据库
        config = DbConfigManager.GetConnectionStringConfig(null);
        TypeNameDict.TryAdd(key, config);
        Database = builder(config);
    }

    /// <summary>
    /// 数据库对象
    /// </summary>
    public TDatabase Database { get; set; }
    /// <summary> 
    /// 数据提供程序类型
    /// </summary>
    public abstract DbDataProvider Provider { get; }

    /// <summary>
    /// 特定数据库的ORM提供
    /// </summary>
    protected abstract IDbOrmProvider<TDatabase, TParameter, TDbType> OrmProvider { get; }

    /// <summary>
    /// 数据对象类型
    /// </summary>
    public abstract DbObjectType SourceType { get; }

    /// <summary>
    /// 数据对象名称
    /// </summary>
    public abstract string SourceName { get; }

    /// <summary>
    /// 执行SQL时Timeout时间
    /// </summary>
    public int CommandTimeout
    {
        get { return Database.CommandTimeout; }
        set { Database.CommandTimeout = value; }
    }
}