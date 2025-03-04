using Microsoft.Extensions.Configuration;
using AiUo.Collections;
using AiUo.Reflection;
using AiUo.AspNet;
using AiUo.Logging;

namespace AiUo.Configuration;

public class AccessIpFilterSection : ConfigSection
{
    public override string SectionName => "AccessIpFilter";
    public string DefaultFilterName { get; set; }
    public string FiltersProvider { get; set; }
    public Dictionary<string, AccessIpFilterElement> Filters { get; set; } = new Dictionary<string, AccessIpFilterElement>();

    public override void Bind(IConfiguration configuration)
    {
        base.Bind(configuration);

        // Filters
        Filters ??= new();
        Filters.ForEach(item => item.Value.Name = item.Key);

        // FiltersProvider
        if (!string.IsNullOrEmpty(FiltersProvider))
        {
            var provider = ReflectionUtil.CreateInstance(FiltersProvider) as IAccessIpFiltersProvider;
            if (provider == null)
                throw new Exception($"配置中AccessIpFilter:FiltersProvider不存在或未实现IAccessIpFiltersProvider: {FiltersProvider}");
            var list = provider.Build();
            foreach (var item in list)
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
public class AccessIpFilterElement
{
    public string Name { get; set; }
    public bool Enabled { get; set; }
    /// <summary>
    /// 是否允许内网访问
    /// </summary>
    public bool EnableIntranet { get; set; } = true;
    public string AllowIps { get; set; }

    private HashSet<string> _allowIpDict;
    public HashSet<string> GetAllowIpDict()
    {
        if (_allowIpDict != null)
            return _allowIpDict;
        var ret = new HashSet<string>();
        if (!string.IsNullOrEmpty(AllowIps))
        {
            var ips = AllowIps.Split('|', ';', ',');
            foreach (var ip in ips)
            {
                if (!ret.Contains(ip))
                    ret.Add(ip);
            }
        }
        _allowIpDict = ret;
        return ret;
    }
}