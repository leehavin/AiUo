using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using AiUo.Logging;
using AiUo.Reflection;

namespace AiUo.Net;

public class JsonHttpResult<TResult>
{
    #region Request & Response
    public HttpMethod Method { get; set; }
    public string RequestUrl { get; set; }
    public HttpRequestHeaders RequestHeaders { get; set; }
    public string RequestContent { get; set; }
    public DateTime RequestUtcTime { get; set; }

    public string ResponseContent { get; set; }
    public HttpResponseHeaders ResponseHeaders { get; set; }
    public HttpStatusCode ResponseStatusCode { get; set; }
    public DateTime ResponseUtcTime { get; set; }
    #endregion

    public bool Success { get; set; }
    public TResult Result { get; set; }
    public Exception Exception { get; set; }

    [JsonIgnore]
    public JsonSerializerOptions JsonOptions { get; set; }
    public TError GetErrorResult<TError>()
    {
        var ret = default(TError);
        try
        {
            ret = ReflectionUtil.IsSimpleType(typeof(TError))
                ? ResponseContent.ConvertTo<TError>()
                : SerializerUtil.DeserializeJson<TError>(ResponseContent, JsonOptions);
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "JsonHttpResult.GetErrorResult()时反序列化出错");
        }
        return ret;
    }
}