using RedisDemoLib;
using AiUo;
using AiUo.Extensions.StackExchangeRedis;

AiUoHost.Start();

var input = string.Empty;
int idx = 0;
do
{
    input = Console.ReadLine();
    idx++;
    switch (input)
    {
        case "1":
            await RedisUtil.PublishAsync(new PubMsg1
            {
                Id = idx,
                Name = "PubMsg1",
            });
            break;
        case "2":
            await RedisUtil.PublishAsync(new PubMsg2
            {
                Id = idx,
                Name = "PubMsg2",
            });
            break;
        case "3":
            await RedisUtil.PublishQueueAsync(new PubMsg3
            {
                Id = idx,
                Name = "PubMsg3",
            });
            break;
        default:
            Console.WriteLine("输入1，2,3");
            break;
    }
}
while (input != "q");


Console.WriteLine("OK");