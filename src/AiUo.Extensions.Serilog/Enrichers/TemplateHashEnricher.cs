using Serilog.Core;
using Serilog.Events;
using AiUo.Security;

namespace Serilog.Enrichers;

public class TemplateHashEnricher : ILogEventEnricher
{
    public const string PropertyName = "TemplateHash";
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var value = MurmurHash3.Hash32(logEvent.MessageTemplate.Text);
        var hash = propertyFactory.CreateProperty(PropertyName, value);
        logEvent.AddPropertyIfAbsent(hash);
    }
}