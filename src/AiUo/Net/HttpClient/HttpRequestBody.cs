using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace AiUo.Net;

/// <summary>
/// HTTP RequestBody
/// </summary>
public class HttpRequestBody
{
    public HttpMethod Method { get; set; }
    public string RequestUri { get; set; }
    [JsonIgnore]
    public IDictionary<string, string> RequestParams { get; set; }
    public HttpContent Content { get; set; }
    /// <summary>
    /// 请求的内容记录，用于日志
    /// </summary>
    public object RequestContent { get; set; }
    public HttpRequestHeaders Headers { get; set; }
    public IDictionary<string, object> Properties { get; set; }
    public string Version { get; set; }
}