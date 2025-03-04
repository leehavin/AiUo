using System;
using System.Linq;
using System.Data;
using System.Reflection;
using System.Collections.Concurrent;
using AiUo.Reflection;

namespace AiUo.Data.DataMapping;

/// <summary>
/// 通过反射实现 IDataReader ==> Entity 的映射类
/// 如果定义了DataColumnMapperAttribute则使用，否则按照属性名称和数据字段名映射
/// </summary>
internal class ReflectionDataMapping
{
    private static readonly ConcurrentDictionary<Type, ReflectionMapperBuilder> _mapCache = new ConcurrentDictionary<Type, ReflectionMapperBuilder>();
    public static Func<IDataReader, T> GetRowMapper<T>()
    {
        Type type = typeof(T);
        var ret = _mapCache.GetOrAdd(type, new ReflectionMapperBuilder(type));
        return ret.Build<T>;
    }

}
internal class ReflectionMapperBuilder
{
    private Type _type;
    // key: columnName小写
    private ConcurrentDictionary<string, PropertySetter> _columnMapper = new ConcurrentDictionary<string, PropertySetter>();

    public ReflectionMapperBuilder(Type t)
    {
        _type = t;
        PropertyInfo[] properties = (from p in _type.GetProperties()
            where p.CanWrite // && p.IsDefined(typeof(DataColumnMapperAttribute), true)
            select p).ToArray();
        if (properties == null || properties.Length == 0) return;
        foreach (PropertyInfo property in properties)
        {
            DataColumnMapperAttribute attr = (DataColumnMapperAttribute)Attribute.GetCustomAttribute(property, typeof(DataColumnMapperAttribute));
            string key = (attr != null && !string.IsNullOrEmpty(attr.ColumnName)) ? attr.ColumnName.ToLower() : property.Name.ToLower();

            var setter = new PropertySetter()
            {
                Handler = ReflectionUtil.SetPropertyValue,
                Type = property.PropertyType,
                Name = property.Name
            };
            _columnMapper.TryAdd(key, setter);
        }
    }
    public T Build<T>(IDataReader reader)
    {
        object ret = ReflectionUtil.CreateInstance(_type);
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var columnName = reader.GetName(i);
            var key = columnName.ToLower();
            if (_columnMapper.ContainsKey(key))
            {
                var item = _columnMapper[key];
                object value = AiUoUtil.ConvertTo(reader[columnName], item.Type);
                item.Handler.Invoke(ret, item.Name, value);
            }
        }

        return (T)ret;
    }
    /// <summary>
    /// 属性设置类
    /// </summary>
    class PropertySetter
    {
        /// <summary>
        /// 属性设置方法
        /// </summary>
        public Action<object, string, object> Handler { get; set; }
        /// <summary>
        /// 属性类型
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// 属性名
        /// </summary>
        public string Name { get; set; }
    }
}