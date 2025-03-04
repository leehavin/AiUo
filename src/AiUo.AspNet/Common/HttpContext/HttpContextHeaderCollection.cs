using Microsoft.AspNetCore.Http;

namespace AiUo.AspNet.Common;

public class HttpContextHeaderCollection
{
    private bool _isRequest;
    private IHeaderDictionary _headers
        => _isRequest
            ? HttpContextEx.Request?.Headers
            : HttpContextEx.Response?.Headers;

    public HttpContextHeaderCollection(bool isRequest)
    {
        _isRequest = isRequest;
    }

    public bool TryGet(string key, out string value)
    {
        value = null;
        if (_headers == null)
            return false;
        var ret = _headers.TryGetValue(key, out var v);
        value = ret ? Convert.ToString(v) : null;
        return ret;
    }
    public string GetOrDefault(string key, string defaultValue)
    {
        if (!TryGet(key, out var ret))
            ret = defaultValue;
        return ret;
    }
    public void Add(string key, string value)
        => _headers?.Add(key, value);
    public bool TryAdd(string key, string value)
        => _headers?.TryAdd(key, value) ?? false;
    public bool AddOrUpdate(string key, string value)
        => AddOrUpdate(key, _ => value);
    public bool AddOrUpdate(string key, Func<string, string> func)
    {
        if (_headers == null)
            return false;
        var value = func(key);
        var ret = !_headers.ContainsKey(key);
        if (ret)
            _headers.Add(key, value);
        else
            _headers[key] = value;
        return ret;
    }
    public bool Remove(string key)
        => _headers?.Remove(key) ?? false;
}