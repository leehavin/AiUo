using System.Diagnostics;
using AiUo.SnowflakeId.Caching;
using AiUo.Configuration;
using AiUo.Extensions.StackExchangeRedis;
using AiUo.Net;

namespace AiUo.SnowflakeId.Common;

internal class RedisWorkerIdProvider : IWorkerIdProvider
{
    private int _maxWorkerId;
    private WorkerIdCurrentDCache _idDCache;
    private WorkerIdsDO _workerDo;

    private SnowflakeIdSection _section;

    public int WorkerId;
    public RedisWorkerIdProvider()
    {
        _section = ConfigUtil.GetSection<SnowflakeIdSection>();
        _maxWorkerId = 1 << _section.WorkerIdBits;
        _idDCache = WorkerIdCurrentDCache.Create();
        _workerDo = new WorkerIdsDO()
        {
            Ip = NetUtil.GetLocalIP(),
            Pid = Process.GetCurrentProcess().Id,
            Env = ConfigUtil.Environment.Name,
            ProjectId = ConfigUtil.Project.ProjectId,
            ServiceId = ConfigUtil.Service.ServiceId
        };
    }

    public async Task<int> GetNextWorkId()
    {
        using (var redlock = await RedisUtil.LockAsync("_SnowflakeIdWorkerId", 20))
        {
            if (!redlock.IsLocked)
                throw new Exception("Redis没有能够获取WorkerId分布式锁");

            for (int i = 0; i < _maxWorkerId; i++)
            {
                var workerId = await _idDCache.GetNextWorkId() - 1;
                if (workerId >= _maxWorkerId)
                {
                    await _idDCache.SetAsync(0);
                    workerId = 0;
                }
                var dcache = new WorkerIdsDCache(workerId);
                if (await dcache.KeyExistsAsync())
                    continue;
                await dcache.SetWorkerIdsDo(_workerDo);
                WorkerId = workerId;
                return workerId;
            }
        }
        throw new Exception("Redis没有足够的WorkerId分配");
    }
    public async Task Active()
    {
        await new WorkerIdsDCache(WorkerId).Active();
    }

    public async Task Dispose()
    {
        await new WorkerIdsDCache(WorkerId).KeyDeleteAsync();
    }
}