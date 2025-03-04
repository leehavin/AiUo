using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AiUo.Security;

/// <summary>
/// JS配合的AES加密
/// </summary>
public static class JsAesUtil
{
    /// <summary>  
    /// AES加密算法  
    /// </summary>  
    /// <param name="input">明文字符串</param>  
    /// <param name="key">密钥（16,24,32位）</param>  
    /// <param name="iv">IV（16位）</param>  
    /// <returns>字符串</returns>  
    public static string Encrypt(string input, string key, string iv = null)
    {
        CheckKey(key, iv);
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = keyBytes;
            aesAlg.IV = !string.IsNullOrEmpty(iv)
                ? Encoding.UTF8.GetBytes(iv)
                : keyBytes;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(input);
                    }
                    byte[] bytes = msEncrypt.ToArray();
                    return ByteArrayToHexString(bytes);
                }
            }
        }
    }

    /// <summary>  
    /// AES解密  
    /// </summary>  
    /// <param name="input">密文字节数组</param>  
    /// <param name="key">密钥（16,24,32位）</param>  
    /// <param name="iv">IV（16位）</param>  
    /// <returns>返回解密后的字符串</returns>  
    public static string Decrypt(string input, string key, string iv = null)
    {
        CheckKey(key, iv);
        byte[] inputBytes = HexStringToByteArray(input);
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = keyBytes;
            aesAlg.IV = !string.IsNullOrEmpty(iv)
                ? Encoding.UTF8.GetBytes(iv)
                : keyBytes;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using (MemoryStream msEncrypt = new MemoryStream(inputBytes))
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srEncrypt = new StreamReader(csEncrypt))
                    {
                        return srEncrypt.ReadToEnd();
                    }
                }
            }
        }
    }

    private static void CheckKey(string key, string iv = null)
    {
        if (key.Length != 16 && key.Length != 24 && key.Length != 32)
            throw new Exception("AES密钥长度必须16,24或32位");
        if (!string.IsNullOrEmpty(iv) && iv.Length != 16)
            throw new Exception("AES IV长度必须16");
    }

    /// <summary>
    /// 将指定的16进制字符串转换为byte数组
    /// </summary>
    /// <param name="s">16进制字符串(如：“7F 2C 4A”或“7F2C4A”都可以)</param>
    /// <returns>16进制字符串对应的byte数组</returns>
    public static byte[] HexStringToByteArray(string s)
    {
        s = s.Replace(" ", "");
        byte[] buffer = new byte[s.Length / 2];
        for (int i = 0; i < s.Length; i += 2)
            buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
        return buffer;
    }

    /// <summary>
    /// 将一个byte数组转换成一个格式化的16进制字符串
    /// </summary>
    /// <param name="data">byte数组</param>
    /// <returns>格式化的16进制字符串</returns>
    public static string ByteArrayToHexString(byte[] data)
    {
        StringBuilder sb = new StringBuilder(data.Length * 3);
        foreach (byte b in data)
        {
            //16进制数字
            sb.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
            //16进制数字之间以空格隔开
            //sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
        }
        return sb.ToString().ToUpper();
    }
}