using AiUo.AspNet.ClientSignFilter;
using AiUo.Logging;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Runtime.CompilerServices;

namespace AiUo.AspNet;

/// <summary>
/// 自有客户端访问API时的sign验证器
/// </summary>
public class ClientSignFilterAttribute : Attribute, IAsyncActionFilter
{
    private string _name;
    private ClientSignFilterService _filterService;

    public ClientSignFilterAttribute(string name = null) => this._name = name;

    public async Task OnActionExecutionAsync(
      ActionExecutingContext context,
      ActionExecutionDelegate next)
    {
        int num;
        if (ClientSignUtil.TryGetService(this._name, out this._filterService))
        {
            ClientSignFilterService filterService = this._filterService;
            num = filterService != null ? (filterService.Enabled ? 1 : 0) : 0;
        }
        else
            num = 0;
        if (num != 0)
        {
            string headerName = this._filterService.HeaderName;
            StringValues value;
            if (!context.HttpContext.Request.Headers.TryGetValue(headerName, out value))
                throw new CustomException("G_UNAUTHORIZED", "header不存在: " + headerName);
            string headerValue = Convert.ToString((string)value);
            string[] data = headerValue?.Split('|');
            if (data == null || data.Length != 2)
            {
                var interpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 2);
                interpolatedStringHandler.AppendLiteral("header ");
                interpolatedStringHandler.AppendFormatted(headerName);
                interpolatedStringHandler.AppendLiteral(" 值格式错误: ");
                interpolatedStringHandler.AppendFormatted<StringValues>(value);
                throw new CustomException("G_UNAUTHORIZED", interpolatedStringHandler.ToStringAndClear());
            }
            string sourceAccessKey = data[0];
            string sign = data[1];
            string content = await AspNetUtil.GetRequestBodyAsync(context.HttpContext);
            content = string.IsNullOrEmpty(content) ? "null" : content;
            bool isValid = this._filterService.VerifyByAccessKey(content, sign, sourceAccessKey);
            if (!isValid)
            {
                var interpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 2);
                interpolatedStringHandler.AppendLiteral("header ");
                interpolatedStringHandler.AppendFormatted(headerName);
                interpolatedStringHandler.AppendLiteral(" 值无效: ");
                interpolatedStringHandler.AppendFormatted<StringValues>(value);
                string msg = interpolatedStringHandler.ToStringAndClear();
                LogUtil.GetContextLogger().SetLevel(LogLevel.Warning).AddMessage(msg).AddField(this.GetType().Name + ".Name", (object)this._name).AddField(this.GetType().Name + ".HeaderValue", (object)headerValue).AddField(this.GetType().Name + ".SourceAccessKey", (object)sourceAccessKey).AddField(this.GetType().Name + ".Sign", (object)sign).AddField(this.GetType().Name + ".Content", (object)content);
                throw new CustomException("G_UNAUTHORIZED", msg);
            }
            headerName = (string)null;
            value = new StringValues();
            headerValue = (string)null;
            data = (string[])null;
            sourceAccessKey = (string)null;
            sign = (string)null;
            content = (string)null;
        }
        ActionExecutedContext actionExecutedContext = await next();
    }
}