using SqlSugar;

namespace SqlSugarDemo.DAL;

///<summary>
///VIEW
///</summary>
[SugarTable("v_demo_user_course")]
public partial class Sv_demo_user_courseEO
{
    public Sv_demo_user_courseEO(){

        this.TestColumn ="";

    }
    /// <summary>
    /// Desc:用户编码（自增字段）
    /// Default:
    /// Nullable:True
    /// </summary>           
    public long? UserID {get;set;}

    /// <summary>
    /// Desc:类别编码
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? ClassID {get;set;}

    /// <summary>
    /// Desc:课程编码（GUID）
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? CourseID {get;set;}

    /// <summary>
    /// Desc:名称
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? Name {get;set;}

    /// <summary>
    /// Desc:说明
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string? Note {get;set;}

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:False
    /// </summary>           
    public string TestColumn {get;set;}

}