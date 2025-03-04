using Microsoft.Extensions.Logging;
using System;
using AiUo.Logging;

namespace Nacos.Logging;

public class NacosLogManager
{
    //private static ILoggerFactory _loggerFactory;

    public static ILogger<T> CreateLogger<T>()
    {
        return new NacosLogger<T>(LogLevel.Error);
    }
    /*
    public static ILogger CreateLogger(string categoryName)
    {
        return _loggerFactory?.CreateLogger(categoryName) ?? NullLogger.Instance;
    }

    public static void UseLoggerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }
    */
}
internal class NacosLogger<T> : ILogger<T>
{
    private ILogger<T> _logger;
    private LogLevel _minLevel;
    public NacosLogger(LogLevel minLevel)
    {
        _minLevel = minLevel;
        _logger = LogUtil.CreateLogger<T>();
    }
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }
        _logger.Log(logLevel, eventId, state, exception, formatter);
    }
}