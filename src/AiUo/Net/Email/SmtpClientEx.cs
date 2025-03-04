using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using AiUo.Configuration;
using AiUo.Collections;

namespace AiUo.Net;

/// <summary>
/// SmtpClient封装扩展
/// </summary>
public class SmtpClientEx
{
    public SmtpSection Section { get; }
    public SmtpClientElement ClientElement { get; }
    public SmtpClient Client { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="clientName">配置文件Smtp:Clients:Name</param>
    public SmtpClientEx(string clientName = null)
    {
        Section = ConfigUtil.GetSection<SmtpSection>();
        if (Section == null)
            throw new Exception($"配置文件中不存在配置节：Smtp");
        clientName = clientName ?? Section.DefaultClientName;
        if (!Section.Clients.ContainsKey(clientName))
            throw new Exception($"配置文件中Smtp:Clients:Name不存在。Name: {clientName}");

        ClientElement = Section.Clients[clientName];
        Client = new SmtpClient(ClientElement.Host, ClientElement.Port);
        Client.EnableSsl = ClientElement.UseSsl;
        Client.Credentials = new NetworkCredential(ClientElement.UserName, ClientElement.Password);
        Client.DeliveryMethod = SmtpDeliveryMethod.Network;
    }
    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="toMails">接收邮件列表。;分割</param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="atts"></param>
    public void Send(string toMails, string subject, string body, params Attachment[] atts)
    {
        var tos = toMails.Split('|', ';', ',');
        SendTo(tos, null, subject, body, atts);
    }
    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="sendToName">配置文件Smtp:SendTos:Name</param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="atts"></param>
    public void SendTo(string sendToName, string subject, string body, params Attachment[] atts)
    {
        var element = Section.SendTos[sendToName];
        SendTo(element.To, element.CC, subject, body, atts);
    }
    private void SendTo(IEnumerable<string> tos, IEnumerable<string> ccs, string subject, string body, params Attachment[] atts)
    {
        var msg = GetMessage();
        tos?.ForEach(t =>
        {
            t = t.Trim();
            if (!string.IsNullOrEmpty(t))
                msg.To.Add(t);
        });
        ccs?.ForEach(c =>
        {
            c = c.Trim();
            if (!string.IsNullOrEmpty(c))
                msg.CC.Add(c);
        });
        msg.Subject = subject;
        msg.Body = body;
        atts?.ForEach(att =>
        {
            if (att != null)
                msg.Attachments.Add(att);
        });
        Client.Send(msg);
    }
    private MailMessage GetMessage()
    {
        var msg = new MailMessage();
        msg.From = new MailAddress(ClientElement.FromAddress, ClientElement.FromName);
        return msg;
    }
}