
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AiUo.Data.Schema;

namespace AiUo.Data.MySql;

/// <summary>
/// MySQL 数据库概要信息提供程序
/// </summary>
public class MySqlSchemaProvider
{
    #region information_schema 说明
    /*
    SCHEMATA 表：提供了当前mysql实例中所有数据库的信息。是show databases的结果取之此表。
    TABLES 表：提供了关于数据库中的表的信息（包括视图）。详细表述了某个表属于哪个schema，表类型，表引擎，创建时间等信息。是show tables from schemaname的结果取之此表。
    COLUMNS 表：提供了表中的列信息。详细表述了某张表的所有列以及每个列的信息。是show columns from schemaname.tablename的结果取之此表。
    STATISTICS 表：提供了关于表索引的信息。是show index from schemaname.tablename的结果取之此表。
    VIEWS 表：给出了关于数据库中的视图的信息。需要有show views权限，否则无法查看视图信息。
    TABLE_CONSTRAINTS 表：描述了存在约束的表。以及表的约束类型。
    KEY_COLUMN_USAGE 表：描述了具有约束的键列。
    ROUTINES 表：提供了关于存储子程序（存储程序和函数）的信息。此时，ROUTINES表不包含自定义函数（UDF）。名为“mysql.proc name”的列指明了对应于INFORMATION_SCHEMA.ROUTINES表的mysql.proc表列。

    CHARACTER_SETS （字符集）表：提供了mysql实例可用字符集的信息。是SHOW CHARACTER SET结果集取之此表。
    COLLATIONS 表：提供了关于各字符集的对照信息。
    COLLATION_CHARACTER_SET_APPLICABILITY 表：指明了可用于校对的字符集。这些列等效于SHOW COLLATION的前两个显示字段。
    USER_PRIVILEGES （用户权限）表：给出了关于全程权限的信息。该信息源自mysql.user授权表。是非标准表。
    SCHEMA_PRIVILEGES （方案权限）表：给出了关于方案（数据库）权限的信息。该信息来自mysql.db授权表。是非标准表。
    TABLE_PRIVILEGES （表权限）表：给出了关于表权限的信息。该信息源自mysql.tables_priv授权表。是非标准表。
    COLUMN_PRIVILEGES （列权限）表：给出了关于列权限的信息。该信息源自mysql.columns_priv授权表。是非标准表。
    TRIGGERS 表：提供了关于触发程序的信息。必须有super权限才能查看该表。
     */
    #endregion

    private readonly MySqlDatabase _db;
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public MySqlSchemaProvider(MySqlDatabase db)
    {
        _db = db;
        if (_db.CommandTimeout < 60)
            _db.CommandTimeout = 60;
    }

    /// <summary>
    /// 获取数据库的Schema
    /// </summary>
    /// <returns></returns>
    public DatabaseSchema GetDatabase()
    {
        var ret = new DatabaseSchema();
        ret.DatabaseName = _db.DatabaseName;
        ret.DbDataProvider = _db.Provider;
        ret.ConnectionString = _db.ConnectionString;
        ret.ConnectionStringInfo = _db.ConnectionStringInfo;
        string sql = string.Format("select * from information_schema.SCHEMATA where SCHEMA_NAME = '{0}'", ret.DatabaseName);
        var row = _db.ExecSqlSingle(sql);
        if (row == null) return null;
        ret.DefaultCharSetName = row.ToString("DEFAULT_CHARACTER_SET_NAME");
        ret.DefaultCollationName = row.ToString("DEFAULT_COLLATION_NAME");
        ret.Tables = GetTables(ret);
        ret.Views = GetViews(ret);
        ret.Procs = GetProcs(ret);
        return ret;
    }

    #region TableSchema
    /// <summary>
    /// 获取数据表集合
    /// </summary>
    /// <returns></returns>
    public List<string> GetTableNames()
    {
        var sql = string.Format("select TABLE_NAME from information_schema.TABLES where TABLE_SCHEMA='{0}' and table_type='BASE TABLE' order by CREATE_TIME", _db.DatabaseName);
        return _db.ExecSqlList<string>(sql, DataMappingMode.PrimitiveType);
    }
    /// <summary>
    /// 获取数据库中的所有表Schema
    /// </summary>
    /// <returns></returns>
    public SchemaCollection<TableSchema> GetTables(DatabaseSchema parent)
        => GetTables(GetTableNames(), parent);
    /// <summary>
    /// 获取数据库中的所有表Schema
    /// </summary>
    /// <param name="tables"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public SchemaCollection<TableSchema> GetTables(List<string> tables, DatabaseSchema parent = null)
    {
        var ret = new SchemaCollection<TableSchema>();
        if (tables != null)
        {
            foreach (var table in tables)
            {
                var newTable = GetTable(table, parent);
                ret.Add(newTable);
            }
        }
        return ret;
    }
    /// <summary>
    /// 获取表的Schema
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public TableSchema GetTable(string tableName, DatabaseSchema parent = null)
    {
        TableSchema ret = null;
        string sql = string.Format("select * from information_schema.TABLES where TABLE_SCHEMA='{0}' and table_name='{1}' and table_type='BASE TABLE'", _db.DatabaseName, tableName);
        using (var dao = new MySqlSqlDao(sql, _db))
        {
            ret = dao.ExecSingle(
                (reader) => {
                    var table = new TableSchema();
                    table.Database = parent;
                    table.DatabaseName = _db.DatabaseName;
                    table.DbDataProvider = _db.Provider;
                    table.SourceName = tableName;
                    table.Comment = reader.ToString("TABLE_COMMENT");
                    table.CollationName = reader.ToString("TABLE_COLLATION");

                    table.CreateTime = reader.ToDateTime("CREATE_TIME", DateTime.MinValue);
                    table.UpdateTime = reader.ToDateTime("UPDATE_TIME", DateTime.MinValue);
                    table.AutoIncrementValue = reader.ToInt64("AUTO_INCREMENT", -1);
                    table.IndexLength = reader.ToInt64("INDEX_LENGTH", 0);
                    table.TableRows = reader.ToInt64("TABLE_ROWS", 0);
                    table.AvgRowLength = reader.ToInt64("AVG_ROW_LENGTH", 0);
                    table.DataLength = reader.ToInt64("DATA_LENGTH", 0);
                    //
                    table.Columns = GetTableColumns(tableName, table);
                    table.PrimaryKey = GetPrimaryKey(tableName, table.Columns, table);
                    table.ForeignKeys = GetForeignKeys(tableName, table.Columns, table);
                    table.Indexes = GetIndexes(tableName, table.Columns, table);
                    return table;
                });
        }
        return ret;
    }

    #region TableColumnSchema
    /// <summary>
    /// 获取指定表的字段集合，表不存在返回null
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public SchemaCollection<ColumnSchema> GetTableColumns(string tableName, TableSchema parent = null)
    {
        var ret = new SchemaCollection<ColumnSchema>();
        string sql = GetTableColumnSchemaSQL(tableName);
        var items = _db.ExecSqlMulti(sql, MapToTableColumnSchema);
        foreach (var item in items)
        {
            item.Parent = parent;
            ret.Add(item);
        }

        return ret.Count > 0 ? ret : null;
    }
    /// <summary>
    /// 获取指定列名的Schema，不存在返回null
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public ColumnSchema GetTableColumn(string tableName, string columnName, TableSchema parent = null)
    {
        string sql = GetTableColumnSchemaSQL(tableName, columnName);
        var ret = _db.ExecSqlSingle(sql, MapToTableColumnSchema);
        ret.Parent = parent;
        return ret;
    }
    // 映射表的字段信息
    private ColumnSchema MapToTableColumnSchema(IDataReader reader)
    {
        var ret = new MySqlColumnSchema();
        MapColumnSchema(reader, ret);
        ret.IsPrimaryKey = ret.ColumnKey == "PRI";
        //IsPrimaryKey,IsForeignKey等在获取对应信息时更新 ret.IsForeignKey = false;
        ret.IsAutoIncrement = ret.Extra.IndexOf("auto_increment") > -1;
        return ret;
    }
    // 映射表或视图的字段信息
    private void MapColumnSchema(IDataReader reader, MySqlColumnSchema column)
    {
        column.DbDataProvider = _db.Provider;
        column.ParentName = reader.ToString("TABLE_NAME");
        column.ColumnName = reader.ToString("COLUMN_NAME");
        column.Ordinal = reader.ToInt32("ORDINAL_POSITION");
        column.Comment = reader.ToString("COLUMN_COMMENT");
        column.EngineTypeString = reader.ToString("DATA_TYPE");
        column.EngineTypeStringFull = reader.ToString("COLUMN_TYPE");
        // 
        int? length = reader.ToInt32N("NUMERIC_PRECISION");
        if (length.HasValue)//数字类型
        {
            column.Length = length;
            column.Precision = reader.ToInt32N("NUMERIC_SCALE");
            column.IsNumeric = true;
        }
        else//字符串类型
        {
            column.Length = reader.ToInt64N("CHARACTER_MAXIMUM_LENGTH");
            column.Precision = reader.ToInt64N("CHARACTER_OCTET_LENGTH");
            column.IsNumeric = false;
        }
        column.AllowDBNull = reader.ToBoolean("IS_NULLABLE");
        column.IsUnsigned = column.EngineTypeStringFull.IndexOf("unsigned") > -1;
        column.DefaultValue = reader.ToString("COLUMN_DEFAULT");
        column.HasDefaultValue = !string.IsNullOrEmpty(column.DefaultValue)&& column.DefaultValue.ToUpper() != "NULL";
        column.ColumnKey = reader.ToString("COLUMN_KEY");
        column.Extra = reader.ToString("EXTRA");
    }

    private const string _tableColumnSchemaSQL = "select *  from information_schema.COLUMNS";
    private string GetTableColumnSchemaSQL(string tableName = null, string columnName = null)
    {
        string ret = string.Format(_tableColumnSchemaSQL + " where TABLE_SCHEMA='{0}'", _db.DatabaseName);//数据库
        if (!string.IsNullOrEmpty(tableName))
        {
            ret += string.Format(" and TABLE_NAME='{0}'", tableName);//表名
        }
        if (!string.IsNullOrEmpty(columnName))
        {
            ret += string.Format(" and COLUMN_NAME='{0}'", columnName);//字段名
        }
        ret += " order by TABLE_SCHEMA,TABLE_NAME,ORDINAL_POSITION";//排序
        return ret;

    }

    #endregion

    #region PrimaryKey,ForeignKeys,Indexes

    // 获取MySQL数据表的主键
    private PrimaryKeySchema GetPrimaryKey(string tableName, SchemaCollection<ColumnSchema> columns, TableSchema parent)
    {
        PrimaryKeySchema ret = null;
        string sql = string.Format("SELECT t1.CONSTRAINT_NAME, t1.COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE t1 INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS t2 ON t2.TABLE_SCHEMA = t1.TABLE_SCHEMA AND t2.TABLE_NAME = t1.TABLE_NAME AND t2.CONSTRAINT_NAME = t1.CONSTRAINT_NAME WHERE t1.TABLE_SCHEMA = '{0}' AND t1.TABLE_NAME = '{1}' AND t2.CONSTRAINT_TYPE = 'PRIMARY KEY' ORDER BY t1.ORDINAL_POSITION", _db.DatabaseName, tableName);
        List<string> columnNames = _db.ExecSqlMulti(sql
            , reader => {
                return reader.ToString("COLUMN_NAME");
            }).ToList();
        if (columnNames != null && columnNames.Count > 0)
        {
            ret = new PrimaryKeySchema();
            ret.DbDataProvider = _db.Provider;
            ret.TableName = tableName;
            ret.Table = parent;
            ret.IsClustered = true; //MySQL主键比为聚集
            foreach (var name in columnNames)
            {
                if (columns == null)
                { 
                    ret.Columns.Add(GetTableColumn(ret.TableName, name, parent));
                }
                else
                {
                    var column = columns[name];
                    column.IsPrimaryKey = true;
                    ret.Columns.Add(column);
                }
            }
            if (ret.Columns.Count == 1)
                ret.Columns[0].IsSinglePKColumn = true;
        }
        return ret;
    }

    /// <summary>
    /// 获取MySQL数据表的外键信息，没有返回NULL
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columns"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    private List<ForeignKeySchema> GetForeignKeys(string tableName, SchemaCollection<ColumnSchema> columns, TableSchema parent)
    {
        DataSet ds = new DataSet();
        string str1 = string.Format("SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS t1 WHERE t1.TABLE_SCHEMA = '{0}' AND t1.TABLE_NAME = '{1}'  AND CONSTRAINT_TYPE = 'FOREIGN KEY'", _db.DatabaseName, tableName);
        string str2 = string.Format("SELECT t1.CONSTRAINT_NAME, t1.COLUMN_NAME, t1.POSITION_IN_UNIQUE_CONSTRAINT,  t1.REFERENCED_TABLE_NAME, REFERENCED_COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE t1  INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS t2  ON t2.TABLE_SCHEMA = t1.TABLE_SCHEMA  AND t2.TABLE_NAME = t1.TABLE_NAME  AND t2.CONSTRAINT_NAME = t1.CONSTRAINT_NAME WHERE t1.TABLE_SCHEMA = '{0}' AND t1.TABLE_NAME = '{1}'  AND t2.CONSTRAINT_TYPE = 'FOREIGN KEY' ORDER BY t1.CONSTRAINT_NAME, t1.POSITION_IN_UNIQUE_CONSTRAINT", _db.DatabaseName, tableName);
        using (var dao = new MySqlSqlDao(str1, _db))
        {
            ds.Tables.Add(dao.ExecTable());
        }
        using (var dao = new MySqlSqlDao(str2, _db))
        {
            ds.Tables.Add(dao.ExecTable());
        }

        List<ForeignKeySchema> list = new List<ForeignKeySchema>();
        if ((ds.Tables.Count > 0) && (ds.Tables[0].Rows.Count > 0))
        {
            ds.Relations.Add("Contraint_to_Keys", ds.Tables[0].Columns["CONSTRAINT_NAME"], ds.Tables[1].Columns["CONSTRAINT_NAME"]);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var fk = new ForeignKeySchema();
                fk.DbDataProvider = _db.Provider;
                fk.TableName = tableName;
                fk.Table = parent;
                fk.ForeignKeyName = row["CONSTRAINT_NAME"].ToString();

                List<DataRow> list2 = new List<DataRow>(row.GetChildRows("Contraint_to_Keys"));
                fk.ReferenceTableName = list2[0]["REFERENCED_TABLE_NAME"].ToString();
                //fk.ReferenceTable = GetTable(fk.ReferenceTableName);
                foreach (DataRow row2 in list2)
                {
                    string columnName = row2["COLUMN_NAME"].ToString();
                    if (columns == null)
                    {
                        fk.Columns.Add(GetTableColumn(fk.TableName, columnName, parent));
                    }
                    else
                    {
                        var column = columns[columnName];
                        column.IsForeignKey = true;
                        fk.Columns.Add(column);
                    }
                    //fk.ReferenceColumns.Add(GetTableColumn(fk.ReferenceTableName, row2["REFERENCED_COLUMN_NAME"].ToString()));

                }
                if (fk.Columns.Count == 1)
                    fk.Columns[0].IsSingleFKColumn = true;
                list.Add(fk);
            }
        }

        return list;
    }

    /// <summary>
    /// 获取MySQL数据表的索引
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columns"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public List<IndexSchema> GetIndexes(string tableName, SchemaCollection<ColumnSchema> columns, TableSchema parent)
    {
        DataSet ds = new DataSet();
        string str1 = string.Format("SELECT INDEX_NAME, COUNT(*) AS COLUMN_COUNT, MAX(NON_UNIQUE) NON_UNIQUE, CASE INDEX_NAME WHEN 'PRIMARY' THEN 1 ELSE 0 END IS_PRIMARY FROM INFORMATION_SCHEMA.STATISTICS WHERE  TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' GROUP BY INDEX_NAME ORDER BY INDEX_NAME;", _db.DatabaseName, tableName);
        string str2 = string.Format("SELECT INDEX_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.STATISTICS WHERE  TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' ORDER BY INDEX_NAME, SEQ_IN_INDEX;", _db.DatabaseName, tableName);
        DataTable dt1, dt2;
        using (var dao = new MySqlSqlDao(str1, _db))
        {
            dt1 = dao.ExecTable();
            dt1.TableName = "TABLE1";
        }
        ds.Tables.Add(dt1);
        using (var dao = new MySqlSqlDao(str2, _db))
        {
            dt2 = dao.ExecTable();
            dt2.TableName = "TABLE2";
        }
        ds.Tables.Add(dt2);

        List<IndexSchema> ret = new List<IndexSchema>();
        if ((ds.Tables.Count > 0) && (ds.Tables[0].Rows.Count > 0))
        {
            ds.Relations.Add("INDEX_to_COLUMNS", ds.Tables[0].Columns["INDEX_NAME"], ds.Tables[1].Columns["INDEX_NAME"]);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                List<DataRow> cols = new List<DataRow>(row.GetChildRows("INDEX_to_COLUMNS"));
                var index = new IndexSchema();
                index.DbDataProvider = _db.Provider;
                index.TableName = tableName;
                index.Table = parent;
                index.IndexName = row["INDEX_NAME"].ToString();
                index.IsUnique = ((long)row["NON_UNIQUE"]) != 1;
                index.IsPrimaryKey = ((int)row["IS_PRIMARY"]) == 1;
                foreach (DataRow rowCol in cols)
                {
                    string columnName = rowCol["COLUMN_NAME"].ToString();
                    if (columns == null)
                    {
                        index.Columns.Add(GetTableColumn(tableName, columnName, parent));
                    }
                    else
                    {
                        var column = columns[columnName];
                        column.IsUniqueColumn = index.IsUnique;
                        index.Columns.Add(column);
                    }
                }
                if (index.IsUnique && index.Columns.Count == 1)
                    index.Columns[0].IsSingleUniqueColumn = true;
                ret.Add(index);
            }
        }
        return ret;
    }
    #endregion

    #endregion //TableSchema

    #region ViewSchema
    /// <summary>
    /// 获取数据库中所有视图Schema
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    public SchemaCollection<ViewSchema> GetViews(DatabaseSchema parent)
    {
        var ret = new SchemaCollection<ViewSchema>();
        var sql = string.Format("select TABLE_NAME from information_schema.VIEWS where TABLE_SCHEMA='{0}' order by table_name", _db.DatabaseName);
        List<string> views = null;
        using (var dao = new MySqlSqlDao(sql, _db))
        {
            views = dao.ExecList<string>((reader) =>
            {
                return reader.ToString(0);
            });
        }
        if (views != null)
        {
            foreach (var view in views)
            {
                var newView = GetView(view);
                if (newView == null)
                    throw new Exception("视图名不存在："+view);
                ret.Add(newView);
            }
        }
        return ret;
    }
    /// <summary>
    /// 获取试图 Schema
    /// </summary>
    /// <param name="viewName"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public ViewSchema GetView(string viewName, DatabaseSchema parent = null)
    {
        string sql = string.Format("select * from information_schema.VIEWS where TABLE_SCHEMA='{0}' and table_name='{1}'", _db.DatabaseName, viewName);
        ViewSchema ret = _db.ExecSqlSingle(sql, (reader) =>
        {
            var item = new ViewSchema();
            item.Database = parent;
            item.DatabaseName = _db.DatabaseName;
            item.DbDataProvider = _db.Provider;
            item.SourceName = viewName;
            item.Definition = reader.ToString("VIEW_DEFINITION");
            item.CollationName = reader.ToString("COLLATION_CONNECTION");
            item.CharSetName = reader.ToString("CHARACTER_SET_CLIENT");
            //item.Comment = reader.ToString("TABLE_COMMENT");
            item.Columns = GetViewColumns(viewName, item);
            if (item.Columns == null || item.Columns.Count == 0)
                throw new Exception("视图字段不可能为空。ViewName:"+viewName);
            return item;
        });
        if (ret == null) return null;
        return ret;
    }

    /// <summary>
    /// 获取试图字段Schema集合
    /// </summary>
    /// <param name="viewName"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public SchemaCollection<ColumnSchema> GetViewColumns(string viewName, ViewSchema parent)
    {
        var ret = new SchemaCollection<ColumnSchema>();
        string sql = GetViewColumnSchemaSQL(viewName);
        foreach (var item in _db.ExecSqlMulti(sql, MapToViewColumnSchema))
        {
            item.Parent = parent;
            ret.Add(item);
        }
        return ret.Count > 0 ? ret : null;
    }
    /// <summary>
    /// 获取视图字段Schema
    /// </summary>
    /// <param name="viewName"></param>
    /// <param name="columnName"></param>
    /// <param name="partent"></param>
    /// <returns></returns>
    public ColumnSchema GetViewColumn(string viewName, string columnName, ViewSchema partent)
    {
        ColumnSchema ret = null;
        string sql = GetTableColumnSchemaSQL(viewName, columnName);
        using (var dao = new MySqlSqlDao(sql, _db))
        {
            ret = dao.ExecReader().MapToSingle(MapToViewColumnSchema);
            ret.Parent = partent;
        }
        return ret;
    }
    private ColumnSchema MapToViewColumnSchema(IDataReader reader)
    {
        var ret = new MySqlColumnSchema();
        MapColumnSchema(reader, ret);
        return ret;
    }
    private string GetViewColumnSchemaSQL(string viewName = null, string columnName = null)
    {
        string ret = string.Format("select * from INFORMATION_SCHEMA.COLUMNS where TABLE_SCHEMA='{0}'", _db.DatabaseName);//数据库
        if (!string.IsNullOrEmpty(viewName))
        {
            ret += string.Format(" and TABLE_NAME='{0}'", viewName);//视图名
        }
        if (!string.IsNullOrEmpty(columnName))
        {
            ret += string.Format(" and COLUMN_NAME='{0}'", columnName);//字段名
        }
        ret += " order by TABLE_SCHEMA,TABLE_NAME,ORDINAL_POSITION";//排序
        return ret;
    }
    #endregion

    #region ProcSchema
    /// <summary>
    /// 获取MySQL数据库中所有存储过程Schema集合
    /// </summary>
    /// <returns></returns>
    public SchemaCollection<ProcSchema> GetProcs(DatabaseSchema parent)
    {
        var ret = new SchemaCollection<ProcSchema>();
        string sql = string.Format("select * from information_schema.ROUTINES where ROUTINE_SCHEMA='{0}' and (ROUTINE_TYPE = 'PROCEDURE' or ROUTINE_TYPE = 'FUNCTION')", _db.DatabaseName);
        var table = _db.ExecSqlTable(sql);
        foreach (DataRow row in table.Rows)
        {
            var newProc = MapProcSchema(row);
            newProc.Database = parent;
            ret.Add(newProc);
        }
        return ret;
    }
    /// <summary>
    /// 获取存储过程Schema
    /// </summary>
    /// <param name="procName"></param>
    /// <returns></returns>
    public ProcSchema GetProc(string procName)
    {
        string sql = string.Format("select * from information_schema.ROUTINES where ROUTINE_SCHEMA='{0}' and ROUTINE_NAME='{1}' and  ROUTINE_TYPE = 'PROCEDURE'", _db.DatabaseName, procName);
        var row = _db.ExecSqlSingle(sql);
        if (row == null) return null;
        return MapProcSchema(row);
    }
    private ProcSchema MapProcSchema(DataRow row)
    {
        var ret = new ProcSchema();
        ret.DatabaseName = row.ToString("ROUTINE_SCHEMA");
        ret.DbDataProvider = _db.Provider;
        ret.ProcName = row.ToString("ROUTINE_NAME");
        ret.Definition = row.ToString("ROUTINE_DEFINITION");
        ret.IsDeterministic = row.ToBoolean("IS_DETERMINISTIC");
        ret.CreateTime = row.ToDateTime("CREATED");
        ret.UpdateTime = row.ToDateTime("LAST_ALTERED");
        ret.CharSetName = row.ToString("CHARACTER_SET_CLIENT");
        ret.CollationName = row.ToString("COLLATION_CONNECTION");
        ret.Comment = row.ToString("ROUTINE_COMMENT");

        string sql = string.Format("SELECT * FROM information_schema.PARAMETERS WHERE ROUTINE_TYPE='PROCEDURE' AND SPECIFIC_SCHEMA='{0}' AND SPECIFIC_NAME='{1}' ORDER BY ORDINAL_POSITION", _db.DatabaseName, ret.ProcName);
        DataTable dtParams = _db.ExecSqlTable(sql);
        if (dtParams.Rows.Count > 0)
        {
            foreach (DataRow rowParam in dtParams.Rows)
            {
                MySqlParameterSchema newParam = new MySqlParameterSchema();
                newParam.DbDataProvider = _db.Provider;
                newParam.ParentName = ret.ProcName;
                newParam.Parent = ret;
                newParam.Ordinal = rowParam.ToInt32("ORDINAL_POSITION");
                switch (rowParam.ToString("PARAMETER_MODE"))
                {
                    case "IN":
                        newParam.Direction = ParameterDirection.Input;
                        break;
                    case "OUT":
                        newParam.Direction = ParameterDirection.Output;
                        break;
                    case "INOUT":
                        newParam.Direction = ParameterDirection.InputOutput;
                        break;
                }
                newParam.ParameterName = rowParam.ToString("PARAMETER_NAME");
                newParam.EngineTypeString = rowParam.ToString("DATA_TYPE");
                newParam.EngineTypeStringFull = rowParam.ToString("DTD_IDENTIFIER");
                newParam.Length = rowParam.ToInt32N("NUMERIC_PRECISION");
                newParam.Precision = rowParam.ToInt32N("NUMERIC_SCALE");
                //
                newParam.AllowDBNull = newParam.Direction == ParameterDirection.Input || newParam.Direction == ParameterDirection.InputOutput;

                ret.Parameters.Add(newParam);
            }
        }
        return ret;
    }
    #endregion 
}