namespace AiUo.Data;

/// <summary>
/// 数据库操作辅助类
/// </summary>
public static partial class DbHelper
{
    #region SQL注入验证
    private static string[] SQL_KEYWORDS => @"select|insert|delete|from|drop table|update|truncate|exec master|netlocalgroup administrators|:|net user|or|and".Split('|');
    private static string[] SQL_REGEXS => @"=|!|'".Split('|');

    /// <summary>
    /// 检查SQL注入，如果存在危险返回true
    /// </summary>
    /// <param name="sql">检查的SQL语句</param>
    /// <returns></returns>
    public static bool CheckSqlInjection(string sql)
    {
        if (string.IsNullOrEmpty(sql)) return false;
        sql = sql.ToLower();
        foreach (string sqlKey in SQL_KEYWORDS)
        {
            if (sql.IndexOf(" " + sqlKey) >= 0 || sql.IndexOf(sqlKey + " ") >= 0)
                return true;
        }
        //开始检查 模式1:Sql注入的可能特殊符号 的注入情况
        foreach (string sqlKey in SQL_REGEXS)
        {
            if (sql.IndexOf(sqlKey) >= 0)
                return true;
        }
        return false;
    }
    #endregion
}

#region 常用SQL语句

#region 获取表所使用的空间
/*
select a.object_id as ObjectID, b.name as SchemaName, a.name as TableName, c.rows as [RowCount], c.used as UsedSpace, c.reserved as ReservedSpace
from sys.tables a
left join sys.schemas b on a.schema_id = b.schema_id
left join (select object_id, SUM(row_count) rows, SUM(used_page_count)*8*1024 used, SUM(reserved_page_count)*8*1024 reserved
    from sys.dm_db_partition_stats
    group by object_id) c on a.object_id = c.object_id
order by c.reserved desc
 */
#endregion

#region 收缩数据库
/*
1.清空日志
DUMP TRANSACTION 库名 WITH NO_LOG

2.截断事务日志：
BACKUP LOG 库名 WITH NO_LOG

3.收缩数据库
DBCC SHRINKDATABASE(库名)

4.收缩指定数据文件,1是文件号,可以通过这个语句查询到:select * from sys.sysfiles
DBCC SHRINKFILE(1)
4.设置自动收缩
EXEC sp_dboption '库名', 'autoshrink', 'TRUE'
 */
#endregion

#endregion