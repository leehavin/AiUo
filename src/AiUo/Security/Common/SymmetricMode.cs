namespace AiUo.Security;

/// <summary>
/// 对称加密类型
/// </summary>
public enum SymmetricMode
{
    /// <summary>
    /// DES（密钥长度8byte）
    /// </summary>
    DES,

    /// <summary>
    ///  TripleDES（密钥长度16byte或24Byte）
    /// </summary>
    TripleDES,

    /// <summary>
    ///  RC2（密钥长度5byte-16byte）
    /// </summary>
    RC2,

    /// <summary>
    /// Rijndael（密钥长度16byte，24byte或者32byte）
    /// </summary>
    Rijndael,
    /// <summary>
    /// AES算法（密钥长度128byte，192byte或者256byte）
    /// </summary>
    AES
}