using SqlSugar;
using System.Collections.Concurrent;
using System.Reflection;
using AiUo.Collections;
using AiUo.Configuration;

namespace AiUo.Data.SqlSugar;

public interface IDbSplitProvider
{
    /// <summary>
    /// 分库(根据类型和分库标识)
    ///     1) 按上下文分库，如：登录用户的集团ID
    ///     2) 按指定值分库，如splitDbKey参数传入合作商ID
    ///     3) 按指定表和值分库，即T+splitDbKey
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    string SplitDb<T>(object splitDbKey = null);

    /// <summary>
    /// 分表(根据实体类型和表数据)
    /// </summary>
    /// <returns></returns>
    ISplitTableService SplitTable();
}
public class DefaultSplitProvider : IDbSplitProvider
{
    private SqlSugarSection _section;
    private ConcurrentDictionary<string, string> _tableDict = new();
    private ConcurrentDictionary<Type, string> _typeDict = new();

    public DefaultSplitProvider()
    {
        _section = ConfigUtil.GetSection<SqlSugarSection>();
        if (_section.ConnectionStrings.Count > 1)
        {
            var dict = new Dictionary<string, DbObjItem>();
            _section.ConnectionStrings.Values.ForEach(conn =>
            {
                var db = new SqlSugarClient(conn);
                db.DbMaintenance.GetTableInfoList().ForEach(t =>
                {
                    if (dict.TryGetValue(t.Name, out var v))
                        v.OnlyOne = false;
                    else
                        dict.Add(t.Name, new DbObjItem(t.Name, conn.ConfigId.ToString()));
                });
                db.DbMaintenance.GetViewInfoList().ForEach(t =>
                {
                    if (dict.TryGetValue(t.Name, out var v))
                        v.OnlyOne = false;
                    else
                        dict.Add(t.Name, new DbObjItem(t.Name, conn.ConfigId.ToString()));
                });
            });
            dict.ForEach(item =>
            {
                if (item.Value.OnlyOne)
                    _tableDict.TryAdd(item.Key, item.Value.ConfigId);
            });
        }
    }
    public string SplitDb<T>(object splitDbKey = null)
    {
        var type = typeof(T);
        if (_typeDict.TryGetValue(type, out var ret))
            return ret;

        // 1) Object类型或者连接唯一
        if (type == typeof(object) || _section.ConnectionStrings.Count == 1)
        {
            _typeDict.TryAdd(type, _section.DefaultConnectionStringName);
            return null;
        }

        // 2) NamespacesMaps
        if (_section.NamespaceMappings.TryGetValue(type.Namespace!, out ret))
        {
            _typeDict.TryAdd(type, ret);
            return ret;
        }

        // 3) 数据库唯一
        var tableName = type.GetCustomAttribute<SugarTable>()?.TableName;
        if (!string.IsNullOrEmpty(tableName) && _tableDict.TryGetValue(tableName, out ret))
        {
            _typeDict.TryAdd(type, ret);
            return ret;
        }

        // 4) SugarConfigIdAttribute
        var configId = type.GetCustomAttribute<SugarConfigIdAttribute>()?.ConfigId;
        if (!string.IsNullOrEmpty(configId) && _section.ConnectionStrings.ContainsKey(configId))
        {
            _typeDict.TryAdd(type, configId);
            return configId;
        }

        throw new Exception($"DefaultSplitProvider.SplitDb()时，无法确定SqlSugar实体类型映射连接。type: {type.FullName}");
    }

    public ISplitTableService SplitTable()
    {
        return null;
    }
    class DbObjItem
    {
        public string Name { get; set; }
        public string ConfigId { get; set; }
        public bool OnlyOne { get; set; }
        public DbObjItem(string name, string configId)
        {
            Name = name;
            ConfigId = configId;
            OnlyOne = true;
        }
    }
}