using System;
using System.Collections.Generic;
using System.Net;

namespace AiUo.Net;

public class HttpResponseResult<TSuccess, TError>: HttpResponseResult
{
    public TSuccess SuccessResult { get; set; }
    public TError ErrorResult { get; set; }
}
public class HttpResponseResult
{
    public bool Success { get; set; }
    public string ResultString { get; set; }
    public HttpRequestBody Request { get; set; }
    public HttpResponseBody Response { get; set; }
    public Exception Exception { get; set; }
    public string ExceptionString { get; set; }
    public List<Cookie> Cookies { get; set; }
    public DateTime RequestUtcTime { get; set; }
    public DateTime ResponseUtcTime { get; set; }
}