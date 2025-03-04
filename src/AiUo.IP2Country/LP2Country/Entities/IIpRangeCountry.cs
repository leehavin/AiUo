using System.Net;

namespace AiUo.IP2Country.Entities;

internal interface IIPRangeCountry
{
    IPAddress Start { get; set; }
    IPAddress End { get; set; }
    string Country { get; set; }
}