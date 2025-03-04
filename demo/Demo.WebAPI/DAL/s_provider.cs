using SqlSugar;

namespace Demo.WebAPI;

///<summary>
///应用提供商
///</summary>
[SugarTable("s_provider")]
public partial class Ss_providerEO
{
    public Ss_providerEO(){

        this.ProviderType =1;
        this.UseBonus =false;
        this.SettlDayNum =0;
        this.Status =0;
        this.RecDate =DateTime.Now;

    }
    /// <summary>
    /// Desc:应用提供商编码（小写，唯一）自有own
    /// Default:
    /// Nullable:False
    /// </summary>           
    [SugarColumn(IsPrimaryKey=true)]
    public string ProviderID {get;set;}

    /// <summary>
    /// Desc:应用提供商名称
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ProviderName {get;set;}

    /// <summary>
    /// Desc:应用提供商类型（0-自有 1-第三方）
    /// Default:1
    /// Nullable:False
    /// </summary>           
    public int ProviderType {get;set;}

    /// <summary>
    /// Desc:是否使用bonus
    /// Default:0
    /// Nullable:False
    /// </summary>           
    public bool UseBonus {get;set;}

    /// <summary>
    /// Desc:获取应用客户端URL的API路径（应用提供商提供）
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ApiUrl {get;set;}

    /// <summary>
    /// Desc:获取应用客户端URL的API路径（仿真）
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ApiUrlStaging {get;set;}

    /// <summary>
    /// Desc:获取应用客户端URL的API路径（联调）
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ApiUrlDebug {get;set;}

    /// <summary>
    /// Desc:应用提供商公钥
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ProvPublicKey {get;set;}

    /// <summary>
    /// Desc:应用提供商私钥（仅测试）
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ProvPrivateKey {get;set;}

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
    /// Desc:应用提供商的配置信息
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ProviderConfig {get;set;}

    /// <summary>
    /// Desc:手机号
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? Mobile {get;set;}

    /// <summary>
    /// Desc:邮箱
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? Email {get;set;}

    /// <summary>
    /// Desc:备注信息
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? Note {get;set;}

    /// <summary>
    /// Desc:结算日，如5表示每月结算包含5号
    /// Default:0
    /// Nullable:False
    /// </summary>           
    public int SettlDayNum {get;set;}

    /// <summary>
    /// Desc:状态(0-无效1-有效)
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
    /// Desc:我方提供的后台账号
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? AdminUser {get;set;}

    /// <summary>
    /// Desc:我方提供的后台密码
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? AdminPwd {get;set;}

    /// <summary>
    /// Desc:应用提供商后台地址
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ProvAdminUrl {get;set;}

    /// <summary>
    /// Desc:应用提供商后台账号
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ProvAdminUser {get;set;}

    /// <summary>
    /// Desc:应用提供商后台密码
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ProvAdminPwd {get;set;}

}