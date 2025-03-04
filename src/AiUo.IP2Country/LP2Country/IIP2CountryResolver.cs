using AiUo.IP2Country.Entities;
using System.Net;

namespace AiUo.IP2Country;

internal interface IIP2CountryResolver
{
    IIPRangeCountry Resolve(string ip);
    IIPRangeCountry Resolve(IPAddress ip);
}