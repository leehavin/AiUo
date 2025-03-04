using SqlSugar;

namespace Demo.ConsoleEXE.DAL;

///<summary>
///VIEW
///</summary>
[SugarTable("v_s_operator")]
public partial class Sv_s_operatorEO
{
	public Sv_s_operatorEO(){

		this.OperatorType =1;
		this.AllowVisitor =false;
		this.SettlDayNum =0;
		this.EnableBonus =true;
		this.ChangeBalanceMode =1;
		this.FlowMode =1;
		this.FirstPayFlowMultip =1;
		this.PayFlowMultip =1;
		this.FlowBalance =0;
		this.FlowDevideBalance =0;
		this.Status =0;
		this.CashAudit =0;

	}
	/// <summary>
	/// Desc:运营商编码（小写，唯一）
	/// Default:
	/// Nullable:False
	/// </summary>           
	public string OperatorID {get;set;}

	/// <summary>
	/// Desc:运营商基础编码（小写，唯一）
	/// Default:
	/// Nullable:False
	/// </summary>           
	public string OperatorBaseID {get;set;}

	/// <summary>
	/// Desc:运营商名称
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? OperatorName {get;set;}

	/// <summary>
	/// Desc:运营商类型
	///              0-自有
	///              1-第三方有用户有充值
	///              2-第三方有用户无充值
	///              3-第三方无用户无充值
	/// Default:1
	/// Nullable:False
	/// </summary>           
	public int OperatorType {get;set;}

	/// <summary>
	/// Desc:独服混服类型(0-混服1-独服)
	/// Default:
	/// Nullable:True
	/// </summary>           
	public int? OperatorMode {get;set;}

	/// <summary>
	/// Desc:是否允许游客
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public bool AllowVisitor {get;set;}

	/// <summary>
	/// Desc:运营商公钥
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? OperPublicKey {get;set;}

	/// <summary>
	/// Desc:运营商私钥（仅测试）
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? OperPrivateKey {get;set;}

	/// <summary>
	/// Desc:我方的公钥
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? OwnPublicKey {get;set;}

	/// <summary>
	/// Desc:我方的私钥
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? OwnPrivateKey {get;set;}

	/// <summary>
	/// Desc:运营商的配置信息
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? OperatorConfig {get;set;}

	/// <summary>
	/// Desc:结算日，如5表示每月结算包含5号
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int SettlDayNum {get;set;}

	/// <summary>
	/// Desc:国家编码ISO 3166-1三位字母
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? CountryID {get;set;}

	/// <summary>
	/// Desc:平台支持的语言。|分割
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? Langs {get;set;}

	/// <summary>
	/// Desc:货币类型（货币缩写USD）
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? CurrencyID {get;set;}

	/// <summary>
	/// Desc:对应的主域名如: 777gana.com
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? MapDomain {get;set;}

	/// <summary>
	/// Desc:大厅客户端线上URL
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? LobbyUrl {get;set;}

	/// <summary>
	/// Desc:大厅客户端仿真URL
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? LobbyUrlStaging {get;set;}

	/// <summary>
	/// Desc:大厅客户端调试URL
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? LobbyUrlDebug {get;set;}

	/// <summary>
	/// Desc:银行客户端线上URL
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? BankUrl {get;set;}

	/// <summary>
	/// Desc:银行客户端仿真URL
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? BankUrlStaging {get;set;}

	/// <summary>
	/// Desc:银行客户端调试URL
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? BankUrlDebug {get;set;}

	/// <summary>
	/// Desc:branch关键字
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? BranchKey {get;set;}

	/// <summary>
	/// Desc:GA关键字
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? GAKey {get;set;}

	/// <summary>
	/// Desc:是否启用bonus（用户是否有bonus账户）
	/// Default:1
	/// Nullable:False
	/// </summary>           
	public bool EnableBonus {get;set;}

	/// <summary>
	/// Desc:扣减balance模式1-优先bonus2-优先真金
	/// Default:1
	/// Nullable:False
	/// </summary>           
	public int ChangeBalanceMode {get;set;}

	/// <summary>
	/// Desc:流水计算方式（不同的执行类）1-仅流水+充值2-
	/// Default:1
	/// Nullable:False
	/// </summary>           
	public int FlowMode {get;set;}

	/// <summary>
	/// Desc:首充流水倍数，如20倍
	/// Default:1
	/// Nullable:False
	/// </summary>           
	public int FirstPayFlowMultip {get;set;}

	/// <summary>
	/// Desc:充值流水倍数，如20倍
	/// Default:1
	/// Nullable:False
	/// </summary>           
	public int PayFlowMultip {get;set;}

	/// <summary>
	/// Desc:用户余额大于此值则流水需继承
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public long FlowBalance {get;set;}

	/// <summary>
	/// Desc:流水除以用户余额的倍数
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int FlowDevideBalance {get;set;}

	/// <summary>
	/// Desc:状态(0-无效1-有效)
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int Status {get;set;}

	/// <summary>
	/// Desc:
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int CashAudit {get;set;}

}