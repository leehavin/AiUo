namespace AiUo.Extensions.StackExchangeRedis;

public interface IRedisPublishMessage
{
    /// <summary>
    /// 模式值
    /// </summary>
    public string PatternKey { get; set; }
}