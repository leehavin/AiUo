using AiUo.Configuration;

namespace AiUo.Demos.Core;

internal class ConfigUtilDemo : DemoBase
{
    public override async Task Execute()
    {
        // 读取配置文件appsettings.json
        Console.WriteLine(ConfigUtil.Project.ProjectId);
        Console.WriteLine(ConfigUtil.AppSettings.Get("myKey"));
    }
}