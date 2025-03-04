using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using ProtoBuf.Grpc.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AiUo.Caching;
using AiUo.Hosting.Services;
using AiUo.Net;

namespace AiUo.Hosting;

public static class HostingUtil
{
    public static readonly IAiUoHostRegisterService RegisterService = new DefaultHostRegisterService();

    #region IAiUoHostLifetimeService
    /// <summary>
    /// 注册Host启动中事件
    /// </summary>
    /// <param name="func"></param>
    public static void RegisterStarting(Func<Task> func)
        => GetLifetimeService().RegisterStarting(func);
    /// <summary>
    /// 注册Host启动完毕事件
    /// </summary>
    /// <param name="func"></param>
    public static void RegisterStarted(Func<Task> func)
        => GetLifetimeService().RegisterStarted(func);
    /// <summary>
    /// 注册Host准备停止事件
    /// </summary>
    /// <param name="func"></param>
    public static void RegisterStopping(Func<Task> func)
        => GetLifetimeService().RegisterStopping(func);
    /// <summary>
    /// 注册Host已经停止事件
    /// </summary>
    /// <param name="func"></param>
    public static void RegisterStopped(Func<Task> func)
        => GetLifetimeService().RegisterStopped(func);

    public static readonly DefaultAiUoHostLifetimeService LifetimeService = new();
    /// <summary>
    /// 获取Host生命周期事件注册服务
    /// </summary>
    /// <returns></returns>
    private static IAiUoHostLifetimeService GetLifetimeService()
    {
        return LifetimeService;
        //var ret = DIUtil.GetService<IAiUoHostLifetimeService>();
        //if (ret == null)
        //    throw new Exception("IAiUoHostLifetimeService没有注入服务，请在配置服务ConfigureServices()里调用");
        //return ret;
    }
    #endregion

    #region IAiUoHostTimerService
    /// <summary>
    /// 注册主机定时任务
    /// </summary>
    /// <param name="item"></param>
    /// <param name="tryUpdate"></param>
    /// <returns></returns>
    public static bool RegisterTimer(AiUoHostTimerItem item, bool tryUpdate = false)
        => GetTimerService().Register(item, tryUpdate);

    /// <summary>
    /// 注销主机定时任务
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool DeregisterTimer(string id)
        => GetTimerService().Deregister(id);

    /// <summary>
    /// 注册延迟任务
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="callback"></param>
    /// <param name="title"></param>
    public static void RegisterDelayTimer(TimeSpan delay, Func<CancellationToken, Task> callback, string title = null)
    {
        RegisterTimer(new AiUoHostTimerItem
        {
            Title = title,
            ExecuteCount = 1,
            TryCount = 0,
            Interval = (int)delay.TotalMilliseconds,
            Callback = callback
        });
    }

    public static readonly DefaultAiUoHostTimerService TimerService = new();
    private static IAiUoHostTimerService GetTimerService()
    {
        return TimerService;
        //var ret = DIUtil.GetService<IAiUoHostTimerService>();
        //if (ret == null)
        //    throw new Exception("IAiUoHostTimerService没有注入服务");
        //return ret;
    }
    #endregion

    #region IAiUoHostMicroService
    public static async Task<List<string>> GetAllServiceNames()
        => await GetMicroService().GetAllServiceNames();
    public static async Task<string> SelectOneServiceUrl(string serviceName)
        => await GetMicroService().SelectOneServiceUrl(serviceName);
    private static IAiUoHostMicroService GetMicroService()
    {
        if (!DIUtil.HostBuilded)
            throw new Exception("Host没有build，IAiUoHostMicroService没有注入服务!");
        var ret = DIUtil.GetService<IAiUoHostMicroService>();
        if (ret == null)
            throw new Exception("IAiUoHostMicroService没有注入服务");
        return ret;
    }

    /// <summary>
    /// 创建指定服务名的HttpClient
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public static async Task<JsonHttpClient> CreateJsonHttpClient(string serviceName)
    {
        var ret = new JsonHttpClient(serviceName);
        var url = await SelectOneServiceUrl(serviceName);
        ret.SetBaseAddress(url);
        return ret;
    }

    /// <summary>
    /// 创建指定服务名的GrpcClient（protobuf-net.Grpc）
    /// 服务协定接口，例如:
    /// [ServiceContract]
    /// public interface IGreeterService
    /// {
    ///     [OperationContract]
    ///     Task SayHelloAsync(HelloRequest request, CallContext context = default);
    /// }
    /// 其中HelloRequest数据协定:
    /// [DataContract]
    /// public class HelloRequest
    /// {
    ///     [DataMember(Order = 1)]
    ///     public string Name { get; set; }
    /// }
    /// </summary>
    /// <typeparam name="TService" />
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public static Task<TService> CreateGrpcClient<TService>(string serviceName)
        where TService : class
    {
        var channel = _grpcChannelDict.GetOrAdd(serviceName, (name) => 
        {
            var ret = GrpcChannel.ForAddress($"nacos:///{serviceName}", new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Insecure,
                ServiceConfig = DIUtil.GetService<ServiceConfig>(),
                ServiceProvider = DIUtil.GetServiceProvider()
            });
            return ret;
        });
        var client = channel.CreateGrpcService<TService>();
        return Task.FromResult(client);
    }

    private static ConcurrentDictionary<string, GrpcChannel> _grpcChannelDict = new();
    #endregion

    #region IAiUoHostDataService
    /// <summary>
    /// 设置主机数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Task SetHostData<T>(string key, T value)
        => GetRegDataService().SetHostData<T>(key, value);

    /// <summary>
    /// 获取主机数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public static Task<CacheValue<T>> GetHostData<T>(string key)
        => GetRegDataService().GetHostData<T>(key);
    public static IAiUoHostDataService GetRegDataService()
    {
        if (!DIUtil.HostBuilded)
            throw new Exception("Host没有build，IAiUoHostDataService没有注入服务");
        var ret = DIUtil.GetService<IAiUoHostDataService>();
        if (ret == null)
            throw new Exception("IAiUoHostDataService没有注入服务");
        return ret;
    }
    #endregion
}