namespace AiUo.Demos.Patterns.Creational;

internal class PrototypeDemo : DemoBase
{
    public override async Task Execute()
    {
        Console.WriteLine("原型模式：");

        Email email = new Email()
        {
            Sender = "noreply@cmb.com",
            Subject = "招商银行月开鑫基金上线啦，最低年化收益7%，速速抢购！",
            Content = "招商银行月开鑫基金上线啦，最低年化收益7%，速速抢购，手慢无，每人限购1万，详情咨询95555！",
            Footer = "招商银行"
        };

        for (int i = 0; i < 10000; i++)
        {
            string receiver = string.Format("kehu{0}@qq.com", i);
            string name = string.Format("尊敬的客户『{0}』:", i);
            var cloneEmail = email.Clone() as Email;
            cloneEmail!.Receiver = receiver;
            cloneEmail.Name = name;

            SendEmail(cloneEmail);
        }
    }
    private static void SendEmail(Email email)
    {
        Console.WriteLine(string.Format("邮件已发送至：『{0}』", email.Receiver));
    }
}
public class Email : ICloneable
{
    public string Receiver { get; set; }
    public string Sender { get; set; }
    public string Subject { get; set; }
    public string Name { get; set; }

    public string Content { get; set; }

    public string Footer { get; set; }


    public object Clone()
    {
        return this.MemberwiseClone();
    }
}