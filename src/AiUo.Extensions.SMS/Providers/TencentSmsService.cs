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
    /// 腾讯云短信服务实现
    /// </summary>
    public class TencentSmsService : BaseSmsService
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
        public TencentSmsService(IOptions<SmsOptions> options, ILogger<TencentSmsService> logger, 
            IHttpClientFactory httpClientFactory, ISmsTemplateService templateService)
            : base(options, logger)
        {
            _httpClient = httpClientFactory.CreateClient("TencentSms");
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

            if (Options.Tencent == null)
            {
                return SmsResult.CreateFailed("ConfigError", "腾讯云短信配置不存在");
            }

            try
            {
                // 获取供应商特定的模板ID
                string templateId;
                try {
                    templateId = _templateService.GetProviderTemplateId(message.TemplateCode, "Tencent");
                } catch (KeyNotFoundException) {
                    // 如果找不到映射，则直接使用原始模板编码
                    templateId = message.TemplateCode;
                }

                // 这里简化了腾讯云短信API的调用，实际使用时需要按照腾讯云API要求构建请求
                // 包括签名计算、参数组装等，建议使用腾讯云官方SDK
                var templateParamSet = new List<string>();
                foreach (var param in message.TemplateParams)
                {
                    templateParamSet.Add(param.Value);
                }

                var requestData = new Dictionary<string, object>
                {
                    { "PhoneNumberSet", new[] { message.PhoneNumber } },
                    { "SmsSdkAppId", Options.Tencent.SmsSdkAppId },
                    { "SignName", Options.Tencent.SignName },
                    { "TemplateId", templateId },
                    { "TemplateParamSet", templateParamSet },
                    // 其他腾讯云API所需参数
                };

                Logger.LogInformation("正在发送腾讯云短信: {PhoneNumber}, {TemplateCode}", message.PhoneNumber, message.TemplateCode);

                // 模拟API调用，实际使用时替换为真实的腾讯云API调用
                // var response = await _httpClient.PostAsJsonAsync("https://sms.tencentcloudapi.com/", requestData);
                // var result = await response.Content.ReadFromJsonAsync<TencentSmsResponse>();

                // 模拟成功响应
                var mockResult = new { 
                    SendStatusSet = new[] { 
                        new { 
                            Code = "Ok", 
                            Message = "OK", 
                            PhoneNumber = message.PhoneNumber, 
                            SerialNo = Guid.NewGuid().ToString() 
                        } 
                    },
                    RequestId = Guid.NewGuid().ToString() 
                };

                var sendStatus = mockResult.SendStatusSet[0];
                if (sendStatus.Code == "Ok")
                {
                    Logger.LogInformation("腾讯云短信发送成功: {PhoneNumber}, {MessageId}", message.PhoneNumber, sendStatus.SerialNo);
                    return SmsResult.CreateSuccess(sendStatus.SerialNo, mockResult.RequestId);
                }
                else
                {
                    Logger.LogWarning("腾讯云短信发送失败: {PhoneNumber}, {ErrorCode}, {ErrorMessage}", 
                        message.PhoneNumber, sendStatus.Code, sendStatus.Message);
                    return SmsResult.CreateFailed(sendStatus.Code, sendStatus.Message, mockResult.RequestId);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "腾讯云短信发送异常: {PhoneNumber}, {Message}", message.PhoneNumber, ex.Message);
                return SmsResult.CreateFailed("InternalError", $"发送短信时发生异常: {ex.Message}");
            }
        }
    }
}