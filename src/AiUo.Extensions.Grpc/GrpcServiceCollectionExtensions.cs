using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace AiUo.Extensions.Grpc
{
    /// <summary>
    /// gRPC服务扩展方法
    /// </summary>
    public static class GrpcServiceCollectionExtensions
    {
        /// <summary>
        /// 添加gRPC客户端工厂
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configureOptions">配置选项</param>
        /// <param name="configureInterceptor">配置拦截器</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddGrpcClientFactory(
            this IServiceCollection services,
            Action<GrpcClientFactoryOptions>? configureOptions = null,
            Action<GrpcInterceptorOptions>? configureInterceptor = null)
        {
            // 注册客户端工厂选项
            var clientOptions = new GrpcClientFactoryOptions();
            configureOptions?.Invoke(clientOptions);
            services.AddSingleton(clientOptions);

            // 注册拦截器选项
            var interceptorOptions = new GrpcInterceptorOptions();
            configureInterceptor?.Invoke(interceptorOptions);
            services.AddSingleton(interceptorOptions);

            // 注册客户端工厂
            services.AddSingleton<GrpcClientFactory>();

            return services;
        }

        /// <summary>
        /// 添加gRPC客户端
        /// </summary>
        /// <typeparam name="TClient">客户端类型</typeparam>
        /// <param name="services">服务集合</param>
        /// <param name="address">服务地址</param>
        /// <param name="configureClient">配置客户端</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddGrpcClient<TClient>(
            this IServiceCollection services,
            string address,
            Action<TClient>? configureClient = null)
            where TClient : class
        {
            services.AddSingleton(sp =>
            {
                var factory = sp.GetRequiredService<GrpcClientFactory>();
                return factory.CreateClient<TClient>(address, configureClient);
            });

            return services;
        }

        /// <summary>
        /// 添加gRPC客户端（带名称）
        /// </summary>
        /// <typeparam name="TClient">客户端类型</typeparam>
        /// <param name="services">服务集合</param>
        /// <param name="name">客户端名称</param>
        /// <param name="address">服务地址</param>
        /// <param name="configureClient">配置客户端</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddGrpcClientNamed<TClient>(
            this IServiceCollection services,
            string name,
            string address,
            Action<TClient>? configureClient = null)
            where TClient : class
        {
            services.AddKeyedSingleton<TClient>(name, (sp, _) =>
            {
                var factory = sp.GetRequiredService<GrpcClientFactory>();
                return factory.CreateClient<TClient>(address, configureClient);
            });

            return services;
        }

        /// <summary>
        /// 添加gRPC客户端（带配置）
        /// </summary>
        /// <typeparam name="TClient">客户端类型</typeparam>
        /// <param name="services">服务集合</param>
        /// <param name="address">服务地址</param>
        /// <param name="configureOptions">配置选项</param>
        /// <param name="configureClient">配置客户端</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddGrpcClientWithOptions<TClient>(
            this IServiceCollection services,
            string address,
            Action<GrpcClientFactoryOptions> configureOptions,
            Action<TClient>? configureClient = null)
            where TClient : class
        {
            services.AddGrpcClientFactory(configureOptions);
            return services.AddGrpcClient<TClient>(address, configureClient);
        }
    }
}