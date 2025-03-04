using Microsoft.Extensions.Configuration;
using System.Text;
using AiUo.Collections;
using AiUo.Configuration;
using AiUo.Logging;
using AiUo.Reflection;
using AiUo.Security;

namespace AiUo.AspNet;

public class RequestSignFilterSection : ConfigSection
{
    public override string SectionName => "RequestSignFilter";
    public string DefaultFilterName { get; set; }
    public string FiltersProvider { get; set; }
    public Dictionary<string, RequestSignFilterElement> Filters { get; set; } = new();
    public override void Bind(IConfiguration configuration)
    {
        base.Bind(configuration);

        // Filters
        Filters ??= new();
        Filters.ForEach(item => item.Value.Name = item.Key);

        // FiltersProvider
        if (!string.IsNullOrEmpty(FiltersProvider))
        {
            var provider = ReflectionUtil.CreateInstance(FiltersProvider) as IRequestSignFilterProvider;
            if (provider == null)
                throw new Exception($"配置中{SectionName}:FiltersProvider不存在或未实现{nameof(IClientSignFiltersProvider)}: {FiltersProvider}");
            var list = provider.Build();
            foreach ( var item in list )
            {
                if (string.IsNullOrEmpty(item.Name))
                    throw new Exception($"配置中{SectionName}:FiltersProvider实现返回的集合name不能为空。provider: {FiltersProvider}");
                if (Filters.ContainsKey(item.Name))
                    LogUtil.Warning($"配置中{SectionName}:FiltersProvider提供的与原有的配置name重复。provider: {FiltersProvider} name: {item.Name}");
                else
                    Filters.Add(item.Name, item);
            }
        }
            
        // DefaultFilterName
        if (string.IsNullOrEmpty(DefaultFilterName))
        {
            if (Filters.Count > 1)
                throw new Exception($"{SectionName}:DefaultFilterName为空但Filters不唯一");
            DefaultFilterName = Filters.First().Key;
        }
        else
        {
            if (!Filters.ContainsKey(DefaultFilterName))
                throw new Exception($"{SectionName}:Filters不存在DefaultFilterName: {DefaultFilterName}");
        }
    }
}
public class RequestSignFilterElement
{
    public string Name { get; set; }
    public bool Enabled { get; set; }
    public string HeaderName { get; set; }

    /// <summary>
    /// 第三方公钥
    /// </summary>
    public string PublicKey { get; set; }
    public RSAKeyMode KeyMode { get; set; } = RSAKeyMode.PublicKey;
    public string HashName { get; set; } = "SHA256";
    public CipherEncode Cipher { get; set; } = CipherEncode.Base64;
    public Encoding Encoding { get; set; } = Encoding.UTF8;
}