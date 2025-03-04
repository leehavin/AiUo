using Microsoft.AspNetCore.Mvc;

namespace AiUo.AspNet.ResponseCaching;

/// <summary>
/// 不缓存
/// </summary>
public class ResponseCacheNone: ResponseCacheAttribute
{
    public ResponseCacheNone(int duration)
    {
        Duration = duration;
        Location = ResponseCacheLocation.None;
        NoStore = true;
    }
}