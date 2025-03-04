using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using AiUo.Configuration;
using AiUo.Logging;
using AiUo.Net;
using AiUo.Randoms;
using AiUo.Security;
using AiUo.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AiUo.AspNet;

/// <summary>
/// 自有客户端访问API时的sign验证器服务
/// sourceKey: 生成密钥时的混入值
/// bothKey: 客户端和服务器根据相同的sourceKey和自定义算法获得的密钥(用于签名)
/// accessKey: 服务器返回给客户端的签名密钥
/// </summary>
public class ClientSignFilterService
{
    public const string DEFAULT_HEADER_NAME = "tinyfx-sign";
    private static TimeSpan _expireSpan = TimeSpan.FromMinutes(20.0);
    private MemoryCache _bothKeyCache = new MemoryCache((IOptions<MemoryCacheOptions>)new MemoryCacheOptions());
    private MemoryCache _accessKeyCache = new MemoryCache((IOptions<MemoryCacheOptions>)new MemoryCacheOptions());

    private ClientSignFilterElement _element { get; set; }

    public bool Enabled { get; set; }

    public string HeaderName { get; set; }

    public string BothKeySeed { get; private set; } = "hNMmcYykGdCluYqe";

    public string AccessKeySeed { get; private set; } = "hNMmcYykGdCluYqe";

    public int[] KeyIndexArray { get; private set; } = new int[16]
    {
        7,
        1,
        4,
        15,
        5,
        2,
        0,
        8,
        13,
        14,
        9,
        12,
        11,
        10,
        6,
        3
    };

    public ClientSignFilterService(ClientSignFilterElement element)
    {
        this._element = element;
        this.Enabled = element.Enabled;
        this.HeaderName = element.HeaderName ?? "tinyfx-sign";
        if (!string.IsNullOrEmpty(element.BothKeySeed))
            this.BothKeySeed = element.BothKeySeed;
        if (!string.IsNullOrEmpty(element.AccessKeySeed))
            this.AccessKeySeed = element.AccessKeySeed;
        if (string.IsNullOrEmpty(element.KeyIndexes))
            return;
        this.KeyIndexArray =
            ((IEnumerable<string>)element.KeyIndexes.Split(',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Select<string, int>((Func<string, int>)(x => x.Trim().ToInt32())).ToArray<int>();
    }

    /// <summary>获取BothKey</summary>
    /// <param name="sourceBothKey"></param>
    /// <returns></returns>
    public string GetBothKey(string sourceBothKey)
    {
        return ClientSignFilterService.GetKey(this.BothKeySeed, this.KeyIndexArray, sourceBothKey);
    }

    public byte[] GetCacheBothKey(string sourceBothKey)
    {
        return this._bothKeyCache.GetOrCreate<byte[]>((object)sourceBothKey, (Func<ICacheEntry, byte[]>)(entry =>
        {
            entry.SetAbsoluteExpiration(ClientSignFilterService._expireSpan);
            return Encoding.UTF8.GetBytes(this.GetBothKey(sourceBothKey));
        }));
    }

    /// <summary>通过BothKey验证请求数据</summary>
    /// <param name="data"></param>
    /// <param name="sign"></param>
    /// <param name="sourceBothKey"></param>
    /// <returns></returns>
    public bool VerifyByBothKey(string data, string sign, string sourceBothKey)
    {
        if (!this.Enabled)
            return true;
        AiUoUtil.ThrowIfNullOrEmpty(this.GetType().Name + ".VerifyByBothKey时,sourceBothKey,data,sign不能为空",
            sourceBothKey, data, sign);
        return Convert.ToBase64String(
            new HMACSHA256(this.GetCacheBothKey(sourceBothKey)).ComputeHash(Encoding.UTF8.GetBytes(data))) == sign;
    }

    /// <summary>获取AccessKey</summary>
    /// <param name="sourceAccessKey"></param>
    /// <returns></returns>
    public string GetAccessKey(string sourceAccessKey)
    {
        return ClientSignFilterService.GetKey(this.AccessKeySeed, this.KeyIndexArray, sourceAccessKey);
    }

    public byte[] GetCacheAccessKey(string sourceAccessKey)
    {
        return this._accessKeyCache.GetOrCreate<byte[]>((object)sourceAccessKey, (Func<ICacheEntry, byte[]>)(entry =>
        {
            entry.SetAbsoluteExpiration(ClientSignFilterService._expireSpan);
            return Encoding.UTF8.GetBytes(this.GetAccessKey(sourceAccessKey));
        }));
    }

    /// <summary>获取加密以后的AccessKey(使用BothKey加密)</summary>
    /// <param name="sourceBothKey"></param>
    /// <param name="sourceAccessKey"></param>
    /// <returns></returns>
    public string GetEncryptedAccessKey(string sourceBothKey, string sourceAccessKey = null)
    {
        if (sourceAccessKey == null)
            sourceAccessKey = sourceBothKey;
        string bothKey = this.GetBothKey(sourceBothKey);
        return JsAesUtil.Encrypt(this.GetAccessKey(sourceAccessKey), bothKey);
    }

    /// <summary>通过accessKey验证数据</summary>
    /// <param name="data"></param>
    /// <param name="sign"></param>
    /// <param name="sourceAccessKey"></param>
    /// <returns></returns>
    public bool VerifyByAccessKey(string data, string sign, string sourceAccessKey)
    {
        if (!this.Enabled)
            return true;
        AiUoUtil.ThrowIfNullOrEmpty(this.GetType().Name + ".VerifyByAccessKey时,sourceAccessKey,data,sign不能为空",
            sourceAccessKey, data, sign);
        return Convert.ToBase64String(
            new HMACSHA256(this.GetCacheAccessKey(sourceAccessKey)).ComputeHash(Encoding.UTF8.GetBytes(data))) == sign;
    }

    private static string GetKey(string seed, int[] indexes, string sourceKey)
    {
        if (seed.Length < indexes.Length)
            throw new Exception("ClientSignFilterService.GetKey()时,约定的seed长度必须大于等于indexes长度");
        int length = indexes.Length;
        int num1 = sourceKey.Length % length;
        sourceKey += seed.Substring(0, length - num1);
        int num2 = sourceKey.Length / length;
        string key = string.Empty;
        for (int index = 0; index < indexes.Length; ++index)
        {
            int num3 = index % num2 * length;
            ReadOnlySpan<char> readOnlySpan1 = (ReadOnlySpan<char>)key;
            char reference = sourceKey[num3 + indexes[index]];
            ReadOnlySpan<char> readOnlySpan2 = new ReadOnlySpan<char>(ref reference);
            key = readOnlySpan1.ToString() + readOnlySpan2.ToString();
        }

        return key;
    }

    /// <summary>获取新的KeySeed</summary>
    /// <returns></returns>
    public static string GenKeySeed(int length = 16)
    {
        return RandomString.Next(CharsScope.NumbersAndLetters, length);
    }

    /// <summary>获取新的KeyIndexes</summary>
    /// <returns></returns>
    public static string GenKeyIndexes(int length = 16)
    {
        return string.Join<int>(',', (IEnumerable<int>)RandomUtil.RandomNotRepeat(length, length));
    }
}