using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace AiUo.Data.MySql;

/// <summary>
/// MySql分页类
/// </summary>
public class MySqlDataPager : DataPagerBase
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="pageSize"></param>
    /// <param name="database"></param>
    public MySqlDataPager(string sql, int pageSize, Database database)
        : base(sql, pageSize, database)
    {
        if (Database.Provider != DbDataProvider.MySqlClient)
            throw new ArgumentException("无法创建MySql分页类，Database必须是MySQL类型。", "database");
    }
    /// <summary>
    /// 添加分页SQL语句中定义的参数
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="value">参数值</param>
    /// <param name="dbType">参数类型</param>
    /// <param name="size">参数大小</param>
    public MySqlParameter AddInParameter(string name, object value = null, MySqlDbType dbType = default(MySqlDbType), int size = 0)
    {
        var param = (Database as MySqlDatabase).CreateParameter(name, dbType, size, ParameterDirection.Input, value);
        _paras.Add(param);
        return param;
    }
    /// <summary>
    /// 获得IDao
    /// </summary>
    /// <param name="pageIndex"></param>
    /// <returns></returns>
    protected override IDao BuildSqlDao(int pageIndex)
    {
        IDao dao = null;
        string sql = string.Empty;
        if (pageIndex == 1 || pageIndex < 1)
        {
            sql = FirstPageSql;
            dao = Database.GetSqlDao(sql);
        }
        else
        {
            sql = string.Format(PagerSql, (pageIndex - 1) * PageSize);
            dao = Database.GetSqlDao(sql);
        }
        return dao;
    }
    /// <summary>
    /// 获得SQL
    /// </summary>
    /// <returns></returns>
    protected override SqlPagerCacheItem GetSqlCacheItem()
    {
        var ret = new SqlPagerCacheItem();
        ret.RecordCountSql = string.Format("SELECT COUNT(*) FROM ({0}) AS TBL", OriginalSql);
        ret.FirstPageSql = string.Format("SELECT * FROM ({0}) AS TBL LIMIT {1}", OriginalSql, PageSize);
        ret.PagerSql = string.Format("SELECT * FROM ({0}) AS TBL LIMIT {{0}},{1}", OriginalSql, PageSize);
        return ret;
    }
}