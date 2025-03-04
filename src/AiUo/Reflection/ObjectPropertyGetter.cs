using System;
using System.Collections.Generic;

namespace AiUo.Reflection;

/// <summary>
/// 对象属性获取器
/// </summary>
public interface IObjectPropertyGetter
{
    /// <summary>
    /// 获取对象指定属性名的属性值
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    object GetValue(string propertyName);
    /// <summary>
    /// 获取对象指定属性名的属性值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    T GetValue<T>(string propertyName);
}

/// <summary>
/// 对象属性获取器，用于获取对象指定属性名的属性值
/// </summary>
public class ObjectPropertyGetter : IObjectPropertyGetter
{
    private static HashSet<Type> _initTypes = new HashSet<Type>();
    private static object _locker = new object();
    private static Dictionary<string, Func<object, object>> _propertyMapper = new Dictionary<string, Func<object, object>>();

    private object _obj;
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="obj"></param>
    public ObjectPropertyGetter(object obj)
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
                        string key = GetKey(type, property.Name);
                        _propertyMapper.Add(key, DynamicCompiler.CreateGetter(type, property));
                    }
                    _initTypes.Add(type);
                }
            }
        }
    }

    /// <summary>
    /// 获取对象指定属性名的属性值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static T GetValue<T>(object obj, string propertyName)
    {
        return AiUoUtil.ConvertTo<T>(GetValue(obj, propertyName));
    }
    /// <summary>
    /// 获取对象指定属性名的属性值
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static object GetValue(object obj, string propertyName)
    {
        object ret = null;
        Type type = obj.GetType();
        Init(type);
        string key = GetKey(type, propertyName);
        if (_propertyMapper.ContainsKey(key))
        {
            var getter = _propertyMapper[key];
            ret = getter.Invoke(obj);
        }
        else
            throw new ArgumentException(string.Format("获取对象属性值时，属性名{0}不存在", propertyName), "propertyName");
        return ret;
    }

    /// <summary>
    /// 获取对象指定属性名的属性值
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public object GetValue(string propertyName)
    {
        return GetValue(_obj, propertyName);
    }
    /// <summary>
    /// 获取对象指定属性名的属性值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public T GetValue<T>(string propertyName)
    {
        return GetValue<T>(_obj, propertyName);
    }
}