using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AiUo.Configuration;
using AiUo.IP2Country.DbIp;
using AiUo.Net;

namespace AiUo.IP2Country;

public interface IIP2CountryService
{
    /// <summary>
    /// 根据ip返回国家编码2位大写（ISO 3166-1）
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    string GetContryId(string ip);

    /// <summary>
    /// 验证Ip是否属于指定国家
    /// 忽略：指定允许，测试环境，内网环境，白名单
    /// </summary>
    /// <param name="userIp"></param>
    /// <param name="countryId">指定的国家2位大写（ISO 3166-1）</param>
    /// <param name="allowIps"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    bool VerifyCountryIp(string userIp, string countryId, params string[] allowIps);
}

internal class IP2CountryService : IIP2CountryService
{
    private const string IP_RESOURCE = "AiUo.IP2Country.dbip-country-lite-2023-10.csv.gz";
    private IP2CountryResolver _resolver = null;
    private static object _sync = new();
    public Task Init()
    {
        _resolver = GetResolver();
        return Task.CompletedTask;
    }
    public string GetContryId(string ip)
    {
        return GetResolver().Resolve(ip)?.Country;
    }
    public bool VerifyCountryIp(string userIp, string countryId, params string[] allowIps)
    {
        if (countryId.Length != 2)
            throw new Exception("国家编码仅支持2位大写（ISO 3166-1）");

        var section = ConfigUtil.GetSection<IP2CountrySection>();
        // 没配置或关闭
        if (!(section?.Enabled ?? false))
            return true;
        // 配置允许
        if (section.AllowIpDict.Contains(userIp) || section.AllowIpDict.Contains("*"))
            return true;
        // 测试环境 内网环境
        if (ConfigUtil.Environment.IsDebug || NetUtil.GetIpMode(userIp) != IpAddressMode.External)
            return true;
        // 白名单
        if (allowIps != null && allowIps.Any(x => x == userIp))
            return true;
        return GetContryId(userIp) == countryId.ToUpper();
    }

    private IP2CountryResolver GetResolver()
    {
        if (_resolver == null)
        {
            lock (_sync)
            {
                if (_resolver == null)
                {
                    var section = ConfigUtil.GetSection<IP2CountrySection>();
                    if (!string.IsNullOrEmpty(section?.DbIpSource))
                    {
                        _resolver = new IP2CountryResolver(new DbIpCSVFileSource(section.DbIpSource));
                    }
                    else
                    {
                        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(IP_RESOURCE)!;
                        _resolver = new IP2CountryResolver(new DbIpCSVStreamSource(stream));
                    }
                }
            }

        }
        return _resolver;
    }
}