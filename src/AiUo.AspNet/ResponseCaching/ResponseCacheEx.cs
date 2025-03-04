using Microsoft.AspNetCore.Mvc;

namespace AiUo.AspNet.ResponseCaching;

/// <summary>
/// 使用配置文件配置的缓存策略
/// </summary>
public class ResponseCacheEx : ResponseCacheAttribute
{
    public ResponseCacheEx(string profileName)
    {
        CacheProfileName = profileName;
    }
}