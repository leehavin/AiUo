using System;
using System.Collections.Generic;
using System.Collections.Generic;

namespace AiUo.Extensions.SMS
{
    /// <summary>
    /// 短信服务配置选项
    /// </summary>
    public class SmsOptions
    {
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
        
        /// <summary>
        /// 短信模板配置
        /// </summary>
        public IEnumerable<Core.SmsTemplate>? Templates { get; set; }
    }

    /// <summary>
    /// 阿里云短信配置
    /// </summary>
    public class AliyunSmsOptions
    {
        /// <summary>
        /// 访问密钥ID
        /// </summary>
        public string AccessKeyId { get; set; } = string.Empty;

        /// <summary>
        /// 访问密钥密钥
        /// </summary>
        public string AccessKeySecret { get; set; } = string.Empty;

        /// <summary>
        /// 短信签名
        /// </summary>
        public string SignName { get; set; } = string.Empty;

        /// <summary>
        /// 区域ID
        /// </summary>
        public string RegionId { get; set; } = "cn-hangzhou";
    }

    /// <summary>
    /// 腾讯云短信配置
    /// </summary>
    public class TencentSmsOptions
    {
        /// <summary>
        /// 密钥ID
        /// </summary>
        public string SecretId { get; set; } = string.Empty;

        /// <summary>
        /// 密钥
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// 短信应用ID
        /// </summary>
        public string SmsSdkAppId { get; set; } = string.Empty;

        /// <summary>
        /// 短信签名
        /// </summary>
        public string SignName { get; set; } = string.Empty;
    }
}