using System.Text;

namespace AiUo.IP2Country.DataSources.CSVFile;

internal interface ICSVRecordParser<T>
{
    bool IgnoreErrors { get; }
    Encoding Encoding { get; }
    T ParseRecord(string record);
}