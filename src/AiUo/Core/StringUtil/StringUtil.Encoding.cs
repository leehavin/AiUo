using System;
using System.Text;

namespace AiUo;

/// <summary>
/// 字符串操作静态辅助类
/// </summary>
public static partial class StringUtil
{
    #region 判断指定字符串是否兼容对应的字符集 "a中啰⿰"

    /// <summary>
    /// 判断字符串是否兼容ASCII编码
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsASCII(string str)
        =>  MatchCharacterSetOfAll(str, IsASCII);

    /// <summary>
    /// 判断是否是兼容GB2312编码(GB2312-1980)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsGB2312(string str)
        => MatchCharacterSetOfAll(str, IsGB2312);

    /// <summary>
    /// 判断是否兼容GBK编码（不包括比CP936多的95个字）
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsGBK(string str)
        => MatchCharacterSetOfAll(str, IsGBK);

    private static bool MatchCharacterSetOfAll(string str, Func<char, bool> func)
    {
        bool ret = true;
        foreach (char chr in str)
        {
            if (!func(chr))
            {
                ret = false;
                break;
            }
        }
        return ret;
    }

    /// <summary>
    /// 判断字符是否是ASCII编码
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    public static bool IsASCII(this char chr)
        => chr <= 127;

    /// <summary>
    /// 判断是否是兼容GB2312编码(GB2312-1980)
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    public static bool IsGB2312(this char chr)
    {
        bool ret = true;
        if (IsASCII(chr)) return true;
        byte[] bytes = Encoding.GetEncoding(20936).GetBytes(new char[] { chr });
        if (bytes.Length == 2)
        {
            byte byte1 = bytes[0];
            byte byte2 = bytes[1];
            ret = (byte1 >= 176 && byte1 <= 247 && byte2 >= 160 && byte2 <= 254);
        }
        else
            ret = false;
        return ret;
    }

    /// <summary>
    /// 判断是否兼容GBK编码（不包括比CP936多的95个字）
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    public static bool IsGBK(this char chr)
    {
        bool ret = true;
        if (IsASCII(chr)) return true;
        byte[] bytes = Encoding.GetEncoding(936).GetBytes(new char[] { chr });//实际的GBK比CP936多95个字
        if (bytes.Length == 2)
        {
            byte byte1 = bytes[0];
            byte byte2 = bytes[1];
            ret = (byte1 >= 129 && byte1 <= 254 && byte2 >= 64 && byte2 <= 254);
        }
        else
            ret = false;
        return ret;
    }

    /// <summary>
    /// 判断是否兼容GB18030编码(GB18030-2000 GB18030-2005)
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    public static bool IsGB18030(this char chr)
    {

        bool ret = true;
        if (IsASCII(chr)) return true;
        byte[] bytes = Encoding.GetEncoding(54936).GetBytes(new char[] { chr });
        if (bytes.Length == 2)
        {
            byte byte1 = bytes[0];
            byte byte2 = bytes[1];
            ret = (byte1 >= 0x81 && byte1 <= 0xFE && byte2 >= 0x40 && byte2 <= 0x7E)
                  || (byte1 >= 0x81 && byte1 <= 0xFE && byte2 >= 0x80 && byte2 <= 0xFE);
        }
        else if (bytes.Length == 4)
        {
            byte byte1 = bytes[0];
            byte byte2 = bytes[1];
            byte byte3 = bytes[2];
            byte byte4 = bytes[3];
            ret = (byte1 >= 0x81 && byte1 <= 0xFE)
                  && (byte2 >= 0x30 && byte2 <= 0x39)
                  && (byte3 >= 0x81 && byte3 <= 0xFE)
                  && (byte4 >= 0x30 && byte4 <= 0x39);
        }
        else
            ret = false;
        return ret;

    }

    #endregion
}