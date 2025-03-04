using AiUo.AspNet;
using AiUo.AspNet.Common;
using AiUo.AspNet.Filters;
using AiUo.AspNet.Hosting;
using AiUo.AspNet.Hosting.Common;
using AiUo.Configuration;
using AiUo.Logging;
using AiUo.Reflection;
using AiUo.Security;
using AiUo.Xml;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.IO.Compression;
using System.Runtime.Loader;

namespace AiUo;

public static class AspNetWebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddAspNetEx(this WebApplicationBuilder builder)
    {
        // Kestrel
        var section = ConfigUtil.GetSection<AspNetSection>();
        if (section != null)
        {
            // REST
            builder.WebHost.ConfigureKestrel(opts =>
            {
                if (ConfigUtil.Service.HttpPort <= 0)
                    throw new Exception("启动AspNet的REST服务时，无效的HttpPort");
                opts.ListenAnyIP(ConfigUtil.Service.HttpPort, listenOptions => listenOptions.Protocols = HttpProtocols.Http1);

                //MinRequestBodyDataRate
                if (section.RequestBytesPerSecond > 0 && section.RequestPeriodSecond > 0)
                {
                    var bytesPerSecond = section.RequestBytesPerSecond;
                    var gracePeriod = TimeSpan.FromSeconds(section.RequestPeriodSecond);
                    opts.Limits.MinRequestBodyDataRate = new MinDataRate(bytesPerSecond, gracePeriod);
                    opts.Limits.MinResponseDataRate = new MinDataRate(bytesPerSecond, gracePeriod);
                }
                else if (section.RequestBytesPerSecond < 0 || section.RequestPeriodSecond < 0)
                {
                    // 不强制执行最低数据速率规则
                    opts.Limits.MinRequestBodyDataRate = null;
                    opts.Limits.MinResponseDataRate = null;
                }
            });
            //AiUoHostingStartupLoader 替换IHostingStartup
            //var asms = string.Join(";", section.HostingStartupAssemblies);
            //builder.WebHost.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, asms);
        }
        builder.AddGrpcEx();
        AddAspNetExDetail(builder.Services);
        //
        AiUoHostingStartupLoader.Instance.ConfigureServices(builder);

        builder.SetEnvironment();
        return builder;
    }
    private static WebApplicationBuilder SetEnvironment(this WebApplicationBuilder builder)
    {
        switch (ConfigUtil.Environment.Type)
        {
            case EnvironmentType.DEV:
            case EnvironmentType.SIT:
            case EnvironmentType.FAT:
                builder.Environment.EnvironmentName = Environments.Development;
                break;
            case EnvironmentType.UAT:
                builder.Environment.EnvironmentName = Environments.Staging;
                break;
            case EnvironmentType.PRE:
            case EnvironmentType.PRO:
                builder.Environment.EnvironmentName = Environments.Production;
                break;
        }
        builder.Environment.ApplicationName = ConfigUtil.Project.ProjectId;
        return builder;
    }
    private static WebApplicationBuilder AddGrpcEx(this WebApplicationBuilder builder)
    {
        GrpcRegisterUtil.AddGrpcEx(builder);
        return builder;
    }
    private static IServiceCollection AddAspNetExDetail(this IServiceCollection services)
    {
        services.AddControllersEx()
            .AddDynamicApi();
        // 解决Multipart body length limit 134217728 exceeded
        services.Configure<FormOptions>(x =>
        {
            x.ValueLengthLimit = int.MaxValue;
            x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
        });
        LogUtil.Info($"注册 => AddControllers");

        services.AddHealthChecks();         // health
        services
            .AddRequestLoggingEx()          // LogBuilder
            .AddCorsEx()                    // Cors
            .AddApiVersioningEx()           // ApiVersion
            .AddSwaggerGenEx()              // Swagger
            .AddApiJwtAuthEx()              // Jwt
            .AddSessionAndCookieEx()        // SessionOrCookie 
            .AddResponseCachingEx()         // ResponseCaching
            .AddResponseCompressionEx()     // ResponseCompression
            .AddForwardedHeadersEx()          // ForwardedHeaders

            .AddHttpContextAccessor()       // .AddOAuth();      // IHttpContextAccessor
            .AddSyncNotifyEx();              // SyncNotify

        return services;
    }
    public static IServiceCollection AddRequestLoggingEx(this IServiceCollection services)
    {
        services.AddScoped<ILogBuilder>((_) =>
        {
            return new LogBuilder()
            {
                IsContext = true
            };
        });
        return services;
    }
    public static IMvcBuilder AddControllersEx(this IServiceCollection services)
    {
        var section = ConfigUtil.GetSection<AspNetSection>();
        return services.AddControllers(options =>
        {
            if (section == null || section.UseApiActionResultFilter)
                options.Filters.Add(typeof(ApiActionResultFilter));
            if (section == null || section.UseModelStateFilter)
                options.Filters.Add(typeof(ValidateModelFilter));

            // 等同设置Nullable=false
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            // 设置缓存
            var rcSection = ConfigUtil.GetSection<ResponseCachingSection>();
            if (rcSection != null && rcSection.ProfileEnabled)
            {
                if (rcSection.Profiles?.Count > 0)
                {
                    foreach (var profile in rcSection.Profiles)
                    {
                        options.CacheProfiles.Add(profile.Key, profile.Value);
                    }
                }
                LogUtil.Trace($"配置 => [ResponseCaching]");
            }
        }).AddJsonOptions(options =>
        {
            SerializerUtil.ConfigJsonOptions(options.JsonSerializerOptions);
        }).ConfigureApiBehaviorOptions(options =>
        {
            // 禁用[ApiController]的自动 400 响应
            if (section != null && section.UseModelStateFilter)
            {
                options.SuppressModelStateInvalidFilter = true;
            }
        });
    }

    public static IMvcBuilder AddDynamicApi(this IMvcBuilder builder)
    {
        var section = ConfigUtil.GetSection<AspNetSection>();
        var list = section?.DynamicApiAssemblies?.FindAll(x => !string.IsNullOrEmpty(x?.Trim()));
        if (list?.Count > 0)
        {

            builder.ConfigureApplicationPartManager(mgr =>
            {
                foreach (var path in list)
                {
                    DynamicApiUtil.Add(path, mgr);
                }
            });
#pragma warning disable ASP5001 // 类型或成员已过时
#pragma warning disable CS0618 // 类型或成员已过时
            builder.SetCompatibilityVersion(CompatibilityVersion.Latest);
#pragma warning restore CS0618 // 类型或成员已过时
#pragma warning restore ASP5001 // 类型或成员已过时
        }
        return builder;
    }
    /// <summary>
    /// CORS
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCorsEx(this IServiceCollection services)
    {
        var section = ConfigUtil.GetSection<CorsSection>();
        if (section == null || !section.UseCors.Enabled)
            return services;

        services.AddCors(opts =>
        {
            section.AddPolicies(opts);
        });
        LogUtil.Info($"注册 => Cors");
        return services;
    }
    /// <summary>
    /// Versioning
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddApiVersioningEx(this IServiceCollection services)
    {
        var section = ConfigUtil.GetSection<AspNetSection>();
        if (section != null && section.UseApiVersioning)
        {
            services.AddApiVersioning(opts =>
                {
                    opts.DefaultApiVersion = new ApiVersion(1, 0);
                    opts.AssumeDefaultVersionWhenUnspecified = true; // 不提供版本时，默认为1.0
                    opts.ReportApiVersions = true; //API返回支持的版本信息
                    opts.ApiVersionReader = new UrlSegmentApiVersionReader();
                    //options.ApiVersionReader = ApiVersionReader.Combine(
                    //    new UrlSegmentApiVersionReader(),
                    //    new QueryStringApiVersionReader("api-version"),
                    //    new HeaderApiVersionReader("x-api-version"),
                    //    new MediaTypeApiVersionReader("x-api-version")
                    //);

                    //默认以当前最高版本进行访问
                    //opts.ApiVersionSelector = new CurrentImplementationApiVersionSelector(opts);
                })
                .AddMvc(options =>
                {
                    // 根据定义控制器的命名空间的名称自动应用 api 版本
                    options.Conventions.Add(new VersionByNamespaceConvention());
                })
                .AddApiExplorer(setup =>
                {
                    setup.GroupNameFormat = "'v'VVV";
                    setup.SubstituteApiVersionInUrl = true;
                });
            LogUtil.Trace($"注册 => ApiVersioning");
        }
        return services;
    }

    /// <summary>
    /// Swagger
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSwaggerGenEx(this IServiceCollection services)
    {
        var section = ConfigUtil.GetSection<AspNetSection>();
        if (section == null || section.Swagger == null || !section.Swagger.Enabled)
            return services;

        //services.AddEndpointsApiExplorer(); //只有最小 API 调用
        services.AddSwaggerGen(opts =>
        {
            if (section.Swagger.UseSchemaFullName)
                opts.CustomSchemaIds(x => x.FullName?.Replace('+', '-'));

            // 添加承载身份验证的安全定义和要求
            opts.AddSecurityDefinition("JwtAuth", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                In = ParameterLocation.Header,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Description = "JWT授权 ==> 输入框输入token"
            });
            opts.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "JwtAuth" }
                    },
                    new string[] {}
                }
            });
            opts.IncludeXmlComments(() =>
            {
                var xmlFiles = new List<string>();
                foreach (var asm in AssemblyLoadContext.Default.Assemblies)
                {
                    if (asm.IsDynamic || asm.GetName().GetPublicKey()?.Length > 0)
                        continue;
                    var name = $"{Path.GetFileNameWithoutExtension(asm.Location)}.xml";
                    var path = Path.Combine(AppContext.BaseDirectory, name);
                    if (System.IO.File.Exists(path))
                        xmlFiles.Add(path);
                }
                var xmlParser = new XmlDocumentParser(xmlFiles);
                return xmlParser.Document;
            }, true);
        });
        if (section.UseApiVersioning)
        {
            services.ConfigureOptions<NamedSwaggerGenOptions>();
        }
        LogUtil.Info($"注册 => Swagger");
        return services;
    }

    /// <summary>
    /// Api采用jwt验证的设置。
    /// Configure中添加
    ///     app.UseAuthentication();
    ///     app.UseAuthorization();
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddApiJwtAuthEx(this IServiceCollection services)
    {
        var section = ConfigUtil.GetSection<JwtAuthSection>();
        if (section != null && section.Enabled)
        {
            if (string.IsNullOrEmpty(section.SigningKey))
                throw new Exception("配置文件ApiJwtAuth:SignSecret不能为空");
            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = JwtUtil.GetParameters(section);
                    // 处理jwt事件
                    x.Events = new TinyJwtBearerEvents();
                });
            LogUtil.Info($"注册 => JwtAuth");
        }

        return services;
    }
    /// <summary>
    /// Session保存在Redis中
    /// Configure中添加
    ///     app.UseSession();
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSessionAndCookieEx(this IServiceCollection services)
    {
        var section = ConfigUtil.GetSection<SessionAndCookieSection>();
        if (section == null || (!section.UseSession && !section.UseCookieIdentity))
            return services;

        // 配置Cookie登录
        if (section.UseCookieIdentity)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(opts =>
                {
                    opts.SlidingExpiration = true; //自动延期
                    opts.Cookie.HttpOnly = true; //禁止js访问
                    opts.Cookie.IsEssential = true;//绕过GDPR

                    opts.Cookie.Name = $".{ConfigUtil.Project.ApplicationName}.Identity";
                    opts.ExpireTimeSpan = (section.CookieTimeout == 0)
                        ? TimeSpan.FromDays(3)
                        : TimeSpan.FromDays(section.CookieTimeout);
                    opts.Cookie.Path = "/";// 跨基路径共享
                    if (!string.IsNullOrEmpty(section.Domain))//跨不同子域共享 .xxx.com
                        opts.Cookie.Domain = section.Domain;
                    opts.Cookie.SameSite = section.SameSiteMode;
                    opts.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                });
        }

        // 配置Sesion
        if (section.UseSession)
        {
            services.AddSession(opts =>
            {
                opts.Cookie.HttpOnly = true; //禁止js访问
                opts.Cookie.IsEssential = true;//绕过GDPR

                opts.Cookie.Name = $".{ConfigUtil.Project.ApplicationName}.Session";
                opts.IdleTimeout = (section.SessionTimeout == 0)
                    ? TimeSpan.FromMinutes(20)
                    : TimeSpan.FromMinutes(section.SessionTimeout);
                opts.Cookie.Path = "/";
                if (!string.IsNullOrEmpty(section.Domain))
                    opts.Cookie.Domain = section.Domain;
                opts.Cookie.SameSite = section.SameSiteMode;
                opts.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });
        }

        LogUtil.Info("注册 => SessionAndRedis [UseSession: {session} UseCookie: {cookie}]"
            , section.UseSession, section.UseCookieIdentity);
        return services;
    }
    public static IServiceCollection AddResponseCachingEx(this IServiceCollection services)
    {
        var section = ConfigUtil.GetSection<ResponseCachingSection>();
        if (section == null || !section.ProfileEnabled)
            return services;

        if (section.Profiles?.Count > 0)
        {
            services.AddResponseCaching();
        }
        LogUtil.Debug($"注册 => ResponseCaching");
        return services;
    }
    public static IServiceCollection AddResponseCompressionEx(this IServiceCollection services)
    {
        var section = ConfigUtil.GetSection<AspNetSection>();
        if (section == null || !section.UseResponseCompression)
            return services;

        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });
        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.SmallestSize;
        });
        LogUtil.Info($"注册 => ResponseCompression");
        return services;
    }
    public static IServiceCollection AddForwardedHeadersEx(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
            //options.KnownNetworks.Clear();
            //options.KnownProxies.Clear();
            // ASPNETCORE_FORWARDEDHEADERS_ENABLED true
        });
        return services;
    }
    public static IServiceCollection AddSyncNotifyEx(this IServiceCollection services)
    {
        var section = ConfigUtil.GetSection<ClientSyncNotifySection>();
        // 总是存在
        var provider = section == null || string.IsNullOrEmpty(section.NotifyProvider)
            ? new RedisSyncNotifyProvider()
            : ReflectionUtil.CreateInstance(section.NotifyProvider) as IClientSyncNotifyProvider;
        services.AddSingleton(provider);
        return services;
    }
}