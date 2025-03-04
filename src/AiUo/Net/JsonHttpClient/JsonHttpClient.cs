using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AiUo.Logging;
using AiUo.Reflection;
using AiUo.Collections;

namespace AiUo.Net;

public class JsonHttpClient
{
    #region Properties
    private HttpClient _client;
    public JsonSerializerOptions JsonOptions { get; set; } = SerializerUtil.GetJsonOptions();

    public JsonHttpClient(HttpClient client)
    {
        _client = client;
    }
    public JsonHttpClient(string name)
    {
        _client = DIUtil.GetRequiredService<IHttpClientFactory>().CreateClient(name);
    }
    public JsonHttpClient(JsonHttpClientElement element)
    {
        if (element.Retry > 0)
        {
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            var policy = new PolicyHttpMessageHandler(retryPolicy);
            _client = new HttpClient(policy);
        }
        else
        {
            _client = new HttpClient();
        }
        if (element.Timeout > 0)
            _client.Timeout = TimeSpan.FromSeconds(element.Timeout);
        if (!string.IsNullOrEmpty(element.BaseAddress))
            _client.BaseAddress = new Uri(element.BaseAddress);
        if (element?.RequestHeaders.Count > 0)
        {
            element.RequestHeaders.ForEach(x => _client.DefaultRequestHeaders.Add(x.Key, x.Value));
        }
    }
    #endregion

    public JsonHttpClient AddRequestHeader(string key, string value)
    {
        _client.DefaultRequestHeaders.Add(key, value);
        return this;
    }

    #region Url
    public JsonHttpClient SetBaseAddress(string baseAddress)
    {
        _client.BaseAddress = new Uri(baseAddress);
        return this;
    }
    private string _url;
    public JsonHttpClient AddUrl(string url)
    {
        _url = url?.Trim()?.TrimStart('/');
        return this;
    }
    private Dictionary<string, string> _queries = new();
    public JsonHttpClient AddQuery(string key, object value)
    {
        var str = Convert.ToString(value);
        if (_queries.ContainsKey(key))
            _queries[key] = str;
        else
            _queries.Add(key, str);
        return this;
    }
    public JsonHttpClient AddQuery<T>(T query)
        where T : class
    {
        var type = query.GetType();
        foreach (var property in type.GetProperties())
        {
            var value = ReflectionUtil.GetPropertyValue(query, property.Name);
            AddQuery(property.Name, Convert.ToString(value));
        }
        return this;
    }
    private string GetUrl(string url)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(url))
            _url = url.Trim()?.TrimStart('/');
        if (!string.IsNullOrEmpty(_url))
            sb.Append($"{_url}");
        if (_queries.Count > 0)
        {
            sb.Append("?");
            var list = _queries.ToList(x => $"{x.Key}={x.Value}");
            sb.Append(string.Join('&', list));
        }
        return sb.ToString();
    }
    #endregion

    public async Task<JsonHttpResult<TResult>> GetAsync<TResult>(string url = null)
    {
        return await Request<TResult>(url, HttpMethod.Get, r => _client.GetAsync(r.RequestUrl));
    }

    public async Task<JsonHttpResult<TResult>> PostAsync<TResult>(string url = null, object body = null)
    {
        return await Request<TResult>(url, HttpMethod.Post, r =>
        {
            r.RequestContent = SerializerUtil.SerializeJson(body, JsonOptions);
            var content = new StringContent(r.RequestContent, Encoding.UTF8, "application/json");
            // content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            // request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return _client.PostAsync(r.RequestUrl, content);
        });
    }

    public async Task<JsonHttpResult<TResult>> DeleteAsync<TResult>(string url = null)
    {
        return await Request<TResult>(url, HttpMethod.Get, r => _client.DeleteAsync(r.RequestUrl));
    }

    public async Task<JsonHttpResult<TResult>> PutAsync<TResult>(string url = null, object body = null)
    {
        return await Request<TResult>(url, HttpMethod.Put, r =>
        {
            r.RequestContent = SerializerUtil.SerializeJson(body, JsonOptions);
            var content = new StringContent(r.RequestContent, Encoding.UTF8, "application/json");
            return _client.PutAsync(r.RequestUrl, content);
        });
    }

    private async Task<JsonHttpResult<TResult>> Request<TResult>(string url, HttpMethod method, Func<JsonHttpResult<TResult>, Task<HttpResponseMessage>> request)
    {
        var ret = new JsonHttpResult<TResult>();
        ret.Method = method;
        ret.RequestUrl = GetUrl(url);
        ret.RequestHeaders = _client.DefaultRequestHeaders;
        ret.RequestUtcTime = DateTime.UtcNow;

        try
        {
            using (var rsp = await request(ret).ConfigureAwait(false))
            {
                ret.ResponseHeaders = rsp.Headers;
                ret.ResponseStatusCode = rsp.StatusCode;
                ret.ResponseContent = await rsp.Content.ReadAsStringAsync().ConfigureAwait(false);
                rsp.EnsureSuccessStatusCode();
                ret.Result = ReflectionUtil.IsSimpleType(typeof(TResult))
                    ? ret.ResponseContent.ConvertTo<TResult>()
                    : SerializerUtil.DeserializeJson<TResult>(ret.ResponseContent, JsonOptions);
                ret.Success = true;
            }
        }
        catch (Exception ex)
        {
            ret.Exception = ex;
            LogUtil.GetContextLogger()
                .SetLevel(LogLevel.Warning)
                .AddField("JsonHttpClient.Error", ret).Save();
        }
        ret.ResponseUtcTime = DateTime.UtcNow;
        return ret;
    }
}