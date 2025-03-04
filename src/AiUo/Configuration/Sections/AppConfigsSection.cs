using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AiUo.Configuration;

public class AppConfigsSection : ConfigSection
{
    public override string SectionName => "AppConfigs";
    private Dictionary<string, IConfigurationSection> _configsDict = new Dictionary<string, IConfigurationSection>();

    public override void Bind(IConfiguration configuration)
    {
        base.Bind(configuration);
        var items = configuration.GetChildren().ToList();
        foreach (var item in items)
        {
            _configsDict.Add(item.Key, item);
        }
    }

    /// <summary>
    /// 默认类名
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key">默认typeof(T).Name</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public T Get<T>(string key = null) where T : new()
    {
        key ??= typeof(T).Name;
        if (!_configsDict.TryGetValue(key, out IConfigurationSection section))
            throw new Exception($"配置文件中AppConfigs:Key {key} 不存在");
        return section.Get<T>();
    }

    public T GetOrDefault<T>(string key, T defaultValue = default)
        => _configsDict.TryGetValue(key, out IConfigurationSection section)
            ? section.Get<T>() : defaultValue;
}