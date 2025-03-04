using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using AiUo.Collections;
using AiUo.Configuration;
using AiUo.Net;
using AiUo.Serialization;

namespace AiUo.Configuration
{
    /// <summary>
    /// HttpClient配置节
    /// </summary>
    public class HttpClientSection : ConfigSection
    {
        public override string SectionName => "HttpClient";
        //public string DefaultClientName { get; set; }
        public Dictionary<string, HttpClientConfig> Clients = new Dictionary<string, HttpClientConfig>();
        public HttpClientConfig GetClient(string name)
            => Clients[name];
        public override void Bind(IConfiguration configuration)
        {
            base.Bind(configuration);
            Clients = configuration
                .Get<Dictionary<string, HttpClientConfig>>() ?? new();
            Clients.ForEach(x =>
            {
                if (string.IsNullOrEmpty(x.Value.Name))
                    x.Value.Name = x.Key;
                x.Value.Settings = configuration.GetSection($"{x.Key}:Settings")
                    .Get<Dictionary<string, string>>() ?? new();
            });
        }

        public static HttpClientConfig GetClientConfig(string clientName)
        {
            var section = ConfigUtil.GetSection<HttpClientSection>();
            if (section == null || section.Clients == null || !section.Clients.TryGetValue(clientName, out var ret))
                throw new Exception($"配置文件中HttpClient:Clients没有配置name: {clientName}");
            return ret;
        }
        public static T GetClientSettingValue<T>(string clientName, string key)
        {
            var config = GetClientConfig(clientName);
            if (!config.Settings.TryGetValue(key, out var value))
                throw new Exception($"配置文件HttpClient:Clients:{clientName}:Settings没有配置key:{key}");
            return value.To<T>();
        }
    }
}

namespace AiUo.Net
{
    /// <summary>
    /// HttpClient配置信息
    /// </summary>
    public class HttpClientConfig
    {
        public string Name { get; set; }
        /// <summary>
        /// 基地址URL
        /// </summary>
        public string BaseAddress { get; set; }
        /// <summary>
        /// Headers
        /// </summary>
        public List<KeyValueItem> RequestHeaders { get; set; } = new List<KeyValueItem>();
        /// <summary>
        /// 请求超时时长（毫秒）默认30秒
        /// </summary>
        public int Timeout { get; set; } = 30000;
        public bool UseCookies { get; set; } = false;
        /// <summary>
        /// 是否忽略ssl验证
        /// </summary>
        public bool IgnoreSslValidation { get; set; }
        /// <summary>
        /// 请求返回时是否保留RequestBody和ResponseBody信息
        /// </summary>
        public bool ReserveBody { get; set; } = true;

        public Encoding Encoding { get; set; }
        /// <summary>
        /// 序列化方式
        /// </summary>
        public SerializeMode SerializeMode { get; set; } = SerializeMode.JsonNet;
        public Dictionary<string, string> Settings;
    }
}
