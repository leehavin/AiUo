using SqlSugar;

namespace GrpcDemo1.DAL;

///<summary>
///分表执行日志
///</summary>
[SugarTable("tfx_split_table_log")]
public partial class Tfx_split_table_logPO
{
	public Tfx_split_table_logPO(){

		this.HandleMode =0;
		this.ColumnType =0;
		this.MoveKeepMode =0;
		this.MoveKeepValue =0;
		this.MoveTableMode =0;
		this.MaxRowCount =0;
		this.MaxRowInterval =1;
		this.HandleOrder =0;
		this.DbTimeout =0;
		this.BathPageSize =0;
		this.Status =0;
		this.RecDate =DateTime.Now;
		this.RowNum =0;
		this.HandleSeconds =0;

	}
	/// <summary>
	/// Desc:日志编码(GUID)
	/// Default:
	/// Nullable:False
	/// </summary>           
	[SugarColumn(IsPrimaryKey=true)]
	public string LogID {get;set;}

	/// <summary>
	/// Desc:数据库标识
	/// Default:
	/// Nullable:False
	/// </summary>           
	public string DatabaseId {get;set;}

	/// <summary>
	/// Desc:分表表名
	/// Default:
	/// Nullable:False
	/// </summary>           
	public string TableName {get;set;}

	/// <summary>
	/// Desc:处理模式
	///              0-无
	///              1-迁移-删除 ==> MoveMode + MoveKeepValue
	///              2-迁移-备份 ==> MoveMode + MoveKeepValue
	///              3-分表-按最大行数 ==> SplitMaxRowCount + SplitMaxRowHours
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int HandleMode {get;set;}

	/// <summary>
	/// Desc:分表字段类型(0-未知1-DateTime(UTC)2-ObjectId3-数值天4-周5-月6-季7-年)
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int ColumnType {get;set;}

	/// <summary>
	/// Desc:分表字段名
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? ColumnName {get;set;}

	/// <summary>
	/// Desc:迁移模式(0-无1-天2-周3-月4-季5-年)
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int MoveKeepMode {get;set;}

	/// <summary>
	/// Desc:迁移保留模式的值
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int MoveKeepValue {get;set;}

	/// <summary>
	/// Desc:迁移表名称模式(0-无1-天2-周3-月4-季5-年)
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int MoveTableMode {get;set;}

	/// <summary>
	/// Desc:迁移数据的条件
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? MoveWhere {get;set;}

	/// <summary>
	/// Desc:分表最大记录数
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int MaxRowCount {get;set;}

	/// <summary>
	/// Desc:分表最大记录数时下一个表的间隔
	///              DateTime和ObjectId为小时数，其他为下一个周期
	/// Default:1
	/// Nullable:False
	/// </summary>           
	public int MaxRowInterval {get;set;}

	/// <summary>
	/// Desc:处理顺序(小到大)
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int HandleOrder {get;set;}

	/// <summary>
	/// Desc:数据库超时（秒）
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int DbTimeout {get;set;}

	/// <summary>
	/// Desc:批处理分页行数
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int BathPageSize {get;set;}

	/// <summary>
	/// Desc:执行基础UTC时间
	/// Default:
	/// Nullable:False
	/// </summary>           
	public DateTime ExecTime {get;set;}

	/// <summary>
	/// Desc:状态(0-进行中1-成功2-失败)
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

	/// <summary>
	/// Desc:起始值
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? BeginValue {get;set;}

	/// <summary>
	/// Desc:起始日期
	/// Default:
	/// Nullable:True
	/// </summary>           
	public DateTime? BeginDate {get;set;}

	/// <summary>
	/// Desc:结束值
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? EndValue {get;set;}

	/// <summary>
	/// Desc:结束日期
	/// Default:
	/// Nullable:True
	/// </summary>           
	public DateTime? EndDate {get;set;}

	/// <summary>
	/// Desc:处理记录数
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int RowNum {get;set;}

	/// <summary>
	/// Desc:执行时长（秒）
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int HandleSeconds {get;set;}

	/// <summary>
	/// Desc:执行日志
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? HandleLog {get;set;}

	/// <summary>
	/// Desc:异常消息
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? Exception {get;set;}

	/// <summary>
	/// Desc:分表后的表名,|分割
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? HandleTables {get;set;}

}