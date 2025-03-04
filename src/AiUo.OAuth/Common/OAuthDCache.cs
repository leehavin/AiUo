using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AiUo.OAuth.Common;

internal class OAuthDCache
{
    private const int EXPIRY_SECONDS = 600;
    private IDistributedCache _cache;
    public OAuthDCache()
    {
        _cache = DIUtil.GetRequiredService<IDistributedCache>();
    }

    public async Task<string> GetState(string key)
    {
        var cacheKey = GetCacheKey(key);
        var ret = await _cache.GetStringAsync(cacheKey);
        await _cache.RemoveAsync(cacheKey);
        return ret;
    }
    public async Task SetState(string key, string value)
    {
        var cacheKey = GetCacheKey(key);
        await _cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(value ?? "default"), new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromSeconds(EXPIRY_SECONDS)
        });
    }
    private string GetCacheKey(string key)
        => $"_AiUo:OAuth:{key}";
}