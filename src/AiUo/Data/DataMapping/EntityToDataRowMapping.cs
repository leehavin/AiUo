using System;
using System.Linq;
using System.Data;
using System.Reflection;
using System.Collections.Concurrent;
using AiUo.Reflection;

namespace AiUo.Data.DataMapping;

/// <summary>
/// 实现 Entity ==> DataRow 的映射类，忽略大小写
/// </summary>
internal class EntityToDataRowMapping
{
    private static readonly ConcurrentDictionary<Type, EntityToDataRowMapperBuilder> _cache = new ConcurrentDictionary<Type, EntityToDataRowMapperBuilder>();

    /// <summary>
    /// 获得实体类T到DataRow的映射方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Action<DataRow, T> GetEntityMapper<T>()
    {
        Type type = typeof(T);
        var ret = _cache.GetOrAdd(type, new EntityToDataRowMapperBuilder(type));
        return ret.SetValues<T>;
    }
}

internal class EntityToDataRowMapperBuilder
{
    private Type _type;
    // key: 小写属性名 
    private ConcurrentDictionary<string, (string propertyName, Func<object, string, object> action)> _mapperCache = new ConcurrentDictionary<string, (string propertyName, Func<object, string, object> action)>();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="type"></param>
    public EntityToDataRowMapperBuilder(Type type)
    {
        _type = type;
        var properties = (from p in _type.GetProperties()
            select p).ToArray<PropertyInfo>();
        foreach (var property in properties)
        {
            var mapper = new Func<object, string, object>(ReflectionUtil.GetPropertyValue);
            _mapperCache.TryAdd(property.Name.ToLower(), (property.Name, mapper));
        }
    }
    /// <summary>
    /// 设置DataRow值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="row"></param>
    /// <param name="item"></param>
    public void SetValues<T>(DataRow row, T item)
    {
        for (int i = 0; i < row.Table.Columns.Count; i++)
        {
            string column = row.Table.Columns[i].ColumnName;
            string name = column.ToLower();
            if (_mapperCache.ContainsKey(name))
            {
                var cacheItem = _mapperCache[name];
                object value = cacheItem.action.Invoke(item, cacheItem.propertyName);
                row.SetValue(column, value);
            }
        }
    }
}