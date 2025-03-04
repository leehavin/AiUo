namespace AiUo.ChineseCalendar;

/// <summary>
/// 按阳历计算的假期
/// </summary>
public class SolarHoliday : HolidayBase
{
    /// <summary>
    /// 阳历月
    /// </summary>
    public int Month { get; set; }
    /// <summary>
    /// 阳历日
    /// </summary>
    public int Day { get; set; }
    /// <summary>
    /// 构造函数
    /// </summary>
    public SolarHoliday()
    {
        Sort = HolidaySort.Solar;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="month">阳历月</param>
    /// <param name="day">阳历日</param>
    /// <param name="recess">是否法定假期</param>
    /// <param name="name">节日名称</param>
    /// <param name="type">节日类型</param>
    public SolarHoliday(int month, int day, bool recess, string name, KnownHoliday type = KnownHoliday.Undefined)
    {
        Sort = HolidaySort.Solar;
        Type = KnownHoliday.Undefined;
        Month = month;
        Day = day;
        IsRecess = recess;
        Name = name;
        Type = type;
    }
}