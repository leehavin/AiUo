using System;

namespace AiUo;

/// <summary>
/// 进制辅助类
/// BIN-二进制
/// OCT-八进制
/// DEC-十进制
/// HEX-十六进制
/// </summary>
public class NumberSystemUtil
{
    private const string Digits = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-_";

    /// <summary>
    /// 其他进制转10进制
    /// </summary>
    /// <param name="value">其他进制的值</param>
    /// <param name="fromBase">value的进制(2-64)</param>
    /// <returns></returns>
    public static long BaseToDec(string value, int fromBase)
    {
        switch (fromBase)
        {
            case 2:
                return Convert.ToInt64(value, 2);
            case 8:
                return Convert.ToInt64(value, 8);
            case 16:
                return Convert.ToInt64(value, 16);
        }

        char[] a = value.ToCharArray();
        int[] i_a = new int[a.Length];

        long ans = 0;
        long k = 1;

        for (int i = 0; i < a.Length; i++)
        {
            int int_a = System.Convert.ToInt32(a[i]);
            //0-9的ASCII码 48-57
            if (int_a >= 48 && int_a <= 57)
            {
                i_a[i] = int_a - 48;
            }
            //A-Zd的ASCII码 65-90
            else if (int_a >= 65 && int_a <= 90)
            {
                i_a[i] = int_a - 65 + 36;
            }
            else
            {
                i_a[i] = int_a - 97 + 10;
            }

        }

        for (int i = i_a.Length - 1; i >= 0; i--)
        {
            if (i == i_a.Length - 1)
            {
                ans = i_a[i];
            }
            else
            {
                for (int j = 0; j < a.Length - i - 1; j++)
                {
                    k *= fromBase;
                }
                ans += (i_a[i] * k);
                k = 1;
            }
        }
        return ans;
    }

    /// <summary>
    /// 10进制转其他进制（2-64）
    /// </summary>
    /// <param name="value">10进制数</param>
    /// <param name="toBase">目标进制(2-64)</param>
    /// <returns></returns>
    public static string DecToBase(long value, int toBase)
    {
        switch (toBase)
        {
            case 2:
                return Convert.ToString(value, 2);
            case 8:
                return Convert.ToString(value, 8);
            case 10:
                return Convert.ToString(value, 10);
            case 16:
                return Convert.ToString(value, 16);
        }

        const int BitsInLong = 128;
        int index = BitsInLong - 1;
        long currentNumber = Math.Abs(value);//对输入的数据取绝对值
        char[] charArray = new char[BitsInLong];

        while (currentNumber != 0)
        {
            int remainder = (int)(currentNumber % toBase);
            charArray[index--] = Digits[remainder];
            currentNumber = currentNumber / toBase;
        }

        string result = new String(charArray, index + 1, BitsInLong - index - 1);
        if (value < 0)
        {
            result = "-" + result;
        }

        return result;
    }
}