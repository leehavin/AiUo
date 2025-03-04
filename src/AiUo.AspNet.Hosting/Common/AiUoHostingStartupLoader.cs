using Microsoft.AspNetCore.Builder;
using AiUo.Configuration;
using AiUo.Reflection;
using AiUo.Logging;

namespace AiUo.AspNet.Hosting;

public class AiUoHostingStartupLoader
{
    public static AiUoHostingStartupLoader Instance = new();
    private List<IAiUoHostingStartup> _startups = new();
    private AiUoHostingStartupLoader()
    {
        var section = ConfigUtil.GetSection<AspNetSection>();
        var asms = DIUtil.GetService<IAssemblyContainer>().GetAssemblies(section?.HostingStartupAssemblies
            , section?.AutoLoad, "加载配置文件AspNet:HostingStartupAssemblies中项失败。");
        foreach (var assembly in asms)
        {
            var types = from t in assembly.GetTypes()
                where t.IsSubclassOfGeneric(typeof(IAiUoHostingStartup))
                select t;
            foreach (var type in types)
            {
                _startups.Add((IAiUoHostingStartup)ReflectionUtil.CreateInstance(type));
            }
        }
    }

    public void ConfigureServices(WebApplicationBuilder webApplicationBuilder)
    {
        if (_startups.Count == 0)
            return;
        var asms = ConfigUtil.GetSection<AspNetSection>()?.HostingStartupAssemblies;
        var asmStr =  string.Join('|', asms);
        LogUtil.Info($"注册 AiUoHostingStartupLoader。HostingStartupAssemblies:{asmStr}");
        _startups.ForEach(x => x.ConfigureServices(webApplicationBuilder));
    }

    public void Configure(WebApplication webApplication)
    {
        if (_startups.Count == 0)
            return;
        _startups.ForEach(x => x.Configure(webApplication));
    }
}