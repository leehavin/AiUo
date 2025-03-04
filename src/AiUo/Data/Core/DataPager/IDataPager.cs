using System;
using System.Data.Common;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AiUo.Data;

/// <summary>
/// 数据分页接口，目前只支持SQL Server和Oracle,MySQL
/// SQL语句中支持参数定义
/// 不支持UNION、EXCEPT 和 INTERSECT
/// 只支持关键键（主键或唯一键）中包含单一字段的分页查询
/// 如果存在Group By子句，则SELECT和Order By中的聚合函数字段必须使用别名
/// </summary>
public interface IDataPager
{
    /// <summary>
    /// 获取上次获取的总记录数或者重新获取记录数
    /// </summary>
    /// <param name="refresh">是否从数据库获取最新记录总数，如果为false则返回上次查询的记录总数</param>
    /// <returns></returns>
    long GetRecordCount(bool refresh = true);
    Task<long> GetRecordCountAsync(bool refresh = true);
    /// <summary>
    /// 最后一次获取总记录数的时间，根据此时间判断是否从新获取最新记录总数
    /// </summary>
    DateTime RefreshCountDate { get; }
    /// <summary>
    /// 获取上次获取的总页数或刷新
    /// </summary>
    long GetPageCount(bool refresh = true);
    Task<long> GetPageCountAsync(bool refresh = true);
    /// <summary>
    /// 添加分页SQL语句中定义的参数
    /// </summary>
    /// <param name="name">参数名称</param>
    /// <param name="value">参数值</param>
    /// <param name="dbType">参数类型</param>
    /// <param name="size">参数大小</param>
    DbParameter AddInParameter(string name, object value = null, DbType dbType = DbType.AnsiString, int size = 0);

    /// <summary>
    /// 添加分页SQL语句中定义的参数集合
    /// </summary>
    /// <param name="paras">DbParameter集合对象</param>
    void AddParameters(IEnumerable<DbParameter> paras);

    /// <summary>
    /// 获取分页数据
    /// </summary>
    /// <param name="pageIndex">页索引</param>
    /// <returns></returns>
    DataReaderWrapper GetPageReader(int pageIndex);
    Task<DataReaderWrapper> GetPageReaderAsync(int pageIndex);

    /// <summary>
    /// 获取分页数据
    /// </summary>
    /// <param name="pageIndex">页索引</param>
    /// <returns></returns>
    DataTable GetPageTable(int pageIndex);
}