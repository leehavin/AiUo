using AiUo.Extensions.SMS;
using AiUo.Extensions.SMS.Core;
using AiUo.Extensions.SMS.Providers;
using AiUo.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmsDemo
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("短信服务示例程序启动...");

            // 创建主机
            var host = AiUoHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureAiUo(aiuo =>
                {
                    // 注册短信服务
                    aiuo.UseSMS();
                })
                .ConfigureServices((context, services) =>
                {
                    // 注册应用服务
                    services.AddTransient<SmsTestService>();
                })
                .Build();

            // 获取服务并执行测试
            var smsTestService = host.Services.GetRequiredService<SmsTestService>();
            await smsTestService.RunTestAsync();

            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// 短信测试服务
    /// </summary>
    public class SmsTestService
    {
        private readonly ILogger<SmsTestService> _logger;
        private readonly ISmsService _smsService;
        private readonly ISmsTemplateService _templateService;

        public SmsTestService(ILogger<SmsTestService> logger, ISmsService smsService, ISmsTemplateService templateService)
        {
            _logger = logger;
            _smsService = smsService;
            _templateService = templateService;
        }

        /// <summary>
        /// 运行测试
        /// </summary>
        public async Task RunTestAsync()
        {
            try
            {
                // 创建短信消息
                var message = new SmsMessage
                {
                    PhoneNumber = "13800138000",
                    TemplateCode = "SMS_123456789",
                    TemplateParams = new Dictionary<string, string>
                    {
                        { "code", "123456" }
                    }
                };

                Console.WriteLine($"正在发送短信到 {message.PhoneNumber}，模板编码：{message.TemplateCode}");

                // 发送短信
                var result = await _smsService.SendAsync(message);

                // 输出结果
                if (result.Success)
                {
                    Console.WriteLine($"短信发送成功！消息ID：{result.MessageId}，请求ID：{result.RequestId}");
                }
                else
                {
                    Console.WriteLine($"短信发送失败！错误码：{result.ErrorCode}，错误信息：{result.ErrorMessage}，请求ID：{result.RequestId}");
                }

                // 测试模板服务
                Console.WriteLine("\n测试短信模板服务...");
                await TestTemplateService();
                
                // 测试不同平台
                Console.WriteLine("\n测试不同平台的短信发送...");
                await TestDifferentProviders();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "短信测试过程中发生异常：{Message}", ex.Message);
                Console.WriteLine($"发生异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 测试模板服务
        /// </summary>
        private async Task TestTemplateService()
        {
            Console.WriteLine("\n测试短信模板服务...");
            try
            {
                // 获取所有模板
                var templates = await _templateService.GetAllTemplatesAsync();
                Console.WriteLine($"共找到 {templates.Count()} 个短信模板：");
                foreach (var template in templates)
                {
                    Console.WriteLine($"- {template.Code}: {template.Name}");
                }

                // 获取特定模板
                var verificationTemplate = await _templateService.GetTemplateAsync("verification_code");
                Console.WriteLine($"\n验证码模板详情：");
                Console.WriteLine($"名称: {verificationTemplate.Name}");
                Console.WriteLine($"内容: {verificationTemplate.Content}");
                Console.WriteLine($"类型: {verificationTemplate.Type}");
                Console.WriteLine("参数说明:");
                foreach (var param in verificationTemplate.ParamDescriptions)
                {
                    Console.WriteLine($"  - {param.Key}: {param.Value}");
                }
                
                // 获取供应商特定的模板ID
                var aliyunTemplateId = _templateService.GetProviderTemplateId("verification_code", "Aliyun");
                var tencentTemplateId = _templateService.GetProviderTemplateId("verification_code", "Tencent");
                Console.WriteLine($"\n供应商特定的模板ID：");
                Console.WriteLine($"阿里云: {aliyunTemplateId}");
                Console.WriteLine($"腾讯云: {tencentTemplateId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试模板服务时发生异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 测试不同平台的短信发送
        /// </summary>
        private async Task TestDifferentProviders()
        {
            // 阿里云短信
            Console.WriteLine("\n测试阿里云短信发送...");
            var aliyunMessage = new SmsMessage
            {
                PhoneNumber = "13800138000",
                TemplateCode = "verification_code", // 使用模板编码
                TemplateParams = new Dictionary<string, string>
                {
                    { "code", "123456" },
                    { "minutes", "5" }
                }
            };

            var aliyunResult = await _smsService.SendAsync(aliyunMessage);
            Console.WriteLine(aliyunResult.Success
                ? $"阿里云短信发送成功！消息ID：{aliyunResult.MessageId}"
                : $"阿里云短信发送失败！错误码：{aliyunResult.ErrorCode}，错误信息：{aliyunResult.ErrorMessage}");

            // 腾讯云短信
            Console.WriteLine("\n测试腾讯云短信发送...");
            var tencentMessage = new SmsMessage
            {
                PhoneNumber = "13800138000",
                TemplateCode = "order_notification", // 使用另一个模板编码
                TemplateParams = new Dictionary<string, string>
                {
                    { "order_no", "ORD20230101001" },
                    { "amount", "99.99" }
                }
            };

            // 临时切换到腾讯云供应商
            var tencentSmsService = _smsService is Providers.TencentSmsService
                ? (Providers.TencentSmsService)_smsService
                : new Providers.TencentSmsService(
                    Microsoft.Extensions.Options.Options.Create(new SmsOptions { Provider = "Tencent" }),
                    _logger.BeginScope<Providers.TencentSmsService>() as ILogger<Providers.TencentSmsService> ?? _logger as ILogger<Providers.TencentSmsService>,
                    new System.Net.Http.HttpClient(),
                    _templateService);
            var tencentResult = await tencentSmsService.SendAsync(tencentMessage);
            Console.WriteLine(tencentResult.Success
                ? $"腾讯云短信发送成功！消息ID：{tencentResult.MessageId}"
                : $"腾讯云短信发送失败！错误码：{tencentResult.ErrorCode}，错误信息：{tencentResult.ErrorMessage}");
        }
    }
}