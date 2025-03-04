using Grpc.Net.Client.Balancer;
using Microsoft.Extensions.Logging;
using Nacos;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AiUo.Configuration;
using AiUo.Logging;
using AiUo.Randoms;

namespace AiUo.Extensions.Nacos;

public class NacosGrpcResolver : PollingResolver
{
    private readonly Uri _address;
    private readonly TimeSpan _refreshInterval;
    private Timer _timer;

    public NacosGrpcResolver(Uri address, TimeSpan refreshInterval, ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        _address = address;
        _refreshInterval = refreshInterval;
    }
    protected override void OnStarted()
    {
        base.OnStarted();

        if (_refreshInterval != Timeout.InfiniteTimeSpan)
        {
            _timer = CreateTimer(OnTimerCallback, state: null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            _timer.Change(_refreshInterval, _refreshInterval);
        }
    }
    public static Timer CreateTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
    {
        bool restoreFlow = false;
        try
        {
            if (!ExecutionContext.IsFlowSuppressed())
            {
                ExecutionContext.SuppressFlow();
                restoreFlow = true;
            }

            return new Timer(callback, state, dueTime, period);
        }
        finally
        {
            if (restoreFlow)
            {
                ExecutionContext.RestoreFlow();
            }
        }
    }

    protected override async Task ResolveAsync(CancellationToken cancellationToken)
    {
        var serviceName = _address.LocalPath.TrimStart('/');
        var section = DIUtil.GetService<NacosSection>();
        var instances = await DIUtil.GetRequiredService<INacosNamingService>()
            .SelectInstances(serviceName, section.GroupName, true, true);
        var addresses = RandomUtil.Random(instances)
            .Select(r => new BalancerAddress(r.Ip, r.Port)).ToArray();
        Listener(ResolverResult.ForResult(addresses));
    }
    private void OnTimerCallback(object state)
    {
        try
        {
            Refresh();
        }
        catch (Exception ex)
        {
            LogUtil.Error(ex, "NacosGrpcResolver刷新回调异常");
        }
    }
}
public class NacosGrpcResolverFactory : ResolverFactory
{
    // nacos:///serviceName
    public override string Name => "nacos";
    private readonly TimeSpan _refreshInterval;
    public NacosGrpcResolverFactory(TimeSpan refreshInterval)
    {
        _refreshInterval = refreshInterval;
    }

    public override Resolver Create(ResolverOptions options)
    {
        return new NacosGrpcResolver(options.Address, _refreshInterval, options.LoggerFactory);
    }
}