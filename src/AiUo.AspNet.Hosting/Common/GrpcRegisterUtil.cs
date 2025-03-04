using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ProtoBuf.Grpc.Configuration;
using ProtoBuf.Grpc.Server;
using System.Reflection;
using System.ServiceModel;
using AiUo.Configuration;
using AiUo.Logging;
using AiUo.Reflection;

namespace AiUo.AspNet.Hosting.Common;

internal static class GrpcRegisterUtil
{
    private static List<Type> _grpcTypes;
    private static List<Assembly> _grpcAssemblies;
    public static WebApplicationBuilder AddGrpcEx(this WebApplicationBuilder builder)
    {
        Init();
        if (_grpcTypes == null || _grpcTypes.Count == 0)
            return builder;

        var grpcPort = ConfigUtil.Service.GrpcPort;
        if (grpcPort <= 0)
            throw new Exception("启动AspNet的Grpc服务时，无效的GrpcPort");
        builder.WebHost.ConfigureKestrel(opts =>
        {
            opts.ListenAnyIP(grpcPort, listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
        });
        builder.Services.AddCodeFirstGrpc();

        var asms = string.Join('|', _grpcAssemblies.Select(x => x.GetName().Name));
        LogUtil.Info("配置 => [Grpc]加载Assemblies: {Assemblies}"
            , asms);

        return builder;
    }
    public static WebApplication UseGrpcEx(this WebApplication app)
    {
        if (_grpcTypes?.Count > 0)
        {
            // 注册
            Type invokeType = typeof(GrpcEndpointRouteBuilderExtensions);
            _grpcTypes.ForEach(grpcType =>
            {
                // app.MapGrpcService<GreeterService>();

                // 反射调用
                //var method = typeof(GrpcEndpointRouteBuilderExtensions)
                //    .GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService))!
                //    .MakeGenericMethod(grpcType);
                //method.Invoke(null, new[] { app });
                ReflectionUtil.InvokeStaticGenericMethod(invokeType, "MapGrpcService", grpcType, app);
                LogUtil.Info($"注册 => Grpc服务 {grpcType.FullName}");
            });
        }
        return app;
    }

    private static void Init()
    {
        var section = ConfigUtil.GetSection<GrpcSection>();
        if (section != null && section.Enabled)
        {
            _grpcTypes = new List<Type>();
            var alllTypes = DIUtil.GetService<IAssemblyContainer>()
                .GetTypes(section.Assemblies, section.AutoLoad, "加载GRPC的Assemblies中项失败。");

            var dict = new HashSet<Assembly>();
            foreach (var type in alllTypes)
            {
                if (!type.IsClass)
                    continue;
                var itypes = type.GetInterfaces();
                if (itypes.Length == 0)
                    continue;
                foreach (var itype in itypes)
                {
                    if (itype.GetCustomAttribute<ServiceContractAttribute>() != null
                        || itype.GetCustomAttribute<ServiceAttribute>() != null)
                    {
                        _grpcTypes.Add(type);
                        break;
                    }
                }
                if(!dict.Contains(type.Assembly))
                    dict.Add(type.Assembly);
            }
            _grpcAssemblies = dict.ToList();
        }
    }
}