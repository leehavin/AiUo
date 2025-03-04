using System;
using System.Collections.ObjectModel;

namespace AiUo.Data.Schema;

/// <summary>
/// Schema集合
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
public class SchemaCollection<T> : KeyedCollection<string, T>
    where T : ISchemaCollectionKey
{
    /// <summary>
    /// 获取Item中的key
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override string GetKeyForItem(T item)
    {
        return item.GetKey();
    }
    /// <summary>
    /// 是否包含
    /// </summary>
    /// <param name="key"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public bool Contains(string key, bool ignoreCase)
    {
        if (!ignoreCase) return Contains(key);
        key = key.ToLower();
        foreach (var item in Items)
            if (item.GetKey().ToLower() == key)
                return true;
        return false;
    }
    /// <summary>
    /// 忽略大小写获得Schema
    /// </summary>
    /// <param name="key"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public T GetSchema(string key, bool ignoreCase = true)
    {
        if (!ignoreCase)
            return this[key];
        key = key.ToLower();
        foreach (var item in Items)
        {
            if (item.GetKey().ToLower() == key)
                return item;
        }
        throw new Exception($"没有找到对应的key: {key}");
    }
}

/// <summary>
/// 定义Schema在集合SchemaColletion时的主键字段
/// </summary>
public interface ISchemaCollectionKey
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    string GetKey();
}