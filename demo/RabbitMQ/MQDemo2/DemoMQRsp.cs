namespace MQDemo2;

/*
public class DemoMQRsp : MQRespondConsumer<ReqMsg, RspMsg>
{
    protected override void Configuration(IResponderConfiguration x)
    {
    }

    protected override async Task<RspMsg> Respond(ReqMsg request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[收到Request消息]: {request.Message}");
        return new RspMsg { Message = "返回Response消息" };
    }
}
public class DemoMQRsp1 : MQRespondConsumer<ReqMsg, RspMsg>
{
    protected override void Configuration(IResponderConfiguration x)
    {
        x.WithQueueName("xxyy.req");
    }

    protected override async Task<RspMsg> Respond(ReqMsg request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[收到Request消息]:xxyy.req=> {request.Message}");
        return new RspMsg { Message = "返回Response消息" };
    }
}
public class DemoMQRsp2 : MQRespondConsumer<ReqMsg, RspMsg>
{
    protected override void Configuration(IResponderConfiguration x)
    {
        x.WithQueueName("xxyy.XXX");
    }

    protected override async Task<RspMsg> Respond(ReqMsg request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[收到Request消息]: xxyy.XXX => {request.Message}");
        return new RspMsg { Message = "返回Response消息" };
    }
}
*/