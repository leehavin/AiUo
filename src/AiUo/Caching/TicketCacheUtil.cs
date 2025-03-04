using System;
using AiUo.Configuration;
using AiUo.Text;

namespace AiUo.Caching;

/// <summary>
/// 可用于email和sms验证码
/// </summary>
public static class TicketCacheUtil
{
    /// <summary>
    /// 生成ticket(可用于email和sms验证码)
    /// </summary>
    /// <param name="captchaId">email或手机号</param>
    /// <param name="length"></param>
    /// <param name="scope"></param>
    /// <param name="expiryMinutes"></param>
    /// <returns></returns>
    public static string GenerateTicket(string captchaId, int length, CharsScope scope = CharsScope.Numbers, int expiryMinutes = 10)
    {
        var code = new RandomString(scope, length).Next();
        var cacheKey = GetTicketDCacheKey(captchaId);
        var expiryTime = TimeSpan.FromMinutes(expiryMinutes);
        CachingUtil.Set(cacheKey, code, expiryTime);
        return code;
    }

    public static void SetTicket(string captchaId, string code, int expiryMinutes = 10)
    {
        var cacheKey = GetTicketDCacheKey(captchaId);
        var expiryTime = TimeSpan.FromMinutes(expiryMinutes);
        CachingUtil.Set(cacheKey, code, expiryTime);
    }

    /// <summary>
    /// 验证ticket(可用于email和sms验证码)
    /// </summary>
    /// <param name="captchaId">email或手机号</param>
    /// <param name="code"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public static bool ValidateTicket(string captchaId, string code, bool ignoreCase = true)
    {
        var cacheKey = GetTicketDCacheKey(captchaId);
        if (!CachingUtil.TryGet(cacheKey, out string val))
            return false;
        var comparisonType = ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
        var result = string.Equals(val, code, comparisonType);
        if (result)
            CachingUtil.Remove(cacheKey);
        return result;
    }
    private static string GetTicketDCacheKey(string captchaId)
        => $"{ConfigUtil.Project.ProjectId}:Ticket:{captchaId}";
}