using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.ComponentModel;
using System.Threading.Tasks;

namespace AiUo.Data;

/// <summary>
/// 分页基类
/// </summary>
public abstract class DataPagerBase : IDataPager
{
    private static object _locker { get; } = new object();
    /// <summary>
    /// 实现分页相关缓存项
    /// </summary>
    protected SqlPagerCacheItem _cacheItem;

    #region Properties
    /// <summary>
    /// 页大小
    /// </summary>
    public int PageSize { get; internal set; }

    /// <summary>
    /// 当前的数据库对象
    /// </summary>
    public Database Database { get; internal set; }
    /// <summary>
    /// 用户数据
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string UserData { get; internal set; }
    /// <summary>
    /// 获取原始SQL语句
    /// </summary>
    public string OriginalSql { get; internal set; }

    /// <summary>
    /// 获取获取第一页SQL语句
    /// </summary>
    public string FirstPageSql =>_cacheItem.FirstPageSql;

    /// <summary>
    /// 获取分页语句
    /// </summary>
    public string PagerSql => _cacheItem.PagerSql;

    /// <summary>
    /// 获取记录总数SQL语句
    /// </summary>
    public string RecordCountSql => _cacheItem.RecordCountSql;
    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sql">sql语句</param>
    /// <param name="pageSize">页大小</param>
    /// <param name="database">数据库</param>
    /// <param name="userData">用户数据，不同数据库可能需要此数据生成分页SQL</param>
    public DataPagerBase(string sql, int pageSize, Database database, string userData = null)
    {
        OriginalSql = sql;
        PageSize = pageSize;
        Database = database;
        UserData = userData;
        _cacheItem = GetSqlCacheItem();
    }

    /// <summary>
    /// 不同的数据库进行不同的SQL解析
    /// </summary>
    /// <returns></returns>
    protected abstract SqlPagerCacheItem GetSqlCacheItem();

    #region 记录数
    /// <summary>
    /// 最后一次获取总记录数的时间
    /// </summary>
    public DateTime RefreshCountDate { get; internal set; }

    private long _recordCount = -1;
    /// <summary>
    /// 获取上次获取的总记录数或者重新获取记录数
    /// </summary>
    public long GetRecordCount(bool refresh = true)
    {
        if (refresh || _recordCount == -1)
        {
            InitRecordCount().Wait();
        }
        return _recordCount;
    }
    public async Task<long> GetRecordCountAsync(bool refresh = true)
    {
        if (refresh || _recordCount == -1)
        {
            await InitRecordCount();
        }
        return _recordCount;
    }

    private long _pageCount = -1;
    /// <summary>
    /// 获取最新总页数或者上次获取的总页数
    /// </summary>
    /// <param name="refresh">true:总是获取最新,false:获取上次</param>
    /// <returns></returns>
    public long GetPageCount(bool refresh = true)
    {
        if (refresh || _recordCount == -1)
        {
            InitRecordCount().Wait();
        }
        return _pageCount;
    }
    public async Task<long> GetPageCountAsync(bool refresh = true)
    {
        if (refresh || _recordCount == -1)
        {
            await InitRecordCount();
        }
        return _pageCount;
    }

    private async Task InitRecordCount()
    {
        IDao dao = Database.GetSqlDao(RecordCountSql);
        if (_paras.Count > 0) dao.AddParameters(CloneParameters());
        _recordCount = await dao.ExecScalarAsync<long>();
        _pageCount = CalcPageCount(_recordCount, PageSize);
        RefreshCountDate = DateTime.Now;
        dao.ClearParameters();
    }
    //计算总页数
    private static long CalcPageCount(long totalCount, long pageSize)
        => (totalCount % pageSize == 0) ? totalCount / pageSize : totalCount / pageSize + 1;
    #endregion
    /// <summary>
    /// 获取分页数据
    /// </summary>
    /// <param name="pageIndex">页索引</param>
    /// <returns></returns>
    public DataReaderWrapper GetPageReader(int pageIndex)
    {
        IDao dao = BuildSqlDao(pageIndex);
        if (_paras.Count > 0) dao.AddParameters(CloneParameters());
        return dao.ExecReader();
    }
    public async Task<DataReaderWrapper> GetPageReaderAsync(int pageIndex)
    {
        IDao dao = BuildSqlDao(pageIndex);
        if (_paras.Count > 0) dao.AddParameters(CloneParameters());
        return await dao.ExecReaderAsync();
    }
    /// <summary>
    /// 获取SqlDao
    /// </summary>
    /// <param name="pageIndex"></param>
    /// <returns></returns>
    protected abstract IDao BuildSqlDao(int pageIndex);

    /// <summary>
    /// 获得分页DataTable
    /// </summary>
    /// <param name="pageIndex"></param>
    /// <returns></returns>
    public DataTable GetPageTable(int pageIndex)
        => GetPageReader(pageIndex).ToTable();
    public async Task<DataTable> GetPageTableAsync(int pageIndex)
    {
        var reader = await GetPageReaderAsync(pageIndex);
        return reader.ToTable();
    }

    /// <summary>
    /// 参数集合
    /// </summary>
    protected List<DbParameter> _paras { get; } = new List<DbParameter>();

    private DbParameter[] CloneParameters()
    {
        DbParameter[] ret = null;
        if (_paras != null && _paras.Count > 0)
        {
            ret = new DbParameter[_paras.Count];
            for (int i = 0; i < _paras.Count; i++)
            {
                ret[i] = (DbParameter)((ICloneable)_paras[i]).Clone();
            }
        }
        return ret;
    }

    /// <summary>
    /// 添加分页SQL语句中定义的参数
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="value">参数值</param>
    /// <param name="dbType">参数类型</param>
    /// <param name="size">参数大小</param>
    public DbParameter AddInParameter(string name, object value, DbType dbType, int size=0)
    {
        var param = Database.CreateParameter(name, dbType, size, ParameterDirection.Input, value);
        _paras.Add(param);
        return param;
    }

    /// <summary>
    /// 添加分页SQL语句中定义的参数集合
    /// </summary>
    /// <param name="paras">DbParameter集合对象</param>
    public void AddParameters(IEnumerable<DbParameter> paras)
    {
        if (paras != null)
            _paras.AddRange(paras);
    }

    /// <summary>
    /// 实现分页相关缓存项
    /// </summary>
    public class SqlPagerCacheItem
    {
        /// <summary>
        /// 获取第一页SQL语句
        /// </summary>
        public string FirstPageSql { get; set; }

        /// <summary>
        /// 分页语句
        /// </summary>
        public string PagerSql { get; set; }

        /// <summary>
        /// 获取记录总数SQL语句
        /// </summary>
        public string RecordCountSql { get; set; }
    }
}