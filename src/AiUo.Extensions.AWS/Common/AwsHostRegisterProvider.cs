using AiUo.Extensions.AWS.LoadBalancing;
using AiUo.Hosting.Services;
using AiUo.Logging;

namespace AiUo.Extensions.AWS.Common;

internal class AwsHostRegisterProvider : IAiUoHostRegisterProvider
{
    private TargetGroupRegisterService _targetGroupRegistor;

    public AwsHostRegisterProvider()
    {
        _targetGroupRegistor = new TargetGroupRegisterService();
    }
    public async Task Register()
    {
        await _targetGroupRegistor.Register();
        if (_targetGroupRegistor.IsRegister)
            LogUtil.Info($"注册Host => AWS[{GetType().Name}] GroupName:{_targetGroupRegistor.GroupName}");
    }

    public async Task Deregistering()
    {
        await _targetGroupRegistor.Deregister();
    }

    public Task Heartbeat()
    {
        return Task.CompletedTask;
    }

    public Task Health()
    {
        return Task.CompletedTask;
    }

    public Task Deregistered()
    {
        return Task.CompletedTask;
    }
}