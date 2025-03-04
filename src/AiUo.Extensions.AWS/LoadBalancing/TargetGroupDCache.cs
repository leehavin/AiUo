using AiUo.Caching;
using AiUo.Extensions.StackExchangeRedis;

namespace AiUo.Extensions.AWS.LoadBalancing;

internal class TargetGroupDCache : RedisHashClient<string>
{
    private LoadBalancingService _lbSvc;
    public TargetGroupDCache(string connectionStringName = null) 
    {
        Options.ConnectionStringName = connectionStringName;
        RedisKey = $"{RedisPrefixConst.AWS}:TargetGroup";
        _lbSvc = new LoadBalancingService();
    }
    protected override async Task<CacheValue<string>> LoadValueWhenRedisNotExistsAsync(string field)
    {
        var ret = new CacheValue<string>();
        var group = await _lbSvc.GetTargetGroup(field);
        if (group != null)
        {
            ret.HasValue = true;
            ret.Value = group.TargetGroupArn;
        }
        return ret;
    }
}