using System.Collections.Generic;
using System.IO;
using AiUo.IP2Country.DbIp;
using AiUo.IP2Country.Entities;

namespace AiUo.IP2Country;

internal class DbIpCSVStreamSource : IP2CountryCSVStreamSource<DbIpIPRangeCountry>
{
    public DbIpCSVStreamSource(Stream stream)
        : base(stream, new DbIpCSVRecordParser()) { }

    public override IEnumerable<IIPRangeCountry> Read() => ReadStream(Stream, Parser);
}