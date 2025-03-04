using System;

namespace AiUo.IP2Country;

/// <summary>
/// 
/// 代码：https://github.com/RobThree/IP2Country
/// IP库：https://db-ip.com/db/download/ip-to-country-lite
/// </summary>
public class IP2CountryUtil
{
    /// <summary>
    /// 根据ip返回国家编码2位大写（ISO 3166-1）
    /// </summary>
    /// <param name="ip">ipv4</param>
    /// <returns></returns>
    public static string GetContryId(string ip)
        => DIUtil.GetRequiredService<IIP2CountryService>().GetContryId(ip);

    /// <summary>
    /// 验证Ip是否属于指定国家
    /// 忽略：指定允许，测试环境，内网环境，白名单
    /// </summary>
    /// <param name="userIp"></param>
    /// <param name="countryId">指定的国家2位大写（ISO 3166-1）</param>
    /// <param name="allowIps"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static bool VerifyCountryIp(string userIp, string countryId, params string[] allowIps)
        => DIUtil.GetRequiredService<IIP2CountryService>().VerifyCountryIp(userIp, countryId, allowIps);
}