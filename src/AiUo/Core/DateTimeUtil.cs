using System;
using System.Globalization;
using AiUo.ChineseCalendar;

namespace AiUo;

public static class DateTimeUtil
{
    public static readonly TimeZoneInfo CNTZ = TimeZoneInfo.CreateCustomTimeZone("GMT+8", TimeSpan.FromHours(8), "China Standard Time", "(UTC+8)China Standard Time");

    #region 日期

    /// <summary>
    /// 获取指定日期所在周的起始日期，时分秒和传入日期相同
    /// </summary>
    /// <param name="date">指定日期</param>
    /// <param name="isMonday">是否以周一为每周第一天</param>
    /// <returns></returns>
    public static DateTime BeginDayOfWeek(DateTime date, bool isMonday = true)
    {
        int days = 0;
        if (isMonday)
        {
            days = date.DayOfWeek - DayOfWeek.Monday;
            if (days == -1) days = 6;
        }
        else
        {
            days = date.DayOfWeek - DayOfWeek.Sunday;
        }
        return date.Subtract(new TimeSpan(days, 0, 0, 0));
    }

    /// <summary>
    /// 日期下一个星期的第一天
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static DateTime BeginDayOfNextWeek(DateTime date)
    {
        var count = WeekCountOfYear(date.Year);
        var week = WeekOfYear(date);
        return week + 1 > count
            ? BeginDayOfWeek(date.Year + 1, 1)
            : BeginDayOfWeek(date.Year, week + 1);
    }

    /// <summary>
    /// 获取指定日期所在周的终止日期，时分秒和传入日期相同
    /// </summary>
    /// <param name="date">指定日期</param>
    /// <param name="isMonday">是否以周一为每周第一天</param>
    /// <returns></returns>
    public static DateTime EndDayOfWeek(DateTime date, bool isMonday = true)
        => BeginDayOfWeek(date, isMonday).AddDays(6);

    /// <summary>
    /// 获取时间段内包含的总周数（含起始周和结束周）
    /// </summary>
    /// <param name="start">起始日期</param>
    /// <param name="end">终止日期</param>
    /// <returns></returns>
    public static int WeekCount(DateTime start, DateTime end)
    {
        DateTime min = DateTime.MinValue;
        start = BeginDayOfWeek(start);
        int days = (int)end.Subtract(min).TotalDays - (int)start.Subtract(min).TotalDays;
        return (int)days / 7 + 1;
    }

    /// <summary>
    /// 获取指定年份的总周数
    /// </summary>
    /// <param name="year">年份</param>
    /// <returns></returns>
    public static int WeekCountOfYear(int year)
    {
        DateTime start = new DateTime(year, 1, 1);
        DateTime end = new DateTime(year + 1, 1, 1).AddDays(-1);
        return WeekCount(start, end);
    }

    /// <summary>
    /// 获取指定日期在这一年的第几周中
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static int WeekOfYear(DateTime date)
    {
        DateTime start = new DateTime(date.Year, 1, 1);
        return WeekCount(start, date);
    }
    public static int ToYearWeek(DateTime date)
        => ToYearWeekString(date).ToInt32();
    public static string ToYearWeekString(DateTime date)
    {
        var week = WeekOfYear(date).ToString().PadLeft(2, '0');
        return $"{date.Year}{week}";
    }

    /// <summary>
    /// 获取指定日期在这一月的第几周中
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static int WeekOfMonth(DateTime date)
    {
        DateTime start = new DateTime(date.Year, date.Month, 1);
        return WeekCount(start, date);
    }

    /// <summary>
    /// 获取指定年份中指定周数（第几周）的起始日期
    /// </summary>
    /// <param name="year">年份</param>
    /// <param name="week">周数</param>
    /// <returns></returns>
    public static DateTime BeginDayOfWeek(int year, int week)
        => BeginDayOfWeek(new DateTime(year, 1, 1)).AddDays((week - 1) * 7);

    /// <summary>
    /// 获取某年第几周的周末日期
    /// </summary>
    /// <param name="year">年份</param>
    /// <param name="week">周数</param>
    /// <returns></returns>
    public static DateTime EndDayOfWeek(int year, int week)
        => BeginDayOfWeek(year, week).AddDays(6);

    /// <summary>
    /// 判断指定日期是否是当前日期所在的周
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static bool IsCurrentWeek(DateTime date)
        => IsSameWeek(date, DateTime.Now);

    /// <summary>
    /// 判断指定的日期是否在同一周中
    /// </summary>
    /// <param name="dtA">要比较的第一个日期</param>
    /// <param name="dtB">要比较的第二个日期</param>
    /// <returns></returns>
    public static bool IsSameWeek(DateTime dtA, DateTime dtB)
    {
        DateTime dt = BeginDayOfWeek(dtA);
        int days = (int)(dtB - dt).TotalDays;
        return (days > 0 && days < 7);
    }

    /// <summary>
    /// 获取上个月的第一天
    /// </summary>
    /// <param name="dateTime">要传入的时间</param>
    /// <returns></returns>
    public static DateTime FirstDayOfPreviousMonth(DateTime dateTime)
        => dateTime.AddDays(1 - dateTime.Day).AddMonths(-1);

    /// <summary>
    /// 获取上个月的最后一天
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime LastDayOfPrdviousMonth(DateTime dateTime)
        => dateTime.AddDays(1 - dateTime.Day).AddDays(-1);

    public static string ToYearQuarter(DateTime date)
    {
        return $"{date.Year}{QuarterOfYear(date)}";
    }
    public static int QuarterOfYear(DateTime date)
    {
        var ret = Math.DivRem(date.Month, 3, out var r);
        if (r > 0)
            ret++;
        return ret;
    }
    #endregion

    #region DateTime和TimeStamp (又叫Unix时间戳, Unix epoch, Unix time, Unix timestamp)
    /// <summary>
    /// 将DateTime转换为Unix时间戳timestamp
    /// </summary>
    /// <param name="date">转换的日期时间</param>
    /// <param name="toMilliseconds">unix时间戳使用秒, javascript时间戳使用毫秒</param>
    /// <param name="trySpecifyUtcKind">是否尝试指定date的Kind为UTC</param>
    /// <returns></returns>
    public static long ToTimestamp(this DateTime date, bool toMilliseconds = true, bool trySpecifyUtcKind = true)
    {
        if (trySpecifyUtcKind && date.Kind == DateTimeKind.Unspecified)
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
        return toMilliseconds ? ((DateTimeOffset)date).ToUnixTimeMilliseconds()
            : ((DateTimeOffset)date).ToUnixTimeSeconds();
    }

    /// <summary>
    /// 将Unix时间戳timestamp转换为DateTime
    /// </summary>
    /// <param name="timeStamp">Unix时间戳或者javascript时间戳</param>
    /// <param name="toUtc">返回utc时间还是本地时间</param>
    /// <returns></returns>
    public static DateTime ParseTimestamp(long timeStamp, bool toUtc = true)
    {
        var hasMs = timeStamp > 1000000000000; // 13位毫秒
        var offset = hasMs ? DateTimeOffset.FromUnixTimeMilliseconds(timeStamp)
            : DateTimeOffset.FromUnixTimeSeconds(timeStamp);
        return toUtc ? offset.UtcDateTime : offset.LocalDateTime;
    }
    public static DateTime ParseTimestamp(string timeStamp, bool toUtc = true)
        => ParseTimestamp(long.Parse(timeStamp), toUtc);
    #endregion

    #region FormatString
    /// <summary>
    /// 日期字符串标准格式 yyyy-MM-dd HH:mm:ss 或yyyy-MM-ddTHH:mm:sszzz
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="isIso"></param>
    /// <returns></returns>
    public static string ToFormatString(this DateTime dateTime, bool isIso = true)
        => isIso
            ? dateTime.ToString("yyyy-MM-dd'T'HH:mm:sszzz")
            : dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    public static DateTime ParseFormatString(string s)
    {
        var isIso = s.Contains('T');
        return isIso
            ? DateTime.ParseExact(s, "yyyy-MM-dd'T'HH:mm:sszzz", DateTimeFormatInfo.InvariantInfo)
            : DateTime.ParseExact(s, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Utc时间转北京时间
    /// </summary>
    /// <param name="utcTime"></param>
    /// <returns></returns>
    public static DateTime UtcToCNTime(this DateTime utcTime)
        => TimeZoneInfo.ConvertTimeFromUtc(utcTime, CNTZ);
    /// <summary>
    /// UTC时间转北京时间如：2024-04-30T11:27:55+08:00
    /// </summary>
    /// <param name="utcTime"></param>
    /// <param name="isIso"></param>
    /// <returns></returns>
    public static string UtcToCNString(this DateTime utcTime, bool isIso = true)
        => UtcToCNTime(utcTime).ToFormatString(isIso);
    /// <summary>
    /// 北京时间字符串转UTC时间
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static DateTime CNStringToUtc(string str)
        => ParseFormatString(str).ToUniversalTime();
    #endregion

    #region 星座
    /// <summary>
    /// 获取星座
    /// </summary>
    /// <param name="month">月份</param>
    /// <param name="day">日期</param>
    /// <returns></returns>
    public static string GetConstellation(int month, int day)
        => new ChineseCalendarHelper(DateTime.Now.Year, month, day).ConstellationText;

    /// <summary>
    /// 获取星座
    /// </summary>
    /// <param name="birthday">出生日期</param>
    /// <returns></returns>
    public static string GetConstellation(DateTime birthday)
        => new ChineseCalendarHelper(birthday).ConstellationText;
    #endregion
}