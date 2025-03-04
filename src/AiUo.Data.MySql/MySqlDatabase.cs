using System;
using AiUo.Data.Instrumentation;
using System.Data.Common;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace AiUo.Data.MySql;

/// <summary>
/// 表示一个MySQL数据库操作对象，需要数据提供程序MySQL Connector Net（mysql.data.dll），参数以@开头
/// </summary>
public class MySqlDatabase : Database<MySqlParameter, MySqlDbType>
{
    #region Constructors
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="config">配置信息</param>
    public MySqlDatabase(ConnectionStringConfig config)
        : base(config) { }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="connectionStringName">配置文件中的 connectionStringName， 如果长度超过50则默认理解为connectionString</param>
    public MySqlDatabase(string connectionStringName = null)
        : base(connectionStringName) { }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="connectionString">数据库连接字符串</param>
    /// <param name="commandTimeout">超时时间，单位秒</param>
    /// <param name="inst">数据操作执行事件监测对象</param>
    public MySqlDatabase(string connectionString, int commandTimeout, IDataInstProvider inst = null)
        : base(connectionString, commandTimeout, inst) { }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="connectionString">数据库连接字符串</param>
    /// <param name="readConnectionString">只读数据库连接字符串</param>
    /// <param name="commandTimeout">超时时间，单位秒</param>
    /// <param name="inst">数据操作执行事件监测对象</param>
    public MySqlDatabase(string connectionString, string readConnectionString, int commandTimeout, IDataInstProvider inst = null)
        : base(connectionString, readConnectionString, commandTimeout, inst) { }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="server">数据库服务器</param>
    /// <param name="database">数据库</param>
    /// <param name="userid">账户</param>
    /// <param name="password">密码</param>
    /// <param name="commandTimeout">超时时间，单位秒</param>
    /// <param name="inst">数据操作执行事件监测对象</param>
    public MySqlDatabase(string server, string database, string userid, string password, int commandTimeout = 30, IDataInstProvider inst = null)
        : base(ConnectionStringUtil.GetMySql(server, database, userid, password), commandTimeout, inst) { }
    #endregion

    #region Overrides
    /// <summary>
    /// 获得符合数据库提供者的参数名称
    /// </summary>
    /// <param name="parameterName">参数名称</param>
    /// <returns></returns>
    public override string GetParameterName(string parameterName)
        => (parameterName[0] != ParameterToken) ? ParameterToken + parameterName : parameterName;
    /// <summary>
    /// 参数前缀
    /// </summary>
    protected override char ParameterToken => '@';
    /// <summary>
    /// 支持异步
    /// </summary>
    public override bool SupportsAsync => false;
    /// <summary>
    /// MySqlClientFactory.Instance
    /// </summary>
    public override DbProviderFactory Factory => MySqlClientFactory.Instance;
    /// <summary>
    /// DbDataProvider.MySqlClient
    /// </summary>
    public override DbDataProvider Provider => DbDataProvider.MySqlClient;
    /// <summary>
    /// 设置MySqlParameter的MySqlDbType
    /// </summary>
    /// <param name="para"></param>
    /// <param name="dbType"></param>
    protected override void SetParameterDbType(MySqlParameter para, MySqlDbType dbType)
        => para.MySqlDbType = dbType;
    /// <summary>
    /// 获得数据库连接字符串信息
    /// </summary>
    /// <returns></returns>
    protected override ConnectionStringInfo GetConnectionStringInfo()
        => GetConnectionStringInfo(ConnectionString);

    /// <summary>
    /// 获取连接字符串信息
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    public static ConnectionStringInfo GetConnectionStringInfo(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException("connectionString", "connectionString参数不能为空。");
        var csb = new MySqlConnectionStringBuilder(connectionString);
        ConnectionStringInfo ret = new ConnectionStringInfo
        {
            Provider = DbDataProvider.MySqlClient,
            ConnectionString = connectionString,
            DataSource = csb.Server,
            Database = csb.Database,
            UserID = csb.UserID,
            Password = csb.Password,
            ConnectTimeout = (int)csb.ConnectionTimeout
        };
        return ret;
    }
    /// <summary>
    /// 克隆参数
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public override IDataParameter CloneParameter(IDataParameter parameter)
    {
        var input = parameter as MySqlParameter;
        var ret = new MySqlParameter()
        {
            ParameterName = input.ParameterName,
            MySqlDbType = input.MySqlDbType,
            Direction = input.Direction,
            DbType = input.DbType,
            IsNullable = input.IsNullable,
            Size = input.Size,
            Precision = input.Precision,
            Scale = input.Scale,
            SourceColumn = input.SourceColumn,
            Value = input.Value,
        };
        return ret;
    }
    /// <summary>
    /// 解析参数
    /// </summary>
    /// <param name="command"></param>
    protected override void DeriveParameters(CommandWrapper command)
    {
        switch (command.CommandType)
        {
            case CommandType.Text:
                foreach (var name in ParseSqlParameterNames(command.CommandText))
                {
                    if (!command.Parameters.Contains(name))
                        command.Parameters.Add(CreateParameter(name));
                }
                break;
            case CommandType.StoredProcedure:
                // 创建一个新的连接获取Parameters，不能使用原来的Command对象。
                using (CommandWrapper cmd = CreateCommand(command.CommandText, command.CommandType, null))
                {
                    OpenConnection(cmd.Connection);
                    MySqlCommandBuilder.DeriveParameters((MySqlCommand)cmd.Command);
                    foreach (var param in cmd.Command.Parameters)
                        command.Parameters.Add(param);
                }
                break;
        }
    }
    protected override void SetParameterDbType(DbParameter para, object value)
    {
        var p = (MySqlParameter)para;
        if (p.DbType != DbType.Decimal || p.MySqlDbType != MySqlDbType.Decimal)
            return;
        p.MySqlDbType = MapDbTypeFromType(value.GetType());
    }
    /// <summary>
    /// C# type => MySqlDbType
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public MySqlDbType MapDbTypeFromType(Type type)
    {
        var ret = default(MySqlDbType);
        switch (type.FullName)
        {
            case SimpleTypeNames.Int64:
                ret = MySqlDbType.Int64;
                break;
            case SimpleTypeNames.UInt64:
                ret = MySqlDbType.UInt64;
                break;
            case SimpleTypeNames.Int32:
                ret = MySqlDbType.Int32;
                break;
            case SimpleTypeNames.UInt32:
                ret = MySqlDbType.UInt32;
                break;
            case SimpleTypeNames.Int16:
                ret = MySqlDbType.Int16;
                break;
            case SimpleTypeNames.UInt16:
                ret = MySqlDbType.UInt16;
                break;
            case SimpleTypeNames.Guid:
                ret = MySqlDbType.Guid;
                break;
            case SimpleTypeNames.DateTime:
                ret = MySqlDbType.DateTime;
                break;
            case SimpleTypeNames.Single:
                ret = MySqlDbType.Float;
                break;
            case SimpleTypeNames.Double:
                ret = MySqlDbType.Double;
                break;
            case SimpleTypeNames.Decimal:
                ret = MySqlDbType.Decimal;
                break;
            case SimpleTypeNames.Boolean:
                ret = MySqlDbType.Bit;
                break;
            case SimpleTypeNames.Byte:
                ret = MySqlDbType.Byte;
                break;
            case SimpleTypeNames.Bytes:
                ret = MySqlDbType.Blob;
                break;
            case SimpleTypeNames.String:
                ret = MySqlDbType.VarString;
                break;
        }
        return ret;
    }
    #endregion

    #region Database Objects
    /// <summary>
    /// 获得Dao
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public override IDao GetSqlDao(string sql)
        => new MySqlSqlDao(sql, this);
    /// <summary>
    /// 获得Dao
    /// </summary>
    /// <param name="proc"></param>
    /// <returns></returns>
    public override IDao GetProcDao(string proc)
        => new MySqlProcDao(proc, this);
    /// <summary>
    /// 获得IDataPager分页对象
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="pageSize"></param>
    /// <param name="keyField"></param>
    /// <returns></returns>
    public override IDataPager GetPager(string sql, int pageSize, string keyField = null)
        => new MySqlDataPager(sql, pageSize, this);

    #endregion

    #region 特定数据库方法
    /// <summary>
    /// 判断数据库表是否存在
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public bool ExistTable(string tableName)
    {
        var sql = $"select t.table_name from information_schema.TABLES t where t.TABLE_SCHEMA ='{DatabaseName}' and t.TABLE_NAME ='{tableName}'";
        var name = ExecSqlScalar(sql);
        return name != null;
    }
    public async Task<bool> ExistTableAsync(string tableName)
    {
        var sql = $"select t.table_name from information_schema.TABLES t where t.TABLE_SCHEMA ='{DatabaseName}' and t.TABLE_NAME ='{tableName}'";
        var name = await ExecSqlScalarAsync(sql);
        return name != null;
    }
    /// <summary>
    /// 获得MySQL环境变量
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="variableName">环境变量名称</param>
    /// <returns></returns>
    public T GetVariable<T>(MySqlVariableNames variableName)
    {
        return this.ExecSqlScalarFormat<T>("SHOW VARIABLES where Variable_name = '{0}'"
            , variableName.ToString().ToLower());
    }
    public async Task<T> GetVariableAsync<T>(MySqlVariableNames variableName)
    {
        return await this.ExecSqlScalarFormatAsync<T>("SHOW VARIABLES where Variable_name = '{0}'"
            , variableName.ToString().ToLower());
    }
    /// <summary>
    /// 获得MySQL环境变量
    /// </summary>
    /// <returns></returns>
    public DataTable GetVariables()
        => this.ExecSqlTable("SHOW VARIABLES");
    public Task<DataTable> GetVariablesAsync()
        => this.ExecSqlTableAsync("SHOW VARIABLES");

    /// <summary>
    /// 获取数据库死锁信息
    /// </summary>
    /// <returns></returns>
    public async Task<DeadLockInfo> GetDeadLockInfoAsync()
    {
        string status = (await this.ExecSqlSingleAsync("show engine innodb status"))["status"].ToString();
        if (string.IsNullOrEmpty(status)) return null;
        var ret = new DeadLockInfo();
        ret.Status = status;
        string sign = "LATEST DETECTED DEADLOCK\n------------------------\n";
        var index = status.IndexOf(sign) + sign.Length;
        status = status.Substring(index);
        ret.DeadLockDate = status.Substring(0, 19).ToDateTime();

        var lines = status.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        ret.TransactionA = lines[7];
        ret.AWaitingFor = lines[9];

        sign = "*** (2) TRANSACTION:\n";
        index = status.IndexOf(sign) + sign.Length;
        status = status.Substring(index);
        lines = status.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        ret.TransactionB = lines[4];
        ret.BHoldsLock = lines[6];

        sign = "*** (2) WAITING FOR THIS LOCK TO BE GRANTED:\n";
        index = status.IndexOf(sign) + sign.Length;
        status = status.Substring(index);
        lines = status.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        ret.BWaitingFor = lines[0];
        return ret;
    }

    #endregion
}