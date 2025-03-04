using AiUo.Configuration;
using AiUo.Extensions.StackExchangeRedis;

namespace AiUo.Extensions.AWS;

public class AwsGlobalDCache : RedisHashClient
{
    public AwsGlobalDCache(string connectionStringName = null)
    {
        Options.ConnectionStringName = connectionStringName;
        RedisKey = $"{RedisPrefixConst.AWS}:Global";
    }

    public async Task<string> GetVpcId()
    {
        var field = "VpcId";
        var ret = await GetOrDefaultAsync<string>(field, null);
        if (string.IsNullOrEmpty(ret))
        {
            var name = ConfigUtil.GetSection<AwsSection>().VpcName;
            ret = (await new EC2.EC2Service().GetVpc(name))?.VpcId;
            if (!string.IsNullOrEmpty(ret))
                await SetAsync(field, ret);
        }
        return ret;
    }
}