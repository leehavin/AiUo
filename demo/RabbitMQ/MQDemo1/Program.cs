using AiUo;
using AiUo; 
using AiUo.Configuration;
using AiUo.Data.SqlSugar;
using AiUo.Extensions.RabbitMQ;
using Microsoft.Extensions.Hosting;
using MQDemoLib;
using SqlSugar;

AiUoHost.CreateHost()
    .Start();

var section = ConfigUtil.GetSection<SqlSugarSection>();
foreach (var conn in section.ConnectionStrings.Values)
{
    var db = new SqlSugarClient(conn);
    var d = db.DbMaintenance.GetTableInfoList();
    var dt = db.MappingTables;
}

var input = string.Empty;
int idx = 0;
var items = new List<WorkItem>()
{
    new WorkItem
    {
        Title = "Publish => Subscribe",
        Action = async()=>{
            await MQUtil.PublishAsync(new SubMsg { Message = $"Publish {idx}" });
        }
    },
    //new WorkItem
    //{
    //    Title = "Republish",
    //    Action = async()=>{
    //        var items = await DbUtil.SelectAsync<S_mq_sub_errorPO>(it=>it.Status == 0);
    //        foreach(var item in items)
    //        {
    //            await MQBizUtil.RepublishAsync(item);
    //        }
    //    }
    //},
    //new WorkItem
    //{
    //    Title = "Publish => Subscribe 广播",
    //    Action = ()=>{
    //        MQUtil.Publish(new SubMsg1 { Message = $"Publish 广播 {idx}" });
    //    }
    //},
    //new WorkItem
    //{
    //    Title = "Publish => Subscribe Topic",
    //    Action = ()=>{
    //        var topic = new Random().Next(2) == 0 ? "a.*":"b.*";
    //        topic = "a.*";
    //        MQUtil.Publish(new SubMsg2 { Message = $"Publish Topic {idx}" }, topic);
    //        Console.WriteLine($"publish topic: {topic}");
    //    }
    //},
    // new WorkItem
    //{
    //    Title = "Request => Respond",
    //    Action = ()=>{
    //        var rsp = MQUtil.Request<ReqMsg, RspMsg>(new ReqMsg { Message = $"Request {idx}" });
    //        Console.WriteLine(rsp.Result.Message);
    //    }
    //},
    // new WorkItem
    //{
    //    Title = "Request => Respond queue",
    //    Action = ()=>{
    //        var rsp = MQUtil.Request<ReqMsg, RspMsg>(new ReqMsg { Message = $"Request queue {idx}" }, "xxyy.req");
    //        Console.WriteLine(rsp.Result.Message);
    //    }
    //},
    //new WorkItem
    //{
    //    Title = "Send=>Receive",
    //    Action = ()=>{
    //        MQUtil.Send(new SendMsg { Message = $"发送消息 {idx}" });
    //    }
    //},
    //new WorkItem
    //{
    //    Title = "Send=>Receive queue",
    //    Action = ()=>{
    //        var queue = new Random().Next(2) == 0 ? "queue1":"queue2";
    //        Console.WriteLine($"queue: {queue}");
    //        MQUtil.Send(new SendMsg1 { Message = $"发送消息 {idx}" }, queue);
    //    }
    //},
};

do
{
    WriteItems();
    input = Console.ReadLine();
    Console.WriteLine();
    input = !string.IsNullOrEmpty(input) ? input : "1";
    if (uint.TryParse(input, out uint inputIdx) && inputIdx < items.Count + 1)
    {
        idx++;
        items[(int)inputIdx - 1].Action();
    }
    else
    {
        Console.WriteLine("输入错误，重新输入");
    }
}
while (input != "q");

void WriteItems()
{
    Console.WriteLine();
    for (int i = 0; i < items.Count; i++)
        Console.WriteLine($"{i + 1} - {items[i].Title}");
    Console.WriteLine("请选择:");
}
public class WorkItem
{
    public string Title { get; set; }
    public Action Action { get; set; }
}