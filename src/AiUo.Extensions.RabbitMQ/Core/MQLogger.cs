using Microsoft.Extensions.Logging;
using AiUo.Configuration;
using AiUo.Logging;

namespace AiUo.Extensions.RabbitMQ;

internal class MQLogger<T> : EasyNetQ.Logging.ILogger<T>
{
    private bool _enabled;
    private Microsoft.Extensions.Logging.ILogger<T> _logger;
    public MQLogger()
    {
        var section = ConfigUtil.GetSection<RabbitMQSection>();
        _enabled = section != null && section.MessageLogEnabled;
        ConfigUtil.RegisterChangedCallback(() =>
        {
            var section = ConfigUtil.GetSection<RabbitMQSection>();
            _enabled = section != null && section.MessageLogEnabled;
        });
        _logger = LogUtil.CreateLogger<T>();
    }
    public bool Log(EasyNetQ.Logging.LogLevel logLevel, Func<string> messageFunc, Exception exception = null, params object[] formatParameters)
    {
        if (!_enabled)
            return false;
        var lv = logLevel switch
        {
            EasyNetQ.Logging.LogLevel.Trace => Microsoft.Extensions.Logging.LogLevel.Trace,
            EasyNetQ.Logging.LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            EasyNetQ.Logging.LogLevel.Info => Microsoft.Extensions.Logging.LogLevel.Information,
            EasyNetQ.Logging.LogLevel.Warn => Microsoft.Extensions.Logging.LogLevel.Warning,
            EasyNetQ.Logging.LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            EasyNetQ.Logging.LogLevel.Fatal => Microsoft.Extensions.Logging.LogLevel.Critical,
            _ => throw new ArgumentOutOfRangeException()
        };
        if (messageFunc == null)
            return _logger.IsEnabled(lv);
        _logger.Log(lv, messageFunc(), exception, formatParameters);
        return true;
    }
}