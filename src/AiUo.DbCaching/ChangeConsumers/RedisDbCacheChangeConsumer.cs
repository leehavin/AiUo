using AiUo.Extensions.StackExchangeRedis;

namespace AiUo.DbCaching.ChangeConsumers;

[RedisConsumerRegisterIgnore]
internal class RedisDbCacheChangeConsumer : RedisSubscribeConsumer<DbCacheChangeMessage>, IDbCacheChangeConsumer
{
    private DbCacheUpdator _uploader;
    public RedisDbCacheChangeConsumer(string redisConnectionStringName)
    {
        ConnectionStringName = redisConnectionStringName;
        _uploader = new DbCacheUpdator(DbCachingPublishMode.Redis);
    }
    protected override async Task OnMessage(DbCacheChangeMessage message)
    {
        await _uploader.Execute(message);
    }
    protected override Task OnError(DbCacheChangeMessage message, Exception ex)
    {
        return Task.CompletedTask;
    }

    public Task RegisterConsumer()
    {
        this.Register();
        return Task.CompletedTask;
    }
}
internal interface IDbCacheChangeConsumer
{
    Task RegisterConsumer();
}