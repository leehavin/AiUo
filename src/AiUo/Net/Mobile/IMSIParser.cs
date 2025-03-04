using System;
using System.Collections.Generic;
using AiUo.ChinaArea;

namespace AiUo.Net.Mobile;

/// <summary>
/// 手机IMSI解析类
/// </summary>
public class IMSIParser
{
    #region Static
    private static SortedDictionary<string, string> _mappingCache = new SortedDictionary<string, string>();

    #region _mappingList
    private const string _mappingList = @"
-----------------------------移动
46000BCD0A 135
46000BCD1A 136
46000BCD2A 137
46000BCD3A 138
46000BCD4A 139

46000BCD5X 1350
46000BCD6X 1360
46000BCD7X 1370
46000BCD8X 1380
46000BCD9X 1390

460020ABCD 134 --A:0-8
460021ABCD 151
460022ABCD 152
460023ABCD 150
460025ABCD 183
460026ABCD 182 --未验证
460027ABCD 187
460028ABCD 158
460029ABCD 159

460077ABCD 157
460078ABCD 188
460079ABCD 147

--------------------------联通
46001BCDA0 130
46001BCDA1 130
46001BCDA9 131
46001BCDA2 132
46001BCDA3 156
46001BCDA4 155
46001BCDA6 186
46001BCDA5 185 --可能是145或者185

-------------------------电信
460036AB0CD 153
4600309ABCD 133
460030900CD 13301
46003090BCD 1330
46003030BCD 1335
460030953CD 13398
460030954CD 13399
";
    #endregion

    static IMSIParser()
    {
        string[] lines = _mappingList.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue;
            if (line.StartsWith("-")) continue;

            var items = line.Split(' ');
            _mappingCache.Add(items[0], items[1]);
        }
    }
    #endregion

    /// <summary>
    /// 运营商类型 0-未知 1-移动 2-联通 3-电信
    /// </summary>
    public TelecomMode Telecom { get; set; }

    /// <summary>
    /// IMSI号
    /// </summary>
    public string IMSI { get; set; }

    /// <summary>
    /// MCC
    /// </summary>
    public string MCC { get; set; }

    /// <summary>
    /// MNC
    /// </summary>
    public string MNC { get; set; }

    /// <summary>
    /// MIN
    /// </summary>
    public string MIN { get; set; }

    /// <summary>
    /// 手机号段,如1330102
    /// </summary>
    public string Segment { get; set; }

    /// <summary>
    /// 省市
    /// </summary>
    public ChinaAreaInfo Province
    {
        get
        {
            ChinaAreaInfo ret = null;
            if (!string.IsNullOrEmpty(Segment))
                ret = MobileParser.GetProvince(Segment);
            return ret;
        }
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="imsi"></param>
    public IMSIParser(string imsi)
    {
        if (imsi.Length != 15) return;
        IMSI = imsi;
        MCC = imsi.Substring(0, 3);
        MNC = imsi.Substring(3, 2);
        MIN = imsi.Substring(5);
        string prefix = imsi.Substring(0, 5);
        Telecom = GetTelecom(prefix);
        switch (prefix)
        {
            case "46000":
                Parse46000();
                break;
            case "46002":
                Parse46002();
                break;
            case "46007":
                Parse46007();
                break;
            case "46001":
                Parse46001();
                break;
            case "46003":
                Parse46003();
                break;
        }
    }

    // 前5位 0-未知 1-移动 2-联通 3-电信
    private static TelecomMode GetTelecom(string telecom)
    {
        TelecomMode ret = TelecomMode.Unknown;
        switch (telecom)
        {
            case "46000":
            case "46002":
            case "46007":
                ret = TelecomMode.ChinaMobile;
                break;
            case "46001":
            case "46006":
                ret = TelecomMode.ChinaUnicom;
                break;
            case "46003":
            case "46005":
                ret = TelecomMode.ChinaTelecom;
                break;
            default:
                ret = TelecomMode.Unknown;
                break;
        }
        return ret;
    }

    private void Parse46000()
    {
        string key = string.Format("46000BCD{0}A", IMSI[8]);
        Parse(key);
    }
    private void Parse46002()
    {
        string key = string.Format("46002{0}ABCD", IMSI[5]);
        Parse(key);
    }
    private void Parse46007()
    {
        char point = IMSI[5];
        if (point == '7' || point == '8' || point == '9')
        {
            string key = string.Format("46007{0}ABCD", point);
            Parse(key);
        }
    }
    private void Parse46001()
    {
        char point = IMSI[9];
        if (point == '7' || point == '8') return;
        string key = string.Format("46001BCDA{0}", IMSI[9]);
        Parse(key);
    }
    private void Parse46003()
    {
        if (IMSI.Substring(0, 6) == "460036") { Parse("460036AB0CD"); return; }
        if (IMSI.Substring(0, 7) == "4600309") { Parse("4600309ABCD"); return; }
        if (IMSI.Substring(0, 8) == "46003090") { Parse("46003090BCD"); return; }
        if (IMSI.Substring(0, 8) == "46003030") { Parse("46003030BCD"); return; }
        if (IMSI.Substring(0, 9) == "460030900") { Parse("460030900CD"); return; }
        if (IMSI.Substring(0, 9) == "460030953") { Parse("460030953CD"); return; }
        if (IMSI.Substring(0, 9) == "460030954") { Parse("460030954CD"); return; }
    }
    private void Parse(string key)
    {
        if (_mappingCache.ContainsKey(key))
        {
            string mobilePrefix = _mappingCache[key];
            string a = null, b = null, c = null, d = null;
            for (int i = 5; i < key.Length; i++)
            {
                switch (key[i])
                {
                    case 'A':
                        a = IMSI[i].ToString();
                        break;
                    case 'B':
                        b = IMSI[i].ToString();
                        break;
                    case 'C':
                        c = IMSI[i].ToString();
                        break;
                    case 'D':
                        d = IMSI[i].ToString();
                        break;
                }
            }
            Segment = string.Format("{0}{1}{2}{3}{4}", mobilePrefix, a, b, c, d);
        }
    }
}