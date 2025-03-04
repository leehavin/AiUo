using AiUo.Configuration;
using AiUo.Extensions.StackExchangeRedis;

namespace AiUo.Extensions.AWS.LoadBalancing;

internal class TargetGroupRegisterService
{
    public bool IsRegister { get; }
    public string GroupName { get; }
    private AwsGlobalDCache _globalDCache;
    private TargetGroupDCache _groupDCache;
    private LoadBalancingService _lbSvc;

    private string _targetGroupArn;
    public TargetGroupRegisterService()
    {
        var section = ConfigUtil.GetSection<AwsSection>();
        if (section != null && section.LoadBalancing != null)
        {
            IsRegister = section.LoadBalancing.RegisterTargetGroup;
            GroupName = section.LoadBalancing.TargetGroupName;
            _globalDCache = new AwsGlobalDCache();
            _groupDCache = new TargetGroupDCache();
            _lbSvc = new LoadBalancingService();
        }
    }

    public async Task Register()
    {
        if (!IsRegister) return;

        var arn = await _groupDCache.GetOrLoadAsync(GroupName);
        if (!arn.HasValue)
        {
            using (var redLock = await RedisUtil.LockAsync($"__TargetGroup:{GroupName}"))
            {
                if (!redLock.IsLocked)
                    throw new Exception($"TargetGroupRegisterService注册时TargetGroup不存在且无法申请锁.groupName:{GroupName}");
                arn = await _groupDCache.GetOrLoadAsync(GroupName);
                if (!arn.HasValue)
                {
                    var vpcId = await _globalDCache.GetVpcId();
                    var group = await _lbSvc.CreateTargetGroup(vpcId, GroupName);
                    if (group == null)
                        throw new Exception($"TargetGroupRegisterService.Register时无法创建targetGroup。vpcId:{vpcId} groupName:{GroupName}");
                    _targetGroupArn = group.TargetGroupArn;
                    await _groupDCache.SetAsync(GroupName, _targetGroupArn);
                }
                else
                    _targetGroupArn = arn.Value;
            }
        }
        else
            _targetGroupArn = arn.Value;
        await _lbSvc.RegisterTarget(_targetGroupArn, ConfigUtil.Service.HostIp, ConfigUtil.Service.HostPort);
    }
    public async Task Deregister()
    {
        if (!IsRegister) return;

        await _lbSvc.DeregisterTarget(_targetGroupArn, ConfigUtil.Service.HostIp, ConfigUtil.Service.HostPort);
    }
}