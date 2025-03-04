using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using AiUo.Logging;
using AiUo.Security;

namespace AiUo.AspNet;

/// <summary>
/// 请求body签名验证器
/// </summary>
public class RequestBodySignValidator
{
    public string PublicKey { get; set; }
    public RSAKeyMode KeyMode { get; set; } = RSAKeyMode.PublicKey;
    public HashAlgorithmName HashName { get; set; } = HashAlgorithmName.SHA256;
    public CipherEncode Cipher { get; set; } = CipherEncode.Base64;
    public Encoding Encoding { get; set; } = Encoding.UTF8;
    public RequestBodySignValidator() { }
    public RequestBodySignValidator(RequestSignFilterElement _element)
    {
        PublicKey = _element.PublicKey;
        KeyMode = _element.KeyMode;
        HashName = string.IsNullOrEmpty(_element.HashName)? HashAlgorithmName.SHA256:new HashAlgorithmName(_element.HashName);
        Cipher = _element.Cipher;
        Encoding = _element.Encoding;
    }
    public RequestBodySignValidator(string publicKey, RSAKeyMode keyMode = RSAKeyMode.PublicKey
        , HashAlgorithmName hashName = default, CipherEncode cipher = CipherEncode.Base64, Encoding encoding = null)
    {
        PublicKey = publicKey;
        KeyMode = keyMode;
        HashName = hashName == default ? HashAlgorithmName.SHA256 : hashName;
        Cipher = cipher;
        Encoding = encoding ?? Encoding.UTF8;
    }
    public async Task<bool> VerifyByHeader(string headerName, HttpContext context = null)
    {
        if (string.IsNullOrEmpty(headerName))
            throw new ArgumentNullException(nameof(headerName));
        if (string.IsNullOrEmpty(PublicKey))
            throw new Exception($"{nameof(RequestBodySignValidator)}验证时PublicKey不能为空");
        context ??= HttpContextEx.Current;

        var logger = LogUtil.GetContextLogger();
        var request = context.Request;

        // header
        if (!request.Headers.TryGetValue(headerName, out var value))
        {
            logger.AddMessage($"{nameof(RequestBodySignValidator)}验证时header不存在: header: {headerName}")
                .SetLevel(Microsoft.Extensions.Logging.LogLevel.Error);
            return false;
        }

        // sign
        var sign = Convert.ToString(value);
        if (string.IsNullOrEmpty(sign))
        {
            logger.AddMessage($"{nameof(RequestBodySignValidator)}验证时header没有值。header: {headerName}")
                .SetLevel(Microsoft.Extensions.Logging.LogLevel.Error);
            return false;
        }

        // source
        string source = await AspNetUtil.GetRequestBodyAsync(context);
        if (string.IsNullOrEmpty(source))
            return true; // 没有requestBody

        // veriry
        var ret = SecurityUtil.RSAVerifyData(source
            , sign
            , PublicKey
            , KeyMode
            , HashName
            , Cipher
            , Encoding
        );
        if (!ret)
        {
            logger.AddMessage($"{nameof(RequestBodySignValidator)}验证失败")
                .SetLevel(Microsoft.Extensions.Logging.LogLevel.Error)
                .AddField($"{nameof(RequestBodySignValidator)}.HeaderName", headerName)
                .AddField($"{nameof(RequestBodySignValidator)}.HeaderSign", sign)
                .AddField($"{nameof(RequestBodySignValidator)}.RequestBody", source)
                .AddField($"{nameof(RequestBodySignValidator)}.PublicKey", PublicKey)
                .AddField($"{nameof(RequestBodySignValidator)}.KeyMode", KeyMode)
                .AddField($"{nameof(RequestBodySignValidator)}.HashName", HashName)
                .AddField($"{nameof(RequestBodySignValidator)}.Cipher", Cipher)
                ;
        }
        return ret;
    }
    public async Task<bool> Verify(string sign, HttpContext context = null)
    {
        if (string.IsNullOrEmpty(PublicKey))
            throw new Exception($"{nameof(RequestBodySignValidator)}验证时PublicKey不能为空");
        context ??= HttpContextEx.Current;

        var logger = LogUtil.GetContextLogger();
        var request = context.Request;
        if (string.IsNullOrEmpty(sign))
        {
            logger.AddMessage($"{nameof(RequestBodySignValidator)}验证时hsign没有值。")
                .SetLevel(Microsoft.Extensions.Logging.LogLevel.Error);
            return false;
        }

        // source
        string source = await AspNetUtil.GetRequestBodyAsync(context);
        if (string.IsNullOrEmpty(source))
            return true; // 没有requestBody

        // veriry
        var ret = SecurityUtil.RSAVerifyData(source
            , sign
            , PublicKey
            , KeyMode
            , HashName
            , Cipher
            , Encoding
        );
        if (!ret)
        {
            logger.AddMessage($"{nameof(RequestBodySignValidator)}验证失败")
                .SetLevel(Microsoft.Extensions.Logging.LogLevel.Error)
                .AddField($"{nameof(RequestBodySignValidator)}.Sign", sign)
                .AddField($"{nameof(RequestBodySignValidator)}.RequestBody", source)
                .AddField($"{nameof(RequestBodySignValidator)}.PublicKey", PublicKey)
                .AddField($"{nameof(RequestBodySignValidator)}.KeyMode", KeyMode)
                .AddField($"{nameof(RequestBodySignValidator)}.HashName", HashName)
                .AddField($"{nameof(RequestBodySignValidator)}.Cipher", Cipher)
                ;
        }
        return ret;
    }
}