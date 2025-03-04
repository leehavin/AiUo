using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using AiUo.Configuration;
using AiUo.Collections;

namespace AiUo.Net;

public class JsonHttpClientSection : ConfigSection
{
    public override string SectionName => "JsonHttpClient";

    public Dictionary<string, JsonHttpClientElement> Clients = new();
    public override void Bind(IConfiguration configuration)
    {
        base.Bind(configuration);
        Clients = configuration
            .Get<Dictionary<string, JsonHttpClientElement>>() ?? new();
        Clients.ForEach(x =>
        {
            x.Value.Name = x.Key;
            x.Value.RequestHeaders = configuration.GetSection($"{x.Key}:RequestHeaders")
                .Get<Dictionary<string, string>>() ?? new();
            x.Value.Settings = configuration.GetSection($"{x.Key}:Settings")
                .Get<Dictionary<string, string>>() ?? new();
        });
    }
    public JsonHttpClientElement Get(string name)
        => Clients.TryGetValue(name, out var ret) ? ret : null;
}
public class JsonHttpClientElement
{
    public string Name { get; set; }
    public string BaseAddress { get; set; }
    public int Timeout { get; set; } = 30000;
    public int Retry { get; set; }
    public Dictionary<string, string> RequestHeaders { get; set; } = new();
    public Dictionary<string, string> Settings = new();

    public bool TryGetSetting<T>(string key, out T value)
    {
        var ret = false;
        value = default;
        try {
            if (Settings.TryGetValue(key, out var str))
            {
                value = str.To<T>();
                ret = true;
            }
        }
        catch 
        {
            ret = false;
        }
        return ret;
    }
    public T GetSettingOrDefault<T>(string key, T defaultValue)
    {
        return Settings.TryGetValue(key, out var str)
            ? str.To(defaultValue) : defaultValue;
    }
    public T GetSettingOrException<T>(string key)
    {
        if (!Settings.TryGetValue(key, out var str))
            throw new Exception($"配置JsonHttpClient:{Name}:Settings中不存在key:{key}");
        return str.To<T>();
    }
}