using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AiUo.Security;

/// <summary>
/// 对称加密基类
/// 支持DES, Rijndael, RC2, TripleDES, AES
/// </summary>
public class SymmetricProvider
{
    #region Properties
    //private SymmetricAlgorithm _provider;
    //private ICryptoTransform _encryptor;     // 加密器对象
    //private ICryptoTransform _decryptor;     // 解密器对象
    //private const int BufferSize = 1024;

    /// <summary>
    /// 对称加密类型
    /// </summary>
    public SymmetricMode Symmetric { get; private set; }
    /// <summary>
    /// 获取或设置运算模式
    /// </summary>
    public CipherMode Mode { get; set; } = CipherMode.CBC;
    /// <summary>
    /// 获取或设置填充模式
    /// </summary>
    public PaddingMode Padding { get; set; } = PaddingMode.PKCS7;

    /// <summary>
    /// 获取或设置密钥
    /// </summary>
    public byte[] Key { get; set; }
    /// <summary>
    /// 获取或设置初始化向量
    /// </summary>
    public byte[] IV { get; set; }
    /// <summary>
    /// 获取或设置字符集
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;
    /// <summary>
    /// 密文的编码格式
    /// </summary>
    public CipherEncode CipherEncode { get; set; } = CipherEncode.Base64;
    #endregion

    #region Constructors
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="symmetric">加密方式</param>
    /// <param name="key">秘钥</param>
    /// <param name="iv">IV</param>
    /// <param name="encoding">字符集</param>
    public SymmetricProvider(SymmetricMode symmetric, string key, byte[] iv = null, Encoding encoding = null)
        : this(symmetric, (encoding ?? Encoding.UTF8).GetBytes(key), iv)
    {
        Encoding = encoding ?? Encoding.UTF8;
    }
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="symmetric">加密方式</param>
    /// <param name="key">秘钥</param>
    /// <param name="iv">IV</param>
    public SymmetricProvider(SymmetricMode symmetric, byte[] key, byte[] iv = null)
    {
        switch (symmetric)
        {
            case SymmetricMode.DES:
                if (key.Length != 8)
                    throw new ArgumentOutOfRangeException(nameof(key), "密钥长度错误，DES算法密钥长度为8字节。");
                break;
            case SymmetricMode.RC2:
                if (key.Length < 1 || key.Length > 128)
                    throw new ArgumentOutOfRangeException(nameof(key), "密钥长度错误，RC2算法密钥长度为1-128字节。");
                break;
            case SymmetricMode.Rijndael:
                if (key.Length != 16 && key.Length != 24 && key.Length != 32)
                    throw new ArgumentOutOfRangeException(nameof(key), "密钥长度错误，Rijndael算法密钥长度为16字节，24字节或者32字节。");
                break;
            case SymmetricMode.TripleDES:
                if (key.Length != 16 && key.Length != 24)
                    throw new ArgumentOutOfRangeException(nameof(key), "密钥长度错误，TripleDES算法密钥长度为16字节或24字节。");
                break;
            case SymmetricMode.AES:
                if (key.Length != 16 && key.Length != 24 && key.Length != 32)
                    throw new ArgumentOutOfRangeException(nameof(key), "密钥长度错误，AES算法密钥长度为16字节或24字节或32字节。");
                break;
            default:
                throw new Exception("对称加密不支持此类型：" + symmetric.ToString());
        }
        Symmetric = symmetric;
        Key = key;
        if (iv != null)
            IV = iv;
        else
            IV = (symmetric == SymmetricMode.Rijndael) ? new byte[] { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF }
                : new byte[] { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
    }
    #endregion

    #region Utils
    private SymmetricAlgorithm GetProvider()
    {
        var ret = SymmetricAlgorithm.Create(Symmetric.ToString());
        ret.Key = Key;
        if (IV != null)
            ret.IV = IV;
        ret.Mode = Mode;
        ret.Padding = Padding;
        return ret;
    }
    private ICryptoTransform GetEncryptor()
        => GetProvider().CreateEncryptor();
    private ICryptoTransform GetDecryptor()
        => GetProvider().CreateDecryptor();
    #endregion

    #region 加密解密

    /// <summary>
    /// 加密字节数组
    /// </summary>
    /// <param name="clearBytes">字节数组</param>
    /// <returns></returns>
    public byte[] Encrypt(byte[] clearBytes)
    {
        using (var input = new MemoryStream(clearBytes))
        using (var output = new MemoryStream())
        using (var cryptStream = new CryptoStream(output, GetEncryptor(), CryptoStreamMode.Write))
        {
            var buffer = new byte[1024];
            var read = input.Read(buffer, 0, buffer.Length);
            while (read > 0)
            {
                cryptStream.Write(buffer, 0, read);
                read = input.Read(buffer, 0, buffer.Length);
            }
            cryptStream.FlushFinalBlock();
            return output.ToArray();
        }
    }


    /// <summary>
    /// 加密字符串
    /// </summary>
    /// <param name="clearText">明文</param>
    /// <returns></returns>
    public string Encrypt(string clearText)
    {
        byte[] cipher = Encrypt(Encoding.GetBytes(clearText));
        return SecurityUtil.CipherEncodeString(cipher, CipherEncode);
    }

    /// <summary>
    /// 解密字节数组
    /// </summary>
    /// <param name="cipherBytes">密文字节数组</param>
    /// <returns></returns>
    public byte[] Decrypt(byte[] cipherBytes)
    {
        using (var input = new MemoryStream(cipherBytes))
        using (var output = new MemoryStream())
        using (var cryptStream = new CryptoStream(input, GetDecryptor(), CryptoStreamMode.Read))
        {
            var buffer = new byte[1024];
            var read = cryptStream.Read(buffer, 0, buffer.Length);
            while (read > 0)
            {
                output.Write(buffer, 0, read);
                read = cryptStream.Read(buffer, 0, buffer.Length);
            }
            cryptStream.Flush();
            return output.ToArray();
        }
    }

    /// <summary>
    /// 解密字符串
    /// </summary>
    /// <param name="cipherText">密文</param>
    /// <returns></returns>
    public string Decrypt(string cipherText)
    {
        byte[] cipher = SecurityUtil.CipherDecodeString(cipherText, CipherEncode);
        return Encoding.GetString(Decrypt(cipher));
    }

    #endregion
}