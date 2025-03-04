namespace AiUo.AspNet.Common;

public class HttpContextItemCollection
{
    public IDictionary<object, object> Items
        => HttpContextEx.Current?.Items;
    internal HttpContextItemCollection() { }

    public bool TryGet<T>(string key, out T value)
    {
        value = default;
        if (Items == null)
            return false;
        var ret = Items.TryGetValue(key, out object v);
        value = ret ? (T)v : default;
        return ret;
    }
    public T GetOrDefault<T>(string key, T defaultValue)
    {
        if (!TryGet<T>(key, out T ret))
            ret = defaultValue;
        return ret;
    }
    public void Add(string key, object value)
        => Items?.Add(key, value);
    public bool TryAdd(string key, object value)
        => Items?.TryAdd(key, value) ?? false;
    public bool AddOrUpdate(string key, object value)
        => AddOrUpdate(key, _ => value);
    public bool AddOrUpdate(string key, Func<string, object> func)
    {
        if (Items == null) 
            return false;
        var value = func(key);
        var ret = !Items.ContainsKey(key);
        if (ret)
            Items.Add(key, value);
        else
            Items[key] = value;
        return ret;
    }
    public bool Remove(string key)
        => Items?.Remove(key) ?? false;
}