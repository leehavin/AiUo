using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using AiUo.Net;
using AiUo.Collections;

namespace AiUo.Configuration
{
    public class SmtpSection : ConfigSection
    {
        public override string SectionName => "Smtp";
        public string DefaultClientName { get; set; }
        public Dictionary<string, SmtpClientElement> Clients = new Dictionary<string, SmtpClientElement>();

        public Dictionary<string, SendToElement> SendTos = new Dictionary<string, SendToElement>();
        public override void Bind(IConfiguration configuration)
        {
            base.Bind(configuration);
            Clients = configuration.GetSection("Clients")
                .Get<Dictionary<string, SmtpClientElement>>() ?? new();
            Clients.ForEach(x =>
            {
                if (string.IsNullOrEmpty(x.Value.FromAddress))
                    x.Value.FromAddress = x.Value.UserName;
            });
            SendTos = configuration.GetSection("SendTos")
                .Get<Dictionary<string, SendToElement>>() ?? new();
        }
    }
}

namespace AiUo.Net
{
    public class SmtpClientElement
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool UseSsl { get; set; }
    }
    public class SendToElement
    {
        public List<string> To { get; set; }
        public List<string> CC { get; set; }
    }
}

