using System;

namespace AiUo.ChineseCalendar;

/// <summary>
/// 按某月第几个星期的星期几计算的假期
/// </summary>
public class WeekHoliday : HolidayBase
{
    /// <summary>
    /// 某一个月
    /// </summary>
    public int Month { get; set; }
    /// <summary>
    /// 本月第几周
    /// </summary>
    public int WeekAtMonth { get; set; }
    /// <summary>
    /// 本周星期几
    /// </summary>
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    public WeekHoliday()
    {
        Sort = HolidaySort.Week;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="month">某一个月</param>
    /// <param name="week">本月第几周</param>
    /// <param name="day">本周星期几</param>
    /// <param name="recess">是否法定节假日</param>
    /// <param name="name">节日名称</param>
    /// <param name="type">节日类型</param>
    public WeekHoliday(int month, int week, int day, bool recess, string name, KnownHoliday type = KnownHoliday.Undefined)
    {
        Sort = HolidaySort.Week;
        Type = KnownHoliday.Undefined;
        Month = month;
        WeekAtMonth = week;
        DayOfWeek = (DayOfWeek)day;
        IsRecess = recess;
        Name = name;
        Type = type;
    }
}