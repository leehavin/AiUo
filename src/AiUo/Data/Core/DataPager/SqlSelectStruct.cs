using System.Text;

namespace AiUo.Data;

/// <summary>
/// SQL语句解析后的结构
/// </summary>
public class SqlSelectStruct
{
    /// <summary>
    /// 
    /// </summary>
    public string Select { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string From { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string Where { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string GroupBy { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string Having { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string OrderBy { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("SELECT ");
        if (!string.IsNullOrEmpty(Select))
            sb.Append(Select);
        else
            sb.Append("*");
        if (!string.IsNullOrEmpty(From))
        {
            sb.Append(" FROM ");
            sb.Append(From);
        }
        if (!string.IsNullOrEmpty(Where))
        {
            sb.Append(" WHERE ");
            sb.Append(Where);
        }
        if (!string.IsNullOrEmpty(GroupBy))
        {
            sb.Append(" GROUP BY ");
            sb.Append(GroupBy);
        }
        if (!string.IsNullOrEmpty(Having))
        {
            sb.Append(" HAVING ");
            sb.Append(Having);
        }
        if (!string.IsNullOrEmpty(OrderBy))
        {
            sb.Append(" ORDER BY ");
            sb.Append(OrderBy);
        }
        return sb.ToString();
    }
}