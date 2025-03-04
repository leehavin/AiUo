using Microsoft.Extensions.Logging;
using SL = Serilog;

namespace AiUo.Extensions.Serilog;

public class SerilogLoggerWrapper : ILogger
{
    private SL.ILogger _logger;
    private LogLevel _minLevel;
    public SerilogLoggerWrapper(SL.ILogger logger, LogLevel minLevel)
    {
        _logger = logger;
        _minLevel = minLevel;
    }
    public SerilogLoggerWrapper(LogLevel minLevel):this(SL.Log.Logger, minLevel) { }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }
        var msg = formatter(state, exception);
        switch (logLevel)
        {
            case LogLevel.Trace:
                _logger.Verbose(exception, msg);
                break;
            case LogLevel.Debug:
                _logger.Debug(exception, msg);
                break;
            case LogLevel.Information:
                _logger.Information(exception, msg);
                break;
            case LogLevel.Warning:
                _logger.Warning(exception, msg);
                break;
            case LogLevel.Error:
            case LogLevel.Critical:
                _logger.Error(exception, msg);
                break;
        }

    }
}
public class SerilogLoggerWrapper<T> : SerilogLoggerWrapper, ILogger<T>
{
    public SerilogLoggerWrapper(LogLevel minLevel)
        : base(SL.Log.Logger, minLevel) { }
}