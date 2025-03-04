namespace AiUo.DbCaching;

public class DbCacheCheckMessage
{
    /// <summary>
    /// 唯一好
    /// </summary>
    public string TraceId { get; set; }
    public string RedisConnectionStringName { get; set; }
    public string CheckDate { get; set; }
    public Dictionary<string, DbCacheCheckItem> CheckItems { get; set; }
}
public class DbCacheCheckItem
{
    public string ConfigId { get; set; }
    public string TableName { get; set; }
    public string DbHash { get; set; }
}