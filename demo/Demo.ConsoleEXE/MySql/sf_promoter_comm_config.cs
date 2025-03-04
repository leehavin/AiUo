using SqlSugar;

namespace Demo.ConsoleEXE.DAL;

///<summary>
///推广返佣比例配置
///</summary>
[SugarTable("sf_promoter_comm_config")]
public partial class Ssf_promoter_comm_configEO
{
    public Ssf_promoter_comm_configEO(){

        this.PromoterType =2;
        this.CommLevel =0;
        this.MinAmountPerf =0;
        this.MinCashPerf =0;
        this.Comm =0d;

    }
    /// <summary>
    /// Desc:运营商编码
    /// Default:
    /// Nullable:False
    /// </summary>           
    [SugarColumn(IsPrimaryKey=true)]
    public string OperatorID {get;set;}

    /// <summary>
    /// Desc:推广类型1-棋牌2-电子3-捕鱼4-真人5-彩票6-体育
    /// Default:2
    /// Nullable:False
    /// </summary>           
    [SugarColumn(IsPrimaryKey=true)]
    public int PromoterType {get;set;}

    /// <summary>
    /// Desc:返佣级别
    /// Default:0
    /// Nullable:False
    /// </summary>           
    [SugarColumn(IsPrimaryKey=true)]
    public int CommLevel {get;set;}

    /// <summary>
    /// Desc:最低业绩数量（含Bonus）
    /// Default:0
    /// Nullable:False
    /// </summary>           
    public long MinAmountPerf {get;set;}

    /// <summary>
    /// Desc:最低业绩数量（仅真金）
    /// Default:0
    /// Nullable:False
    /// </summary>           
    public long MinCashPerf {get;set;}

    /// <summary>
    /// Desc:返还佣金（每一万有效投注流水返奖值）
    /// Default:0
    /// Nullable:False
    /// </summary>           
    public double Comm {get;set;}

}