using System;
using System.Security.Cryptography;
using System.Text;

namespace AiUo.Security;

public static class RSAKeyUtil
{
    public static string ChangePublicKey(string key, RSAKeyType fromType, RSAKeyType toType)
    {
        if (fromType == toType)
            return key;
        using (var rsa = new RSACryptoServiceProvider())
        {
            switch (fromType)
            {
                case RSAKeyType.MSXml:
                    rsa.FromXmlString(key);
                    break;
                case RSAKeyType.Pkcs1:
                    rsa.ImportRSAPublicKey(Encoding.UTF8.GetBytes(key), out _);
                    break;
                case RSAKeyType.Pkcs8:
                    rsa.ImportSubjectPublicKeyInfo(Encoding.UTF8.GetBytes(key), out _);
                    break;
            }
            switch(toType)
            {
                case RSAKeyType.MSXml:
                    return rsa.ToXmlString(false);
                case RSAKeyType.Pkcs1:
                    return Encoding.UTF8.GetString(rsa.ExportRSAPublicKey());
                case RSAKeyType.Pkcs8:
                    return Encoding.UTF8.GetString(rsa.ExportSubjectPublicKeyInfo());
                default:
                    throw new Exception($"未知RSAKeyType: {toType}");
            }
        }
    }

    public static string ChangePrivateKey(string key, RSAKeyType fromType, RSAKeyType toType)
    {
        if (fromType == toType)
            return key;
        using (var rsa = new RSACryptoServiceProvider())
        {
            switch (fromType)
            {
                case RSAKeyType.MSXml:
                    rsa.FromXmlString(key);
                    break;
                case RSAKeyType.Pkcs1:
                    rsa.ImportRSAPrivateKey(Encoding.UTF8.GetBytes(key), out _);
                    break;
                case RSAKeyType.Pkcs8:
                    rsa.ImportPkcs8PrivateKey(Encoding.UTF8.GetBytes(key), out _);
                    break;
            }
            switch (toType)
            {
                case RSAKeyType.MSXml:
                    return rsa.ToXmlString(false);
                case RSAKeyType.Pkcs1:
                    return Encoding.UTF8.GetString(rsa.ExportRSAPrivateKey());
                case RSAKeyType.Pkcs8:
                    return Encoding.UTF8.GetString(rsa.ExportPkcs8PrivateKey());
                default:
                    throw new Exception($"未知RSAKeyType: {toType}");
            }
        }
    }
}
public enum RSAKeyType
{
    MSXml,
    Pkcs1,
    Pkcs8
}