using AiUo.Extensions.SMS.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace AiUo.Extensions.SMS
{
    /// <summary>
    /// 短信工具类
    /// </summary>
    public static class SmsUtil
    {
        private static IServiceProvider? _serviceProvider;

        /// <summary>
        /// 初始化服务提供者
        /// </summary>
        /// <param name="serviceProvider">服务提供者</param>
        internal static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 获取短信服务
        /// </summary>
        /// <returns>短信服务</returns>
        private static ISmsService GetSmsService()
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("短信服务未初始化，请先调用 UseSMS() 方法");
            }

            return _serviceProvider.GetRequiredService<ISmsService>();
        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="message">短信消息</param>
        /// <returns>发送结果</returns>
        public static SmsResult Send(SmsMessage message)
        {
            return GetSmsService().Send(message);
        }

        /// <summary>
        /// 异步发送短信
        /// </summary>
        /// <param name="message">短信消息</param>
        /// <returns>发送结果</returns>
        public static Task<SmsResult> SendAsync(SmsMessage message)
        {
            return GetSmsService().SendAsync(message);
        }
    }
}