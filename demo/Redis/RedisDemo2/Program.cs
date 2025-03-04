using RedisDemoLib;
using AiUo;
using AiUo.Extensions.StackExchangeRedis;

AiUoHost.Start();

var input = string.Empty;
do
{
    var sub1 = new SubConsumer1(); sub1.Register();
    var sub2 = new SubConsumer2(); sub2.Register();
    var sub3 = new SubConsumer3(); sub3.Register();
    Console.WriteLine("OK");
    input = Console.ReadLine();
}
while (input != "q");

class SubConsumer1 : RedisSubscribeConsumer<PubMsg1>
{
    protected override async Task OnMessage(PubMsg1 message)
    {
        Console.WriteLine(message.Id);
    }
    protected override Task OnError(PubMsg1 message, Exception ex)
    {
        return Task.CompletedTask;
    }
}
class SubConsumer2 : RedisSubscribeConsumer<PubMsg2>
{
    protected override async Task OnMessage(PubMsg2 message)
    {
        Console.WriteLine(message.Id);
    }
    protected override Task OnError(PubMsg2 message, Exception ex)
    {
        return Task.CompletedTask;
    }
}
class SubConsumer3 : RedisQueueConsumer<PubMsg3>
{
    protected override async Task OnMessage(PubMsg3 message)
    {
        Console.WriteLine(message.Id);
    }
    protected override Task OnError(PubMsg3 message, Exception ex)
    {
        return Task.CompletedTask;
    }
}