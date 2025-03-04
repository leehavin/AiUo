using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace AiUo.Reflection;

/// <summary>
/// 动态对象构建器，比反射快
/// </summary>
public static class DynamicCompiler
{
    private static object _locker = new object();
    private static readonly Dictionary<Type, Func<object>> _builderCache = new Dictionary<Type, Func<object>>();

    #region Builder
    /// <summary>
    /// 创建构建对象代理
    /// </summary>
    /// <param name="type">类型信息</param>
    /// <param name="useCache">是否使用缓存</param>
    /// <returns></returns>
    public static Func<object> CreateBuilder(Type type, bool useCache = true)
    {
        Func<object> ret = null;
        if (useCache)
        {
            if (!_builderCache.ContainsKey(type))
            {
                lock (_locker)
                {
                    if (!_builderCache.ContainsKey(type))
                    {
                        _builderCache.Add(type, CompilerBuilder(type));
                    }
                }
            }
            ret = _builderCache[type];
        }
        else
        {
            ret = CompilerBuilder(type);
        }
        return ret;
    }

    /// <summary>
    /// 创建构建对象代理
    /// </summary>
    /// <param name="typeName">类型信息</param>
    /// <param name="useCache">是否使用缓存</param>
    /// <returns></returns>
    public static Func<object> CreateBuilder(string typeName, bool useCache = true)
        => CreateBuilder(Type.GetType(typeName, true), useCache);
    #endregion

    #region Getter
    /// <summary>
    /// 创建获得属性值的代理
    /// </summary>
    /// <param name="type">类型信息</param>
    /// <param name="propertyInfo">属性元数据</param>
    /// <returns></returns>
    public static Func<object, object> CreateGetter(Type type, PropertyInfo propertyInfo)
    {
        MethodInfo getMethodInfo = propertyInfo.GetGetMethod(true);
        DynamicMethod dynamicGet = CreateGetDynamicMethod(type);
        ILGenerator getGenerator = dynamicGet.GetILGenerator();

        getGenerator.Emit(OpCodes.Ldarg_0);
        getGenerator.Emit(OpCodes.Call, getMethodInfo);
        BoxIfNeeded(getMethodInfo.ReturnType, getGenerator);
        getGenerator.Emit(OpCodes.Ret);

        return (Func<object, object>)dynamicGet.CreateDelegate(typeof(Func<object, object>));
    }

    /// <summary>
    /// 创建获得字段值的代理
    /// </summary>
    /// <param name="type">类型信息</param>
    /// <param name="fieldInfo">字段元数据</param>
    /// <returns></returns>
    public static Func<object, object> CreateGetter(Type type, FieldInfo fieldInfo)
    {
        DynamicMethod dynamicGet = CreateGetDynamicMethod(type);
        ILGenerator getGenerator = dynamicGet.GetILGenerator();

        getGenerator.Emit(OpCodes.Ldarg_0);
        getGenerator.Emit(OpCodes.Ldfld, fieldInfo);
        BoxIfNeeded(fieldInfo.FieldType, getGenerator);
        getGenerator.Emit(OpCodes.Ret);

        return (Func<object, object>)dynamicGet.CreateDelegate(typeof(Func<object, object>));
    }
    #endregion

    #region Setter
    /// <summary>
    /// 创建设置属性值的代理
    /// </summary>
    /// <param name="type">类型信息</param>
    /// <param name="propertyInfo">属性元数据</param>
    /// <returns></returns>
    public static Action<object, object> CreateSetter(Type type, PropertyInfo propertyInfo)
    {
        MethodInfo setMethodInfo = propertyInfo.GetSetMethod(true);
        DynamicMethod dynamicSet = CreateSetDynamicMethod(type);
        ILGenerator setGenerator = dynamicSet.GetILGenerator();

        setGenerator.Emit(OpCodes.Ldarg_0);
        setGenerator.Emit(OpCodes.Ldarg_1);
        UnboxIfNeeded(setMethodInfo.GetParameters()[0].ParameterType, setGenerator);
        setGenerator.Emit(OpCodes.Call, setMethodInfo);
        setGenerator.Emit(OpCodes.Ret);

        return (Action<object, object>)dynamicSet.CreateDelegate(typeof(Action<object, object>));
    }

    /// <summary>
    /// 创建设置字段值的代理
    /// </summary>
    /// <param name="type">类型信息</param>
    /// <param name="fieldInfo">字段元数据</param>
    /// <returns></returns>
    public static Action<object, object> CreateSetter(Type type, FieldInfo fieldInfo)
    {
        DynamicMethod dynamicSet = CreateSetDynamicMethod(type);
        ILGenerator setGenerator = dynamicSet.GetILGenerator();

        setGenerator.Emit(OpCodes.Ldarg_0);
        setGenerator.Emit(OpCodes.Ldarg_1);
        UnboxIfNeeded(fieldInfo.FieldType, setGenerator);
        setGenerator.Emit(OpCodes.Stfld, fieldInfo);
        setGenerator.Emit(OpCodes.Ret);

        return (Action<object, object>)dynamicSet.CreateDelegate(typeof(Action<object, object>));
    }
    #endregion

    #region Utils
    //编译Builder
    private static Func<object> CompilerBuilder(Type type)
    {
        ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null);
        if (constructorInfo == null)
        {
            throw new ApplicationException(string.Format("必须存在空参数的构造函数。", type));
        }

        DynamicMethod dynamicMethod = new DynamicMethod("Instantiate_Object", MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(object), null, type, true);
        ILGenerator generator = dynamicMethod.GetILGenerator();
        generator.Emit(OpCodes.Newobj, constructorInfo);
        generator.Emit(OpCodes.Ret);

        return (Func<object>)dynamicMethod.CreateDelegate(typeof(Func<object>));
    }

    // CreateGetDynamicMethod
    private static DynamicMethod CreateGetDynamicMethod(Type type)
        => new DynamicMethod("Dynamic_Get", typeof(object), new Type[] { typeof(object) }, type, true);

    // CreateSetDynamicMethod
    private static DynamicMethod CreateSetDynamicMethod(Type type)
        => new DynamicMethod("Dynamic_Set", typeof(void), new Type[] { typeof(object), typeof(object) }, type, true);

    // BoxIfNeeded
    private static void BoxIfNeeded(Type type, ILGenerator generator)
    {
        if (type.IsValueType)
        {
            generator.Emit(OpCodes.Box, type);
        }
    }

    // UnboxIfNeeded
    private static void UnboxIfNeeded(Type type, ILGenerator generator)
    {
        if (type.IsValueType)
        {
            generator.Emit(OpCodes.Unbox_Any, type);
        }
    }
    #endregion
}