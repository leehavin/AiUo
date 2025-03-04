using System.Collections.Concurrent;

namespace AiUo.Demos.Patterns.Structural;

/// <summary>
/// 享元模式
/// 对象池
/// </summary>
internal class FlyweightDemo : DemoBase
{
    public override async Task Execute()
    {
        for (int i = 0; i < 10; i++)
        {
            string receiver = $"kehu{i}@qq.com";
            //通过简单工厂维护的对象池获取已经封装好的内部状态的对象。
            var email = EmailTemplateFactory.GetTemplate("阿里云故障通告");
            //修改外部状态
            email.Receiver = receiver;
            SendEmail(email);
        }

    }
    private static void SendEmail(EmailTemplateFactory.Email email)
    {
        Console.WriteLine($"主题为『{email.Subject}』的邮件已发送至：{email.Receiver}");
    }
}
public static class EmailTemplateFactory
{
    /// <summary>
    /// 预置模板
    /// </summary>
    private static readonly Dictionary<string, string> _dic = new Dictionary<string, string>()
    {
        {"阿里云故障通告", "您的服务器存在故障，请您了解！"},
        {"阿里云升级通知", "我们将对阿里云进行升级，会存在服务器短暂不可用情况，请知悉！"}
    };

    /// <summary>
    /// 定义对象池
    /// </summary>
    static readonly ConcurrentDictionary<string, Email> _pool = new ConcurrentDictionary<string, Email>();


    /// <summary>
    /// 根据主题获取模板
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static Email GetTemplate(string subject)
    {
        Email email = null;

        if (!_pool.ContainsKey(subject))
        {
            string template;
            _dic.TryGetValue(subject, out template);
            email = new Email("system@notice.aliyun.com", subject, string.IsNullOrWhiteSpace(template) ? subject : template, "阿里云计算公司");
            _pool.TryAdd(subject, email);
        }
        else
        {
            _pool.TryGetValue(subject, out email);
        }

        return email;

    }

    public class Email
    {
        public string Receiver { get; set; }
        public string Sender { get; }
        public string Subject { get; }
        public string Template { get; }
        public string Signature { get; }

        public Email(string sender, string subject, string template, string signature)
        {
            Sender = sender;
            Subject = subject;
            Template = template;
            Signature = signature;
        }

    }
}