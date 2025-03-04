using System;
using System.Collections.Generic;

namespace AiUo.Reflection;

/// <summary>
/// 设置对象指定属性名的属性值的类
/// </summary>
public interface IObjectPropertySetter
{
    /// <summary>
    /// 设置对象指定属性名的属性值
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    void SetValue(string propertyName, object value);
}
/// <summary>
/// 设置对象指定属性名的属性值的类
/// </summary>
public class ObjectPropertySetter : IObjectPropertySetter
{
    private static HashSet<Type> _initTypes = new HashSet<Type>();
    private static object _locker = new object();
    private static Dictionary<string, PropertySetter> _propertyMapper = new Dictionary<string, PropertySetter>();

    private object _obj;
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="obj"></param>
    public ObjectPropertySetter(object obj)
    {
        _obj = obj;
    }

    private static string GetKey(Type type, string propertyName)
    {
        return string.Format("{0}_{1}", type.FullName, propertyName);
    }
    private static void Init(Type type)
    {
        if (!_initTypes.Contains(type))
        {
            lock (_locker)
            {
                if (!_initTypes.Contains(type))
                {
                    foreach (var property in type.GetProperties())
                    {
                        var setter = new PropertySetter()
                        {
                            SetValueHandler = DynamicCompiler.CreateSetter(type, property),
                            PropertyType = property.PropertyType
                        };
                        string key = GetKey(type, property.Name);
                        _propertyMapper.Add(key, setter);
                    }
                    _initTypes.Add(type);
                }
            }
        }
    }
    /// <summary>
    /// 设置对象指定属性名的属性值
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    public static void SetValue(object obj, string propertyName, object value)
    {
        Type type = obj.GetType();
        Init(type);
        string key = GetKey(type, propertyName);
        if (_propertyMapper.ContainsKey(key))
        {
            var setter = _propertyMapper[key];
            object resultValue = AiUoUtil.ConvertTo(value, setter.PropertyType);
            setter.SetValueHandler.Invoke(obj, resultValue);
        }
        else
            throw new ArgumentException(string.Format("对象属性赋值时，属性名{0}不存在", propertyName), "propertyName");
    }
    /// <summary>
    /// 设置对象指定属性名的属性值
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    public void SetValue(string propertyName, object value)
    {
        SetValue(_obj, propertyName, value);
    }

    class PropertySetter
    {
        public Action<object, object> SetValueHandler { get; set; }
        public Type PropertyType { get; set; }
    }
}