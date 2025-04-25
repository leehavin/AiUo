using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace AiUo.Extensions.SMS.Core
{
    /// <summary>
    /// 短信服务抽象基类
    /// </summary>
    public abstract class BaseSmsService : ISmsService
    {
        /// <summary>
        /// 配置选项
        /// </summary>
        protected readonly SmsOptions Options;

        /// <summary>
        /// 日志记录器
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options">配置选项</param>
        /// <param name="logger">日志记录器</param>
        protected BaseSmsService(IOptions<SmsOptions> options, ILogger logger)
        {
            Options = options.Value;
            Logger = logger;
        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="message">短信消息</param>
        /// <returns>发送结果</returns>
        public virtual SmsResult Send(SmsMessage message)
        {
            try
            {
                return SendAsync(message).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "发送短信时发生异常: {Message}", ex.Message);
                return SmsResult.CreateFailed("InternalError", $"发送短信时发生异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 异步发送短信
        /// </summary>
        /// <param name="message">短信消息</param>
        /// <returns>发送结果</returns>
        public abstract Task<SmsResult> SendAsync(SmsMessage message);

        /// <summary>
        /// 验证短信消息
        /// </summary>
        /// <param name="message">短信消息</param>
        /// <returns>验证结果</returns>
        protected virtual bool ValidateMessage(SmsMessage message)
        {
            if (string.IsNullOrEmpty(message.PhoneNumber))
            {
                Logger.LogWarning("手机号码不能为空");
                return false;
            }

            if (string.IsNullOrEmpty(message.TemplateCode))
            {
                Logger.LogWarning("模板编码不能为空");
                return false;
            }

            return true;
        }
    }
}