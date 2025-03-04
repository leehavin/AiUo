using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using AiUo.Configuration.Common;
using AiUo.Logging;

namespace AiUo.Configuration;

/// <summary>
/// 配置文件Util类
/// </summary>
public static class ConfigUtil
{
    #region Properties
    /// <summary>
    /// AiUo配置IConfiguration
    /// </summary>
    public static IConfiguration Configuration { get; private set; }
    private static event EventHandler _changedEvent;
    /// <summary>
    /// AiUo配置节集合
    /// </summary>
    public static readonly ConcurrentDictionary<string, object> Sections = new ConcurrentDictionary<string, object>();

    /// <summary>
    /// 环境变量信息
    /// </summary>
    public static EnvironmentInfo Environment { get; private set; }

    /// <summary>
    /// Host服务信息
    /// </summary>
    public static ServiceInfo Service { get; private set; }
    #endregion

    #region Init
    public static IConfiguration BuildConfiguration(string envStr = null)
    {
        Environment = new EnvironmentInfo(envStr);
        Service = new ServiceInfo(Environment);

        var builder = new AppSettingsFileConfigBuilder();
        Environment.ConfigFiles = builder.GetConfigFiles(Environment.Name);
        var ret = builder.Build(Environment.ConfigFiles);
        return ret;
    }
    public static void InitConfiguration(IConfiguration configuration)
    {
        Configuration = configuration;
        ClearCacheData();

        // EnvironmentType
        if (!string.IsNullOrEmpty(Project.Environment))
            Environment.Type = new EnvironmentTypeParser().Parse(Project.Environment);

        // serviceId
        if (!string.IsNullOrEmpty(Service.HostIp) && Service.HostPort > 0)
            Service.SID = $"{Service.HostIp}_{Service.HostPort}";
        Service.ServiceId = $"{Project.ProjectId}:{Service.SID}";

        Configuration.GetReloadToken().RegisterChangeCallback((_) =>
        {
            LogUtil.Warning("配置更新: {changeTime}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            ClearCacheData();
            _changedEvent?.Invoke(null, null);
        }, null);
    }
    private static void ClearCacheData()
    {
        _project = null;
        _host = null;
        _appSettings = null;
        _appConfigs = null;
        Sections.Clear();
    }
    #endregion

    #region Sections
    /// <summary>
    /// 获取配置节信息（T类型继承IConfigSection）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetSection<T>() where T : class, IConfigSection, new()
    {
        CheckInit();
        var key = typeof(T).FullName;
        if (Sections.TryGetValue(key, out object value))
            return (T)value;
        T ret = null;
        var newItem = new T();
        var section = Configuration.GetSection(newItem.SectionName);
        if (section.Exists())
        {
            newItem.Bind(section);
            ret = newItem;
        }
        Sections.TryAdd(key, ret);
        return ret;
    }

    /// <summary>
    /// 获取配置节信息（T可以是任意类型）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    public static T GetSection<T>(string sectionName)
    {
        CheckInit();
        var key = typeof(T).FullName;
        if (Sections.TryGetValue(key, out object value))
            return (T)value;
        var ret = default(T);
        if (Configuration != null)
        {
            var section = Configuration.GetSection(sectionName);
            if (section.Exists())
            {
                ret = section.Get<T>();
            }
        }
        Sections.TryAdd(key, ret);
        return ret;
    }

    private static ProjectSection _project;
    /// <summary>
    /// 项目配置信息
    /// </summary>
    public static ProjectSection Project
    {
        get
        {
            if (_project == null)
            {
                var proj = GetSection<ProjectSection>() ?? new ProjectSection();
                if (string.IsNullOrEmpty(proj.ProjectId))
                {
                    proj.ProjectId = Assembly.GetEntryAssembly().GetName().Name;
                }
                if (string.IsNullOrEmpty(proj.ApplicationName))
                    proj.ApplicationName = proj.ProjectId;
                _project = proj;
            }
            return _project;
        }
    }
    private static HostSection _host;
    public static HostSection Host
    {
        get
        {
            if (_host == null)
            {
                _host = GetSection<HostSection>() ?? new HostSection();
            }
            return _host;
        }
    }

    private static AppSettingsSection _appSettings;
    /// <summary>
    /// app自定义配置key/value数据
    /// </summary>
    public static AppSettingsSection AppSettings
        => _appSettings ??= GetSection<AppSettingsSection>() ?? new AppSettingsSection();

    private static AppConfigsSection _appConfigs;
    /// <summary>
    /// app自定义配置类数据
    /// </summary>
    public static AppConfigsSection AppConfigs
        => _appConfigs ??= GetSection<AppConfigsSection>() ?? new AppConfigsSection();

    private static void CheckInit()
    {
        if (Configuration == null)
            throw new Exception("AiUo应用程序配置没有初始化!");
    }
    #endregion

    /// <summary>
    /// 注册配置改变回调
    /// </summary>
    /// <param name="callback"></param>
    public static void RegisterChangedCallback(Action callback)
    {
        _changedEvent += (_, _) => callback();
    }
}