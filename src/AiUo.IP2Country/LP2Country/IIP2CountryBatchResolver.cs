using AiUo.IP2Country.Entities;
using System.Collections.Generic;
using System.Net;

namespace AiUo.IP2Country;

internal interface IIP2CountryBatchResolver : IIP2CountryResolver
{
    IIPRangeCountry[] Resolve(string[] ips);
    IIPRangeCountry[] Resolve(IPAddress[] ips);
    IIPRangeCountry[] Resolve(IEnumerable<string> ips);
    IIPRangeCountry[] Resolve(IEnumerable<IPAddress> ips);
    IDictionary<string, IIPRangeCountry> ResolveAsDictionary(IEnumerable<string> ips);
    IDictionary<IPAddress, IIPRangeCountry> ResolveAsDictionary(IEnumerable<IPAddress> ips);
    IDictionary<string, IIPRangeCountry> ResolveAsDictionary(string[] ips);
    IDictionary<IPAddress, IIPRangeCountry> ResolveAsDictionary(IPAddress[] ips);
}