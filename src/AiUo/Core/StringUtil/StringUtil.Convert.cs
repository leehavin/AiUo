using System;
using System.Text;

namespace AiUo;

/// <summary>
/// 字符串操作静态辅助类
/// </summary>
public static partial class StringUtil
{
    #region 字符串和16进制字符串表示互转
    /// <summary>
    /// 将字符串转换成为16进制字符串
    /// </summary>
    /// <param name="str">要转换成16进制表示的字符串</param>
    /// <param name="encode">字符编码</param>
    /// <returns></returns>
    public static string StrToHex(this string str, Encoding encode = null)
    {
        byte[] bytes = (encode ?? Encoding.UTF8).GetBytes(str);
        return BitConverter.ToString(bytes).Replace("-", "");
    }
        
    /// <summary>
    /// 字节数组转换成为16进制字符串
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string BytesToHex(byte[] input)
        => BitConverter.ToString(input).Replace("-", "");

    /// <summary>
    /// 将16进制字符串转换成为字节数组，如果要将byte[]转换成hex字符串，可使用BitConverter.ToString()实现。
    /// </summary>
    /// <param name="hex">要转换成字节数组的16进制字符串</param>
    /// <returns></returns>
    public static byte[] HexToBytes(this string hex)
    {
        if (hex.Length == 0)
            return new byte[] { 0 };
        if (hex.Length % 2 == 1)
            hex = "0" + hex;
        byte[] ret = new byte[hex.Length / 2];
        for (int i = 0; i < ret.Length; i++)
            ret[i] = byte.Parse(hex.Substring(2 * i, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
        return ret;
    }

    /// <summary>
    /// 将16进制字符串转换成为字符编码对应的字符串
    /// </summary>
    /// <param name="hex">16进制表示的字符串</param>
    /// <param name="encode">字符编码</param>
    /// <returns></returns>
    public static string HexToStr(this string hex, Encoding encode = null)
    {
        encode = encode ?? Encoding.UTF8;
        return encode.GetString(HexToBytes(hex));
    }
    #endregion

    #region Base64 for Url
    /// <summary>
    /// 将字符串编码为base64 for url格式
    /// </summary>
    /// <param name="str"></param>
    /// <param name="encode"></param>
    /// <returns></returns>
    public static string Base64UrlEncode(this string str, Encoding encode = null)
    {
        byte[] bytes = (encode ?? Encoding.UTF8).GetBytes(str);
        return Base64UrlEncodeBytes(bytes);
    }
    /// <summary>
    /// 将字符串字节数组编码为base64 for url格式
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string Base64UrlEncodeBytes(byte[] bytes)
    {
        var ret = Convert.ToBase64String(bytes);
        ret = ret.Split('=')[0];
        ret = ret.Replace('+', '-');
        ret = ret.Replace('/', '_');
        return ret;
    }
    /// <summary>
    /// 将base64 for url格式字符串解码为原始字符串
    /// </summary>
    /// <param name="base64Str"></param>
    /// <param name="encode"></param>
    /// <returns></returns>
    public static string Base64UrlDecode(this string base64Str, Encoding encode = null)
    {
        var bytes = Base64UrlDecodeBytes(base64Str);
        return (encode ?? Encoding.UTF8).GetString(bytes);
    }
    /// <summary>
    /// 将base64 for url格式字符串解码为原始字符串字节数组
    /// </summary>
    /// <param name="base64Str"></param>
    /// <returns></returns>
    public static byte[] Base64UrlDecodeBytes(string base64Str)
    {
        string str = base64Str;
        str = str.Replace('-', '+');
        str = str.Replace('_', '/');
        switch (str.Length % 4)
        {
            case 0: break;
            case 2: str += "=="; break;
            case 3: str += "="; break;
            default:
                throw new Exception("无效的 base64url 字符串!");
        }
        return Convert.FromBase64String(str);
    }
    #endregion

    #region Base64转换
    /// <summary>
    /// 将字符串转换成Base64编码
    /// </summary>
    /// <param name="str"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string ToBase64String(string str, Encoding encoding = null)
        => Convert.ToBase64String((encoding ?? Encoding.UTF8).GetBytes(str));
    /// <summary>
    /// 将Base64编码的字符串还原
    /// </summary>
    /// <param name="str"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string FromBase64String(string str, Encoding encoding = null)
        => (encoding ?? Encoding.UTF8).GetString(Convert.FromBase64String(str));
    #endregion
}