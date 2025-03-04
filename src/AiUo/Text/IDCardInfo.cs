using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AiUo.ChinaArea;

namespace AiUo.Text;

/// <summary>
/// 居民身份证信息类
/// </summary>
public class IDCardInfo
{
    #region 身份证号码含义说明
    /*
    18位身份证号码各位的含义:
        1-2位省、自治区、直辖市代码；
        3-4位地级市、盟、自治州代码；
        5-6位县、县级市、区代码；
        7-14位出生年月日
        15-17位为顺序号，其中17位男为单数，女为双数；
        18位为校验码，0-9和 X。前17位的乘积和除以11取余数对应余数表获取末位置


    15位身份证号码各位的含义:
        1-2位省、自治区、直辖市代码；
        3-4位地级市、盟、自治州代码；
        5-6位县、县级市、区代码；
        7-12位出生年月日；
        13-15位为顺序号，其中15位男为单数，女为双数；
     */
    #endregion

    #region Init
    //前两位省市对应信息
    private static Dictionary<byte, string> _provinceCache = new Dictionary<byte, string>();
    //末尾验证系数
    private static byte[] _rates = new byte[] { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };
    //余数表
    private static char[] _rems = new char[] { '1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2' };

    /// <summary>
    /// 静态构造函数
    /// </summary>
    static IDCardInfo()
    {
        _provinceCache.Add(11, "北京");
        _provinceCache.Add(12, "天津");
        _provinceCache.Add(13, "河北");
        _provinceCache.Add(14, "山西");
        _provinceCache.Add(15, "内蒙古");
        _provinceCache.Add(21, "辽宁");
        _provinceCache.Add(22, "吉林");
        _provinceCache.Add(23, "黑龙江");
        _provinceCache.Add(31, "上海");
        _provinceCache.Add(32, "江苏");
        _provinceCache.Add(33, "浙江");
        _provinceCache.Add(34, "安徽");
        _provinceCache.Add(35, "福建");
        _provinceCache.Add(36, "江西");
        _provinceCache.Add(37, "山东");
        _provinceCache.Add(41, "河南");
        _provinceCache.Add(42, "湖北");
        _provinceCache.Add(43, "湖南");
        _provinceCache.Add(44, "广东");
        _provinceCache.Add(45, "广西");
        _provinceCache.Add(46, "海南");
        _provinceCache.Add(50, "重庆");
        _provinceCache.Add(51, "四川");
        _provinceCache.Add(52, "贵州");
        _provinceCache.Add(53, "云南");
        _provinceCache.Add(54, "西藏");
        _provinceCache.Add(61, "陕西");
        _provinceCache.Add(62, "甘肃");
        _provinceCache.Add(63, "青海");
        _provinceCache.Add(64, "宁夏");
        _provinceCache.Add(65, "新疆");
        _provinceCache.Add(71, "台湾");
        _provinceCache.Add(81, "香港");
        _provinceCache.Add(82, "澳门");
        _provinceCache.Add(91, "国外");
    }
    #endregion

    /// <summary>
    /// 身份证号码
    /// </summary>
    public string Number { get; internal set; }

    /// <summary>
    /// 省市信息
    /// </summary>
    public string ProvinceName { get; internal set; }

    /// <summary>
    /// 获取省市信息
    /// </summary>
    public ChinaAreaInfo Province
    {
        get {
            var provs = ChinaAreaUtil.GetByName(ProvinceName);
            if (provs == null || provs.Count != 1)
                throw new Exception("获取省份信息错误:" + ProvinceName);
            return provs[0];
        }
    }

    /// <summary>
    /// 生日
    /// </summary>
    public DateTime Birthday { get; internal set; }

    /// <summary>
    /// 性别，男true 女false
    /// </summary>
    public bool Sex { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="number"></param>
    public IDCardInfo(string number)
    {
        //格式验证
        if (!IsValid(number))
            throw new ArgumentException("身份证号码错误。");

        Regex regx = null;
        string[] arr = null;
        if (number.Length == 18)
        {
            regx = new Regex(@"^(\d{2})(\d{4})(\d{8})(\d{3})([0-9]|X)$");
            arr = regx.Split(number);
        }
        else if (number.Length == 15)
        {
            regx = new Regex(@"^(\d{2})(\d{4})(\d{6})(\d{3})$");
            arr = regx.Split(number);
            arr[3] = "19" + arr[3];
        }
        Number = number;
        ProvinceName = _provinceCache[byte.Parse(arr[1])];
        Birthday = DateTime.ParseExact(arr[3], "yyyyMMdd", null);
        Sex = (int.Parse(arr[4]) % 2 != 0);
    }

    /// <summary>
    /// 验证身份证号码是否有效
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool IsValid(string number)
    {
        bool ret = Regex.IsMatch(number, @"(^\d{15}$)|(^\d{17}([0-9]|X)$)");
        if (ret)
        {
            ret = _provinceCache.ContainsKey(byte.Parse(number.Substring(0, 2)));
        }
        if (ret)
        {
            if (number.Length == 18)
            {
                int sum = 0;
                for (int i = 0; i < 17; i++)
                {
                    sum += int.Parse(number[i].ToString()) * _rates[i];
                }

                ret = (_rems[sum % 11] == number[17]);
            }
        }
        return ret;
    }
}