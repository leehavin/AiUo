using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;
using AiUo.Logging;

namespace AiUo.Extensions.Serilog;

public class ElasticsearchFailureSink : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        var ev = SerializerUtil.SerializeJson(logEvent);
        LogUtil.BootstrapLogger.LogError("保存日志到Elasticsearch失败! {logEvent}", ev);
    }
}