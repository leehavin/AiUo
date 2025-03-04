namespace AiUo.Demos;

internal class TestDemo : DemoBase
{
    public override async Task Execute()
    {
        var t = TimeSpan.Parse("7.00:00:00");
        Console.WriteLine(t);
    }
}


public class UserInfo
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
}