using AiUo.Collections;
using AiUo.Data.SqlSugar;
using AiUo.DbCaching.Caching;
using AiUo.Extensions.StackExchangeRedis;
using AiUo.Security;

namespace AiUo.DbCaching;

internal class PageDataProvider
{
    private const int DATA_PAGE_SIZE = 1000;
    private string _configId { get; }
    private string _tableName { get; }
    public string ConnectionStringName { get; }

    private string _cacheKey;
    private DbCacheListDCache _listDCache;
    private DbCacheDataDCache _dataDCache;
    public PageDataProvider(string configId, string tableName, string connectionStringName = null)
    {
        _configId = configId ?? DbUtil.DefaultConfigId;
        _tableName = tableName;
        ConnectionStringName = connectionStringName;
        //
        _cacheKey = DbCachingUtil.GetCacheKey(_configId, _tableName);
        _listDCache = new DbCacheListDCache(ConnectionStringName);
        _dataDCache = new DbCacheDataDCache(_configId, _tableName, ConnectionStringName);
    }

    public async Task<DbTableRedisData> SetRedisValues()
    {
        // 避免并发
        using var redLock = await RedisUtil.LockAsync($"_DbCachingDataProvider:{_cacheKey}", 180);
        if (!redLock.IsLocked)
            throw new Exception($"DbCacheDataDCache获取缓存锁超时。key:{_cacheKey}");

        // 装载数据
        var ret = await GetDbTableData();

        var listDo = await _listDCache.GetAsync(_cacheKey);
        // 数据不相同
        if (!listDo.HasValue || listDo.Value.DataHash != ret.DataHash
                             || listDo.Value.PageSize != ret.PageSize || listDo.Value.PageCount != ret.PageCount)
        {
            int i = 0;
            await _dataDCache.KeyDeleteAsync();
            foreach (var pageString in ret.PageList)
            {
                await _dataDCache.SetAsync($"{++i}", pageString);
            }
        }
        await _dataDCache.SetAsync("0", ret.UpdateDate);
        await _listDCache.SetAsync(_cacheKey, new DbCacheListDO()
        {
            ConfigId = _configId,
            TableName = _tableName,
            PageSize = ret.PageSize,
            PageCount = ret.PageCount,
            DataHash = ret.DataHash,
            UpdateDate = ret.UpdateDate
        });
        return ret;
    }

    public async Task<DbTableRedisData> GetRedisValues()
    {
        var listDo = await _listDCache.GetAsync(_cacheKey);
        if (!listDo.HasValue || !await _dataDCache.KeyExistsAsync())
        {
            return await SetRedisValues();
        }
        var ret = new DbTableRedisData()
        {
            ConfigId = listDo.Value.ConfigId,
            TableName = listDo.Value.TableName,
            PageSize = listDo.Value.PageSize,
            PageCount = listDo.Value.PageCount,
            DataHash = listDo.Value.DataHash,
            UpdateDate = listDo.Value.UpdateDate,
        };

        // 避免并发
        using var redLock = await RedisUtil.LockAsync($"_DbCachingDataProvider:{_cacheKey}", 180);
        if (!redLock.IsLocked)
            throw new Exception($"DbCacheDataDCache获取缓存锁超时。key:{_cacheKey}");

        var updateDate = await _dataDCache.GetOrDefaultAsync("0", null);
        if (!string.IsNullOrEmpty(updateDate) && updateDate != listDo.Value.UpdateDate)
            throw new Exception($"DbCacheDataDCache.GetRedisValues()时，DbCacheListDCache的UpdateDate与DbCacheDataDCache的不同。key:{_cacheKey}");
        for (int i = 1; i <= listDo.Value.PageCount; i++)
        {
            var pageString = await _dataDCache.GetOrExceptionAsync(i.ToString());
            ret.PageList.Add(pageString);
        }
        return ret;
    }
    public async Task<DbTableRedisData> GetDbTableData()
    {
        var ret = new DbTableRedisData()
        {
            ConfigId = _configId,
            TableName = _tableName,
            PageSize = DATA_PAGE_SIZE,
            UpdateDate = DateTime.UtcNow.UtcToCNString()
        };
        var totalList = await DbUtil.GetDbById(_configId).Queryable<object>()
            .AS(_tableName).ToListAsync();
        var pageList = totalList.ToPage(ret.PageSize);
        ret.PageCount = pageList.Count;
        string dataString = null;
        foreach (var item in pageList)
        {
            var pageString = SerializerUtil.SerializeJson(item);
            ret.PageList.Add(pageString);
            dataString += pageString;
        }
        ret.DataHash = !string.IsNullOrEmpty(dataString)
            ? SecurityUtil.MD5Hash(dataString)
            : null;
        return ret;
    }
}
public class DbTableRedisData
{
    public string ConfigId { get; set; }
    public string TableName { get; set; }
    public int PageSize { get; set; }
    public int PageCount { get; set; }
    public string DataHash { get; set; }
    public string UpdateDate { get; set; }

    public List<string> PageList { get; set; } = new();
}