using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using AiUo.OAuth;
using AiUo.Reflection;

namespace AiUo.Configuration
{
    public class OAuthSection : ConfigSection
    {
        public override string SectionName => "OAuth";
        public bool Enabled { get; set; }
        public string ProvidersProvider { get; set; }
        /// <summary>
        /// OAuth服务提供商
        /// </summary>
        public Dictionary<string, OAuthProviderElement> Providers { get; set; }

        public override void Bind(IConfiguration configuration)
        {
            base.Bind(configuration);
            if (!string.IsNullOrEmpty(ProvidersProvider))
            {
                var prov = ReflectionUtil.CreateInstance(ProvidersProvider) as IOAuthProvidersProvider;
                if (prov == null)
                    throw new Exception($"配置文件OAuth:ProvidersProvider必须继承IOAuthProvidersProvider: {ProvidersProvider}");
                Providers = prov.GetProvidersAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
            else
            {
                Providers = configuration?.GetSection("Providers")?
                    .Get<Dictionary<string, OAuthProviderElement>>() ?? new();
            }
        }
        public IOAuthProviderElement GetProviderElement(OAuthProviders provider)
        {
            var key = provider.ToString();
            if (!Providers.TryGetValue(key, out var ret))
                throw new Exception($"配置文件OAuth:Providers不存在key: {provider}");
            return ret;
        }
    }
}
namespace AiUo.OAuth
{
    public interface IOAuthProviderElement
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
    public class OAuthProviderElement : IOAuthProviderElement
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
