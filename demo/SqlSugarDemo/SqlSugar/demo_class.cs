using SqlSugar;

namespace SqlSugarDemo.DAL;

///<summary>
///类别
/// 这里有很多说明
///</summary>
[SugarTable("demo_class")]
public partial class Sdemo_classEO
{
    public Sdemo_classEO(){


    }
    /// <summary>
    /// Desc:类别编码
    /// Default:
    /// Nullable:False
    /// </summary>           
    [SugarColumn(IsPrimaryKey=true)]
    public string ClassID {get;set;}

    /// <summary>
    /// Desc:类别
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? Name {get;set;}

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:False
    /// </summary>           
    public int Sort1 {get;set;}

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:False
    /// </summary>           
    public int Sort2 {get;set;}

}