using AiUo.Extensions.StackExchangeRedis;

namespace AiUo.Demos.Redis;

internal class RedisDemo : DemoBase
{
    public override async Task Execute()
    {
        //var cache = new DemoHashDCache();
        //cache.Set("a", new TestInfo { Id=1,Name="aa"});
        //Console.WriteLine(cache.GetOrDefault("a", null).Name);
        var cache = new DemoHashDCache2();
        var value = await cache.GetOrLoadAsync("1");
        Console.WriteLine(value.Value.Name);
    }
}
class DemoHashDCache : RedisHashClient<TestInfo>
{
}
class DemoHashDCache2 : RedisHashExpireClient<TestInfo>
{
}

class TestInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
}