using System;

namespace AiUo.Extensions.Grpc
{
    /// <summary>
    /// 标记一个接口为gRPC服务
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public class GrpcServiceAttribute : Attribute
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// 初始化gRPC服务特性
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        public GrpcServiceAttribute(string serviceName)
        {
            ServiceName = serviceName;
        }
    }

    /// <summary>
    /// 标记一个方法为gRPC方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class GrpcMethodAttribute : Attribute
    {
        /// <summary>
        /// 方法名称
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// 初始化gRPC方法特性
        /// </summary>
        /// <param name="methodName">方法名称</param>
        public GrpcMethodAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
    
    /// <summary>
    /// gRPC服务超时时间特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class GrpcTimeoutAttribute : Attribute
    {
        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        public int TimeoutSeconds { get; }

        /// <summary>
        /// 初始化gRPC服务超时时间特性
        /// </summary>
        /// <param name="timeoutSeconds">超时时间（秒）</param>
        public GrpcTimeoutAttribute(int timeoutSeconds)
        {
            TimeoutSeconds = timeoutSeconds;
        }
    }
    
    /// <summary>
    /// gRPC服务重试策略特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class GrpcRetryAttribute : Attribute
    {
        /// <summary>
        /// 最大重试次数
        /// </summary>
        public int MaxRetries { get; }

        /// <summary>
        /// 重试间隔（毫秒）
        /// </summary>
        public int RetryDelayMilliseconds { get; }

        /// <summary>
        /// 初始化gRPC服务重试策略特性
        /// </summary>
        /// <param name="maxRetries">最大重试次数</param>
        /// <param name="retryDelayMilliseconds">重试间隔（毫秒）</param>
        public GrpcRetryAttribute(int maxRetries = 3, int retryDelayMilliseconds = 200)
        {
            MaxRetries = maxRetries;
            RetryDelayMilliseconds = retryDelayMilliseconds;
        }
    }
}