//using EasyNetQ;
//using MQDemoLib; 
//using AiUo.Extensions.RabbitMQ;

//namespace MQDemo2;

//public class DemoHandler : IMQSubscribeHandler<SubMsg>
//{
//    public Task OnMessage(SubMsg message, CancellationToken cancellationToken)
//    {
//        Console.WriteLine("");
//        return Task.CompletedTask;
//    }
//}
//public class DemoMQSub1 : MQBizSubConsumer<SubMsg>
//{
//    public override MQSubscribeMode SubscribeMode => MQSubscribeMode.OneQueue;
//    public DemoMQSub1()
//    {
//        AddHandler(OnMessage2);
//        AddHandler<DemoHandler>();
//    }

//    protected override void Configuration(ISubscriptionConfiguration x)
//    {
//    }

//    protected override async Task OnMessage(SubMsg message, CancellationToken cancellationToken)
//    {
//        Console.WriteLine(message.Message);
//        //throw new Exception(message.Message);
//    }
//    protected async Task OnMessage2(SubMsg message, CancellationToken cancellationToken)
//    {
//        Console.WriteLine(message.Message);
//    }
//}
///*
//public class DemoMQSub1 : MQSubscribeConsumer<SubMsg1>
//{

//    protected override bool IsBroadcast => true;
//    protected override void Configuration(ISubscriptionConfiguration x)
//    {
//    }

//    protected override async Task OnMessage(SubMsg1 message, CancellationToken cancellationToken)
//    {
//        Console.WriteLine($"[广播]: {message.Message}");
//    }
//}

//public class DemoMQSub2 : MQSubscribeConsumer<SubMsg2>
//{
//    protected override void Configuration(ISubscriptionConfiguration x)
//    {
//        x.WithTopic("a.*");
//    }

//    protected override async Task OnMessage(SubMsg2 message, CancellationToken cancellationToken)
//    {
//        Console.WriteLine($"[收到广播消息]: topic: a.* => {message.Message}");
//    }
//}
//public class DemoMQSub21 : MQSubscribeConsumer<SubMsg2>
//{
//    protected override void Configuration(ISubscriptionConfiguration x)
//    {
//        x.WithTopic("xxyy.operators.*");
//    }

//    protected override async Task OnMessage(SubMsg2 message, CancellationToken cancellationToken)
//    {
//        Console.WriteLine($"[收到广播消息]: topic: xxyy.operators.* => {message.Message}");
//    }
//}
//*/