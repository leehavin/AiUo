namespace AiUo.ChineseCalendar;

/// <summary>
/// 假期基类
/// </summary>
public class HolidayBase
{
    /// <summary>
    /// 假期定义的类别
    /// </summary>
    public HolidaySort Sort { get; protected set; }

    /// <summary>
    /// 已定义假期类型
    /// </summary>
    public KnownHoliday Type { get; set; }

    /// <summary>
    /// 假期名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 是否法定休息日
    /// </summary>
    public bool IsRecess { get; set; }
}