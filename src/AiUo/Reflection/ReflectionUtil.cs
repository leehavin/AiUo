using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections;
using System.Linq.Expressions;
using System.Collections.Concurrent;
using AiUo.IO;

namespace AiUo.Reflection;

/// <summary>
/// 反射辅助方法类
/// </summary>
public static class ReflectionUtil
{
    #region AssemblyInfo.cs
    /// <summary>
    /// 获取AssemblyInfo.cs中定义的Product
    /// </summary>
    /// <param name="asm"></param>
    /// <returns></returns>
    public static AssemblyProductAttribute GetAssemblyProduct(Assembly asm)
        => asm.GetCustomAttribute<AssemblyProductAttribute>();

    /// <summary>
    /// 获取AssemblyInfo.cs中定义的版本信息
    /// </summary>
    /// <param name="asm"></param>
    /// <returns></returns>
    public static Version GetAssemblyVersion(Assembly asm)
        => asm.GetName().Version;

    /// <summary>
    /// 获取指定Assembly的GUID
    /// </summary>
    /// <param name="asm"></param>
    /// <returns></returns>
    public static string GetAssemblyGuidString(Assembly asm)
    {
        string ret = null;
        GuidAttribute[] attrs = (GuidAttribute[])asm.GetCustomAttributes(typeof(GuidAttribute), false);
        if (attrs != null && attrs.Length > 0)
            ret = attrs[0].Value;
        return ret;
    }
    #endregion

    #region PrimitiveType & SimpleType & MapToJsType
    /// <summary>
    /// 类型是否是基元类型
    /// https://msdn.microsoft.com/en-us/library/system.type.isprimitive.aspx
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsPrimitiveType(Type type)
        => SimpleTypeNames.PrimitiveTypes.Contains(type.FullName);

    /// <summary>
    /// 简单类型：基元类型 + TimeSpan + DateTime + Guid + Decimal + String + Byte[] + 任何可从字符串转入的对象（暂未加入判断）
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsSimpleType(Type type)
        => SimpleTypeNames.PrimitiveTypes.Contains(type.FullName) || SimpleTypeNames.SimpleTypes.Contains(type.FullName);

    // .Net简单类型 => JS类型 映射缓存
    private static readonly Dictionary<string, string> _jsTypeMapCache = new Dictionary<string, string>() {
        { "System.Sbyte", "Number" },
        { "System.Byte", "Number"},
        { "System.Int16", "Number"},
        { "System.UInt16", "Number"},
        { "System.Int32", "Number"},
        { "System.UInt32", "Number"},
        { "System.Int64", "Number"},
        { "System.UInt64", "Number"},
        { "System.Single", "Number"},
        { "System.Double", "Number"},
        { "System.Decimal", "Number"},
        { "System.Boolean", "Boolean"},
        { "System.Char", "String"},
        { "System.String", "String"},
        { "System.DateTime", "String"},
        { "System.TimeSpan", "String"},
        { "System.Enum", "String"},
        { "System.Guid", "String"},
        { "System.Object", "Object"}
    };

    /// <summary>
    /// 获得.NET类型映射的JS类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string MapToJsType(Type type)
    {
        string ret = null;
        string key = type.FullName;
        if (_jsTypeMapCache.ContainsKey(key))
            return _jsTypeMapCache[key];
        if (typeof(ICollection).IsAssignableFrom(type) || typeof(IEnumerable<>).IsAssignableFrom(type))
            return "Array";
        return ret;
    }
    #endregion

    #region CreateInstance
    internal delegate T ObjectActivator<out T>(params object[] args);
    public static Delegate GetActivator<T>(ConstructorInfo ctor)
    {
        Type type = ctor.DeclaringType;
        ParameterInfo[] paramsInfo = ctor.GetParameters();

        //create a single param of type object[]
        ParameterExpression param =
            Expression.Parameter(typeof(object[]), "args");

        var argsExp =
            new Expression[paramsInfo.Length];

        //pick each arg from the params array 
        //and create a typed expression of them
        for (int i = 0; i < paramsInfo.Length; i++)
        {
            Expression index = Expression.Constant(i);
            Type paramType = paramsInfo[i].ParameterType;

            Expression paramAccessorExp =
                Expression.ArrayIndex(param, index);

            Expression paramCastExp =
                Expression.Convert(paramAccessorExp, paramType);

            argsExp[i] = paramCastExp;
        }

        //make a NewExpression that calls the
        //ctor with the args we just created
        NewExpression newExp = Expression.New(ctor, argsExp);

        //create a lambda with the New
        //Expression as body and our param object[] as arg
        LambdaExpression lambda =
            Expression.Lambda(typeof(ObjectActivator<T>), newExp, param);

        //compile it
        return lambda.Compile();
    }
    // 构造函数缓存
    private static readonly ConcurrentDictionary<string, Delegate> ObjectActivators = new ConcurrentDictionary<string, Delegate>();
    public static object CreateInstance(Type type, params object[] args)
    {
        if (!ObjectActivators.TryGetValue(type.FullName, out Delegate activator))
        {
            var constructors = type.GetConstructors();
            if (constructors.Count() == 1)
                activator = GetActivator<object>(constructors.First());
            ObjectActivators.AddOrUpdate(type.FullName, activator, (k, v) => v);
        }
        return (activator == null)
            ? Activator.CreateInstance(type, args)
            : ((ObjectActivator<object>)activator)(args);
    }
    /// <summary>
    /// 根据创建类实例(只有一个构造函数时使用lambda表达式)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T CreateInstance<T>(params object[] args)
        => (T)CreateInstance(typeof(T), args);
    /// <summary>
    /// 根据类型名称创建实例
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static object CreateInstance(string typeName, params object[] args)
        => CreateInstance(Type.GetType(typeName), args);
    #endregion

    #region InvokeMethod
    public static object InvokeMethod(string invokeType, string invokeMethod, params object[] args)
    {
        var type = Type.GetType(invokeType);
        var method = type.GetMethod(invokeMethod);
        var obj = Activator.CreateInstance(type);
        return method.Invoke(obj, args);
    }
    public static object InvokeStaticMethod(string invokeType, string invokeMethod, params object[] args)
    {
        var type = Type.GetType(invokeType);
        var method = type.GetMethod(invokeMethod);
        return method.Invoke(null, args);
    }
    public static object InvokeStaticGenericMethod(string invokeType, string invokeMethod, string genericType, params object[] args)
    {
        var itype = Type.GetType(invokeType);
        var gtype = Type.GetType(genericType);
        return InvokeStaticGenericMethod(itype, invokeMethod, gtype, args);
    }
    public static object InvokeStaticGenericMethod(Type invokeType, string invokeMethod, Type genericType, params object[] args)
    {
        var method = invokeType.GetMethod(invokeMethod);
        var gmethod = method.MakeGenericMethod(genericType);
        return gmethod.Invoke(null, args);
    }
    #endregion

    #region GetPropertyValue
    private static readonly ConcurrentDictionary<PropertyInfo, MethodInfo> _propertyGetterCache = new ConcurrentDictionary<PropertyInfo, MethodInfo>();
    /// <summary>
    /// 通过反射获取对象属性值
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    public static object GetPropertyValue(object obj, PropertyInfo property)
    {
        if (!_propertyGetterCache.TryGetValue(property, out MethodInfo ret))
        {
            ret = property.GetGetMethod();
            _propertyGetterCache.TryAdd(property, ret);
        }
        return ret.Invoke(obj, null);
    }

    private static readonly ConcurrentDictionary<string, MethodInfo> _propertyNameGetterCache = new ConcurrentDictionary<string, MethodInfo>();
    /// <summary>
    /// 通过反射获取对象属性值
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static object GetPropertyValue(object obj, string propertyName)
    {
        if (!TryGetPropertyValue(obj, propertyName, out var value))
            throw new Exception($"ReflectionUtil.GetPropertyValue时没有属性。type:{obj.GetType().FullName} propertyName:{propertyName}");
        return value;
    }
    public static bool TryGetPropertyValue(object obj, string propertyName, out object value)
    {
        value = null;
        var key = $"{obj.GetType().FullName}:{propertyName}";
        if (!_propertyNameGetterCache.TryGetValue(key, out MethodInfo ret))
        {
            var property = obj.GetType().GetProperty(propertyName);
            ret = property != null ? property.GetGetMethod() : null;
            _propertyNameGetterCache.TryAdd(key, ret);
        }
        value = ret?.Invoke(obj, null);
        return ret != null;
    }
    /// <summary>
    /// 通过反射获取对象属性值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static T GetPropertyValue<T>(object obj, string propertyName)
        => AiUoUtil.ConvertTo<T>(GetPropertyValue(obj, propertyName));
    public static bool TryGetPropertyValue<T>(object obj, string propertyName, out T value)
    {
        value = default;
        if (!TryGetPropertyValue(obj, propertyName, out object v))
            return false;
        value = AiUoUtil.ConvertTo<T>(v);
        return true;
    }
    #endregion

    #region SetPropertyValue
    private static readonly ConcurrentDictionary<PropertyInfo, MethodInfo> _propertySetterCache = new ConcurrentDictionary<PropertyInfo, MethodInfo>();
    /// <summary>
    /// 通过反射设置对象属性值
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="property"></param>
    /// <param name="value"></param>
    public static void SetPropertyValue(object obj, PropertyInfo property, object value)
    {
        if (!_propertySetterCache.TryGetValue(property, out MethodInfo ret))
        {
            ret = property.GetSetMethod();
            _propertySetterCache.TryAdd(property, ret);
        }
        ret.Invoke(obj, new object[] { value });
    }
    private static readonly ConcurrentDictionary<string, MethodInfo> _propertyNameSetterCache = new ConcurrentDictionary<string, MethodInfo>();
    /// <summary>
    /// 通过反射设置对象属性值
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    public static void SetPropertyValue(object obj, string propertyName, object value)
    {
        var key = $"{obj.GetType().FullName}:{propertyName}";
        if (!_propertyNameSetterCache.TryGetValue(key, out MethodInfo ret))
        {
            var property = obj.GetType().GetProperty(propertyName);
            ret = property.GetSetMethod();
            _propertyNameSetterCache.TryAdd(key, ret);
        }
        ret.Invoke(obj, new object[] { value });
    }
    #endregion

    #region GetProperties
    private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _propertyCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
    public static Dictionary<string, PropertyInfo> GetPropertyDic<T>()
        where T : new()
    {
        var type = typeof(T);
        if (!_propertyCache.TryGetValue(type, out Dictionary<string, PropertyInfo> ret))
        {
            ret = new Dictionary<string, PropertyInfo>();
            foreach (var p in type.GetProperties())
            {
                ret.Add(p.Name, p);
            }
            _propertyCache.Add(type, ret);
        }
        return ret;
    }
    #endregion

    /// <summary>
    /// 获取文本类型的嵌入资源
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="name"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string GetManifestResourceString(Assembly assembly, string name, Encoding encoding = null)
    {
        var stream = assembly.GetManifestResourceStream(name);
        using (var reader = new StreamReader(stream, encoding ?? Encoding.UTF8))
            return reader.ReadToEnd();
    }
    /// <summary>
    /// 保存嵌入资源文件到指定目录
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="name"></param>
    /// <param name="path"></param>
    /// <param name="overwrite"></param>
    public static void SaveManifestResourceFileToDir(Assembly assembly, string name, string path, bool overwrite = false)
    {
        var names = name.Split('.');
        var file = $"{names[names.Length - 2]}.{names[names.Length - 1]}";
        var target = Path.Combine(path, file);
        SaveManifestResourceFileToFile(assembly, name, target, overwrite);
    }
    public static void SaveManifestResourceFileToFile(Assembly assembly, string name, string target, bool overwrite = false)
    {
        if (File.Exists(target) && !overwrite)
            return;
        var stream = assembly.GetManifestResourceStream(name);
        IOUtil.WriteStreamToFile(stream, target);
    }
    /// <summary>
    /// 判断类型type是否继承自某泛型类
    /// </summary>
    /// <param name="type"></param>
    /// <param name="generic"></param>
    /// <returns></returns>
    public static bool IsSubclassOfGeneric(this Type type, Type generic)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (generic == null) throw new ArgumentNullException(nameof(generic));
        // 测试接口。
        var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);
        if (isTheRawGenericType) return true;
        // 测试类型。
        while (type != null && type != typeof(object))
        {
            isTheRawGenericType = IsTheRawGenericType(type);
            if (isTheRawGenericType) return true;
            type = type.BaseType;
        }

        // 没有找到任何匹配的接口或类型。
        return false;

        // 测试某个类型是否是指定的原始接口。
        bool IsTheRawGenericType(Type test)
            => generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
    }

    /// <summary>
    /// 是否是C#内置类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsBulitinType(Type type)
    {
        return type.FullName.StartsWith("System.");
        //return (type == typeof(object) || Type.GetTypeCode(type) != TypeCode.Object);
    }
}