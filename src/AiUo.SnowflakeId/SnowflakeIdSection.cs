using Microsoft.Extensions.Configuration;

namespace AiUo.Configuration;

public class SnowflakeIdSection : ConfigSection
{
    public override string SectionName => "SnowflakeId";

    public bool Enabled { get; set; }
    public bool UseRedis { get; set; }
    public string RedisConnectionStringName { get; set; }
    public int RedisExpireMinutes { get; set; } = 3;

    /// <summary>
    /// 数据中心编码（默认范围0-7)
    /// </summary>
    public int DataCenterId { get; set; }
    public byte DataCenterIdBits { get; set; } = 3;

    /// <summary>
    /// 机器ID（默认范围 0-1023)
    /// </summary>
    public int WorkerId { get; set; }
    /// <summary>
    /// 机器码位长度
    /// </summary>
    public byte WorkerIdBits { get; set; } = 10;

    public override void Bind(IConfiguration configuration)
    {
        base.Bind(configuration);
        if (RedisExpireMinutes < 3)
            RedisExpireMinutes = 3;
    }
}