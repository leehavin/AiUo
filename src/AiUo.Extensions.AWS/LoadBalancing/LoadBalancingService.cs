using Amazon.ElasticLoadBalancingV2;
using Amazon.ElasticLoadBalancingV2.Model;

namespace AiUo.Extensions.AWS.LoadBalancing;

public class LoadBalancingService
{
    public LoadBalancingService() { }

    #region TargetGroup
    public async Task<List<TargetGroup>> GetTargetGroups()
    {
        var req = new DescribeTargetGroupsRequest();
        var rsp = await GetClient().DescribeTargetGroupsAsync(req);
        return rsp.HttpStatusCode == System.Net.HttpStatusCode.OK
            ? rsp.TargetGroups : new List<TargetGroup>();
    }

    public async Task<TargetGroup> GetTargetGroup(string groupName)
    {
        var req = new DescribeTargetGroupsRequest();
        req.Names.Add(groupName);
        try
        {
            var rsp = await GetClient().DescribeTargetGroupsAsync(req);
            return rsp.HttpStatusCode == System.Net.HttpStatusCode.OK
                ? rsp.TargetGroups.FirstOrDefault() : null;
        }
        catch (TargetGroupNotFoundException)
        {
            return null;
        }
    }

    public async Task<TargetGroup> CreateTargetGroup(string vpcId, string groupName)
    {
        var req = new CreateTargetGroupRequest()
        {
            VpcId = vpcId,
            Name = groupName,
            TargetType = TargetTypeEnum.Ip,
            Protocol = ProtocolEnum.HTTP,
            Port = 80,
            HealthCheckEnabled = true,
            HealthCheckIntervalSeconds = 10,
            HealthCheckPath = "/healthz",
            HealthCheckProtocol = ProtocolEnum.HTTP,
            IpAddressType = TargetGroupIpAddressTypeEnum.Ipv4
        };
        var rsp = await GetClient().CreateTargetGroupAsync(req);
        return rsp.HttpStatusCode == System.Net.HttpStatusCode.OK
            ? rsp.TargetGroups.First() : null;
    }


    public async Task RegisterTarget(string groupArn, string ip, int port)
    {
        var req = new RegisterTargetsRequest();
        req.TargetGroupArn = groupArn;
        req.Targets.Add(new TargetDescription
        {
            Id = ip,
            Port = port
        });
        var rsp = await GetClient().RegisterTargetsAsync(req);
        if (rsp.HttpStatusCode != System.Net.HttpStatusCode.OK)
            throw new Exception($"LoadBalancingService.RegisterTargetGroup失败: targetGroupArn: {groupArn}");
    }
    public async Task DeregisterTarget(string groupArn, string ip, int port)
    {
        var req = new DeregisterTargetsRequest();
        req.TargetGroupArn = groupArn;
        req.Targets.Add(new TargetDescription
        {
            Id = ip,
            Port = port
        });
        var rsp = await GetClient().DeregisterTargetsAsync(req);
        if (rsp.HttpStatusCode != System.Net.HttpStatusCode.OK)
            throw new Exception($"LoadBalancingService.DeregisterTargets失败: targetGroupArn: {groupArn}");
    }
    #endregion

    private IAmazonElasticLoadBalancingV2 GetClient()
        => DIUtil.GetService<IAmazonElasticLoadBalancingV2>();
}