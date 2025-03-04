namespace AiUo.ChineseCalendar;

/// <summary>
/// 按阴历计算的假期
/// </summary>
public class LunarHoliday : HolidayBase
{
    /// <summary>
    /// 阴历月
    /// </summary>
    public int Month { get; set; }
    /// <summary>
    /// 阴历日
    /// </summary>
    public int Day { get; set; }
    /// <summary>
    /// 构造函数
    /// </summary>
    public LunarHoliday()
    {
        Sort = HolidaySort.Lunar;
    }
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="month">阴历月</param>
    /// <param name="day">阴历日</param>
    /// <param name="recess">是否法定节假日</param>
    /// <param name="name">节日名称</param>
    /// <param name="type">节日类型</param>
    public LunarHoliday(int month, int day, bool recess, string name, KnownHoliday type = KnownHoliday.Undefined)
    {
        Sort = HolidaySort.Lunar;
        Type = KnownHoliday.Undefined;
        Month = month;
        Day = day;
        IsRecess = recess;
        Name = name;
        Type = type;
    }
}