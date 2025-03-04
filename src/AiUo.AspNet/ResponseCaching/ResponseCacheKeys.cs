using Microsoft.AspNetCore.Mvc;

namespace AiUo.AspNet.ResponseCaching;

public class ResponseCacheKeys : ResponseCacheAttribute
{
    public ResponseCacheKeys(int duration, params string[] keys)
    {
        Duration = duration;
        Location = ResponseCacheLocation.Any;
        VaryByQueryKeys = keys ?? new string[] { "*" };
    }
}