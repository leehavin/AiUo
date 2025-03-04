using AiUo.Configuration;

namespace AiUo.Net.SMS.Bluegoon;

internal class BluegoonSMSElement : SMSClientElement
{
    public override SMSProvider Provider => SMSProvider.Bluegoon;
    public string CompanyCode { get; set; }
    public string MD5Key { get; set; }
}