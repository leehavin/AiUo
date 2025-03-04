using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace AiUo.Configuration;

public class SMSSection : ConfigSection
{
    public override string SectionName => "SMS";
    public string DefaultClientName { get; set; }
    public Dictionary<string, IConfigurationSection> Clients = new ();

    public override void Bind(IConfiguration configuration)
    {
        base.Bind(configuration);
        var section = configuration.GetSection("Clients");
        //section.GetChildren().ToArray()[0].GetSection("Provider").Value;
        foreach (var item in section.GetChildren())
        {
            var provider = item.GetSection("Provider").Value.ToEnum<SMSProvider>();
            SMSClientElement element= null;
            switch (provider)
            {
                case SMSProvider.Tencent:
                    element = item.Get<SMSClientElement>();
                    break;
            }
            //Clients.Add(element.Name, element);
        }
    }
}
public abstract class SMSClientElement
{
    public abstract SMSProvider Provider { get; }
    /// <summary>
    /// 是否为 Debug 模式 默认为 False
    /// </summary>
    public bool Debug { get; set; }
    /// <summary>
    /// 获得/设置 验证码有效时长
    /// </summary>
    public int ExpireMinutes { get; set; }
    /// <summary>
    /// 获得/设置 短信下发网关地址
    /// </summary>
    public string RequestUrl { get; set; } = "http://open.bluegoon.com/api/sms/sendcode";
}
public enum SMSProvider
{
    Tencent,
    Bluegoon
}