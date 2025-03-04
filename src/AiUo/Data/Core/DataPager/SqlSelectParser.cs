using System;

namespace AiUo.Data;

/// <summary>
/// SQL查询语句结构
/// </summary>
public class SqlSelectParser
{
    /// <summary>
    /// 原始SQL
    /// </summary>
    public string OriginalSql { get; internal set; }
    /// <summary>
    /// Select
    /// </summary>
    public SelectClause Select { get; set; }
    /// <summary>
    /// From
    /// </summary>
    public FromClause From { get; set; }
    /// <summary>
    /// Where
    /// </summary>
    public WhereClause Where { get; set; }
    /// <summary>
    /// GroupBy
    /// </summary>
    public GroupByClause GroupBy { get; set; }
    /// <summary>
    /// Having
    /// </summary>
    public HavingClause Having { get; set; }
    /// <summary>
    /// OrderBy
    /// </summary>
    public OrderByClause OrderBy { get; set; }
    /// <summary>
    /// Select解析
    /// </summary>
    /// <param name="sql"></param>
    public SqlSelectParser(string sql)
    {
        OriginalSql = sql;
        Parse(sql.Trim().ReplaceWhiteSpace(' '));
    }

    private static int GetIndex(string sql, string key)
    {
        int ret = 0;
        string tmp = string.Empty;
        while (true)
        {
            ret = sql.IndexOf(key, ret, StringComparison.CurrentCultureIgnoreCase);
            if (ret < 0 || CheckFieldFull(sql.Substring(0, ret)))
                break;
            ret += key.Length;
        }
        return ret;
    }

    private void Parse(string sql)
    {
        int selectPos = 0;
        int fromPos = -1;
        int wherePos = -1;
        int groupPos = -1;
        int havingPos = -1;
        int orderPos = -1;

        if (!sql.StartsWith("SELECT ", StringComparison.CurrentCultureIgnoreCase))
            throw new ArgumentException("SQL语句不是SELECT 语句，SELECT语句必须以SELECT 开头。");

        fromPos = GetIndex(sql, " FROM ");
        wherePos = GetIndex(sql, " WHERE ");
        groupPos = GetIndex(sql, " GROUP BY ");
        havingPos = GetIndex(sql, " HAVING ");
        orderPos = GetIndex(sql, " ORDER BY ");


        string text = string.Empty;
        //select
        if (fromPos > -1)
            text = sql.Substring(selectPos, fromPos - selectPos);
        else if (wherePos > -1)
            text = sql.Substring(selectPos, wherePos - selectPos);
        else if (groupPos > -1)
            text = sql.Substring(selectPos, groupPos - selectPos);
        else if (orderPos > -1)
            text = sql.Substring(selectPos, orderPos - selectPos);
        else
            text = sql.Substring(selectPos);
        Select = new SelectClause(text);
        //from
        if (fromPos > -1)
        {
            if (wherePos > -1)
                text = sql.Substring(fromPos, wherePos - fromPos);
            else if (groupPos > -1)
                text = sql.Substring(fromPos, groupPos - fromPos);
            else if (orderPos > -1)
                text = sql.Substring(fromPos, orderPos - fromPos);
            else
                text = sql.Substring(fromPos);
            From = new FromClause(text);
        }
        //where
        if (wherePos > -1)
        {
            if (groupPos > -1)
                text = sql.Substring(wherePos, groupPos - wherePos);
            else if (orderPos > -1)
                text = sql.Substring(wherePos, orderPos - wherePos);
            else
                text = sql.Substring(wherePos);
            Where = new WhereClause(text);
        }
        //group by
        if (groupPos > -1)
        {
            if (havingPos > -1)
                text = sql.Substring(groupPos, havingPos - groupPos);
            else if (orderPos > -1)
                text = sql.Substring(groupPos, orderPos - groupPos);
            else
                text = sql.Substring(groupPos);
            GroupBy = new GroupByClause(text);
            //having
            if (havingPos > -1)
            {
                if (orderPos > -1)
                    text = sql.Substring(havingPos, orderPos - havingPos);
                else
                    text = sql.Substring(havingPos);
                Having = new HavingClause(text);

            }
        }
        //order by
        if (orderPos > -1)
        {
            text = sql.Substring(orderPos);
            OrderBy = new OrderByClause(text);
        }
    }
    private static bool CheckFieldFull(string text)
    {
        if (string.IsNullOrEmpty(text)) return true;
        int left = 0;
        int right = 0;
        foreach (char chr in text)
        {
            if (chr == '(') left++;
            if (chr == ')') right++;
        }
        return left == right;
    }
    /// <summary>
    /// 获得结构
    /// </summary>
    /// <returns></returns>
    public SqlSelectStruct GetStruct()
    {
        var ret = new SqlSelectStruct() {
            Select = Select.Value,
            From = From.Value,
            Where = Where.Value,
            GroupBy = GroupBy.Value,
            Having = Having.Value,
            OrderBy = OrderBy.Value
        };
        return ret;
    }
}
/// <summary>
/// 基类
/// </summary>
public class ClauseBase
{
    /// <summary>
    /// 
    /// </summary>
    public string Original { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string Value { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string KeyWords { get; set; }
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="text"></param>
    /// <param name="keywords"></param>
    public ClauseBase(string text, string keywords)
    {
        Original = text;
        Value = text.Trim().TrimStart(keywords);
        KeyWords = keywords;
    }
}
/// <summary>
/// Select
/// </summary>
public class SelectClause : ClauseBase
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="text"></param>
    public SelectClause(string text)
        : base(text, "SELECT ")
    { }
}
/// <summary>
/// From
/// </summary>
public class FromClause : ClauseBase
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="text"></param>
    public FromClause(string text)
        : base(text, "FROM ")
    { }
}
/// <summary>
/// Where
/// </summary>
public class WhereClause : ClauseBase
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="text"></param>
    public WhereClause(string text)
        : base(text, "WHERE ")
    { }
}
/// <summary>
/// Group
/// </summary>
public class GroupByClause : ClauseBase
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="text"></param>
    public GroupByClause(string text)
        : base(text, "GROUP BY ")
    { }
}
/// <summary>
/// Having
/// </summary>
public class HavingClause : ClauseBase
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="text"></param>
    public HavingClause(string text)
        : base(text, "HAVING ")
    { }
}
/// <summary>
/// Order
/// </summary>
public class OrderByClause : ClauseBase
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="text"></param>
    public OrderByClause(string text)
        : base(text, "ORDER BY ")
    { }
}