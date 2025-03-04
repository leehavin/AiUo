using SqlSugar;

namespace SqlSugarDemo;

///<summary>
///数据迁移日志
///</summary>
[SugarTable("s_data_move_log")]
public partial class Ss_data_move_logEO
{
	public Ss_data_move_logEO(){

		this.HandleMode =0;
		this.KeepValue =0;
		this.RowNum =0;
		this.Status =0;
		this.RecDate =DateTime.Now;

	}
	/// <summary>
	/// Desc:日志编码(GUID)
	/// Default:
	/// Nullable:False
	/// </summary>           
	[SugarColumn(IsPrimaryKey=true)]
	public string LogID {get;set;}

	/// <summary>
	/// Desc:数据迁移编码
	/// Default:
	/// Nullable:False
	/// </summary>           
	public int DataMoveID {get;set;}

	/// <summary>
	/// Desc:数据库名（配置文件连接名或连接字符串）
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? DatabaseName {get;set;}

	/// <summary>
	/// Desc:表名
	/// Default:
	/// Nullable:False
	/// </summary>           
	public string TableName {get;set;}

	/// <summary>
	/// Desc:处理方式
	///              0-删除数据
	///              1-按年生成备份表：tablename_yyyy
	///              2-按季度生成备份表：tablename_yyyy1,2,3,4
	///              3-按月生成备份表：tablename_yyyyMM
	///              4-按日生成备份表：tablename_yyyyMMdd
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int HandleMode {get;set;}

	/// <summary>
	/// Desc:保留数据长度（根据HandleMode值不同）HandleMode=
	///              0-删除数据 ==> 表示保留的天数
	///              1-按年生成备份表：tablename_yyyy =》 表示保留的年数
	///              2-按季度生成备份表：tablename_yyyy1,2,3,4 ==》 表示保留的季节数
	///              3-按月生成备份表：tablename_yyyyMM ==> 表示保留的月份数
	///              4-按日生成备份表：tablename_yyyyMMdd ==> 表示保留的天数
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int KeepValue {get;set;}

	/// <summary>
	/// Desc:起始时间
	/// Default:
	/// Nullable:False
	/// </summary>           
	public DateTime BeginDate {get;set;}

	/// <summary>
	/// Desc:结束时间
	/// Default:
	/// Nullable:False
	/// </summary>           
	public DateTime EndDate {get;set;}

	/// <summary>
	/// Desc:备份表名，|分割
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? BackupTableName {get;set;}

	/// <summary>
	/// Desc:执行SQL
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? ExecSQL {get;set;}

	/// <summary>
	/// Desc:处理记录数
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int RowNum {get;set;}

	/// <summary>
	/// Desc:异常消息
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? Exception {get;set;}

	/// <summary>
	/// Desc:状态(0-未知1-成功2-无数据3-失败)
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int Status {get;set;}

	/// <summary>
	/// Desc:记录时间
	/// Default:CURRENT_TIMESTAMP
	/// Nullable:False
	/// </summary>           
	public DateTime RecDate {get;set;}

}