namespace AiUo.SnowflakeId.Common;

internal interface IWorkerIdProvider
{
    Task<int> GetNextWorkId();
    Task Active();
    Task Dispose();
}