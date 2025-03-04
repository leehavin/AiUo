namespace AiUo.SnowflakeId;

public static class SnowflakeIdUtil
{
    /// <summary>
    /// 生成雪花算法ID
    /// </summary>
    /// <returns></returns>
    public static long NextId()
    {
        return DIUtil.GetRequiredService<ISnowflakeIdService>().NextId();
    }
}