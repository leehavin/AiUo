using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AiUo.Reflection;

namespace AiUo.Net;

public class ClientAgent
{
    #region Properties
    private HttpClientEx _client;
    public string Url { get; private set; }
    public readonly Dictionary<string, string> Parameters = new Dictionary<string, string>();
    public readonly Dictionary<string, string> RequestHeaders = new Dictionary<string, string>();

    private HttpContent PostContent;
    private object RequestContent;
    public PostContentType PostContentType { get; private set; } = PostContentType.Unknow;

    public ClientAgent(HttpClientEx client)
    {
        _client = client;
    }
    #endregion

    #region Methods
    public ClientAgent AddUrl(string url)
    {
        Url = url?.Trim()?.TrimStart('/');
        return this;
    }
    public ClientAgent AppendUrl(string url)
    {
        Url+= url?.Trim();
        return this;
    }
    /// <summary>
    /// 添加key/value数据
    ///     get时拼接url
    ///     post时服务器使用x-www-form-urlencoded接收数据
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public ClientAgent AddParameter(string key, string value)
    {
        Parameters.Add(key, value);
        return this;
    }
    /// <summary>
    /// 解析对象的属性值并添加为请求参数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="objParams"></param>
    /// <returns></returns>
    public ClientAgent AddParameter<T>(T objParams)
    {
        foreach (var param in ParseKeyValueContent<T>(objParams))
            Parameters.Add(param.Key, param.Value);
        return this;
    }
    /// <summary>
    /// 添加请求Header
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public ClientAgent AddRequestHeader(string key, string value)
    {
        RequestHeaders.Add(key, value);
        return this;
    }
    #endregion

    #region PostContent
    /// <summary>
    /// Content-Type = application/json
    /// 对应asp.net core中的 [FromBody]，默认模式
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public ClientAgent BuildJsonContent(string json)
    {
        CheckPostContent();
        PostContent = new StringContent(json, _client.Encoding, "application/json");
        PostContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        PostContentType = PostContentType.JsonContent;
        RequestContent = json;
        return this;
    }
    /// <summary>
    /// Content-Type = application/json
    /// 对应asp.net core中的 [FromBody]，默认模式
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public ClientAgent BuildJsonContent(object obj)
    {
        var json = string.Empty;
        switch (_client.Config.SerializeMode)
        {
            case Serialization.SerializeMode.Json:
                json = SerializerUtil.SerializeJson(obj, _client.JsonOptions);
                break;
            case Serialization.SerializeMode.JsonNet:
                json = SerializerUtil.SerializeJsonNet(obj, _client.JsonNetSettings);
                break;
            default:
                throw new Exception("未知的json序列化类型");
        }
        return BuildJsonContent(json);
    }

    /// <summary>
    /// Content-Type = text/plain
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public ClientAgent BuildStringContent(string content)
    {
        CheckPostContent();
        PostContent = new StringContent(content);
        PostContentType = PostContentType.StringContent;
        RequestContent = content;
        return this;
    }
    /// <summary>
    /// Content-Type = x-www-form-urlencoded
    /// 表单数据编码为名称/值对, POST 的默认格式。有大小限制
    /// 对应asp.net core中的 [FromForm]
    /// </summary>
    /// <returns></returns>
    public ClientAgent BuildFormUrlEncodedContent()
    {
        CheckPostContent();
        PostContent = new FormUrlEncodedContent(Parameters);
        PostContentType = PostContentType.FormUrlEncoded;
        RequestContent = Parameters;
        return this;
    }
    /// <summary>
    /// Content-Type = multipart/form-data
    /// 表单数据编码为多部分 MIME 消息。 如果要将文件上传到服务器，请使用此格式
    /// 对应asp.net core [FromForm]
    /// </summary>
    /// <returns></returns>
    public ClientAgent BuildFormDataContent()
    {
        CheckPostContent();
        var content = new MultipartFormDataContent();
        foreach (var kvPair in Parameters)
        {
            content.Add(new StringContent(kvPair.Value), kvPair.Key);
        }
        PostContent = content;
        PostContentType = PostContentType.MultipartFormData;
        RequestContent = Parameters;
        return this;
    }
    /// <summary>
    /// 向FormDataContent中添加上传文件
    /// </summary>
    /// <param name="file"></param>
    /// <param name="mediaType">"image/png"</param>
    /// <returns></returns>
    public ClientAgent AddFileToFormDataContent(string file, string mediaType)
    {
        using var stream = File.OpenRead(file);
        return AddFileToFormDataContent(stream, mediaType, Path.GetFileName(file));
    }
    public ClientAgent AddFileToFormDataContent(Stream stream, string mediaType, string fileName)
    {
        var content = new ByteArrayContent(new StreamContent(stream).ReadAsByteArrayAsync().Result);
        content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileName = fileName,
        };
        if (PostContent == null)
            PostContent = new MultipartContent();
        ((MultipartFormDataContent)PostContent).Add(content);
        return this;
    }
    #endregion

    #region GET & POST
    /// <summary>
    /// GET返回字符串
    /// </summary>
    /// <returns></returns>
    public async Task<HttpResponseResult> GetStringAsync()
        => await _client.RequestAsync(BuildGetRequest(), Parameters, RequestContent);
    /// <summary>
    /// GET返回对象
    /// </summary>
    /// <typeparam name="TSuccess"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <returns></returns>
    public async Task<HttpResponseResult<TSuccess, TError>> GetAsync<TSuccess, TError>()
        => await _client.RequestAsync<TSuccess, TError>(BuildGetRequest(), Parameters, RequestContent);

    /// <summary>
    /// POST返回字符串
    /// </summary>
    /// <returns></returns>
    public async Task<HttpResponseResult> PostStringAsync()
        => await _client.RequestAsync(BuildPostRequest(), Parameters, RequestContent);
    /// <summary>
    /// POST返回对象
    /// </summary>
    /// <typeparam name="TSuccess"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <returns></returns>
    public async Task<HttpResponseResult<TSuccess, TError>> PostAsync<TSuccess, TError>()
        => await _client.RequestAsync<TSuccess, TError>(BuildPostRequest(), Parameters, RequestContent);
    #endregion

    #region Utils
    private HttpRequestMessage BuildGetRequest()
    {
        var url = Url.Trim().Trim('?', '&');
        if (Parameters.Count > 0)
        {
            url = url.Contains("?") ? $"{url}&" : $"{url}?";
            foreach (var item in Parameters)
            {
                url += $"{item.Key}={WebUtility.UrlEncode(item.Value)}&";
            }
            url = url.TrimEnd('&');
        }

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        RequestContent = Parameters;
        AddRequestHeaders(request);
        return request;
    }
    private HttpRequestMessage BuildPostRequest()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, Url);
        AddRequestHeaders(request);
        if (PostContent == null)
            throw new Exception("PostContent不能为空，请通过对应BuildContent方法添加");
        request.Content = PostContent;
        return request;
    }
    private void AddRequestHeaders(HttpRequestMessage request)
    {
        foreach (var header in RequestHeaders)
            request.Headers.Add(header.Key, header.Value);
    }
    private static Dictionary<string, string> ParseKeyValueContent<T>(T paramObj)
    {
        var ret = new Dictionary<string, string>();
        var type = paramObj.GetType();
        if (ReflectionUtil.IsSimpleType(type))
        {
            ret.Add("", Convert.ToString(paramObj));
        }
        else
        {
            foreach (var property in type.GetProperties())
            {
                var value = ReflectionUtil.GetPropertyValue(paramObj, property.Name);
                ret.Add(property.Name, Convert.ToString(value));
            }
        }
        return ret;
    }
    private void CheckPostContent()
    {
        if (PostContent != null)
            throw new Exception($"已设置当前PostContent: {PostContentType}");
    }
    #endregion
}