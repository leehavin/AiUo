using SqlSugar;

namespace GrpcDemo1.DAL;

///<summary>
///mq订阅异常日志
///</summary>
[SugarTable("tfx_mq_sub_error")]
public partial class Tfx_mq_sub_errorPO
{
    public Tfx_mq_sub_errorPO(){

        this.MessageMode =0;
        this.RepublishCount =0;
        this.Status =0;
        this.RecDate =DateTime.Now;

    }
    /// <summary>
    /// Desc:MQ异常日志编码
    /// Default:
    /// Nullable:False
    /// </summary>           
    [SugarColumn(IsPrimaryKey=true)]
    public string MQLogID {get;set;}

    /// <summary>
    /// Desc:项目名称
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ProjectId {get;set;}

    /// <summary>
    /// Desc:消息类型
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? MessageType {get;set;}

    /// <summary>
    /// Desc:消息模式(0-无 1-IMQMessage 2- MQMeta属性)
    /// Default:0
    /// Nullable:False
    /// </summary>           
    public int MessageMode {get;set;}

    /// <summary>
    /// Desc:消费类类型
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ConsumerType {get;set;}

    /// <summary>
    /// Desc:消费模式
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? SubscribeMode {get;set;}

    /// <summary>
    /// Desc:消息ID
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? MessageId {get;set;}

    /// <summary>
    /// Desc:消息UTC时间
    /// Default:
    /// Nullable:True
    /// </summary>           
    public DateTime? MessageTime {get;set;}

    /// <summary>
    /// Desc:消息topic集合|分割
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? MessageTopic {get;set;}

    /// <summary>
    /// Desc:消息数据（包含异常Action）
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? MessageData {get;set;}

    /// <summary>
    /// Desc:消息Meta
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? MessageMeta {get;set;}

    /// <summary>
    /// Desc:异常Handler列表|分割
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ErrorHandlers {get;set;}

    /// <summary>
    /// Desc:异常消息
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? Exception {get;set;}

    /// <summary>
    /// Desc:重新发送次数
    /// Default:0
    /// Nullable:False
    /// </summary>           
    public int RepublishCount {get;set;}

    /// <summary>
    /// Desc:MQ链接名
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? MQConnName {get;set;}

    /// <summary>
    /// Desc:消息状态（0-初始异常1-已重新发送2-关闭）
    /// Default:0
    /// Nullable:False
    /// </summary>           
    public int Status {get;set;}

    /// <summary>
    /// Desc:记录UTC时间
    /// Default:CURRENT_TIMESTAMP
    /// Nullable:False
    /// </summary>           
    public DateTime RecDate {get;set;}

    /// <summary>
    /// Desc:重新发布时间
    /// Default:
    /// Nullable:True
    /// </summary>           
    public DateTime? RepublishDate {get;set;}

}