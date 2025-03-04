using AiUo.AspNet;
using AiUo.AspNet.Hosting;

namespace CorsAPI;

public class CorsProvider : DbCachingCorsProvider<Ss_operatorEO>
{
    protected override List<CorsPolicyElement> GetPolicies(List<Ss_operatorEO> list)
    {
        var ret = new List<CorsPolicyElement>();
        foreach (var policy in list)
        {
            var domains = policy.MapDomain?.Split('|');
            if (domains == null)
                continue;
            foreach (var domain in domains)
            {
                ret.Add(new CorsPolicyElement
                {
                    Name = "default",
                    Headers = "*",
                    Methods = "*",
                    Origins = domain
                });
            }
        }
        return ret;
    }

    protected override object GetSplitDbKey()
    {
        return null;
    }
}