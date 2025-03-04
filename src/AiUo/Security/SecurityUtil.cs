using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AiUo.Security;

/// <summary>
/// 安全辅助类
/// </summary>
public static class SecurityUtil
{
    #region Password处理

    /// <summary>
    /// 获取用户密码Hash所需要的Salt
    /// </summary>
    /// <returns></returns>
    public static string GetPasswordSalt()
    {
        var data = new byte[0x10];
        RandomNumberGenerator.Fill(data);
        return Convert.ToBase64String(data);
    }

    /// <summary>
    /// 使用Salt加密密码
    /// </summary>
    /// <param name="password">密码明文</param>
    /// <param name="salt">加密Salt</param>
    /// <returns></returns>
    public static string EncryptPassword(string password, string salt)
    {
        using (var sha256 = SHA256.Create())
        {
            var saltedPassword = string.Format("{0}{1}", salt, password);
            var saltedPasswordAsBytes = Encoding.UTF8.GetBytes(saltedPassword);
            return Convert.ToBase64String(sha256.ComputeHash(saltedPasswordAsBytes));
        }
    }

    /// <summary>
    /// 验证传入的密码
    /// </summary>
    /// <param name="password">用户密码，一般有用户传入</param>
    /// <param name="passwordHash">hash后的密码，一般存在数据库</param>
    /// <param name="salt">hash所需的salt，一般存储在数据库</param>
    /// <returns></returns>
    public static bool ValidatePassword(string password, string passwordHash, string salt)
    {
        var hash = EncryptPassword(password, salt);
        return string.Equals(hash, passwordHash);
    }
    #endregion

    #region Base64
    /// <summary>
    /// Base64编码
    /// </summary>
    /// <param name="str"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string Base64Encrypt(string str, Encoding encoding = null)
        => Convert.ToBase64String((encoding ?? Encoding.UTF8).GetBytes(str));

    /// <summary>
    /// Base64解码
    /// </summary>
    /// <param name="base64Str"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string Base64Decrypt(string base64Str, Encoding encoding = null)
        => (encoding ?? Encoding.UTF8).GetString(Convert.FromBase64String(base64Str));
    /// <summary>
    /// 将字符串编码为base64 for url格式
    /// </summary>
    /// <param name="str"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string Base64UrlEncrypt(string str, Encoding encoding = null)
        => StringUtil.Base64UrlEncode(str, encoding);
    /// <summary>
    /// 解码base64 for url字符串
    /// </summary>
    /// <param name="base64Str"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string Base64UrlDecrypt(string base64Str, Encoding encoding = null)
        => StringUtil.Base64UrlDecode(base64Str, encoding);
    #endregion

    #region Hash 哈希值

    #region SHA1
    /// <summary>
    /// SHA1 哈希值
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static byte[] SHA1Hash(byte[] src)
        => SHA1.Create().ComputeHash(src);

    /// <summary>
    /// SHA1 哈希值的Base64字符串
    /// </summary>
    /// <param name="src"></param>
    /// <param name="cipherEncode"></param>
    /// <param name="encoding"></param>
    /// <returns>返回Base64字符串</returns>
    public static string SHA1Hash(string src, CipherEncode cipherEncode = CipherEncode.Base64, Encoding encoding = null)
    {
        var bytes = (encoding ?? Encoding.UTF8).GetBytes(src);
        return CipherEncodeString(SHA1.Create().ComputeHash(bytes), cipherEncode);
    }

    /// <summary>
    /// SHA1 哈希值验证
    /// </summary>
    /// <param name="source">原始字符串</param>
    /// <param name="hashValue">需要验证的哈希字符串</param>
    /// <returns></returns>
    public static bool SHA1Verify(byte[] source, byte[] hashValue)
    {
        var hashSource = SHA1Hash(source);
        return ArrayEquals(hashSource, hashValue);
    }

    /// <summary>
    /// SHA1 哈希值验证
    /// </summary>
    /// <param name="source">原始字符串</param>
    /// <param name="hashValue">需要验证的哈希字符串</param>
    /// <param name="ignoreCase">忽略大小写</param>
    /// <param name="cipherEncode"></param>
    /// <param name="encoding">字符集</param>
    /// <returns></returns>
    public static bool SHA1Verify(string source, string hashValue, bool ignoreCase = false, CipherEncode cipherEncode = CipherEncode.Base64, Encoding encoding = null)
    {
        var hashSource = SHA1Hash(source, cipherEncode, encoding);
        var s1 = ignoreCase ? hashSource.ToLower() : hashSource;
        var s2 = ignoreCase ? hashValue.ToLower() : hashValue;
        return string.Equals(s1, s2);
    }
    #endregion

    #region HMACSHA256
    public static byte[] HMACSHA256Hash(byte[] key, byte[] buffer)
    {
        var hmacsha256 = new HMACSHA256(key);
        return hmacsha256.ComputeHash(buffer);
    }
    public static string HMACSHA256Hash(string key, string src, CipherEncode cipherEncode = CipherEncode.Base64, Encoding encoding = null)
    {
        encoding = encoding == null ? Encoding.UTF8 : encoding;
        var k = encoding.GetBytes(key);
        var buffer = encoding.GetBytes(src);
        var data = HMACSHA256Hash(k, buffer);
        return CipherEncodeString(data, cipherEncode);
    }
    public static bool HMACSHA256Verify(byte[] key, byte[] source, byte[] hashValue)
    {
        var hashSource = HMACSHA256Hash(key, source);
        return ArrayEquals(hashSource, hashValue);
    }
    public static bool HMACSHA256Verify(string key, string source, string hashValue, bool ignoreCase = false, CipherEncode cipherEncode = CipherEncode.Base64, Encoding encoding = null)
    {
        var hashSource = HMACSHA256Hash(key, source, cipherEncode, encoding);
        var s1 = ignoreCase ? hashSource.ToLower() : hashSource;
        var s2 = ignoreCase ? hashValue.ToLower() : hashValue;
        return string.Equals(s1, s2);
    }
    #endregion

    #region SHA256
    /// <summary>
    /// SHA256 哈希值
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static byte[] SHA256Hash(byte[] src)
        => SHA256.Create().ComputeHash(src);

    /// <summary>
    /// SHA256 哈希值的Base64字符串
    /// </summary>
    /// <param name="src"></param>
    /// <param name="cipherEncode"></param>
    /// <param name="encoding"></param>
    /// <returns>返回Base64字符串</returns>
    public static string SHA256Hash(string src, CipherEncode cipherEncode = CipherEncode.Base64, Encoding encoding = null)
    {
        var bytes = (encoding ?? Encoding.UTF8).GetBytes(src);
        var data = SHA256.Create().ComputeHash(bytes);
        return CipherEncodeString(data, cipherEncode);
    }
    /// <summary>
    /// SHA256 哈希值验证
    /// </summary>
    /// <param name="source">原始字符串</param>
    /// <param name="hashValue">需要验证的哈希字符串</param>
    /// <returns></returns>
    public static bool SHA256Verify(byte[] source, byte[] hashValue)
    {
        var hashSource = SHA256Hash(source);
        return ArrayEquals(hashSource, hashValue);
    }

    /// <summary>
    /// SHA256 哈希值验证
    /// </summary>
    /// <param name="source">原始字符串</param>
    /// <param name="hashValue">需要验证的哈希字符串</param>
    /// <param name="ignoreCase">忽略大小写</param>
    /// <param name="cipherEncode"></param>
    /// <param name="encoding">字符集</param>
    /// <returns></returns>
    public static bool SHA256Verify(string source, string hashValue, bool ignoreCase = false, CipherEncode cipherEncode = CipherEncode.Base64, Encoding encoding = null)
    {
        var hashSource = SHA256Hash(source, cipherEncode, encoding);
        var s1 = ignoreCase ? hashSource.ToLower() : hashSource;
        var s2 = ignoreCase ? hashValue.ToLower() : hashValue;
        return string.Equals(s1, s2);
    }
    #endregion

    #region MD5
    /// <summary>
    /// MD5 哈希值
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static byte[] MD5Hash(byte[] src)
        => MD5.Create().ComputeHash(src);

    /// <summary>
    /// MD5 哈希值的Base64字符串
    /// </summary>
    /// <param name="src"></param>
    /// <param name="cipherEncode"></param>
    /// <param name="encoding"></param>
    /// <returns>返回Base64字符串</returns>
    public static string MD5Hash(string src, CipherEncode cipherEncode = CipherEncode.Base64, Encoding encoding = null)
    {
        var bytes = (encoding ?? Encoding.UTF8).GetBytes(src);
        var data = MD5.Create().ComputeHash(bytes);
        return CipherEncodeString(data, cipherEncode);
    }

    /// <summary>
    /// MD5 哈希值验证
    /// </summary>
    /// <param name="source">原始字符串</param>
    /// <param name="hashValue">需要验证的哈希字符串</param>
    /// <returns></returns>
    public static bool MD5Verify(byte[] source, byte[] hashValue)
    {
        var hashSource = MD5Hash(source);
        return ArrayEquals(hashSource, hashValue);
    }

    /// <summary>
    /// MD5 哈希值验证
    /// </summary>
    /// <param name="source">原始字符串</param>
    /// <param name="hashValue">需要验证的哈希字符串</param>
    /// <param name="ignoreCase">忽略大小写</param>
    /// <param name="cipherEncode"></param>
    /// <param name="encoding">字符集</param>
    /// <returns></returns>
    public static bool MD5Verify(string source, string hashValue, bool ignoreCase = false, CipherEncode cipherEncode = CipherEncode.Base64, Encoding encoding = null)
    {
        var hashSource = MD5Hash(source, cipherEncode, encoding);
        var s1 = ignoreCase ? hashSource.ToLower() : hashSource;
        var s2 = ignoreCase ? hashValue.ToLower() : hashValue;
        return string.Equals(s1, s2);
    }

    // 哈希值 -- 将任意长度的二进制值映射到较小的固定长度的二进制值, 同样的数据获得的哈希值也相同
    //判断字节数组是否相等
    private static bool ArrayEquals(byte[] a1, byte[] a2)
    {
        if (a1.Length != a2.Length) return false;
        for (int i = 0; i < a1.Length; i++)
        {
            if (a1[i] != a2[i]) return false;
        }
        return true;
    }
    #endregion

    #endregion

    #region Symmetric 对称
    /// <summary>
    /// 构建DES加密类【对称加密】
    /// </summary>
    /// <param name="key">8字节</param>
    /// <param name="mode">CBC</param>
    /// <param name="padding">PKCS7</param>
    /// <param name="cipher">Base64</param>
    /// <param name="encoding">UTF8</param>
    /// <returns></returns>
    public static SymmetricProvider CreateDESProvider(string key, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7, CipherEncode cipher = CipherEncode.Base64, Encoding encoding = null)
    {
        var ret = new SymmetricProvider(SymmetricMode.DES, key, null, encoding);
        ret.Mode = mode;
        ret.Padding = padding;
        ret.CipherEncode = cipher;
        return ret;
    }
    /// <summary>
    /// 构建TripleDES加密类【对称加密】
    /// </summary>
    /// <param name="key">16或24字节</param>
    /// <param name="mode">CBC</param>
    /// <param name="padding">PKCS7</param>
    /// <param name="cipher">Base64</param>
    /// <param name="encoding">UTF8</param>
    /// <returns></returns>
    public static SymmetricProvider CreateTripleDESProvider(string key, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7, CipherEncode cipher = CipherEncode.Base64, Encoding encoding = null)
    {
        var ret = new SymmetricProvider(SymmetricMode.TripleDES, key, null, encoding);
        ret.Mode = mode;
        ret.Padding = padding;
        ret.CipherEncode = cipher;
        return ret;
    }
    /// <summary>
    /// 构建RC2加密类【对称加密】
    /// </summary>
    /// <param name="key">1-128字节</param>
    /// <param name="mode">CBC</param>
    /// <param name="padding">PKCS7</param>
    /// <param name="cipher">Base64</param>
    /// <param name="encoding">UTF8</param>
    /// <returns></returns>
    public static SymmetricProvider CreateRC2Provider(string key, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7, CipherEncode cipher = CipherEncode.Base64, Encoding encoding = null)
    {
        var ret = new SymmetricProvider(SymmetricMode.RC2, key, null, encoding);
        ret.Mode = mode;
        ret.Padding = padding;
        ret.CipherEncode = cipher;
        return ret;
    }
    /// <summary>
    /// 构建Rijndael加密类【对称加密】
    /// </summary>
    /// <param name="key">16或24或32字节</param>
    /// <param name="mode">CBC</param>
    /// <param name="padding">PKCS7</param>
    /// <param name="cipher">Base64</param>
    /// <param name="encoding">UTF8</param>
    /// <returns></returns>
    public static SymmetricProvider CreateRijndaelProvider(string key, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7, CipherEncode cipher = CipherEncode.Base64, Encoding encoding = null)
    {
        var ret = new SymmetricProvider(SymmetricMode.Rijndael, key, null, encoding);
        ret.Mode = mode;
        ret.Padding = padding;
        ret.CipherEncode = cipher;
        return ret;
    }
    /// <summary>
    /// 构建AES加密类【对称加密】
    /// </summary>
    /// <param name="key">16或24或32字节</param>
    /// <param name="mode">CBC</param>
    /// <param name="padding">PKCS7</param>
    /// <param name="cipher">Base64</param>
    /// <param name="encoding">UTF8</param>
    /// <returns></returns>
    public static SymmetricProvider CreateAESProvider(string key, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7, CipherEncode cipher = CipherEncode.Base64, Encoding encoding = null)
    {
        var ret = new SymmetricProvider(SymmetricMode.AES, key, null, encoding);
        ret.Mode = mode;
        ret.Padding = padding;
        ret.CipherEncode = cipher;
        return ret;
    }
    #endregion

    #region Asymmetric 非对称

    #region RSA 公钥加密、私钥解密、私钥签名、公钥验签

    #region 公钥加密 & 私钥解密
    /// <summary>
    /// RSA 公钥加密，使用 PKCS#1 1.5 版填充
    /// </summary>
    /// <param name="clearBytes">明文</param>
    /// <param name="publicKey">公钥(pkcs1,pkcs8,msxml)</param>
    /// <param name="keyMode">指定提供的RSA公钥格式，默认Auto自动推断</param>
    /// <returns></returns>
    public static byte[] RSAEncrypt(byte[] clearBytes, string publicKey, RSAKeyMode keyMode = RSAKeyMode.Auto)
    {
        var rsa = CreateRsaProviderFromPublicKey(publicKey, keyMode);
        return rsa.Encrypt(clearBytes, RSAEncryptionPadding.Pkcs1);
    }
    /// <summary>
    /// RSA 公钥加密，使用 PKCS#1 1.5 版填充。返回base64格式字符串
    /// js可使用jsencrypt.min.js进行openssl加密
    /// </summary>
    /// <param name="clearText">明文</param>
    /// <param name="publicKey">公钥(pkcs1,pkcs8,msxml)</param>
    /// <param name="keyMode">指定提供的RSA公钥格式，默认Auto自动推断</param>
    /// <param name="cipherEncode">指定返回密文的编码格式。默认base64</param>
    /// <param name="encoding">字符集，默认UTF8</param>
    /// <returns></returns>
    public static string RSAEncrypt(string clearText, string publicKey, RSAKeyMode keyMode = RSAKeyMode.Auto, CipherEncode cipherEncode = CipherEncode.Base64, Encoding encoding = null)
    {
        var bytes = (encoding ?? Encoding.UTF8).GetBytes(clearText);
        var data = RSAEncrypt(bytes, publicKey, keyMode);
        return CipherEncodeString(data, cipherEncode);
    }
    /// <summary>
    /// RSA 私钥解密，使用 PKCS#1 1.5 版填充
    /// </summary>
    /// <param name="cipherBytes">密文</param>
    /// <param name="privateKey">私钥(pkcs1,pkcs8,msxml)</param>
    /// <param name="keyMode">指定提供的RSA私钥格式，默认Auto自动推断</param>
    /// <returns></returns>
    public static byte[] RSADecrypt(byte[] cipherBytes, string privateKey, RSAKeyMode keyMode = RSAKeyMode.Auto)
    {
        var rsa = CreateRsaProviderFromPrivateKey(privateKey, keyMode);
        return rsa.Decrypt(cipherBytes, RSAEncryptionPadding.Pkcs1);
    }
    /// <summary>
    /// RSA 私钥解密，使用 PKCS#1 1.5 版填充
    /// </summary>
    /// <param name="cipherText">密文(base64格式)</param>
    /// <param name="privateKey">私钥(pkcs1,pkcs8,msxml)</param>
    /// <param name="keyMode">指定提供的RSA私钥格式，默认Auto自动推断</param>
    /// <param name="cipherEncode">指定输入密文的编码格式。默认base64</param>
    /// <param name="encoding">字符集，默认UTF8</param>
    /// <returns></returns>
    public static string RSADecrypt(string cipherText, string privateKey, RSAKeyMode keyMode = RSAKeyMode.Auto, CipherEncode cipherEncode = CipherEncode.Base64, Encoding encoding = null)
    {
        byte[] bytes = CipherDecodeString(cipherText, cipherEncode);
        var ret = RSADecrypt(bytes, privateKey, keyMode);
        return (encoding ?? Encoding.UTF8).GetString(ret);
    }
    #endregion

    #region 私钥加密 & 公钥解密
    private const int MAX_ENCRYPT_BLOCK = 245;
    private const int MAX_DECRYPT_BLOCK = 128;

    /// <summary>
    /// RSA 私钥加密，使用 PKCS#1 1.5 版填充
    /// </summary>
    /// <param name="clearBytes">明文</param>
    /// <param name="privateKey">私钥(pkcs1,pkcs8,msxml)</param>
    /// <param name="keyMode">指定提供的RSA私钥格式，默认Auto自动推断</param>
    /// <returns></returns>
    public static byte[] RSAEncryptUsePrivateKey(byte[] clearBytes, string privateKey, RSAKeyMode keyMode = RSAKeyMode.PrivateKey)
    {
        var rsa = CreateRsaProviderFromPrivateKey(privateKey, keyMode);
        var keyPair = DotNetUtilities.GetKeyPair(rsa);
        //使用RSA/ECB/PKCS1Padding格式
        var cipher = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");
        //第一个参数为true表示加密，为false表示解密；第二个参数表示密钥
        cipher.Init(true, keyPair.Private);

        byte[] cache;
        int time = 0;//次数
        int inputLen = clearBytes.Length;
        int offset = 0;

        using (MemoryStream stream = new MemoryStream())
        {
            while (inputLen - offset > 0)
            {
                if (inputLen - offset > MAX_ENCRYPT_BLOCK)
                {
                    cache = cipher.DoFinal(clearBytes, offset, MAX_ENCRYPT_BLOCK);
                }
                else
                {
                    cache = cipher.DoFinal(clearBytes, offset, inputLen - offset);
                }
                //写入
                stream.Write(cache, 0, cache.Length);

                time++;
                offset = time * MAX_ENCRYPT_BLOCK;
            }
            return stream.ToArray();
        }
    }

    /// <summary>
    /// RSA 私钥加密，使用 PKCS#1 1.5 版填充
    /// </summary>
    /// <param name="clearText">明文</param>
    /// <param name="privateKey">私钥(pkcs1,pkcs8,msxml)</param>
    /// <param name="keyMode">指定提供的RSA私钥格式，默认Auto自动推断</param>
    /// <param name="cipherEncode">指定输入密文的编码格式。默认base64</param>
    /// <param name="encoding">字符集，默认UTF8</param>
    /// <returns></returns>
    public static string RSAEncryptUsePrivateKey(string clearText, string privateKey, RSAKeyMode keyMode = RSAKeyMode.PrivateKey, CipherEncode cipherEncode = CipherEncode.Base64, Encoding encoding = null)
    {
        var bytes = (encoding ?? Encoding.UTF8).GetBytes(clearText);
        var data = RSAEncryptUsePrivateKey(bytes, privateKey, keyMode);
        return CipherEncodeString(data, cipherEncode);
    }

    /// <summary>
    /// RSA 公钥解密，使用 PKCS#1 1.5 版填充
    /// </summary>
    /// <param name="cipherBytes">密文</param>
    /// <param name="publicKey">仅pkcs8公钥(-----BEGIN PUBLIC KEY-----)</param>
    /// <returns></returns>
    public static byte[] RSADecryptUsePublicKey(byte[] cipherBytes, string publicKey)
    {
        var keyParameter = PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
        var engine = new Pkcs1Encoding(new RsaEngine());
        engine.Init(false, keyParameter);
        int inputLen = cipherBytes.Length;
        using (var ms = new MemoryStream())
        {
            int offSet = 0;
            byte[] cache;
            int i = 0;
            // 对数据分段加密
            while (inputLen - offSet > 0)
            {
                if (inputLen - offSet > MAX_DECRYPT_BLOCK)
                {
                    cache = engine.ProcessBlock(cipherBytes, offSet, MAX_DECRYPT_BLOCK);
                }
                else
                {
                    cache = engine.ProcessBlock(cipherBytes, offSet, inputLen - offSet);
                }
                ms.Write(cache, 0, cache.Length);
                i++;
                offSet = i * MAX_DECRYPT_BLOCK;
            }
            return ms.ToArray();
        }
    }
    /// <summary>
    /// RSA 公钥解密，使用 PKCS#1 1.5 版填充
    /// </summary>
    /// <param name="cipherText">密文</param>
    /// <param name="publicKey">仅pkcs8公钥(-----BEGIN PUBLIC KEY-----)</param>
    /// <param name="cipherEncode">指定输入密文的编码格式。默认base64</param>
    /// <param name="encoding">字符集，默认UTF8</param>
    /// <returns></returns>
    public static string RSADecryptUsePublicKey(string cipherText, string publicKey, CipherEncode cipherEncode = CipherEncode.Base64, Encoding encoding = null)
    {
        byte[] bytes = CipherDecodeString(cipherText, cipherEncode);
        var ret = RSADecryptUsePublicKey(bytes, publicKey);
        return (encoding ?? Encoding.UTF8).GetString(ret);
    }
    #endregion

    #region 私钥签名 & 公钥验签
    /// <summary>
    /// RSA 使用指定的哈希算法计算指定字节数组的哈希值，并对计算所得的哈希值签名。
    /// 默认使用的哈希算法：SHA256 填充方式：PKCS1 编码：Base64
    /// </summary>
    /// <param name="source">原文</param>
    /// <param name="privateKey">私钥(pkcs1,pkcs8,msxml)。例如 openssl genrsa -out private.pem 2048 生成的密钥</param>
    /// <param name="keyMode">指定提供的RSA私钥格式，默认Auto自动推断</param>
    /// <param name="hashName">用于创建哈希值的哈希算法， 默认：HashAlgorithmName.SHA256</param>
    /// <param name="cipherEncode">编码格式处理，默认Base64</param>
    /// <param name="encoding">字符集,默认UTF8</param>
    /// <returns></returns>
    public static string RSASignData(string source, string privateKey, RSAKeyMode keyMode = RSAKeyMode.Auto, HashAlgorithmName hashName = default, CipherEncode cipherEncode = CipherEncode.Base64, Encoding encoding = null)
    {
        var rsa = CreateRsaProviderFromPrivateKey(privateKey, keyMode);
        encoding = encoding ?? Encoding.UTF8;
        hashName = (hashName == default) ? HashAlgorithmName.SHA256 : hashName;
        var sourceBytes = encoding.GetBytes(source);
        var bytes = rsa.SignData(sourceBytes, hashName, RSASignaturePadding.Pkcs1);
        return CipherEncodeString(bytes, cipherEncode);
    }

    /// <summary>
    /// RSA 使用公钥确定签名中的哈希值并将其与所提供数据的哈希值进行比较验证数字签名是否有效。
    /// 默认使用的哈希算法：SHA256 填充方式：PKCS1 编码：Base64
    /// </summary>
    /// <param name="source">原文</param>
    /// <param name="sign">签名</param>
    /// <param name="publicKey">公钥(pkcs1,pkcs8,msxml)。例如：openssl rsa -in private.pem -pubout > public.pub </param>
    /// <param name="keyMode">指定提供的RSA公钥格式，默认Auto自动推断</param>
    /// <param name="hashName">用于创建哈希值的哈希算法，HashAlgorithmName.SHA256</param>
    /// <param name="cipherEncode">编码格式处理，默认Base64</param>
    /// <param name="encoding">字符集,默认UTF8</param>
    /// <returns></returns>
    public static bool RSAVerifyData(string source, string sign, string publicKey, RSAKeyMode keyMode = RSAKeyMode.Auto, HashAlgorithmName hashName = default, CipherEncode cipherEncode = CipherEncode.Base64, Encoding encoding = null)
    {
        var rsa = CreateRsaProviderFromPublicKey(publicKey, keyMode);
        encoding = encoding ?? Encoding.UTF8;
        hashName = (hashName == default) ? HashAlgorithmName.SHA256 : hashName;
        var sourceBytes = encoding.GetBytes(source);
        var signBytes = CipherDecodeString(sign, cipherEncode);
        return rsa.VerifyData(sourceBytes, signBytes, hashName, RSASignaturePadding.Pkcs1);
    }
    #endregion

    #region Utils
    private static RSA CreateRsaProviderFromPrivateKey(string privateKey, RSAKeyMode keyMode)
    {
        var ret = RSA.Create();
        var keys = CheckAndRepairRsaKey(privateKey, keyMode);
        switch (keys.mode)
        {
            case RSAKeyMode.MSXml:
                ret.FromXmlString(keys.key);
                break;
            case RSAKeyMode.PrivateKey:
                var key1 = Convert.FromBase64String(keys.key);
                ret.ImportPkcs8PrivateKey(key1, out int _);
                break;
            case RSAKeyMode.RSAPrivateKey:
                var key2 = Convert.FromBase64String(keys.key);
                ret.ImportRSAPrivateKey(key2, out _);
                break;
        }
        return ret;
    }
    private static RSA CreateRsaProviderFromPublicKey(string publicKey, RSAKeyMode keyMode)
    {
        var ret = RSA.Create();
        var keys = CheckAndRepairRsaKey(publicKey, keyMode);
        switch (keys.mode)
        {
            case RSAKeyMode.MSXml:
                ret.FromXmlString(keys.key);
                break;
            case RSAKeyMode.PublicKey:
                var key1 = Convert.FromBase64String(keys.key);
                ret.ImportPkcs8PublicKey(key1);
                break;
            case RSAKeyMode.RSAPublicKey:
                var key2 = Convert.FromBase64String(keys.key);
                ret.ImportRSAPublicKey(key2, out _);
                break;
        }
        return ret;
    }
    private static ConcurrentDictionary<string, (string key, RSAKeyMode mode)> _rsaKeyCache = new ConcurrentDictionary<string, (string key, RSAKeyMode mode)>();
    private static (string key, RSAKeyMode mode) CheckAndRepairRsaKey(string key, RSAKeyMode mode)
    {
        if (_rsaKeyCache.TryGetValue(key, out (string key, RSAKeyMode mode) ret))
            return ret;
        var tmpkey = key.Trim();

        // MSXml
        if (tmpkey.Contains("<RSAKeyValue>"))
            return _returnValue(key, tmpkey, RSAKeyMode.MSXml);

        tmpkey = tmpkey.Replace("\r", "").Replace("\n", "");
        // pkcs8 私钥
        if (tmpkey.Contains("-----BEGIN PRIVATE KEY-----"))
        {
            tmpkey = tmpkey.Replace("-----BEGIN PRIVATE KEY-----", "")
                .Replace("-----END PRIVATE KEY-----", "");
            return _returnValue(key, tmpkey, RSAKeyMode.PrivateKey);
        }

        // pkcs8 公钥
        if (tmpkey.Contains("-----BEGIN PUBLIC KEY-----"))
        {
            tmpkey = tmpkey.Replace("-----BEGIN PUBLIC KEY-----", "")
                .Replace("-----END PUBLIC KEY-----", "");
            return _returnValue(key, tmpkey, RSAKeyMode.PublicKey);
        }

        // pkcs1 私钥
        if (tmpkey.Contains("-----BEGIN RSA PRIVATE KEY-----"))
        {
            tmpkey = tmpkey.Replace("-----BEGIN RSA PRIVATE KEY-----", "")
                .Replace("-----END RSA PRIVATE KEY-----", "");
            return _returnValue(key, tmpkey, RSAKeyMode.RSAPrivateKey);
        }

        // pkcs1 公钥
        if (tmpkey.Contains("-----BEGIN RSA PUBLIC KEY-----"))
        {
            tmpkey = tmpkey.Replace("-----BEGIN RSA PUBLIC KEY-----", "")
                .Replace("-----END RSA PUBLIC KEY-----", "");
            return _returnValue(key, tmpkey, RSAKeyMode.RSAPublicKey);
        }
        if (mode == RSAKeyMode.Auto)
            throw new Exception("推断rsakey格式时无有效信息");
        return _returnValue(key, tmpkey, mode);

        (string key, RSAKeyMode mode) _returnValue(string srcKey, string destKey, RSAKeyMode destMode)
        {
            var destRet = (destKey, destMode);
            _rsaKeyCache.TryAdd(srcKey, destRet);
            return destRet;
        }
    }
    public static void ImportPkcs8PublicKey(this RSA rsa, byte[] publicKey)
    {
        RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(publicKey);
        var pub = new RSAParameters
        {
            Modulus = publicKeyParam.Modulus.ToByteArrayUnsigned(),
            Exponent = publicKeyParam.Exponent.ToByteArrayUnsigned()
        };
        rsa.ImportParameters(pub);
    }
    #endregion

    #endregion // RSA

    #endregion // Asymmetric

    #region Methods
    // 加密后字符串处理
    internal static string CipherEncodeString(byte[] data, CipherEncode cipherEncode)
    {
        string ret = null;
        switch (cipherEncode)
        {
            case CipherEncode.Base64:
                ret = Convert.ToBase64String(data);
                break;
            case CipherEncode.Base64Url:
                ret = StringUtil.Base64UrlEncodeBytes(data);
                break;
            case CipherEncode.Hex:
                ret = StringUtil.BytesToHex(data);
                break;
            case CipherEncode.Bit:
                ret = BitConverter.ToString(data).Replace("-", "");
                break;
            case CipherEncode.Bit16Lower:
                ret = BitConverter.ToString(data, 4, 8).Replace("-", "").ToLower();
                break;
            case CipherEncode.Bit16Upper:
                ret = BitConverter.ToString(data, 4, 8).Replace("-", "").ToUpper();
                break;
            case CipherEncode.Bit32Lower:
                for (int i = 0; i < data.Length; i++)
                    ret += data[i].ToString("x2");
                break;
            case CipherEncode.Bit32Upper:
                for (int i = 0; i < data.Length; i++)
                    ret += data[i].ToString("X2");
                break;
        }
        return ret;
    }
    // 解密后字符串处理
    internal static byte[] CipherDecodeString(string cipherText, CipherEncode cipherEncode)
    {
        byte[] ret = null;
        switch (cipherEncode)
        {
            case CipherEncode.Base64:
                ret = Convert.FromBase64String(cipherText);
                break;
            case CipherEncode.Base64Url:
                ret = StringUtil.Base64UrlDecodeBytes(cipherText);
                break;
            case CipherEncode.Hex:
                ret = StringUtil.HexToBytes(cipherText);
                break;
            case CipherEncode.Bit:
                if (cipherText.Contains('-'))
                {
                    ret = cipherText.Split('-').Select(b => Convert.ToByte(b, 16)).ToArray();
                }
                else
                {
                    ret = new byte[cipherText.Length / 2];
                    for (int i = 0; i < cipherText.Length / 2; i++)
                    {
                        ret[i] = Convert.ToByte(cipherText.Substring(i * 2, 2), 16);
                    }
                }
                break;
            //case CipherEncode.Bit16Lower:
            //    ret = BitConverter.ToString(data, 4, 8).Replace("-", "").ToLower();
            //    break;
            //case CipherEncode.Bit16Upper:
            //    ret = BitConverter.ToString(data, 4, 8).Replace("-", "").ToUpper();
            //    break;
            //case CipherEncode.Bit32Lower:
            //    for (int i = 0; i < data.Length; i++)
            //        ret += data[i].ToString("x2");
            //    break;
            //case CipherEncode.Bit32Upper:
            //    for (int i = 0; i < data.Length; i++)
            //        ret += data[i].ToString("X2");
            //    break;
        }
        return ret;
    }
    #endregion
}


/// <summary>
/// 密文的编码格式
/// </summary>
public enum CipherEncode
{
    /// <summary>
    /// 密文采用Base64编码
    /// </summary>
    Base64,
    /// <summary>
    /// 可用于url传输的Base64编码
    /// </summary>
    Base64Url,
    /// <summary>
    /// 密文采用16进制字符串处理
    /// </summary>
    Hex,
    /// <summary>
    /// BitConverter.ToString(bytes).Replace("-", "")
    /// </summary>
    Bit,
    /// <summary>
    /// 16位大写
    /// </summary>
    Bit16Upper,
    /// <summary>
    /// 16位小写
    /// </summary>
    Bit16Lower,
    /// <summary>
    /// 32位大写
    /// </summary>
    Bit32Upper,
    /// <summary>
    /// 32位小写
    /// </summary>
    Bit32Lower,
}