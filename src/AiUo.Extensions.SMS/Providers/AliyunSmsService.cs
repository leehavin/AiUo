using AiUo.Extensions.SMS.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace AiUo.Extensions.SMS.Providers
{
    /// <summary>
    /// 阿里云短信服务实现
    /// </summary>
    public class AliyunSmsService : BaseSmsService
    {
        private readonly HttpClient _httpClient;

        private readonly ISmsTemplateService _templateService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options">配置选项</param>
        /// <param name="logger">日志记录器</param>
        /// <param name="httpClientFactory">HTTP客户端工厂</param>
        /// <param name="templateService">模板服务</param>
        public AliyunSmsService(IOptions<SmsOptions> options, ILogger<AliyunSmsService> logger, 
            IHttpClientFactory httpClientFactory, ISmsTemplateService templateService)
            : base(options, logger)
        {
            _httpClient = httpClientFactory.CreateClient("AliyunSms");
            _templateService = templateService;
        }

        /// <summary>
        /// 异步发送短信
        /// </summary>
        /// <param name="message">短信消息</param>
        /// <returns>发送结果</returns>
        public override async Task<SmsResult> SendAsync(SmsMessage message)
        {
            if (!ValidateMessage(message))
            {
                return SmsResult.CreateFailed("InvalidMessage", "短信消息验证失败");
            }

            if (Options.Aliyun == null)
            {
                return SmsResult.CreateFailed("ConfigError", "阿里云短信配置不存在");
            }

            try
            {
                // 获取供应商特定的模板ID
                string templateCode;
                try {
                    templateCode = _templateService.GetProviderTemplateId(message.TemplateCode, "Aliyun");
                } catch (KeyNotFoundException) {
                    // 如果找不到映射，则直接使用原始模板编码
                    templateCode = message.TemplateCode;
                }

                // 这里简化了阿里云短信API的调用，实际使用时需要按照阿里云API要求构建请求
                // 包括签名计算、参数组装等，建议使用阿里云官方SDK
                var requestData = new Dictionary<string, string>
                {
                    { "PhoneNumbers", message.PhoneNumber },
                    { "SignName", Options.Aliyun.SignName },
                    { "TemplateCode", templateCode },
                    { "TemplateParam", JsonSerializer.Serialize(message.TemplateParams) },
                    { "AccessKeyId", Options.Aliyun.AccessKeyId },
                    // 其他阿里云API所需参数
                };

                Logger.LogInformation("正在发送阿里云短信: {PhoneNumber}, {TemplateCode}", message.PhoneNumber, message.TemplateCode);

                // 模拟API调用，实际使用时替换为真实的阿里云API调用
                // var response = await _httpClient.PostAsJsonAsync("https://dysmsapi.aliyuncs.com/", requestData);
                // var result = await response.Content.ReadFromJsonAsync<AliyunSmsResponse>();

                // 模拟成功响应
                var mockResult = new { Code = "OK", Message = "OK", RequestId = Guid.NewGuid().ToString(), BizId = Guid.NewGuid().ToString() };

                if (mockResult.Code == "OK")
                {
                    Logger.LogInformation("阿里云短信发送成功: {PhoneNumber}, {MessageId}", message.PhoneNumber, mockResult.BizId);
                    return SmsResult.CreateSuccess(mockResult.BizId, mockResult.RequestId);
                }
                else
                {
                    Logger.LogWarning("阿里云短信发送失败: {PhoneNumber}, {ErrorCode}, {ErrorMessage}", 
                        message.PhoneNumber, mockResult.Code, mockResult.Message);
                    return SmsResult.CreateFailed(mockResult.Code, mockResult.Message, mockResult.RequestId);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "阿里云短信发送异常: {PhoneNumber}, {Message}", message.PhoneNumber, ex.Message);
                return SmsResult.CreateFailed("InternalError", $"发送短信时发生异常: {ex.Message}");
            }
        }
    }
}