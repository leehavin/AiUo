using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;

namespace AiUo.Extensions.Grpc
{
    /// <summary>
    /// gRPC客户端工厂配置选项
    /// </summary>
    public class GrpcClientFactoryOptions
    {
        /// <summary>
        /// 默认连接超时时间(秒)
        /// </summary>
        public int DefaultConnectionTimeoutSeconds { get; set; } = 10;

        /// <summary>
        /// 是否启用SSL
        /// </summary>
        public bool EnableSsl { get; set; } = false;

        /// <summary>
        /// 是否保持通道活跃
        /// </summary>
        public bool KeepChannelAlive { get; set; } = true;

        /// <summary>
        /// 最大重试次数
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// 重试间隔(毫秒)
        /// </summary>
        public int RetryDelayMilliseconds { get; set; } = 200;

        /// <summary>
        /// 自定义通道配置
        /// </summary>
        public Action<GrpcChannelOptions>? ConfigureChannel { get; set; }

        /// <summary>
        /// 自定义HTTP处理程序配置
        /// </summary>
        public Action<SocketsHttpHandler>? ConfigureHttpHandler { get; set; }

        /// <summary>
        /// 是否启用压缩
        /// </summary>
        public bool EnableCompression { get; set; } = false;

        /// <summary>
        /// 最大接收消息大小(字节)
        /// </summary>
        public int? MaxReceiveMessageSize { get; set; } = 50 * 1024 * 1024; // 默认50MB

        /// <summary>
        /// 最大发送消息大小(字节)
        /// </summary>
        public int? MaxSendMessageSize { get; set; } = 50 * 1024 * 1024; // 默认50MB
    }

    /// <summary>
    /// gRPC客户端工厂，用于创建和管理gRPC客户端
    /// </summary>
    public class GrpcClientFactory : IDisposable
    {
        private readonly ILogger<GrpcClientFactory> _logger;
        private readonly GrpcClientFactoryOptions _options;
        private readonly GrpcInterceptorOptions _interceptorOptions;
        private readonly ConcurrentDictionary<string, GrpcChannel> _channels;
        private readonly ConcurrentDictionary<string, object> _clients;
        private bool _disposed;

        /// <summary>
        /// 初始化gRPC客户端工厂
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="options">客户端工厂配置选项</param>
        /// <param name="interceptorOptions">拦截器配置选项</param>
        public GrpcClientFactory(
            ILogger<GrpcClientFactory> logger,
            GrpcClientFactoryOptions options,
            GrpcInterceptorOptions? interceptorOptions = null)
        {
            _logger = logger;
            _options = options;
            _interceptorOptions = interceptorOptions ?? new GrpcInterceptorOptions();
            _channels = new ConcurrentDictionary<string, GrpcChannel>();
            _clients = new ConcurrentDictionary<string, object>();
            _disposed = false;
        }

        /// <summary>
        /// 创建gRPC通道
        /// </summary>
        /// <param name="address">服务地址</param>
        /// <param name="configureOptions">自定义通道配置</param>
        /// <returns>gRPC通道</returns>
        public GrpcChannel CreateChannel(string address, Action<GrpcChannelOptions>? configureOptions = null)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(GrpcClientFactory));

            return _channels.GetOrAdd(address, addr =>
            {
                _logger.LogInformation("创建gRPC通道 - 地址: {Address}", addr);

                var httpHandler = new SocketsHttpHandler
                {
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
                    EnableMultipleHttp2Connections = true,
                    ConnectTimeout = TimeSpan.FromSeconds(_options.DefaultConnectionTimeoutSeconds)
                };

                // 应用自定义HTTP处理程序配置
                _options.ConfigureHttpHandler?.Invoke(httpHandler);

                var channelOptions = new GrpcChannelOptions
                {
                    HttpHandler = httpHandler,
                    MaxReceiveMessageSize = _options.MaxReceiveMessageSize,
                    MaxSendMessageSize = _options.MaxSendMessageSize
                };

                // 应用默认通道配置
                _options.ConfigureChannel?.Invoke(channelOptions);
                
                // 应用自定义通道配置
                configureOptions?.Invoke(channelOptions);

                return GrpcChannel.ForAddress(addr, channelOptions);
            });
        }

        /// <summary>
        /// 从客户端类型获取超时时间
        /// </summary>
        /// <typeparam name="TClient">客户端类型</typeparam>
        /// <param name="defaultTimeout">默认超时时间</param>
        /// <returns>超时时间（秒）</returns>
        private int GetTimeoutFromClientType<TClient>(int defaultTimeout)
            where TClient : class
        {
            var clientType = typeof(TClient);
            var timeoutAttribute = clientType.GetCustomAttributes(typeof(GrpcTimeoutAttribute), true)
                .FirstOrDefault() as GrpcTimeoutAttribute;
            return timeoutAttribute?.TimeoutSeconds ?? defaultTimeout;
        }

        /// <summary>
        /// 创建gRPC客户端
        /// </summary>
        /// <typeparam name="TClient">客户端类型</typeparam>
        /// <param name="address">服务地址</param>
        /// <param name="configureClient">客户端配置</param>
        /// <returns>gRPC客户端</returns>
        public TClient CreateClient<TClient>(string address, Action<TClient>? configureClient = null)
            where TClient : class
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(GrpcClientFactory));

            var clientType = typeof(TClient);
            var cacheKey = $"{clientType.FullName}_{address}";

            if (_clients.TryGetValue(cacheKey, out var cachedClient))
            {
                return (TClient)cachedClient;
            }
            
            // 应用GrpcAttributes配置

            var channel = CreateChannel(address);
            // 创建适合GrpcInterceptor的日志记录器
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton(_interceptorOptions)
                .BuildServiceProvider();
            var interceptorLogger = serviceProvider.GetRequiredService<ILogger<GrpcInterceptor>>();
            var interceptor = new GrpcInterceptor(interceptorLogger, _interceptorOptions);
            var interceptedChannel = channel.Intercept(interceptor);

            // 使用反射创建客户端实例
            var client = (TClient)Activator.CreateInstance(
                clientType, 
                interceptedChannel)!;

            configureClient?.Invoke(client);

            _clients[cacheKey] = client;
            return client;
        }

        /// <summary>
        /// 创建gRPC客户端（带自定义通道配置）
        /// </summary>
        /// <typeparam name="TClient">客户端类型</typeparam>
        /// <param name="address">服务地址</param>
        /// <param name="configureChannel">通道配置</param>
        /// <param name="configureClient">客户端配置</param>
        /// <returns>gRPC客户端</returns>
        public TClient CreateClient<TClient>(
            string address, 
            Action<GrpcChannelOptions> configureChannel, 
            Action<TClient>? configureClient = null)
            where TClient : class
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(GrpcClientFactory));

            var clientType = typeof(TClient);
            var cacheKey = $"{clientType.FullName}_{address}";

            if (_clients.TryGetValue(cacheKey, out var cachedClient))
            {
                return (TClient)cachedClient;
            }

            var channel = CreateChannel(address, configureChannel);
            // 创建适合GrpcInterceptor的日志记录器
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton(_interceptorOptions)
                .BuildServiceProvider();
            var interceptorLogger = serviceProvider.GetRequiredService<ILogger<GrpcInterceptor>>();
            var interceptor = new GrpcInterceptor(interceptorLogger, _interceptorOptions);
            var interceptedChannel = channel.Intercept(interceptor);

            // 使用反射创建客户端实例
            var client = (TClient)Activator.CreateInstance(
                clientType, 
                interceptedChannel)!;

            configureClient?.Invoke(client);

            _clients[cacheKey] = client;
            return client;
        }

        /// <summary>
        /// 尝试创建gRPC客户端，如果失败则返回false
        /// </summary>
        /// <typeparam name="TClient">客户端类型</typeparam>
        /// <param name="address">服务地址</param>
        /// <param name="client">输出的客户端实例</param>
        /// <param name="configureClient">客户端配置</param>
        /// <returns>是否成功创建客户端</returns>
        public bool TryCreateClient<TClient>(
            string address, 
            out TClient client, 
            Action<TClient>? configureClient = null)
            where TClient : class
        {
            client = default!;
            
            try
            {
                client = CreateClient<TClient>(address, configureClient);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建gRPC客户端失败 - 类型: {ClientType}, 地址: {Address}", typeof(TClient).Name, address);
                return false;
            }
        }

        /// <summary>
        /// 使用重试策略执行gRPC调用
        /// </summary>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="func">gRPC调用函数</param>
        /// <returns>调用结果</returns>
        public async Task<TResult> CallWithRetryAsync<TResult>(Func<Task<TResult>> func)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(GrpcClientFactory));

            int retryCount = 0;
            Exception? lastException = null;

            while (retryCount <= _options.MaxRetries)
            {
                try
                {
                    if (retryCount > 0)
                    {
                        _logger.LogWarning("gRPC调用重试 - 第{RetryCount}次尝试", retryCount);
                        await Task.Delay(_options.RetryDelayMilliseconds * retryCount);
                    }

                    return await func();
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable ||
                                            ex.StatusCode == StatusCode.DeadlineExceeded ||
                                            ex.StatusCode == StatusCode.ResourceExhausted)
                {
                    lastException = ex;
                    _logger.LogWarning(ex, "gRPC调用失败 - 状态码: {StatusCode}, 将进行重试", ex.StatusCode);
                    retryCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "gRPC调用发生不可重试的异常");
                    throw;
                }
            }

            _logger.LogError("gRPC调用在{MaxRetries}次重试后仍然失败", _options.MaxRetries);
            throw lastException ?? new RpcException(new Status(StatusCode.Internal, "最大重试次数已用尽"));
        }

        /// <summary>
        /// 尝试使用重试策略执行gRPC调用，如果失败则返回默认值
        /// </summary>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="func">gRPC调用函数</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>调用结果或默认值</returns>
        public async Task<TResult> TryCallWithRetryAsync<TResult>(Func<Task<TResult>> func, TResult defaultValue)
        {
            try
            {
                return await CallWithRetryAsync(func);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "gRPC调用失败并返回默认值");
                return defaultValue;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _clients.Clear();

            foreach (var channel in _channels.Values)
            {
                try
                {
                    channel.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "关闭gRPC通道时发生异常");
                }
            }

            _channels.Clear();
            GC.SuppressFinalize(this);
        }
    }
}