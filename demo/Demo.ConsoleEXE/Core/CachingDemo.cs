using AiUo.Caching;

namespace AiUo.Demos.Core;

internal class CachingDemo : DemoBase
{
    public override async Task Execute()
    {
        // 生成ticket(可用于注册或登录时email和sms使用的验证码)
        // 生成时ticket保存到IDistributedCache中
        // 如果没有配置，保存到CachingUtil.MemoryCache
        var key = "aiuo";
        var ticket = TicketCacheUtil.GenerateTicket(key, 5, CharsScope.Numbers);
        Console.WriteLine(ticket);
        var success = TicketCacheUtil.ValidateTicket(key, ticket);
        Console.WriteLine(success);
    }
}