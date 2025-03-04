using System.Diagnostics;

namespace AiUo.SnowflakeId.Common;

internal class DefaultTimeSource
{
    private static readonly Stopwatch _sw = new();
    private static readonly DateTimeOffset _initialized = DateTimeOffset.UtcNow;
    protected static TimeSpan Elapsed => _sw.Elapsed;

    public DateTimeOffset Epoch { get; private set; }
    protected TimeSpan Offset { get; private set; }
    public TimeSpan TickDuration { get; private set; }

    public DefaultTimeSource(DateTimeOffset epoch, TimeSpan tickDuration)
    {
        Epoch = epoch;
        Offset = _initialized - Epoch;
        TickDuration = tickDuration;
        _sw.Start();
    }
    public long GetTicks() 
        => (Offset.Ticks + Elapsed.Ticks) / TickDuration.Ticks;
}