using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AiUo.Logging;
using AiUo.Net;
using AiUo.Configuration;

namespace AiUo.AspNet;

/// <summary>
/// 访问我方API时的IP验证器
/// </summary>
public class AccessIpFilterAttribute : ActionFilterAttribute
{
    private string _name;

    public AccessIpFilterAttribute(string name = null)
    {
        _name = name;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userIp = AspNetUtil.GetRemoteIpString(context.HttpContext);
        if (!CheckAllow(userIp))
        {
            context.Result = new UnauthorizedResult();
            LogUtil.GetContextLogger()
                .SetLevel(Microsoft.Extensions.Logging.LogLevel.Warning)
                .AddField("AccessIpFilter.UserIp", userIp)
                .AddField("AccessIpFilter.FilterName", _name)
                .AddMessage("AccessIpFilterAttribute禁止访问。");
            return;
        }
        await base.OnActionExecutionAsync(context, next);
    }
    private bool CheckAllow(string userIp)
    {
        // 无IP有问题
        if (userIp == null || string.IsNullOrEmpty(userIp))
            return false;

        var section = ConfigUtil.GetSection<AccessIpFilterSection>();
        if (section == null || !section.Filters.TryGetValue(_name ?? section.DefaultFilterName, out var element))
            throw new Exception($"配置中AccessIpFilter:Filters未定义。name:{_name}");
        // 不限制
        if (!element.Enabled)
            return true;
        // 允许
        if (element.GetAllowIpDict().Contains(userIp))
            return true;

        var ipMode = NetUtil.GetIpMode(userIp);
        // 本机 => 放行
        if (ipMode == IpAddressMode.Local || ipMode == IpAddressMode.Loopback)
            return true;
        // 局域网
        return element.EnableIntranet && ipMode == IpAddressMode.Intranet;
    }
}