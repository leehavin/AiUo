namespace AiUo.Data;

/// <summary>
/// 数据源（数据库）类型
/// </summary>
public enum DatabaseType
{
    /// <summary>
    /// 未知
    /// </summary>
    Unknown,
    /// <summary>
    /// MySQL
    /// </summary>
    MySql,
    /// <summary>
    /// Microsoft SQL Server
    /// </summary>
    SqlServer,
    /// <summary>
    /// Oracle 数据库
    /// </summary>
    Oracle,
    /*
    /// <summary>
    /// Microsoft SQL Server 数据库文件
    /// </summary>
    SqlFile,
    /// <summary>
    /// Microsoft Access 数据库文件
    /// </summary>
    Access,
    */
}