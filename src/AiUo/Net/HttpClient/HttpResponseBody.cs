using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AiUo.Net;

public class HttpResponseBody
{
    public HttpContent Content { get; set; }
    public HttpResponseHeaders Headers { get; set; }
    public bool Success { get; set; }
    public string ReasonPhrase { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public string Version { get; set; }
    public string ResponseString { get; set; }
}