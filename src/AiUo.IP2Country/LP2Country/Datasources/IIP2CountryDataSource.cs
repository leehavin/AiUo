using AiUo.IP2Country.Entities;
using System.Collections.Generic;

namespace AiUo.IP2Country.Datasources;

internal interface IIP2CountryDataSource
{
    IEnumerable<IIPRangeCountry> Read();
}