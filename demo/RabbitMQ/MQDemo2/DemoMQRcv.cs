namespace MQDemo2;

/*
public class DemoMQRcv1 : MQReceiveConsumer
{
    public override string QueueName => "queue1";

    protected override void Configuration(IReceiveConfiguration x)
    {
    }

    protected override void AddHandlers(IReceiveRegistration reg)
    {
        reg.Add<SendMsg>(Do);
        reg.Add<SendMsg1>(msg => {
            Console.WriteLine($"[收到Send消息]: queue: queue1 msg:SendMsg1 {msg.Message}");
        });
    }
    private async Task Do(SendMsg msg, CancellationToken token)
    {
        Console.WriteLine($"[收到Send消息]: queue: queue1 msg:SendMsg {msg.Message}");
    }
}
public class DemoMQRcv2 : MQReceiveConsumer
{
    public override string QueueName => "queue2";

    protected override void Configuration(IReceiveConfiguration x)
    {
    }

    protected override void AddHandlers(IReceiveRegistration reg)
    {
        reg.Add<SendMsg>(msg => {
            Console.WriteLine($"[收到Send消息]: queue: queue2 msg:SendMsg {msg.Message}");
        });
        reg.Add<SendMsg1>(msg => {
            Console.WriteLine($"[收到Send消息]: queue: queue2 msg:SendMsg1 {msg.Message}");
        });
    }
}
*/