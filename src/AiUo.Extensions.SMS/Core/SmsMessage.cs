using System;
using System.Collections.Generic;

namespace AiUo.Extensions.SMS.Core
{
    /// <summary>
    /// 短信消息模型
    /// </summary>
    public class SmsMessage
    {
        /// <summary>
        /// 手机号码
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// 模板编码
        /// </summary>
        public string TemplateCode { get; set; } = string.Empty;

        /// <summary>
        /// 模板参数
        /// </summary>
        public Dictionary<string, string> TemplateParams { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 扩展字段，用于传递额外参数
        /// </summary>
        public Dictionary<string, object> ExtendData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// 短信发送结果
    /// </summary>
    public class SmsResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 错误码
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 请求ID
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// 短信发送ID
        /// </summary>
        public string? MessageId { get; set; }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        /// <param name="messageId">短信发送ID</param>
        /// <param name="requestId">请求ID</param>
        /// <returns>成功结果</returns>
        public static SmsResult CreateSuccess(string messageId, string requestId = "")
        {
            return new SmsResult
            {
                Success = true,
                MessageId = messageId,
                RequestId = requestId
            };
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <param name="errorCode">错误码</param>
        /// <param name="errorMessage">错误消息</param>
        /// <param name="requestId">请求ID</param>
        /// <returns>失败结果</returns>
        public static SmsResult CreateFailed(string errorCode, string errorMessage, string requestId = "")
        {
            return new SmsResult
            {
                Success = false,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage,
                RequestId = requestId
            };
        }
    }
}