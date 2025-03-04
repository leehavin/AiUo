using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace AiUo.BIZ.DataSplit.DAL
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("s_split_demo")]
    public partial class S_split_demoPO
    {
           public S_split_demoPO(){

            this.NumDay =0;
            this.NumWeek =0;
            this.NumMonth =0;
            this.NumQuarter =0;
            this.NumYear =0;
            this.OrderNum =0;
            this.RecDate =DateTime.Now;

           }
           /// <summary>
           /// Desc:ObjectID
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true)]
           public string ObjectID {get;set;}

           /// <summary>
           /// Desc:数值天
           /// Default:0
           /// Nullable:False
           /// </summary>           
           public int NumDay {get;set;}

           /// <summary>
           /// Desc:数值周
           /// Default:0
           /// Nullable:False
           /// </summary>           
           public int NumWeek {get;set;}

           /// <summary>
           /// Desc:数值月
           /// Default:0
           /// Nullable:False
           /// </summary>           
           public int NumMonth {get;set;}

           /// <summary>
           /// Desc:数值季
           /// Default:0
           /// Nullable:False
           /// </summary>           
           public int NumQuarter {get;set;}

           /// <summary>
           /// Desc:数值年
           /// Default:0
           /// Nullable:False
           /// </summary>           
           public int NumYear {get;set;}

           /// <summary>
           /// Desc:顺序号
           /// Default:0
           /// Nullable:False
           /// </summary>           
           public int OrderNum {get;set;}

           /// <summary>
           /// Desc:记录时间
           /// Default:CURRENT_TIMESTAMP
           /// Nullable:False
           /// </summary>           
           public DateTime RecDate {get;set;}

    }
}
