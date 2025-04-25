using System.Collections.Generic;

namespace AiUo.Extensions.SMS.Core
{
    /// <summary>
    /// 短信模板信息
    /// </summary>
    public class SmsTemplate
    {
        /// <summary>
        /// 模板ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 模板编码，用于在代码中引用
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 模板名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 模板内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 模板类型（验证码、通知、营销等）
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 模板参数说明
        /// </summary>
        public Dictionary<string, string> ParamDescriptions { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 供应商特定的模板ID映射
        /// </summary>
        public Dictionary<string, string> ProviderTemplateIds { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; } = string.Empty;
    }
}