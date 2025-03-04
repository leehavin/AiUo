namespace AiUo.DbCaching;

public interface IDbCachePreloadProvider
{
    List<DbCachePreloadItem> GetPreloadList();
}
public class DbCachePreloadItem
{
    public Type EntityType { get; set; }
    public object SplitDbKey { get; set; }
    public DbCachePreloadItem(Type entityType, object splitDbKey = null)
    {
        EntityType = entityType;
        SplitDbKey = splitDbKey;
    }
}