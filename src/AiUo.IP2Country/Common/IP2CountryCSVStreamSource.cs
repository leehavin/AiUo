using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using AiUo.IP2Country.DataSources.CSVFile;
using AiUo.IP2Country.Datasources;
using AiUo.IP2Country.Entities;
using System.IO;

namespace AiUo.IP2Country;

internal abstract class IP2CountryCSVStreamSource<T> : IIP2CountryDataSource
    where T : IIPRangeCountry
{
    public Encoding Encoding { get; private set; }
    public Stream Stream { get; set; }
    public ICSVRecordParser<T> Parser { get; private set; }

    public IP2CountryCSVStreamSource(Stream stream, ICSVRecordParser<T> parser, Encoding encoding = null)
    {
        Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        Parser = parser ?? throw new ArgumentNullException(nameof(parser));
        Encoding = encoding ?? Encoding.UTF8;
    }

    protected IEnumerable<TRecord> ReadStream<TRecord>(Stream stream, ICSVRecordParser<TRecord> parser)
        where TRecord : IIPRangeCountry
    {
        using (var g = new GZipStream(stream, CompressionMode.Decompress))
            foreach (var r in Read(g, parser))
                yield return r;
    }
    public IEnumerable<TRecord> Read<TRecord>(Stream stream, ICSVRecordParser<TRecord> parser)
        where TRecord : IIPRangeCountry => ReadLines(stream)
        .Where(l => l.Length > 0 && l[0] != '#' && !char.IsWhiteSpace(l[0]))
        .Select(l => parser.ParseRecord(l))
        .Where(r => r != null);

    private IEnumerable<string> ReadLines(Stream stream)
    {
        using (var r = new StreamReader(stream, Encoding))
        {
            string line;
            while ((line = r.ReadLine()) != null)
                yield return line;
        }
    }

    public abstract IEnumerable<IIPRangeCountry> Read();
}