using Microsoft.AspNetCore.Mvc;

namespace AiUo.AspNet.ResponseCaching;

/// <summary>
/// 客户端和服务器同时缓存
/// Cache-Control : public, max-age=60
/// </summary>
public class ResponseCacheAny : ResponseCacheAttribute
{
    public ResponseCacheAny(int duration)
    {
        Duration = duration;
        Location = ResponseCacheLocation.Any;
        VaryByHeader = "User-Agent";
    }
}