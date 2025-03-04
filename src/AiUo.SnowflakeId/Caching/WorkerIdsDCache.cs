using AiUo.Configuration; 
using AiUo.Extensions.StackExchangeRedis;

namespace AiUo.SnowflakeId.Caching;

internal class WorkerIdsDCache : RedisStringClient<WorkerIdsDO>
{
    private SnowflakeIdSection _section;
    private int EXPIRE_MINUTES;
    public WorkerIdsDCache(int workerId)
    {
        _section = ConfigUtil.GetSection<SnowflakeIdSection>();
        Options.ConnectionStringName = _section.RedisConnectionStringName;
        RedisKey = $"{RedisPrefixConst.SNOWFLAKE_ID}:WorkerIds:{workerId}";
        EXPIRE_MINUTES = ConfigUtil.Environment.IsDebug
            ? (int)TimeSpan.FromMinutes(10).TotalMinutes : _section.RedisExpireMinutes;
    }
    public async Task SetWorkerIdsDo(WorkerIdsDO value)
    {
        await SetAndExpireMinutesAsync(value, EXPIRE_MINUTES);
    }
    public async Task Active()
    {
        await KeyExpireMinutesAsync(EXPIRE_MINUTES);
    }
}
internal class WorkerIdsDO
{
    public string Ip { get; set; }
    public int Pid { get; set; }
    public string Env { get; set; }
    public string ProjectId { get; set; }
    public string ServiceId { get; set; }
}