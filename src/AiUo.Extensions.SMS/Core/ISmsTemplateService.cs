using System.Collections.Generic;
using System.Threading.Tasks;

namespace AiUo.Extensions.SMS.Core
{
    /// <summary>
    /// 短信模板服务接口
    /// </summary>
    public interface ISmsTemplateService
    {
        /// <summary>
        /// 获取模板
        /// </summary>
        /// <param name="code">模板编码</param>
        /// <returns>短信模板</returns>
        SmsTemplate GetTemplate(string code);

        /// <summary>
        /// 异步获取模板
        /// </summary>
        /// <param name="code">模板编码</param>
        /// <returns>短信模板</returns>
        Task<SmsTemplate> GetTemplateAsync(string code);

        /// <summary>
        /// 获取所有模板
        /// </summary>
        /// <returns>模板列表</returns>
        IEnumerable<SmsTemplate> GetAllTemplates();

        /// <summary>
        /// 异步获取所有模板
        /// </summary>
        /// <returns>模板列表</returns>
        Task<IEnumerable<SmsTemplate>> GetAllTemplatesAsync();

        /// <summary>
        /// 根据模板编码获取对应供应商的模板ID
        /// </summary>
        /// <param name="code">模板编码</param>
        /// <param name="provider">供应商名称</param>
        /// <returns>供应商特定的模板ID</returns>
        string GetProviderTemplateId(string code, string provider);
    }
}