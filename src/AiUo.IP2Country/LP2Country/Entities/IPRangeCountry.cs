using System.Net;

namespace AiUo.IP2Country.Entities;

internal class IPRangeCountry : IIPRangeCountry
{
    public IPAddress Start { get; set; }
    public IPAddress End { get; set; }
    public string Country { get; set; }
}