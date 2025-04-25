using System.Threading.Tasks;

namespace AiUo.Extensions.SMS.Core
{
    /// <summary>
    /// 短信服务接口
    /// </summary>
    public interface ISmsService
    {
        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="message">短信消息</param>
        /// <returns>发送结果</returns>
        SmsResult Send(SmsMessage message);

        /// <summary>
        /// 异步发送短信
        /// </summary>
        /// <param name="message">短信消息</param>
        /// <returns>发送结果</returns>
        Task<SmsResult> SendAsync(SmsMessage message);
    }
}