using System;
using System.Collections.Generic;
using System.Text;
using AiUo.Data.ORM;
using AiUo.Data.Schema;
using System.Linq;
using MySql.Data.MySqlClient;

namespace AiUo.Data.MySql;

/// <summary>
/// 
/// </summary>
public class MySqlOrmProvider : IDbOrmProvider<MySqlDatabase, MySqlParameter, MySqlDbType>
{
    private MySqlTypeMapper _typeMapper = new MySqlTypeMapper();
    /// <summary>
    /// 获得Select SQL
    /// </summary>
    /// <param name="sourceName"></param>
    /// <param name="where"></param>
    /// <param name="top"></param>
    /// <param name="sort"></param>
    /// <param name="fields"></param>
    /// <param name="isForUpdate"></param>
    /// <returns></returns>
    public string BuildSelectSQL(string sourceName, string where, int top, string sort, string fields = null, bool isForUpdate = false)
    {
        var topStr = (top > 0) ? " LIMIT " + top : string.Empty;
        var whereStr = !string.IsNullOrEmpty(where) ? " WHERE " + where : string.Empty;
        var sortStr = !string.IsNullOrEmpty(sort) ? " ORDER BY " + sort : string.Empty;
        var fieldsStr = !string.IsNullOrEmpty(fields) ? fields : "*";
        var forUpdate = isForUpdate ? " FOR UPDATE" : string.Empty;
        return $"SELECT {fieldsStr} FROM {sourceName}{whereStr}{sortStr}{topStr}{forUpdate}";
    }
    /// <summary>
    /// 获取表或试图列参数名对应的MySqlDbType集合
    /// </summary>
    /// <param name="database"></param>
    /// <param name="sourceName"></param>
    /// <param name="objectType"></param>
    /// <returns></returns>
    public Dictionary<string, MySqlDbType> GetDbTypeMappings(MySqlDatabase database, string sourceName, DbObjectType objectType)
    {
        sourceName = sourceName.Trim('`');
        var provider = new MySqlSchemaProvider(database);
        Schema.SchemaCollection<Schema.ColumnSchema> columns = null;
        switch (objectType)
        {
            case DbObjectType.Table:
                columns = provider.GetTableColumns(sourceName, null);
                break;
            case DbObjectType.View:
                columns = provider.GetViewColumns(sourceName, null);
                break;
        }
        Dictionary<string, MySqlDbType> dic = new Dictionary<string, MySqlDbType>();
        foreach (MySqlColumnSchema column in columns)
            dic.Add(column.SqlParamName.ToUpper(), column.DbType);
        return dic;
    }
    /// <summary>
    /// 获得插入Dao
    /// </summary>
    /// <param name="db"></param>
    /// <param name="tableName"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public MySqlSqlDao BuildInsertDao(MySqlDatabase db, string tableName, List<object> values = null)
    {
        var provider = new MySqlSchemaProvider(db);
        var tableSchema = provider.GetTable(tableName);
        var columnSchemas = tableSchema.ColumnsFilter(ColumnSelectMode.NoAutoIncrement).ToList();
        if (values != null && values.Count != columnSchemas.Count)
            throw new Exception($"传入的参数值values和所需参数数量不匹配。columnSchemas.Count={columnSchemas.Count}. values.Count={values.Count}");
        var sql = BuildInsertSql(tableSchema, columnSchemas);
        var ret = new MySqlSqlDao(sql, db);
        for (int i = 0; i < columnSchemas.Count; i++)
        {
            object value = values?[i];
            var column = columnSchemas[i] as MySqlColumnSchema;
            ret.AddInParameter(column.SqlParamName, value, column.DbType);
        }
        return ret;
    }
    public string BuildInsertSql(TableSchema table, IEnumerable<ColumnSchema> columns = null)
    {
        if (columns == null)
            columns = table.Columns;
        var ret = new StringBuilder($"INSERT INTO {table.SqlTableName} ");
        var columnNames = columns.Select(item => { return item.SqlColumnName; });
        ret.Append($"({string.Join(", ", columnNames)}) ");
        var paramNames = columns.Select(item => { return item.SqlParamName; });
        ret.Append($"VALUE ({string.Join(", ", paramNames)})");
        return ret.ToString();
    }

    public MySqlDbType MapDotNetTypeToDbType(Type type)
    {
        return _typeMapper.MapDotNetTypeToDbType(type);
    }
}