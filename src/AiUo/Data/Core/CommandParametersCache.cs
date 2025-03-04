using System;
using System.Data;
using System.Collections.Concurrent;

namespace AiUo.Data;

/// <summary>
/// 缓存程序中所有Command对象执行时所定义的参数集合
/// Key：ConnectionString|CommandText
/// </summary>
internal class CommandParametersCache
{
    private ConcurrentDictionary<string, IDataParameter[]> _parametersCache = new ConcurrentDictionary<string, IDataParameter[]>();

    /// <devdoc>
    /// 添加一个参数定义缓存
    /// </devdoc>        
    public void Set(CommandWrapper command, Database database)
    {
        string key = GetKey(command);
        IDataParameter[] value = null;
        if (command.Parameters.Count > 0)
        {
            value = new IDataParameter[command.Parameters.Count];
            command.Parameters.CopyTo(value, 0);
        }
        _parametersCache.TryAdd(key, value);
    }

    /// <devdoc>
    /// 获取一个缓存集合
    /// </devdoc>        
    public IDataParameter[] Get(CommandWrapper command, Database database)
    {
        string key = GetKey(command);
        if (!_parametersCache.TryGetValue(key, out IDataParameter[] value))
            throw new Exception($"Parameters缓存中不存在此Key: {key}");
        return CloneParameters(value, database);
    }

    /// <devdoc>
    /// 是否存在此缓存项
    /// </devdoc>        
    public bool Contains(CommandWrapper command)
    {
        string key = GetKey(command);
        return _parametersCache.ContainsKey(key);
        //return _cache.ContainsKey(key);
    }

    /// <summary>
    /// 清除缓存
    /// </summary>
    public void Clear()
    {
        _parametersCache.Clear();
        //this._cache.Clear();
    }

    //拷贝一个IDataParameter数组副本
    private static IDataParameter[] CloneParameters(IDataParameter[] originalParameters, Database database)
    {
        if (originalParameters == null) return null;
        IDataParameter[] ret = new IDataParameter[originalParameters.Length];
        for (int i = 0, j = originalParameters.Length; i < j; i++)
        {
            ret[i] = database.CloneParameter(originalParameters[i]);
        }
        return ret;
    }

    private static string GetKey(CommandWrapper command)
    {
        // Connection有事务的情况下未被创建
        return $"{command.ConnectionString}|{command.CommandText}";
    }
}