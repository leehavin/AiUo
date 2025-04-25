using AiUo.Extensions.SMS.Core;
using AiUo.Extensions.SMS.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AiUo.Extensions.SMS
{
    /// <summary>
    /// 主机构建器扩展
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// 添加短信服务
        /// </summary>
        /// <param name="builder">主机构建器</param>
        /// <param name="sectionName">配置节名称</param>
        /// <returns>主机构建器</returns>
        public static IHostBuilder AddSmsService(this IHostBuilder builder, string sectionName = "SMS")
        {
            return builder.ConfigureServices((context, services) =>
            {
                services.Configure<SmsOptions>(context.Configuration.GetSection(sectionName));
                services.AddHttpClient();

                // 注册短信模板服务
                services.AddTransient<ISmsTemplateService, SmsTemplateService>();

                var provider = context.Configuration.GetValue<string>($"{sectionName}:Provider");
                switch (provider?.ToLower())
                {
                    case "tencent":
                        services.AddTransient<ISmsService, TencentSmsService>();
                        break;
                    case "aliyun":
                    default:
                        services.AddTransient<ISmsService, AliyunSmsService>();
                        break;
                }
            });
        }
    }
}