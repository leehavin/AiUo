using System;
using System.Collections.Concurrent;
using System.Net.Http;
using AiUo.Configuration;

namespace AiUo.Net;

//TODO: https://github.com/reactiveui/refit
public static class HttpClientExFactory
{
    private static ConcurrentDictionary<string, HttpClientEx> _clientDict = new ConcurrentDictionary<string, HttpClientEx>();
    public static void ClearClientCaching()
    {
        _clientDict.Clear();
    }

    #region 从配置文件

    /// <summary>
    /// 根据配置文件创建HttpClientEx
    /// </summary>
    /// <param name="clientName">client名称，如果配置文件有配置，设置对应name，如果没有，每个场景使用各自的name公用</param>
    /// <param name="useCaching">是否使用缓存</param>
    /// <returns></returns>
    public static HttpClientEx CreateClientExFromConfig(string clientName, bool useCaching = true)
    {
        if (string.IsNullOrEmpty(clientName))
            throw new ArgumentNullException($"clientName不能为空");
        if (useCaching && _clientDict.TryGetValue(clientName, out var ret))
            return ret;

        var config = HttpClientSection.GetClientConfig(clientName);
        ret = new HttpClientEx(config);
        if (useCaching)
            _clientDict.TryAdd(clientName, ret);
        return ret;
    }
    /// <summary>
    /// 获取Client.Settings配置值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="clientName"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static T GetClientSettingValueFromConfig<T>(string clientName, string key)
    {
        var config = HttpClientSection.GetClientConfig(clientName);
        if (!config.Settings.TryGetValue(key, out var value))
            throw new Exception($"配置文件HttpClient:Clients:{clientName}:Settings没有配置key:{key}");
        return value.To<T>();
    }
    #endregion


    public static HttpClientEx CreateClientEx(HttpClientConfig config)
    {
        return new HttpClientEx(config);
    }
    /// <summary>
    /// 创建HttpClientEx
    /// </summary>
    /// <param name="name"></param>
    /// <param name="reserveBody"></param>
    /// <param name="useCookies"></param>
    /// <returns></returns>
    public static HttpClientEx CreateClientEx(string name = null, bool reserveBody = true, bool useCookies = false)
    {
        name = name ?? "default";
        var config = new HttpClientConfig
        {
            Name = name,
            ReserveBody = reserveBody,
            UseCookies = useCookies
        };
        return new HttpClientEx(config);
    }

    /// <summary>
    /// 创建HttpClient
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static HttpClient CreateClient(string name = null)
    {
        var factory = DIUtil.GetService<IHttpClientFactory>();
        return factory != null ? factory.CreateClient(name) : new HttpClient();
    }
}