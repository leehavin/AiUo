using Microsoft.AspNetCore.Http;
using AiUo.Configuration;

namespace AiUo.AspNet;

public class SyncNotifyMiddleware
{
    private readonly RequestDelegate _next;
    public const string DEFAULT_HEADER_NAME = "tfxc-sync";
    private IClientSyncNotifyProvider _provider;
    public string _headerName;
    private bool _enabled;
    public SyncNotifyMiddleware(RequestDelegate next)
    {
        _next = next;
        _provider = DIUtil.GetService<IClientSyncNotifyProvider>();
        var section = ConfigUtil.GetSection<ClientSyncNotifySection>();
        _headerName = section.HeaderName ?? DEFAULT_HEADER_NAME;
        _enabled = section.Enabled && _provider != null;
    }
    public async Task Invoke(HttpContext context)
    {
        if (_enabled)
        {
            var userId = context?.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                var value = await _provider.GetNotifyValue(userId);
                if (!string.IsNullOrEmpty(value))
                {
                    context?.Response?.Headers?.Add("Access-Control-Expose-Headers", _headerName);
                    context?.Response?.Headers?.Add(_headerName, value);
                }
            }
        }
        await _next(context); // 继续执行
    }
}