using AiUo.Configuration;

namespace AiUo.SnowflakeId.Common;

internal class ConfigWorkerIdProvider : IWorkerIdProvider
{
    private SnowflakeIdSection _section;
    public ConfigWorkerIdProvider() 
    {
        _section = ConfigUtil.GetSection<SnowflakeIdSection>();
    }
    public Task Active()
    {
        return Task.CompletedTask;
    }

    public Task<int> GetNextWorkId()
    {
        return Task.FromResult(_section.WorkerId);
    }

    public Task Dispose()
    {
        return Task.CompletedTask;
    }
}