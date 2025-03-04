namespace AiUo.Extensions.AutoMapper;

/// <summary>
/// 当前对象映射到T对象的接口，不实现MapTo则使用AutoMapper默认映射方法
/// Entry ==> T
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IMapTo<T>
{
    /// <summary>
    /// 实现当前对象映射到T对象destination
    /// </summary>
    /// <param name="destination"></param>
    void MapTo(T destination);
}

/// <summary>
/// 当前对象映射到T1,T2对象的接口，不实现MapTo则使用AutoMapper默认映射方法
/// Entry ==> T
/// </summary>
/// <typeparam name="T1"></typeparam>
/// <typeparam name="T2"></typeparam>
public interface IMapTo<T1, T2>: IMapTo<T1>
{
    /// <summary>
    /// 实现当前对象映射到T1对象destination
    /// </summary>
    /// <param name="destination"></param>
    void MapTo(T2 destination);
}

/// <summary>
/// 当前对象映射到T1,T2,T3对象的接口，不实现MapTo则使用AutoMapper默认映射方法
/// Entry ==> T
/// </summary>
/// <typeparam name="T1"></typeparam>
/// <typeparam name="T2"></typeparam>
/// <typeparam name="T3"></typeparam>
public interface IMapTo<T1, T2, T3>: IMapTo<T1, T2>
{
    /// <summary>
    /// 实现当前对象映射到T3对象destination
    /// </summary>
    /// <param name="destination"></param>
    void MapTo(T3 destination);
}

/// <summary>
/// 当前对象映射到T1,T2,T3,T4对象的接口，不实现MapTo则使用AutoMapper默认映射方法
/// Entry ==> T
/// </summary>
/// <typeparam name="T1"></typeparam>
/// <typeparam name="T2"></typeparam>
/// <typeparam name="T3"></typeparam>
/// <typeparam name="T4"></typeparam>
public interface IMapTo<T1, T2, T3, T4>: IMapTo<T1, T2, T3>
{
    /// <summary>
    /// 实现当前对象映射到T4对象destination
    /// </summary>
    /// <param name="destination"></param>
    void MapTo(T4 destination);
}

/// <summary>
/// 当前对象映射到T1,T2,T3,T4,T5对象的接口，不实现MapTo则使用AutoMapper默认映射方法
/// Entry ==> T
/// </summary>
/// <typeparam name="T1"></typeparam>
/// <typeparam name="T2"></typeparam>
/// <typeparam name="T3"></typeparam>
/// <typeparam name="T4"></typeparam>
/// <typeparam name="T5"></typeparam>
public interface IMapTo<T1, T2, T3, T4, T5> : IMapTo<T1, T2, T3, T4>
{
    /// <summary>
    /// 实现当前对象映射到T5对象destination
    /// </summary>
    /// <param name="destination"></param>
    void MapTo(T5 destination);
}