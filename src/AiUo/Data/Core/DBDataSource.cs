namespace AiUo.Data;

/// <summary>
/// 数据源（数据库）类型
/// </summary>
public enum DBDataSource
{
    /// <summary>
    /// Microsoft Access 数据库文件
    /// </summary>
    Access,
    /// <summary>
    /// Microsoft ODBC 数据源
    /// </summary>
    Odbc,
    /// <summary>
    /// Oracle 数据库
    /// </summary>
    Oracle,
    /// <summary>
    /// Microsoft SQL Server
    /// </summary>
    SqlServer,
    /// <summary>
    /// Microsoft SQL Server 数据库文件
    /// </summary>
    SqlFile,
    /// <summary>
    /// MySQL
    /// </summary>
    MySql
}