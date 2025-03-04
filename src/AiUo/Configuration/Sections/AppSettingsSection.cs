using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace AiUo.Configuration;

/// <summary>
/// 项目自定义配置
/// </summary>
public class AppSettingsSection : ConfigSection
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public override string SectionName => "AppSettings";

    /// <summary>
    /// 配置集合
    /// </summary>
    public Dictionary<string, string> Settings = new Dictionary<string, string>();
    /// <summary>
    /// 绑定Section数据
    /// </summary>
    /// <param name="configuration"></param>
    public override void Bind(IConfiguration configuration)
    {
        Settings = configuration.Get<Dictionary<string, string>>() ?? new();
    }

    /// <summary>
    /// 获取配置值，没有配置返回null
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string Get(string key)
        => Settings.TryGetValue(key, out var ret) ? ret : null;

    public T GetOrDefault<T>(string key, T defaultValue)
    {
        var ret = Get(key);
        return ret != null ? ret.To<T>() : defaultValue;
    }
    public T GetOrException<T>(string key)
    {
        var ret = Get(key);
        if (ret == null)
            throw new Exception($"配置文件AppSettings中不存在key: {key}");
        return ret.To<T>();
    }
}
/// <summary>
/// KeyValue类型
/// </summary>
public class KeyValueItem
{
    /// <summary>
    /// Key
    /// </summary>
    public string Key { get; set; }
    /// <summary>
    /// Value
    /// </summary>
    public string Value { get; set; }
    public KeyValueItem()
    { }
    public KeyValueItem(string key, string value)
    {
        Key = key;
        Value = value;
    }
}