using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiUo.Extensions.SMS.Core
{
    /// <summary>
    /// 短信模板服务实现
    /// </summary>
    public class SmsTemplateService : ISmsTemplateService
    {
        private readonly SmsOptions _options;
        private readonly ILogger _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options">配置选项</param>
        /// <param name="logger">日志记录器</param>
        public SmsTemplateService(IOptions<SmsOptions> options, ILogger<SmsTemplateService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// 获取模板
        /// </summary>
        /// <param name="code">模板编码</param>
        /// <returns>短信模板</returns>
        public SmsTemplate GetTemplate(string code)
        {
            return GetTemplateAsync(code).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 异步获取模板
        /// </summary>
        /// <param name="code">模板编码</param>
        /// <returns>短信模板</returns>
        public Task<SmsTemplate> GetTemplateAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException("模板编码不能为空", nameof(code));
            }

            var template = _options.Templates?.FirstOrDefault(t => t.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            if (template == null)
            {
                _logger.LogWarning("未找到编码为 {Code} 的短信模板", code);
                throw new KeyNotFoundException($"未找到编码为 {code} 的短信模板");
            }

            return Task.FromResult(template);
        }

        /// <summary>
        /// 获取所有模板
        /// </summary>
        /// <returns>模板列表</returns>
        public IEnumerable<SmsTemplate> GetAllTemplates()
        {
            return GetAllTemplatesAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 异步获取所有模板
        /// </summary>
        /// <returns>模板列表</returns>
        public Task<IEnumerable<SmsTemplate>> GetAllTemplatesAsync()
        {
            return Task.FromResult(_options.Templates ?? Enumerable.Empty<SmsTemplate>());
        }

        /// <summary>
        /// 根据模板编码获取对应供应商的模板ID
        /// </summary>
        /// <param name="code">模板编码</param>
        /// <param name="provider">供应商名称</param>
        /// <returns>供应商特定的模板ID</returns>
        public string GetProviderTemplateId(string code, string provider)
        {
            var template = GetTemplate(code);
            if (template.ProviderTemplateIds.TryGetValue(provider, out var templateId))
            {
                return templateId;
            }

            _logger.LogWarning("未找到供应商 {Provider} 对应的模板ID，模板编码: {Code}", provider, code);
            throw new KeyNotFoundException($"未找到供应商 {provider} 对应的模板ID，模板编码: {code}");
        }
    }
}