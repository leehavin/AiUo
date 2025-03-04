using System.Collections.Generic;
using System.Linq;
using AiUo.ChinaArea;

namespace AiUo.Net.Mobile;

/// <summary>
/// 手机的ICCID解析类
/// </summary>
public class ICCIDParser
{
    /// <summary>
    /// 电信运营商类型
    /// </summary>
    public TelecomMode Telecom { get; private set; }

    /// <summary>
    /// 生产年份字符串，如：12 代表2012
    /// </summary>
    public string YearString { get; private set; }

    /// <summary>
    /// 制卡年份
    /// </summary>
    public int Year => int.Parse($"20{YearString}");

    /// <summary>
    /// 解析的省份字符串
    /// </summary>
    public string ProvinceString { get; private set; }

    /// <summary>
    /// 省市
    /// </summary>
    public ChinaAreaInfo Province { get; private set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="iccid"></param>
    public ICCIDParser(string iccid)
    {
        if (iccid.Length < 6) return;
        string prefix = iccid.Substring(0, 6);
        switch (prefix)
        {
            case "898600":
            case "898602":
                Telecom = TelecomMode.ChinaMobile;
                ParseChinaMobile(iccid);
                break;
            case "898601":
            case "898609":
                Telecom = TelecomMode.ChinaUnicom;
                ParseChinaUnicom(iccid);
                break;
            case "898603":
            case "898606":
                Telecom = TelecomMode.ChinaTelecom;
                ParseChinaTelecom(iccid);
                break;
            default:
                Telecom = TelecomMode.Unknown;
                break;
        }
    }

    #region Static
    private static Dictionary<string, ChinaAreaInfo> _chinaMobileProv;
    //private static Dictionary<string, ChineseProvinceInfo> _chinaUnicomProv;
    static ICCIDParser()
    {
        #region 移动
        _chinaMobileProv = new Dictionary<string, ChinaAreaInfo>();
        _chinaMobileProv.Add("01", ChinaAreaUtil.GetByName("北京").First());
        _chinaMobileProv.Add("02", ChinaAreaUtil.GetByName("天津").First());
        _chinaMobileProv.Add("03", ChinaAreaUtil.GetByName("河北").First());
        _chinaMobileProv.Add("04", ChinaAreaUtil.GetByName("山西").First());
        _chinaMobileProv.Add("05", ChinaAreaUtil.GetByName("内蒙古").First());
        _chinaMobileProv.Add("06", ChinaAreaUtil.GetByName("辽宁").First());
        _chinaMobileProv.Add("07", ChinaAreaUtil.GetByName("吉林").First());
        _chinaMobileProv.Add("08", ChinaAreaUtil.GetByName("黑龙江").First());
        _chinaMobileProv.Add("09", ChinaAreaUtil.GetByName("上海").First());
        _chinaMobileProv.Add("10", ChinaAreaUtil.GetByName("江苏").First());
        _chinaMobileProv.Add("11", ChinaAreaUtil.GetByName("浙江").First());
        _chinaMobileProv.Add("12", ChinaAreaUtil.GetByName("安徽").First());
        _chinaMobileProv.Add("13", ChinaAreaUtil.GetByName("福建").First());
        _chinaMobileProv.Add("14", ChinaAreaUtil.GetByName("江西").First());
        _chinaMobileProv.Add("15", ChinaAreaUtil.GetByName("山东").First());
        _chinaMobileProv.Add("16", ChinaAreaUtil.GetByName("河南").First());
        _chinaMobileProv.Add("17", ChinaAreaUtil.GetByName("湖北").First());
        _chinaMobileProv.Add("18", ChinaAreaUtil.GetByName("湖南").First());
        _chinaMobileProv.Add("19", ChinaAreaUtil.GetByName("广东").First());
        _chinaMobileProv.Add("20", ChinaAreaUtil.GetByName("广西").First());
        _chinaMobileProv.Add("21", ChinaAreaUtil.GetByName("海南").First());
        _chinaMobileProv.Add("22", ChinaAreaUtil.GetByName("四川").First());
        _chinaMobileProv.Add("23", ChinaAreaUtil.GetByName("贵州").First());
        _chinaMobileProv.Add("24", ChinaAreaUtil.GetByName("云南").First());
        _chinaMobileProv.Add("25", ChinaAreaUtil.GetByName("西藏").First());
        _chinaMobileProv.Add("26", ChinaAreaUtil.GetByName("陕西").First());
        _chinaMobileProv.Add("27", ChinaAreaUtil.GetByName("甘肃").First());
        _chinaMobileProv.Add("28", ChinaAreaUtil.GetByName("青海").First());
        _chinaMobileProv.Add("29", ChinaAreaUtil.GetByName("宁夏").First());
        _chinaMobileProv.Add("30", ChinaAreaUtil.GetByName("新疆").First());
        _chinaMobileProv.Add("31", ChinaAreaUtil.GetByName("重庆").First());
        #endregion
    }
    #endregion

    /// <summary>
    /// 解析中国移动iccid
    /// 号码规则：898600(898602) MFSS YYGXX XXXXP
    /// 规则解析：
    ///     M - 用户号码前3位。0：159 1：158 2：150 3：151 4-9：134-139 A：157 B：188 C：152 D：147 E：187
    ///     F - 用户号码第 4 位
    ///     SS - 省份 
    ///     YY - 编制 ICCID 时年号的后两位
    ///     G - SIM卡供应商
    ///     P - 校验位
    /// </summary>
    /// <param name="iccid"></param>
    private void ParseChinaMobile(string iccid)
    {
        if (iccid.Length != 20) return;
        YearString = iccid.Substring(10, 2);
        //char lastNum = iccid[6]; 网号最后一位，135的5
        ProvinceString = iccid.Substring(8, 2);
        Province = _chinaMobileProv.ContainsKey(ProvinceString) ? _chinaMobileProv[ProvinceString] : null;
    }

    /// <summary>
    /// 解析联通iccid
    /// 号码位置：898601(898609) Y1Y2 MH0 A1A2A3 N1N2N3N4N5N6 S
    /// 位置解析：
    ///     Y1Y2 - 制卡年号 
    ///     M - 手机号码前三位的最后一位,例如130的0 
    ///     A1A2A3 - 地区号 如上海区号为010，为‘010’;长沙区号为0731，则为‘731’
    ///     N1N2N3N4N5N6 - 6位流水号
    ///     S - SIM卡供应商
    /// </summary>
    /// <param name="iccid"></param>
    private void ParseChinaUnicom(string iccid)
    {
        if (iccid.Length != 20) return;
        YearString = iccid.Substring(6, 2);
        //char lastNum = iccid[8];
        ProvinceString = iccid.Substring(10, 3);
        Province = GetProvince(ProvinceString);
        if (Province == null)
        {
            ProvinceString = iccid.Substring(9, 3);
            Province = GetProvince(ProvinceString);
        }
    }

    /// <summary>
    /// 解析电信iccid 898603，898606
    /// 898606 MY1Y2H1H2H3 XXXXXXXX
    /// 898603 YYXMHHHXXXXXXP
    ///     YY - 年
    ///     HHH - 地区代码
    /// </summary>
    /// <param name="iccid"></param>
    private void ParseChinaTelecom(string iccid)
    {
        if (iccid.Length != 20) return;
        if (iccid[5] == '3')
        {
            YearString = iccid.Substring(6, 2);
            ProvinceString = iccid.Substring(10, 3);
            Province = GetProvince(ProvinceString);
        }
        else // 898606
        {
            YearString = iccid.Substring(7, 2);
            ProvinceString = iccid.Substring(9, 3);
            Province = GetProvince(ProvinceString);
        }
    }

    private ChinaAreaInfo GetProvince(string prov)
    {
        ChinaAreaInfo ret = null;
        if (prov[0] != '0')
            prov = "0" + prov;
        var cities = ChinaAreaUtil.GetByCityCode(prov);
        if (cities != null && cities.Count > 0)
            ret = ChinaAreaUtil.GetById(cities[0].AreaID);
        return ret;
    }
}