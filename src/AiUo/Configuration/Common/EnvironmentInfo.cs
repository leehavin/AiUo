using System;
using System.Collections.Generic;
using AiUo.Configuration.Common;
using AiUo.Hosting;
using AiUo.Net;

namespace AiUo.Configuration;

public class EnvironmentInfo
{

    public DateTime StartUtcTime { get; }

    #region Name & Type
    /// <summary>
    /// 配置的程序运行环境：Development/Testing/....
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// 当前程序运行环境
    /// </summary>
    public EnvironmentType Type { get; internal set; }
    /// <summary>
    /// 当前项目是否处于测试环境(Development,Testing)
    /// </summary>
    public bool IsDebug => Type == EnvironmentType.DEV
                           || Type == EnvironmentType.SIT
                           || Type == EnvironmentType.FAT;

    /// <summary>
    /// 是否仿真环境
    /// </summary>
    public bool IsStaging => Type == EnvironmentType.UAT;
    public bool IsProduction => Type == EnvironmentType.PRE || Type == EnvironmentType.PRO;

    /// <summary>
    /// 配置文件
    /// </summary>
    public List<string> ConfigFiles { get; internal set; }
    #endregion

    #region Env
    /// <summary>
    /// 是否运行在docker
    /// </summary>
    public bool IsRunningDocker { get; }
    public string TZ { get; }
    public string DotNetVersion { get; }
    public AiUoHostEndPoint UrlsEndPoint { get; }

    #endregion

    public EnvironmentInfo(string env)
    {
        StartUtcTime = DateTime.UtcNow;
        Name = GetEnvName(env);
        Type = new EnvironmentTypeParser().Parse(Name);

        IsRunningDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER").ToBoolean(false);
        TZ = Environment.GetEnvironmentVariable("TZ");
        DotNetVersion = Environment.GetEnvironmentVariable("DOTNET_VERSION")
                        ?? Environment.GetEnvironmentVariable("ASPNET_VERSION");
        UrlsEndPoint = GetUrlsEndPoint();
    }

    private string GetEnvName(string envString)
    {
        if (string.IsNullOrEmpty(envString))
            envString = System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        if (string.IsNullOrEmpty(envString))
            envString = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return envString;
    }
    private AiUoHostEndPoint GetUrlsEndPoint()
    {
        var list = new List<string>() {
            "URLS",
            "ASPNETCORE_URLS",
            "DOTNET_URLS"
        };
        var hasEnv = false;
        var ret = new AiUoHostEndPoint();
        foreach (var name in list)
        {
            var env = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(env))
                continue;
            hasEnv = true;
            var urls = env.Split(';');
            foreach (var str in urls)
            {
                if (string.IsNullOrEmpty(str))
                    continue;
                var url = str.TrimEnd().ToLower();
                if (url.StartsWith("http://"))
                {
                    url = url.TrimStart("http://");
                }
                else if (url.StartsWith("https://"))
                {
                    ret.Secure = true;
                    url = url.TrimStart("https://");
                }
                else
                    throw new Exception($"环境变量{name}无法解析: {env}");
                var values = url.Split(':');
                ret.Port = values[1].ToInt32(0);
                if (ret.Port <= 0)
                    throw new Exception($"环境变量{name} 无法解析: {env}");
                var ip = values[0];
                if (NetUtil.IsPrivateNetwork(ip))
                {
                    ret.Ip = ip;
                    break;
                }
            }
        }
        return hasEnv ? ret : null;
    }
}