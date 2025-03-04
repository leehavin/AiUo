using AiUo.AspNet;
using AiUo.AspNet.Auth.Cors;
using AiUo.AspNet.Hosting;
using AiUo.AspNet.Hosting.Common;
using AiUo.AspNet.RequestLogging;
using AiUo.Configuration;
using AiUo.Logging;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace AiUo;

public static class AspNetWebApplicationExtensions
{
    public static WebApplication UseAspNetEx(this WebApplication app)
    {
        // 中间件顺序
        // https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/middleware/?view=aspnetcore-7.0#middleware-order
        app.UseStaticFilesEx(); //静态文件
        app.UseEnableBufferingEx();
        app.UseForwardedHeaders();
        app.UseResponseCompressionEx();
        //app.UseSerilogRequestLoggingEx();
        app.UseRequestLoggingEx();
        app.UseGlobalExceptionEx();
        app.UsePathBaseEx();
        app.UseCookiePolicyEx();
        app.UseRouting();
        //app.UseRateLimiter(); .net 7
        app.UseCorsEx();
        app.UseJwtAuthEx();
        app.UseSessionEx();
        app.UseResponseCachingEx(); // 必须放在UseCors之后
        app.UseSwaggerEx();
        app.UseInternalMap();
        app.UseGrpcEx();
        app.UseSyncNotifyEx();
        //
        AiUoHostingStartupLoader.Instance.Configure(app);

        app.Lifetime.ApplicationStarted.Register(() =>
        {
            app.UseAiUo(serviceProvider =>
            {
                var ihttp = serviceProvider?.GetService<IHttpContextAccessor>();
                return (ihttp != null && ihttp.HttpContext != null)
                    ? ihttp.HttpContext.RequestServices
                    : null;
            });
            LogUtil.Warning("===> 【AspNet服务已启动】 ProjectId: {ProjectId} Env: {EnvironmentName}({EnvironmentString}) HostUrl: [{ApiType}]{Urls} PathBase: {PathBase} ServiceId: {ServiceId}"
                , ConfigUtil.Project?.ProjectId
                , ConfigUtil.Environment.Type
                , ConfigUtil.Environment.Name
                , ConfigUtil.Service.HostApiType.ToString()
                , ConfigUtil.Service.HostUrl
                , ConfigUtil.GetSection<AspNetSection>()?.PathBase
                , ConfigUtil.Service.ServiceId);
        });
        app.Lifetime.ApplicationStopped.Register(() =>
        {
            LogUtil.Warning("===> 【AspNet服务已停止】 ProjectId:{ProjectId} Env:{EnvironmentName}({EnvironmentString}) URL:{Urls} PathBase:{PathBase} ServiceId:{ServiceId}"
                , ConfigUtil.Project?.ProjectId
                , ConfigUtil.Environment.Type
                , ConfigUtil.Environment.Name
                , ConfigUtil.Service.HostUrl
                , ConfigUtil.GetSection<AspNetSection>()?.PathBase
                , ConfigUtil.Service.ServiceId);
        });
        return app;
    }

    public static WebApplication UseEnableBufferingEx(this WebApplication app)
    {
        var section = ConfigUtil.GetSection<AspNetSection>();
        if (section != null && section.UseRequestBuffering)
        {
            // 支持读取Request.Body
            app.Use((context, next) =>
            {
                context.Request.EnableBuffering(); 
                return next(context);
            });
            /*
            // 支持读取Response.Body
            app.Use(async (context, next) =>
            {
                var originalResponseBody = context.Response.Body;
                try
                {
                    using var swapStream = new MemoryStream();
                    context.Response.Body = swapStream;
                    await next(context);
                    context.Response.Body.Seek(0, SeekOrigin.Begin);
                    await swapStream.CopyToAsync(originalResponseBody);
                }
                finally
                {
                    context.Response.Body = originalResponseBody;
                }
            });
            */
        }
        return app;
    }
    public static WebApplication UseResponseCompressionEx(this WebApplication app)
    {
        var section = ConfigUtil.GetSection<AspNetSection>();
        if (section != null && section.UseResponseCompression && ConfigUtil.Environment.IsProduction)
        {
            app.UseResponseCompression();
        }
        return app;
    }
    public static WebApplication UseRequestLoggingEx(this WebApplication app)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();
        return app;
    }
    public static WebApplication UseGlobalExceptionEx(this WebApplication app)
    {
        var section = ConfigUtil.GetSection<AspNetSection>();
        if (section?.UseApiActionResultFilter ?? false)
        {
            app.UseMiddleware<GlobalExceptionMiddleware>();
        }
        return app;
    }
    public static WebApplication UsePathBaseEx(this WebApplication app)
    {
        var section = ConfigUtil.GetSection<AspNetSection>();
        if (section == null || string.IsNullOrEmpty(section.PathBase))
            return app;
        app.UsePathBase(section.PathBase);
        return app;
    }
    public static WebApplication UseCookiePolicyEx(this WebApplication app)
    {
        var section = ConfigUtil.GetSection<SessionAndCookieSection>();
        if (section != null && section.UseCookieIdentity)
        {
            app.UseCookiePolicy();
        }
        return app;
    }
    public static WebApplication UseCorsEx(this WebApplication app)
    {
        var section = ConfigUtil.GetSection<CorsSection>();
        if (section != null && section.UseCors.Enabled)
        {
            app.UseCors();
            if (section.UseCors.EnabledReferer)
                app.UseMiddleware<RefererMiddleware>();
            //
            section.PoliciesProvider?.SetAutoRefresh();
        }
        return app;
    }
    public static WebApplication UseJwtAuthEx(this WebApplication app)
    {
        /*
        var section = ConfigUtil.GetSection<JwtAuthSection>();
        if (section != null)
        {
            app.UseMiddleware<JwtMiddleware>();
        }*/
        return app;
    }
    public static WebApplication UseSessionEx(this WebApplication app)
    {
        var section = ConfigUtil.GetSection<SessionAndCookieSection>();
        if (section != null && section.UseSession)
        {
            app.UseSession();
        }
        return app;
    }
    public static WebApplication UseResponseCachingEx(this WebApplication app)
    {
        var section = ConfigUtil.GetSection<ResponseCachingSection>();
        if (section != null && section.ProfileEnabled)
        {
            app.UseResponseCaching();
        }
        return app;
    }
    public static WebApplication UseSwaggerEx(this WebApplication app)
    {
        var section = ConfigUtil.GetSection<AspNetSection>();
        if (section != null && section.Swagger != null && section.Swagger.Enabled)
        {
            app.UseSwagger(opts =>
            {
            });
            app.UseSwaggerUI(opts =>
            {
                var pathBase = !string.IsNullOrEmpty(section.PathBase)
                    ? $"/{section.PathBase.Trim().TrimStart('/')}" : null;
                var provider = app.Services.GetService<IApiVersionDescriptionProvider>();
                if (provider != null)
                {
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        //var path = $"/swagger/{description.GroupName}/swagger.json";
                        var path = $"{pathBase}/swagger/{description.GroupName}/swagger.json";
                        opts.SwaggerEndpoint(path, description.GroupName.ToUpperInvariant());
                    }
                }
            });
        }
        return app;
    }
    public static WebApplication UseInternalMap(this WebApplication app)
    {
        app.MapHealthChecks("/healthz");
        app.MapGet("/env", () => AspNetHost.MapEnvPath());
        app.MapGet("/dump", (DumpType? t) => AspNetHost.MapDumpPath(t ?? DumpType.WithHeap));
        return app;
    }
    public static WebApplication UseGrpcEx(this WebApplication app)
    {
        GrpcRegisterUtil.UseGrpcEx(app);
        return app;
    }
    public static WebApplication UseSyncNotifyEx(this WebApplication app)
    {
        var section = ConfigUtil.GetSection<ClientSyncNotifySection>();
        if (section != null && section.Enabled)
        {
            app.UseMiddleware<SyncNotifyMiddleware>();
        }
        return app;
    }
    public static WebApplication UseStaticFilesEx(this WebApplication app)
    {
        var section = ConfigUtil.GetSection<ResponseCachingSection>();
        if (section != null && section.StaticEnabled)
        {
            app.UseStaticFiles();
            if (!ConfigUtil.Environment.IsDebug)
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    OnPrepareResponse = ctx =>
                    {
                        var ext = Path.GetExtension(ctx.File.PhysicalPath)?.ToLower() ?? "";
                        var canCache = section.Static.FileDict.Contains(ext);
                        if (canCache && ext.Equals(".js", StringComparison.OrdinalIgnoreCase))
                        {
                            canCache = false;
                            if (ctx.Context.Request.QueryString.HasValue)
                            {
                                var paras = QueryHelpers.ParseQuery(ctx.Context.Request.QueryString.Value);
                                canCache = paras.ContainsKey("v");
                            }
                        }
                        if (canCache)
                        {
                            ctx.Context.Response.Headers[HeaderNames.CacheControl]
                                = $"public, max-age={section.Static.MaxAge}";
                        }
                    }
                });
            }
        }
        return app;
    }
}