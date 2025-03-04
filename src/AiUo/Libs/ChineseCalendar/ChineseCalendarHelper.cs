using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace AiUo.ChineseCalendar;

/// <summary>
/// 中国阴历日期处理类
/// </summary>
public class ChineseCalendarHelper
{
    #region Properties
    //Lunar Solar
    private static ChineseLunisolarCalendar _calendar = new ChineseLunisolarCalendar();
    //年份
    private const string _numbers = "〇一二三四五六七八九";
    //天干
    private const string _stems = "甲乙丙丁戊己庚辛壬癸";
    //地支
    private const string _branchs = "子丑寅卯辰巳午未申酉戌亥";
    //属相
    private const string _animals = "鼠牛虎兔龙蛇马羊猴鸡狗猪";
    //月
    private static readonly string[] _monthNames = new string[] { "正", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "十二" };
    //日
    private static readonly string[] _dayNames = new string[] {
        "初一","初二","初三","初四","初五","初六","初七","初八","初九","初十",
        "十一","十二","十三","十四","十五","十六","十七","十八","十九","二十",
        "廿一","廿二","廿三","廿四","廿五","廿六","廿七","廿八","廿九","三十"};

    /// <summary>
    /// 星期集合
    /// </summary>
    public static readonly string[] Weeks = new string[] { "星期天", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };

    /// <summary>
    /// 星座集合
    /// </summary>
    public static readonly string[] Constellations = new string[] { "白羊", "金牛", "双子", "巨蟹", "狮子", "处女", "天秤", "天蝎", "射手", "摩羯", "水瓶", "双鱼" };

    /// <summary>
    /// 诞生石集合
    /// </summary>
    public static readonly string[] BirthStones = new string[] { "钻石", "蓝宝石", "玛瑙", "珍珠", "红宝石", "红条纹玛瑙", "蓝宝石", "猫眼石", "黄宝石", "土耳其玉", "紫水晶", "月长石，血石" };

    /// <summary>
    /// 节气集合
    /// </summary>
    public static readonly string[] SolarTerms = new string[] { "小寒", "大寒", "立春", "雨水", "惊蛰", "春分", "清明", "谷雨", "立夏", "小满", "芒种", "夏至", "小暑", "大暑", "立秋", "处暑", "白露", "秋分", "寒露", "霜降", "立冬", "小雪", "大雪", "冬至" };
    private static readonly int[] _solarTermsInfo = new int[] { 0, 21208, 42467, 63836, 85337, 107014, 128867, 150921, 173149, 195551, 218072, 240693, 263343, 285989, 308563, 331033, 353350, 375494, 397447, 419210, 440795, 462224, 483532, 504758 };

    /// <summary>
    /// 获取当前阳历时间
    /// </summary>
    public DateTime SolarDate { get; protected set; }

    /// <summary>
    /// 获取当前日期的后一天
    /// </summary>
    public ChineseCalendarHelper Next { get { return new ChineseCalendarHelper(SolarDate.AddDays(1)); } }

    /// <summary>
    /// 获取当前日期的前一天
    /// </summary>
    public ChineseCalendarHelper Previous { get { return new ChineseCalendarHelper(SolarDate.AddDays(-1)); } }
    #endregion

    #region 年
    /// <summary>
    /// 是否为闰年
    /// </summary>
    public bool IsLeapYear
    {
        get
        {
            return _calendar.IsLeapYear(SolarDate.Year);
        }
    }
    private int _year;
    /// <summary>
    /// 获取阴历年份
    /// </summary>
    public int LunarYear
    {
        get
        {
            if (_year == 0)
                _year = _calendar.GetYear(SolarDate);
            return _year;
        }
    }
    private string _yearText;
    /// <summary>
    /// 获取阴历年份中文名称
    /// </summary>
    public string LunarYearText
    {
        get
        {
            if (string.IsNullOrEmpty(_yearText))
            {
                StringBuilder sb = new StringBuilder();
                int year = LunarYear;
                int day;
                do
                {
                    day = year % 10;
                    sb.Insert(0, _numbers[day]);
                    year = year / 10;
                } while (year > 0);
                _yearText = sb.ToString();
            }
            return _yearText;
        }
    }
    #endregion

    #region 月
    /// <summary>
    /// 是否为闰月
    /// </summary>
    public bool IsLeapMonth
    {
        get
        {
            return _calendar.IsLeapMonth(SolarDate.Year, SolarDate.Month);
        }
    }
    private int _month;
    /// <summary>
    /// 获取阴历月份
    /// </summary>
    public int LunarMonth
    {
        get
        {
            if (_month == 0)
            {
                _month = _calendar.GetMonth(SolarDate);
                int leapMonth = _calendar.GetLeapMonth(LunarYear);
                if (leapMonth > 0 && leapMonth <= _month)
                {
                    _month -= 1;
                }
            }
            return _month;
        }
    }

    private string _monthText;
    /// <summary>
    /// 获取阴历月份中文名称
    /// </summary>
    public string MonthText
    {
        get
        {
            if (string.IsNullOrEmpty(_monthText))
            {
                _monthText = (IsLeapMonth ? "闰" : "") + _monthNames[LunarMonth - 1];
            }
            return _monthText;
        }
    }
    #endregion

    #region 日
    /// <summary>
    /// 是否为闰日
    /// </summary>
    public bool IsLeapDay
    {
        get
        {
            return _calendar.IsLeapDay(SolarDate.Year, SolarDate.Month, SolarDate.Day);
        }
    }
    private int _day;
    /// <summary>
    /// 获取阴历日期
    /// </summary>
    public int LunarDay
    {
        get
        {
            if (_day == 0)
            {
                _day = _calendar.GetDayOfMonth(SolarDate);
            }
            return _day;
        }
    }

    /// <summary>
    /// 获取阴历日期中文名称
    /// </summary>
    public string LunarDayText { get { return _dayNames[LunarDay - 1]; } }

    /// <summary>
    /// 获取星期几
    /// </summary>
    public string WeekText
    {
        get
        {
            int index = (int)SolarDate.DayOfWeek;
            return Weeks[index];
        }
    }
    #endregion

    #region 甲子
    private int _sexagenaryYear;
    private string _sexagenaryYearText;
    /// <summary>
    /// 获取甲子年份
    /// </summary>
    public int SexagenaryYear
    {
        get
        {
            if (_sexagenaryYear == 0)
            {
                _sexagenaryYear = _calendar.GetSexagenaryYear(SolarDate);
            }
            return _sexagenaryYear;
        }
    }

    /// <summary>
    /// 获取甲子年份中文名称
    /// </summary>
    public string SexagenaryYearText
    {
        get
        {
            if (string.IsNullOrEmpty(_sexagenaryYearText))
            {
                _sexagenaryYearText = string.Concat(_stems[(SexagenaryYear - 1) % 10], _branchs[(SexagenaryYear - 1) % 12]);
            }
            return _sexagenaryYearText;
        }
    }
    #endregion

    #region 生肖
    private int _animal = -1;
    /// <summary>
    /// 获取属相
    /// </summary>
    public int Animal
    {
        get
        {
            if (_animal == -1)
            {
                _animal = (SexagenaryYear - 1) % 12;
            }
            return _animal;
        }
    }

    /// <summary>
    /// 获取属相
    /// </summary>
    public string AnimalText { get { return _animals[Animal].ToString(); } }
    #endregion

    #region 二十四节气
    private int _solarTerm = -1;

    /// <summary>
    /// 获取二十四节气
    /// </summary>
    public int SolarTerm
    {
        get
        {
            if (_solarTerm == -1)
            {
                DateTime baseDate = new DateTime(1900, 1, 6, 2, 5, 0); //#1/6/1900 2:05:00 AM#
                DateTime newDate;
                double num;
                int year;

                year = SolarDate.Year;

                for (int i = 1; i <= 24; i++)
                {
                    num = 525948.76 * (year - 1900) + _solarTermsInfo[i - 1];

                    newDate = baseDate.AddMinutes(num);//按分钟计算
                    if (newDate.DayOfYear == SolarDate.DayOfYear)
                    {
                        _solarTerm = i - 1;
                        break;
                    }
                }
            }
            return _solarTerm;
        }
    }

    /// <summary>
    /// 获取二十四节气
    /// </summary>
    public string SolarTermText { get { return SolarTerms[SolarTerm]; } }
    #endregion

    #region 星座和诞生石
    private int _constellation = -1;

    /// <summary>
    /// 获取星座
    /// </summary>
    public int Constellation
    {
        get
        {
            if (_constellation == -1)
            {
                int num = SolarDate.Month * 100 + SolarDate.Day;
                if (num >= 321 && num <= 419)
                    _constellation = 0;
                else if (num >= 420 && num <= 520)
                    _constellation = 1;
                else if (num >= 521 && num <= 621)
                    _constellation = 2;
                else if (num >= 622 && num <= 722)
                    _constellation = 3;
                else if (num >= 723 && num <= 822)
                    _constellation = 4;
                else if (num >= 823 && num <= 922)
                    _constellation = 5;
                else if (num >= 923 && num <= 1023)
                    _constellation = 6;
                else if (num >= 1024 && num <= 1121)
                    _constellation = 7;
                else if (num >= 1122 && num <= 1221)
                    _constellation = 8;
                else if (num >= 1222 || num <= 119)
                    _constellation = 9;
                else if (num >= 120 && num <= 218)
                    _constellation = 10;
                else if (num >= 219 && num <= 320)
                    _constellation = 11;
                else
                    throw new Exception("日期错误。");

                #region 星座划分
                //白羊座：   3月21日------4月19日     诞生石：   钻石   
                //金牛座：   4月20日------5月20日   诞生石：   蓝宝石   
                //双子座：   5月21日------6月21日     诞生石：   玛瑙   
                //巨蟹座：   6月22日------7月22日   诞生石：   珍珠   
                //狮子座：   7月23日------8月22日   诞生石：   红宝石   
                //处女座：   8月23日------9月22日   诞生石：   红条纹玛瑙   
                //天秤座：   9月23日------10月23日     诞生石：   蓝宝石   
                //天蝎座：   10月24日-----11月21日     诞生石：   猫眼石   
                //射手座：   11月22日-----12月21日   诞生石：   黄宝石   
                //摩羯座：   12月22日-----1月19日   诞生石：   土耳其玉   
                //水瓶座：   1月20日-----2月18日   诞生石：   紫水晶   
                //双鱼座：   2月19日------3月20日   诞生石：   月长石，血石  
                #endregion
            }
            return _constellation;
        }
    }

    /// <summary>
    /// 获取星座
    /// </summary>
    public string ConstellationText { get { return Constellations[Constellation]; } }

    /// <summary>
    /// 获取诞生石
    /// </summary>
    public string BirthStoneText { get { return BirthStones[Constellation]; } }
    #endregion

    #region Constructors
    /// <summary>
    /// 构造函数
    /// </summary>
    public ChineseCalendarHelper()
        : this(DateTime.Now)
    { }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="date">阳历日期</param>
    public ChineseCalendarHelper(DateTime date)
    {
        SolarDate = date;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="year">阳历年</param>
    /// <param name="month">阳历月</param>
    /// <param name="day">阳历日</param>
    public ChineseCalendarHelper(int year, int month, int day)
        : this(new DateTime(year, month, day))
    { }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="year">阴历年</param>
    /// <param name="month">阴历月</param>
    /// <param name="day">阴历日</param>
    /// <param name="isLeapMonth">是否闰月</param>
    public ChineseCalendarHelper(int year, int month, int day, bool isLeapMonth)
        : this(GetDateFromLunarDate(year, month, day, isLeapMonth))
    { }
    #endregion

    #region 阴历转阳历
    /// <summary>
    /// 获取指定年份春节当日（正月初一）的阳历日期
    /// </summary>
    /// <param name="year">指定的年份</param>
    public static DateTime GetLunarNewYearDate(int year)
    {
        DateTime dt = new DateTime(year, 1, 1);
        int cnYear = _calendar.GetYear(dt);
        int cnMonth = _calendar.GetMonth(dt);

        int num1 = 0;
        int num2 = _calendar.IsLeapYear(cnYear) ? 13 : 12;

        while (num2 >= cnMonth)
        {
            num1 += _calendar.GetDaysInMonth(cnYear, num2--);
        }

        num1 = num1 - _calendar.GetDayOfMonth(dt) + 1;
        return dt.AddDays(num1);
    }

    /// <summary>
    /// 阴历转阳历
    /// </summary>
    /// <param name="year">阴历年</param>
    /// <param name="month">阴历月</param>
    /// <param name="day">阴历日</param>
    /// <param name="isLeapMonth">是否闰月</param>
    public static DateTime GetDateFromLunarDate(int year, int month, int day, bool isLeapMonth = false)
    {
        if (year < 1902 || year > 2100)
            throw new Exception("只支持1902～2100期间的农历年");
        if (month < 1 || month > 12)
            throw new Exception("表示月份的数字必须在1～12之间");

        if (day < 1 || day > _calendar.GetDaysInMonth(year, month))
            throw new Exception("农历日期输入有误");

        int num1 = 0, num2 = 0;
        int leapMonth = _calendar.GetLeapMonth(year);

        if (((leapMonth == month + 1) && isLeapMonth) || (leapMonth > 0 && leapMonth <= month))
            num2 = month;
        else
            num2 = month - 1;

        while (num2 > 0)
        {
            num1 += _calendar.GetDaysInMonth(year, num2--);
        }

        DateTime dt = GetLunarNewYearDate(year);
        return dt.AddDays(num1 + day - 1);
    }
    #endregion

    #region 节假日
    /// <summary>
    /// 阳历节日
    /// </summary>
    public static Dictionary<int, List<SolarHoliday>> SolarHolidays = new Dictionary<int, List<SolarHoliday>>();
    /// <summary>
    /// 阴历节日
    /// </summary>
    public static Dictionary<int, List<LunarHoliday>> LunarHolidays = new Dictionary<int, List<LunarHoliday>>();
    /// <summary>
    /// 按某月第几个星期的星期几计算的节日
    /// </summary>
    public static List<WeekHoliday> WeekHolidays = new List<WeekHoliday>();

    static ChineseCalendarHelper()
    {
        InitSolarHolidays();
        InitLunarHolidays();
        InitWeekHolidays();
    }

    private static void InitSolarHolidays()
    {
        //元旦
        AddSolarHoliday(new SolarHoliday(1, 1, true, "元旦", KnownHoliday.YuanDan));
        AddSolarHoliday(new SolarHoliday(2, 2, false, "世界湿地日"));
        AddSolarHoliday(new SolarHoliday(2, 10, false, "国际气象节"));
        AddSolarHoliday(new SolarHoliday(2, 14, false, "情人节"));
        AddSolarHoliday(new SolarHoliday(3, 1, false, "国际海豹日"));
        AddSolarHoliday(new SolarHoliday(3, 5, false, "学雷锋纪念日"));
        AddSolarHoliday(new SolarHoliday(3, 8, false, "妇女节"));
        AddSolarHoliday(new SolarHoliday(3, 12, false, "植树节"));
        AddSolarHoliday(new SolarHoliday(3, 12, false, "孙中山逝世纪念日"));
        AddSolarHoliday(new SolarHoliday(3, 14, false, "国际警察日"));
        AddSolarHoliday(new SolarHoliday(3, 15, false, "消费者权益日"));
        AddSolarHoliday(new SolarHoliday(3, 17, false, "中国国医节"));
        AddSolarHoliday(new SolarHoliday(3, 17, false, "国际航海日"));
        AddSolarHoliday(new SolarHoliday(3, 21, false, "世界森林日"));
        AddSolarHoliday(new SolarHoliday(3, 21, false, "消除种族歧视国际日"));
        AddSolarHoliday(new SolarHoliday(3, 22, false, "世界水日"));
        AddSolarHoliday(new SolarHoliday(3, 24, false, "世界防治结核病日"));
        AddSolarHoliday(new SolarHoliday(4, 1, false, "愚人节"));
        AddSolarHoliday(new SolarHoliday(4, 7, false, "世界卫生日"));
        AddSolarHoliday(new SolarHoliday(4, 22, false, "世界地球日"));
        //劳动节
        AddSolarHoliday(new SolarHoliday(5, 1, true, "劳动节", KnownHoliday.LaoDong));
        AddSolarHoliday(new SolarHoliday(5, 2, true, "劳动节假日"));
        AddSolarHoliday(new SolarHoliday(5, 3, true, "劳动节假日"));
        AddSolarHoliday(new SolarHoliday(5, 4, false, "青年节"));
        AddSolarHoliday(new SolarHoliday(5, 8, false, "世界红十字日"));
        AddSolarHoliday(new SolarHoliday(5, 12, false, "国际护士节"));
        AddSolarHoliday(new SolarHoliday(5, 31, false, "世界无烟日"));
        AddSolarHoliday(new SolarHoliday(6, 1, false, "国际儿童节"));
        AddSolarHoliday(new SolarHoliday(6, 5, false, "世界环境保护日"));
        AddSolarHoliday(new SolarHoliday(6, 26, false, "国际禁毒日"));
        AddSolarHoliday(new SolarHoliday(7, 1, false, "建党节"));
        AddSolarHoliday(new SolarHoliday(7, 11, false, "世界人口日"));
        AddSolarHoliday(new SolarHoliday(8, 1, false, "建军节"));
        AddSolarHoliday(new SolarHoliday(8, 8, false, "父亲节"));
        AddSolarHoliday(new SolarHoliday(8, 8, false, "中国男子节"));
        AddSolarHoliday(new SolarHoliday(8, 15, false, "抗日战争胜利纪念"));
        AddSolarHoliday(new SolarHoliday(9, 9, false, "毛泽东逝世纪念"));
        AddSolarHoliday(new SolarHoliday(9, 10, false, "教师节"));
        AddSolarHoliday(new SolarHoliday(9, 18, false, "九·一八事变纪念日"));
        AddSolarHoliday(new SolarHoliday(9, 20, false, "国际爱牙日"));
        AddSolarHoliday(new SolarHoliday(9, 27, false, "世界旅游日"));
        AddSolarHoliday(new SolarHoliday(9, 28, false, "孔子诞辰"));
        //国庆节
        AddSolarHoliday(new SolarHoliday(10, 1, true, "国庆节", KnownHoliday.GuoQing));
        AddSolarHoliday(new SolarHoliday(10, 1, true, "国际音乐日"));
        AddSolarHoliday(new SolarHoliday(10, 2, true, "国庆节假日"));
        AddSolarHoliday(new SolarHoliday(10, 3, true, "国庆节假日"));
        AddSolarHoliday(new SolarHoliday(10, 6, false, "老人节"));
        AddSolarHoliday(new SolarHoliday(10, 24, false, "联合国日"));
        AddSolarHoliday(new SolarHoliday(11, 10, false, "世界青年节"));
        AddSolarHoliday(new SolarHoliday(11, 12, false, "孙中山诞辰纪念"));
        AddSolarHoliday(new SolarHoliday(12, 1, false, "世界艾滋病日"));
        AddSolarHoliday(new SolarHoliday(12, 3, false, "世界残疾人日"));
        AddSolarHoliday(new SolarHoliday(12, 20, false, "澳门回归纪念"));
        AddSolarHoliday(new SolarHoliday(12, 24, false, "平安夜"));
        AddSolarHoliday(new SolarHoliday(12, 25, false, "圣诞节"));
        AddSolarHoliday(new SolarHoliday(12, 26, false, "毛泽东诞辰纪念"));
    }
    private static void AddSolarHoliday(SolarHoliday item)
    {
        int key = item.Month * 100 + item.Day;
        List<SolarHoliday> items = null;
        if (!SolarHolidays.ContainsKey(key))
        {
            items = new List<SolarHoliday>();
            SolarHolidays.Add(key, items);
        }
        items = SolarHolidays[key];
        items.Add(item);
    }

    private static void InitLunarHolidays()
    {
        //春节
        AddLunarHoliday(new LunarHoliday(1, 1, true, "春节", KnownHoliday.ChunJie));
        AddLunarHoliday(new LunarHoliday(1, 15, false, "元宵节"));
        AddLunarHoliday(new LunarHoliday(5, 5, false, "端午节"));
        AddLunarHoliday(new LunarHoliday(7, 7, false, "七夕情人节"));
        AddLunarHoliday(new LunarHoliday(7, 15, false, "中元节"));
        AddLunarHoliday(new LunarHoliday(7, 15, false, "盂兰盆节"));
        AddLunarHoliday(new LunarHoliday(8, 15, false, "中秋节"));
        AddLunarHoliday(new LunarHoliday(9, 9, false, "重阳节"));
        AddLunarHoliday(new LunarHoliday(12, 8, false, "腊八节"));
        AddLunarHoliday(new LunarHoliday(12, 23, false, "北方小年(扫房)"));
        AddLunarHoliday(new LunarHoliday(12, 24, false, "南方小年(掸尘)"));
        //除夕特殊处理
    }
    private static void AddLunarHoliday(LunarHoliday item)
    {
        int key = item.Month * 100 + item.Day;
        List<LunarHoliday> items = null;
        if (!LunarHolidays.ContainsKey(key))
        {
            items = new List<LunarHoliday>();
            LunarHolidays.Add(key, items);
        }
        items = LunarHolidays[key];
        items.Add(item);
    }
    private static void InitWeekHolidays()
    {
        WeekHolidays.Add(new WeekHoliday(5, 2, 1, false, "母亲节"));
        WeekHolidays.Add(new WeekHoliday(5, 3, 1, false, "全国助残日"));
        WeekHolidays.Add(new WeekHoliday(6, 3, 1, false, "父亲节"));
        WeekHolidays.Add(new WeekHoliday(9, 3, 3, false, "国际和平日"));
        WeekHolidays.Add(new WeekHoliday(9, 4, 1, false, "国际聋人节"));
        WeekHolidays.Add(new WeekHoliday(10, 1, 2, false, "国际住房日"));
        WeekHolidays.Add(new WeekHoliday(10, 1, 4, false, "国际减轻自然灾害日"));
        WeekHolidays.Add(new WeekHoliday(11, 4, 5, false, "感恩节"));
    }

    private List<HolidayBase> _holidays;

    /// <summary>
    /// 节假日
    /// </summary>
    public List<HolidayBase> Holidays
    {
        get
        {
            if (_holidays == null)
            {
                _holidays = new List<HolidayBase>();

                //阳历
                int key = SolarDate.Month * 100 + SolarDate.Day;
                if (SolarHolidays.ContainsKey(key))
                {
                    foreach (SolarHoliday item in SolarHolidays[key])
                    {
                        _holidays.Add(item);
                    }
                }

                //阴历
                if (!IsLeapMonth)
                {
                    key = LunarMonth * 100 + LunarDay;
                    if (LunarHolidays.ContainsKey(key))
                    {
                        foreach (LunarHoliday item in LunarHolidays[key])
                        {
                            _holidays.Add(item);
                        }
                    }
                }

                //星期
                int weekAtMonth = DateTimeUtil.WeekOfMonth(SolarDate);
                foreach (WeekHoliday item in WeekHolidays)
                {
                    if (item.Month == SolarDate.Month && item.WeekAtMonth == weekAtMonth && item.DayOfWeek == SolarDate.DayOfWeek)
                        _holidays.Add(item);
                }

                //除夕
                DateTime dt = GetLunarNewYearDate(LunarYear + 1).AddDays(-1);
                if (dt.Year == SolarDate.Year && dt.Month == SolarDate.Month && dt.Day == SolarDate.Day)
                {
                    _holidays.Add(new LunarHoliday(0, 0, true, "除夕", KnownHoliday.Undefined));
                }
            }
            return _holidays;
        }
    }
    #endregion
}