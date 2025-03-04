using SqlSugar;

namespace Demo.WebAPI;

///<summary>
///
///</summary>
[SugarTable("s_operator_app")]
public partial class Ss_operator_appEO
{
	public Ss_operator_appEO(){

		this.UseBonus =false;
		this.CashLimit =0;
		this.PayLimit =0;
		this.Status =0;
		this.EffectStatus =0;

	}
	/// <summary>
	/// Desc:应用编码
	/// Default:
	/// Nullable:False
	/// </summary>           
	[SugarColumn(IsPrimaryKey=true)]
	public string AppID {get;set;}

	/// <summary>
	/// Desc:运营商编码
	/// Default:
	/// Nullable:False
	/// </summary>           
	[SugarColumn(IsPrimaryKey=true)]
	public string OperatorID {get;set;}

	/// <summary>
	/// Desc:对应的运营商应用编码
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? OperatorAppId {get;set;}

	/// <summary>
	/// Desc:是否使用bonus
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public bool UseBonus {get;set;}

	/// <summary>
	/// Desc:进入游戏cash最小数量限制0-不限制
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int CashLimit {get;set;}

	/// <summary>
	/// Desc:进入游戏充值最小数量限制0-不限制
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int PayLimit {get;set;}

	/// <summary>
	/// Desc:支持的活动ID集合，|分割，如: 100001|100002
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? ActivityIds {get;set;}

	/// <summary>
	/// Desc:应用客户端线上URL
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? AppUrl {get;set;}

	/// <summary>
	/// Desc:应用客户端仿真URL
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? AppUrlStaging {get;set;}

	/// <summary>
	/// Desc:应用客户端联调URL
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? AppUrlDebug {get;set;}

	/// <summary>
	/// Desc:状态0-无效1-在线2-维护中3-测试中
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int Status {get;set;}

	/// <summary>
	/// Desc:游戏入口效果标识Flag值 
	/// 0-无 
	/// 1-Hot
	/// 2-New
	/// 4-UseBonus
	/// 8-闪光效果
	/// 可用值0、1、2、4、8、9、10、12，其他值为非法值
	/// Default:0
	/// Nullable:False
	/// </summary>           
	public int EffectStatus {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? OperatorThumb {get;set;}

	/// <summary>
	/// Desc:游戏背景图url
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? OperatorBg {get;set;}

	/// <summary>
	/// Desc:游戏回合url
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? OperatorRoundUrl {get;set;}

}