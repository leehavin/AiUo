using System.Runtime.CompilerServices;
using AiUo.Configuration;
using AiUo.SnowflakeId.Common; 

namespace AiUo.SnowflakeId;

public interface ISnowflakeIdService
{
    long DataCenterId { get; }
    int MaxDataCenterId { get; }
    int MaxSequence { get; }
    int MaxWorkerId { get; }
    int WorkerId { get; }

    long NextId();
}

internal class SnowflakeIdService : ISnowflakeIdService
{
    #region Properties
    private readonly object _async = new object();
    private SnowflakeIdSection _section;
    private IWorkerIdProvider _provider;
    public static readonly DateTime DefaultEpoch = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DefaultTimeSource _timeSource = new DefaultTimeSource(DefaultEpoch, TimeSpan.FromMilliseconds(1));

    private byte _timestampBits = 41;
    private byte _dataCenterIdBits;
    private byte _workerIdBits;
    private byte _sequenceBits;

    public int MaxDataCenterId { get; private set; }
    public int MaxWorkerId { get; private set; }
    public int MaxSequence { get; private set; }

    private readonly int _shiftWorkerId;
    private readonly int _shiftDataCenterId;
    private readonly int _shiftTimestamp;

    private readonly long MASK_TIME;
    private readonly long MASK_SEQUENCE;
    public long DataCenterId { get; private set; }

    private long _workerId;
    public int WorkerId => (int)_workerId;
    private int _sequence = 0;
    private long _lastgen = -1L;

    private bool _isInited = false;
    #endregion

    public SnowflakeIdService()
    {
        _section = ConfigUtil.GetSection<SnowflakeIdSection>();
        if (_section == null || !_section.Enabled)
            throw new Exception("SnowflakeIdUtil生成ID时，没有配置SnowflakeIdSection或者Enabled=false");
        _provider = _section.UseRedis
            ? new RedisWorkerIdProvider()
            : new ConfigWorkerIdProvider();

        _dataCenterIdBits = _section.DataCenterIdBits;
        _workerIdBits = _section.WorkerIdBits;
        _sequenceBits = (byte)(63 - _timestampBits - _dataCenterIdBits - _workerIdBits);
        if (_sequenceBits < 1)
            throw new Exception("TimestampBits(41)+ DataCenterBits(3) + WorkerIdBits(10) + SequenceBits(9) 不能大于63");

        MaxDataCenterId = 1 << _dataCenterIdBits;
        MaxWorkerId = 1 << _workerIdBits;
        MaxSequence = 1 << _sequenceBits;

        _shiftWorkerId = _sequenceBits;
        _shiftDataCenterId = _sequenceBits + _workerIdBits;
        _shiftTimestamp = _sequenceBits + _workerIdBits + _dataCenterIdBits;
        DataCenterId = _section.DataCenterId << _shiftDataCenterId;

        MASK_TIME = GetMask(_timestampBits);
        MASK_SEQUENCE = GetMask(_sequenceBits);
    }

    public async Task Init()
    {
        _workerId = await _provider.GetNextWorkId();
        _isInited = true;
    }
    public long NextId()
    {
        if (!_isInited)
            throw new Exception("没有初始化设置WorkId值");
        lock (_async)
        {
            var ticks = GetTicks();
            var timestamp = ticks & MASK_TIME;
            if (timestamp < _lastgen || ticks < 0)
            {
                //发生时钟回拨，切换workId，可解决。
                _workerId = _provider.GetNextWorkId().ConfigureAwait(false).GetAwaiter().GetResult();
                return NextId();
            }

            if (timestamp == _lastgen)
            {
                if (_sequence >= MASK_SEQUENCE)
                {
                    SpinWait.SpinUntil(() => _lastgen != GetTicks());
                    return NextId();
                }
                _sequence++;
            }
            else
            {
                _sequence = 0;
                _lastgen = timestamp;
            }
            return timestamp << _shiftTimestamp
                   | DataCenterId
                   | _workerId << _shiftWorkerId
                   | (long)_sequence;
        }
    }
    public Task Active()
    {
        _provider.Active();
        return Task.CompletedTask;
    }
    public async Task Dispose()
    {
        await _provider.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private long GetTicks()
    {
        return _timeSource.GetTicks();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetMask(byte bits) => (1L << bits) - 1;
}