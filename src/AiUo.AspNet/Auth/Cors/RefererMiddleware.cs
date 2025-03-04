using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using AiUo.Configuration;

namespace AiUo.AspNet.Auth.Cors;

public class RefererMiddleware
{
    private readonly RequestDelegate _next;
    public RefererMiddleware(RequestDelegate next)
    {
        if (next == null)
            throw new ArgumentNullException(nameof(next));
        _next = next;
    }
    public async Task Invoke(HttpContext context)
    {
        var section = ConfigUtil.GetSection<CorsSection>();
        if (section != null && section.UseCors.EnabledReferer)
        {
            var endpoint = context.GetEndpoint();
            var corsMetadata = endpoint?.Metadata.GetMetadata<ICorsMetadata>();
            string policyName = corsMetadata is IEnableCorsAttribute enableCorsAttribute
                                && enableCorsAttribute.PolicyName != null
                ? enableCorsAttribute.PolicyName : null;

            if (!string.IsNullOrEmpty(policyName)
                && section.Policies.TryGetValue(policyName, out var element)
                && !string.IsNullOrEmpty(element.Origins)
                && element.Origins != "*")
            {
                var url = AspNetUtil.GetRefererUrl(context);
                if (!string.IsNullOrEmpty(url))
                {
                    var idx = url.IndexOf('/', 8);
                    var baseUrl = idx > 0
                        ? url.Substring(0, idx)
                        : url;
                    if (!element.OriginSet.Contains(baseUrl))
                        throw new Exception($"Referer Error: {baseUrl}");
                }
            }
        }
        await _next(context);
    }
}