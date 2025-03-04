using SqlSugar;

namespace SqlSugarDemo.DAL;

///<summary>
///课程
///</summary>
[SugarTable("demo_course")]
public partial class Sdemo_courseEO
{
    public Sdemo_courseEO(){


    }
    /// <summary>
    /// Desc:学年
    /// Default:
    /// Nullable:False
    /// </summary>           
    [SugarColumn(IsPrimaryKey=true)]
    public int Year {get;set;}

    /// <summary>
    /// Desc:课程编码（GUID）
    /// Default:
    /// Nullable:False
    /// </summary>           
    [SugarColumn(IsPrimaryKey=true)]
    public string CourseID {get;set;}

    /// <summary>
    /// Desc:名称
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? Name {get;set;}

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:False
    /// </summary>           
    public int OrderNum {get;set;}

}