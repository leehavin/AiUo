using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AiUo.Net.Email;

public interface IEmailProvider
{
    Task<string> Send(SendEmailData email);
}
public class SendEmailData
{
    public List<string> ToAddresses { get; set; }
    public List<string> CcAddresses { get; set; }
    public List<string> BccAddresses { get; set; }
    public string BodyHtml { get; set; }
    public string BodyText { get; set; }
    public string Subject { get; set; }
    public string SenderAddress { get; set; }
    public List<(string name, Stream stream)> Attachments { get; set; }
}