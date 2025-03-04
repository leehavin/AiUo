namespace AiUo.Data.MySql;

/// <summary>
/// MySQL环境变量项
/// SHOW VARIABLES显示的Variable_name
/// </summary>
public enum MySqlVariableNames
{
    /// <summary>
    /// MySQL最大接受的数据包大小， 默认4M
    /// </summary>
    max_allowed_packet,
    /// <summary>
    /// 日志缓存大小，默认4M
    /// </summary>
    innodb_log_buffer_size
}