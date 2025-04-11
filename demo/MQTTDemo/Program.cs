using AiUo;
using AiUo.Configuration;
using AiUo.Extensions.MQTT;
using Microsoft.Extensions.Hosting;
using MQTTDemoLib;

// 创建并启动主机
AiUoHost.CreateBuilder().AddMQTTEx().StartAsync();

Console.WriteLine("MQTT测试客户端已启动...");
Console.WriteLine("请选择操作：");

var items = new List<WorkItem>()
{
    new WorkItem
    {
        Title = "发布消息",
        Action = async () =>
        {
            // 使用消息类型上的主题和QoS
            var message = new MQTTPublishMsg { Content = "Hello MQTT", Value = 42 };
            MQTTUtil.Publish(message);
            Console.WriteLine($"已发布消息到主题: {message.MQTTMeta.Topic}");
        }
    },
    new WorkItem
    {
        Title = "异步发布消息",
        Action = async () =>
        {
            var message = new MQTTPublishMsg { Content = "Async Hello MQTT", Value = 100 };
            await MQTTUtil.PublishAsync(message);
            Console.WriteLine($"已异步发布消息到主题: {message.MQTTMeta.Topic}");
        }
    },
    new WorkItem
    {
        Title = "发布自定义主题消息",
        Action = async () =>
        {
            var message = new MQTTPublishMsg { Content = "Custom Topic Message", Value = 200 };
            MQTTUtil.Publish(message, "custom/mqtt/topic", 1, false);
            Console.WriteLine("已发布消息到自定义主题: custom/mqtt/topic");
        }
    },
    new WorkItem
    {
        Title = "订阅主题",
        Action = async () =>
        {
            Console.WriteLine("已通过MQTTSubscribeConsumer自动订阅主题");
            Console.WriteLine("请发布消息到相应主题以测试订阅功能");
        }
    },
    new WorkItem
    {
        Title = "退出",
        Action = async () =>
        {
            Environment.Exit(0);
        }
    }
};

while (true)
{
    for (int i = 0; i < items.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {items[i].Title}");
    }

    Console.Write("请输入选项编号: ");
    if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= items.Count)
    {
        Console.WriteLine($"执行: {items[choice - 1].Title}");
        try
        {
            await items[choice - 1].Action();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"执行出错: {ex.Message}");
        }
        Console.WriteLine("按任意键继续...");
        Console.ReadKey();
        Console.Clear();
    }
    else
    {
        Console.WriteLine("无效的选项，请重新输入");
    }
}

public class WorkItem
{
    public string Title { get; set; }
    public Func<Task> Action { get; set; }
}