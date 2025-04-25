using System;

namespace AiUo.Extensions.SMS
{
    /// <summary>
    /// 短信服务配置节
    /// </summary>
    public class SmsSection
    {
        /// <summary>
        /// 配置节名称
        /// </summary>
        public const string SectionName = "SMS";

        /// <summary>
        /// 短信服务提供商
        /// </summary>
        public string Provider { get; set; } = "Aliyun";

        /// <summary>
        /// 阿里云短信配置
        /// </summary>
        public AliyunSmsOptions? Aliyun { get; set; }

        /// <summary>
        /// 腾讯云短信配置
        /// </summary>
        public TencentSmsOptions? Tencent { get; set; }
    }
}