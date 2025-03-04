using System;

namespace AiUo.Text;

/// <summary>
/// 金额转换类。静态类，用于金额大小写转换的类
/// </summary>
public static class MoneyUtil
{
    // 最大转化金额 （16位整数位，2位小数位）
    private const decimal _cnMaxValue = 9999999999999999.99M;
    private static readonly string _cnNumber = "零壹贰叁肆伍陆柒捌玖";
    private static readonly string _cnUnit = "分角元拾佰仟万拾佰仟亿拾佰仟兆拾佰仟";

    // 最大转化金额 （15位整数位，2位小数位）
    private const decimal _enMaxValue = 999999999999999.99M;
    private static readonly string[] _enSmallNumber = { "", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE", "TEN", "ELEVEN", "TWELVE", "THIRTEEN", "FOURTEEN", "FIFTEEN", "SIXTEEN", "SEVENTEEN", "EIGHTEEN", "NINETEEN" };
    private static readonly string[] _enLargeNumber = { "TWENTY", "THIRTY", "FORTY", "FIFTY", "SIXTY", "SEVENTY", "EIGHTY", "NINETY" };
    private static readonly string[] _enUnit = { "", "THOUSAND", "MILLION", "BILLION", "TRILLION" };

    /// <summary>
    /// 货币金额中文大写转换方法, 符合财务记帐要求
    /// </summary>
    /// <param name="money">金额</param>
    /// <returns></returns>
    public static string ToCN(decimal money)
    {
        if (money > _cnMaxValue)
            throw new ArgumentOutOfRangeException("money", string.Format("金额{0}超过范围，最大仅支持16位整数位的金额。", money));
        string moneyString = money.ToString("#0.00").Replace(".", string.Empty);
        string ret = string.Empty;
        int moneyLength = moneyString.Length - 1; ;
        int currNum = 0;
        int nextNum = 0;
        for (int i = 0; i <= moneyLength; i++)
        {
            currNum = (int)moneyString[i] - 48;
            nextNum = ((i + 1) >= moneyLength) ? (int)moneyString[moneyLength] - 48 : (int)moneyString[i + 1] - 48;
            if (currNum == 0)
            {
                if (moneyLength - i == 2 || moneyLength - i == 6 || moneyLength - i == 10 || moneyLength - i == 14)
                {
                    ret += _cnUnit[moneyLength - i];
                }
                else
                {
                    if (nextNum != 0)
                    {
                        ret += _cnNumber[currNum];
                    }
                }
            }
            else
            {
                ret = ret + _cnNumber[currNum] + _cnUnit[moneyLength - i];
            }
        }

        ret = ret.Replace("兆亿万", "兆");
        ret = ret.Replace("兆亿", "兆");
        ret = ret.Replace("亿万", "亿");
        ret = ret.TrimStart('元');
        ret = ret.TrimStart('零');

        if (ret.EndsWith("元"))
        {
            ret += "整";
        }
        return ret;
    }
    /// <summary>
    /// 货币金额中文大写转换方法, 符合财务记帐要求
    /// </summary>
    /// <param name="money">金额字符串</param>
    /// <returns></returns>
    public static string ToCN(string money)
    {
        decimal value;
        if (!decimal.TryParse(money, out value))
            throw new ArgumentException(string.Format("{0}为无效的货币格式字符串。", money), "money");
        return ToCN(value);
    }
    /// <summary>
    /// 货币金额英文大写转换方法
    /// </summary>
    /// <param name="money">金额</param>
    /// <returns></returns>
    public static string ToEN(decimal money)
    {
        if (money > _enMaxValue)
            throw new ArgumentOutOfRangeException("money", string.Format("金额{0}超过范围，最大支持15位整数位的金额。", money));

        string[] moneys = money.ToString("#0.00").Split('.');
        string intPart = moneys[0];
        string decPart = moneys[1];
        string ret = string.Empty;
        string strBuff1;
        string strBuff2;
        string strBuff3;
        int curPoint;
        int i1;
        int i2;
        int i3;
        int k;
        int n;
        // 以下处理整数部分
        curPoint = intPart.Length - 1;
        k = 0;
        while (curPoint >= 0)
        {
            strBuff1 = "";
            strBuff2 = "";
            strBuff3 = "";
            if (curPoint >= 2)
            {
                n = int.Parse(intPart.Substring(curPoint - 2, 3));
                if (n != 0)
                {
                    i1 = n / 100;            // 取佰位数值
                    i2 = (n - i1 * 100) / 10;    // 取拾位数值
                    i3 = n - i1 * 100 - i2 * 10;   // 取个位数值
                    if (i1 != 0)
                    {
                        strBuff1 = _enSmallNumber[i1] + " HUNDRED ";
                    }
                    if (i2 != 0)
                    {
                        if (i2 == 1)
                        {
                            strBuff2 = _enSmallNumber[i2 * 10 + i3] + " ";
                        }
                        else
                        {
                            strBuff2 = _enLargeNumber[i2 - 2] + " ";
                            if (i3 != 0)
                            {
                                strBuff3 = _enSmallNumber[i3] + " ";
                            }
                        }
                    }
                    else
                    {
                        if (i3 != 0)
                        {
                            strBuff3 = _enSmallNumber[i3] + " ";
                        }
                    }
                    ret = strBuff1 + strBuff2 + strBuff3 + _enUnit[k] + " " + ret;
                }
            }
            else
            {
                n = int.Parse(intPart.Substring(0, curPoint + 1));
                if (n != 0)
                {
                    i2 = n / 10;      // 取拾位数值
                    i3 = n - i2 * 10;   // 取个位数值
                    if (i2 != 0)
                    {
                        if (i2 == 1)
                        {
                            strBuff2 = _enSmallNumber[i2 * 10 + i3] + " ";
                        }
                        else
                        {
                            strBuff2 = _enLargeNumber[i2 - 2] + " ";
                            if (i3 != 0)
                            {
                                strBuff3 = _enSmallNumber[i3] + " ";
                            }
                        }
                    }
                    else
                    {
                        if (i3 != 0)
                        {
                            strBuff3 = _enSmallNumber[i3] + " ";
                        }
                    }
                    ret = strBuff2 + strBuff3 + _enUnit[k] + " " + ret;
                }
            }

            ++k;
            curPoint -= 3;
        }
        ret = ret.TrimEnd();


        // 以下处理小数部分
        strBuff2 = "";
        strBuff3 = "";
        n = int.Parse(decPart);
        if (n != 0)
        {
            i2 = n / 10;      // 取拾位数值
            i3 = n - i2 * 10;   // 取个位数值
            if (i2 != 0)
            {
                if (i2 == 1)
                {
                    strBuff2 = _enSmallNumber[i2 * 10 + i3] + " ";
                }
                else
                {
                    strBuff2 = _enLargeNumber[i2 - 2] + " ";
                    if (i3 != 0)
                    {
                        strBuff3 = _enSmallNumber[i3] + " ";
                    }
                }
            }
            else
            {
                if (i3 != 0)
                {
                    strBuff3 = _enSmallNumber[i3] + " ";
                }
            }

            // 将小数字串追加到整数字串后
            if (ret.Length > 0)
            {
                ret = ret + " AND CENTS " + strBuff2 + strBuff3;   // 有整数部分时
            }
            else
            {
                ret = "CENTS " + strBuff2 + strBuff3;   // 只有小数部分时
            }
        }

        ret = ret.TrimEnd();
        return ret;
    }
    /// <summary>
    /// 货币金额英文大写转换方法
    /// </summary>
    /// <param name="money">金额</param>
    /// <returns></returns>
    public static string ToEN(string money)
    {
        decimal value;
        if (!decimal.TryParse(money, out value))
            throw new ArgumentException(string.Format("{0}为无效的货币格式字符串。", money), "money");
        return ToEN(value);
    }
}