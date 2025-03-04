using System;
using System.Linq;
using System.Data;
using System.Collections.Concurrent;
using AiUo.Reflection;

namespace AiUo.Data.DataMapping;

/// <summary>
/// 通过 IRowMapper 接口定义实现 IDataReader ==> Entity 的映射类
/// </summary>
internal class InterfaceDataMapping
{
    private static readonly ConcurrentDictionary<Type, InterfaceMapperBuilder> _builderCache = new ConcurrentDictionary<Type, InterfaceMapperBuilder>();
    /// <summary>
    /// 获取IDataReader映射到实体类的构建方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Func<IDataReader, T> GetRowMapper<T>()
    {
        Type type = typeof(T);
        var ret = _builderCache.GetOrAdd(type, (t) =>
        {
            return t.GetInterfaces().Contains(typeof(IRowMapper<T>))
                ? new InterfaceMapperBuilder(type) : null;
        });
        return ret != null ? ret.Build<T> : null;
    }
}
internal class InterfaceMapperBuilder
{
    private object _mapper;
    public InterfaceMapperBuilder(Type t)
    {
        _mapper = ReflectionUtil.CreateInstance(t);
    }
    public T Build<T>(IDataReader reader)
    {
        IRowMapper<T> ret = (IRowMapper<T>)_mapper;
        return ret.MapRow(reader);
    }
}