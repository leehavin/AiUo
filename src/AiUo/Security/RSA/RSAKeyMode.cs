namespace AiUo.Security;

/// <summary>
/// RSA 秘钥格式
/// </summary>
public enum RSAKeyMode
{
    /// <summary>
    /// 根据key内容自行推断
    /// </summary>
    Auto,
    /// <summary>
    /// 微软生成的Xml格式的秘钥
    /// </summary>
    MSXml,


    /// <summary>
    /// pkcs8 私钥(java默认)
    /// 开头：BEGIN PRIVATE KEY
    /// RSA.ImportPkcs8PrivateKey()
    /// </summary>
    PrivateKey,
    /// <summary>
    /// pkcs8 公钥(java默认)
    /// 开头：BEGIN PUBLIC KEY
    /// RSA.ImportSubjectPublicKeyInfo()
    /// </summary>
    PublicKey,

    /// <summary>
    /// pkcs1 私钥
    /// BEGIN RSA PRIVATE KEY
    /// RSA.ImportRSAPrivateKey()
    /// </summary>
    RSAPrivateKey,
    /// <summary>
    /// pkcs1 公钥
    /// BEGIN RSA PUBLIC KEY
    /// RSA.ImportRSAPublicKey()
    /// </summary>
    RSAPublicKey
}