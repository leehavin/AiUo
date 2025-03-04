using Newtonsoft.Json.Linq;
using System;

namespace AiUo.Serialization;

/// <summary>
/// Json结构Meta数据
/// </summary>
public class JsonMetaData
{
    private JObject _root;
    public JsonMetaData(string json = null)
    {
        _root = !string.IsNullOrEmpty(json)
            ? JObject.Parse(json)
            : new JObject();
    }
    public void SetSection<T>(string name, T value)
    {
        if (value == null)
            _root[name] = null;
        else
            _root[name] = JToken.FromObject(value);
    }
    public bool TryGetSection<T>(string name, out T value)
    {
        value = default;
        var section = _root[name];
        if (section == null)
            return false;
        value = section.ToObject<T>();
        return true;
    }
    public T GetOrDefault<T>(string name, T defaultValue)
    {
        return TryGetSection(name, out T value)
            ? value : defaultValue;
    }
    public T GetOrException<T>(string name)
    {
        if (!TryGetSection(name, out T value))
            throw new Exception($"{GetType().FullName}.GetOrException()异常。name: {name}");
        return value;
    }
    public string ToJson()
    {
        return JToken.FromObject(_root).ToString();
    }
}