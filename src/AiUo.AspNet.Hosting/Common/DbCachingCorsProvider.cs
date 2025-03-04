using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using AiUo.Configuration;
using AiUo.DbCaching;

namespace AiUo.AspNet.Hosting;

public abstract class DbCachingCorsProvider<TEntity> : ICorsPoliciesProvider
    where TEntity : class, new()
{
    protected virtual object GetSplitDbKey() => null;
    protected abstract List<CorsPolicyElement> GetPolicies(List<TEntity> list);

    public List<CorsPolicyElement> GetPolicies()
    {
        var list = GetCache().GetAllList();
        var ret = GetPolicies(list);
        return ret;
    }

    private DbCacheMemory<TEntity> _cache;
    private DbCacheMemory<TEntity> GetCache()
    {
        if (_cache == null)
        {
            _cache = DbCachingUtil.GetCache<TEntity>(GetSplitDbKey());
        }
        return _cache;
    }

    public void SetAutoRefresh()
    {
        GetCache().UpdateCallback = UpdateCallback;
    }
    protected void UpdateCallback(List<TEntity> oldList, List<TEntity> newList)
    {
        var opts = DIUtil.GetRequiredService<IOptions<CorsOptions>>();
        var section = ConfigUtil.GetSection<CorsSection>();
        section?.AddPolicies(opts.Value, true);
    }
}