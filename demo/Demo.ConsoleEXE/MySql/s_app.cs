using SqlSugar;

namespace Demo.ConsoleEXE.DAL;

///<summary>
///应用列表
///</summary>
[SugarTable("s_app")]
public partial class Ss_appEO
{
    public Ss_appEO(){

        this.AppType =0;
        this.PromoterType =2;
        this.ServerType =0;
        this.FlowRatio =1f;
        this.Status =0;
        this.HasJackpot =false;

    }
    /// <summary>
    /// Desc:App编码
    /// Default:
    /// Nullable:False
    /// </summary>           
    [SugarColumn(IsPrimaryKey=true)]
    public string AppID {get;set;}

    /// <summary>
    /// Desc:应用提供商编码
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ProviderID {get;set;}

    /// <summary>
    /// Desc:应用提供商的应用编码
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ProviderAppId {get;set;}

    /// <summary>
    /// Desc:App名称
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? AppName {get;set;}

    /// <summary>
    /// Desc:App类型1-大厅2-游戏3-应用
    /// Default:0
    /// Nullable:False
    /// </summary>           
    public int AppType {get;set;}

    /// <summary>
    /// Desc:推广类型1-棋牌2-电子3-捕鱼4-真人5-彩票6-体育
    /// Default:2
    /// Nullable:False
    /// </summary>           
    public int PromoterType {get;set;}

    /// <summary>
    /// Desc:app服务端类型0-未知1-api2-websocket
    /// Default:0
    /// Nullable:False
    /// </summary>           
    public int ServerType {get;set;}

    /// <summary>
    /// Desc:应用服务端线上URL（仅api game需要配置）
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ServerUrl {get;set;}

    /// <summary>
    /// Desc:应用服务端线上URL2（仅api game需要配置）
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ServerUrl2 {get;set;}

    /// <summary>
    /// Desc:应用服务端线上URL3（仅api game需要配置）
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ServerUrl3 {get;set;}

    /// <summary>
    /// Desc:应用服务端仿真URL
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ServerUrlStaging {get;set;}

    /// <summary>
    /// Desc:应用服务端联调URL
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ServerUrlDebug {get;set;}

    /// <summary>
    /// Desc:流水bet计算比率
    /// Default:1
    /// Nullable:False
    /// </summary>           
    public float FlowRatio {get;set;}

    /// <summary>
    /// Desc:App支持的语言。|分割
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? Langs {get;set;}

    /// <summary>
    /// Desc:备注信息
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? Note {get;set;}

    /// <summary>
    /// Desc:状态(0-无效1-有效)
    /// Default:0
    /// Nullable:False
    /// </summary>           
    public int Status {get;set;}

    /// <summary>
    /// Desc:是否有Jackpot
    /// Default:0
    /// Nullable:False
    /// </summary>           
    public bool HasJackpot {get;set;}

}