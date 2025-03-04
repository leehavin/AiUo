using StackExchange.Redis;
using AiUo.Logging;

namespace AiUo.Extensions.StackExchangeRedis;

/// <summary>
/// 分布式锁对象
/// </summary>
public class RedLock : IDisposable
{
    public IDatabase Database { get; }
    /// <summary>
    /// 要锁定资源的键值，针对每一个需要锁定的资源，名称必须唯一，如需要锁定操作用户coin，LockKey="lock_key_user_coin"
    /// </summary>
    public string LockKey { get; }
    private string _token { get; }

    public int RetryCount { get; }
    public TimeSpan RetryInterval { get; }

    public TimeSpan Expiry { get; }
    private System.Timers.Timer _timer;
    public bool IsLocked { get; private set; }

    public RedLock(IDatabase database, string lockKey, TimeSpan waitSpan, TimeSpan? retryInterval = null)
    {
        Database = database;
        LockKey = $"{RedisPrefixConst.REDIS_LOCK}:{lockKey}";
        _token = Guid.NewGuid().ToString("N");
        RetryInterval = retryInterval.HasValue ? retryInterval.Value : TimeSpan.FromMilliseconds(500);
        RetryCount = (int)(waitSpan.TotalMilliseconds / RetryInterval.TotalMilliseconds);

        Expiry = TimeSpan.FromSeconds(3); ;
        _timer = new System.Timers.Timer(Expiry.TotalMilliseconds / 3);
    }

    public async Task StartAsync()
    {
        IsLocked = false;
        int retry = RetryCount;
        do
        {
            if (await Database.LockTakeAsync(LockKey, _token, Expiry))
            {
                LogUtil.Trace("RedLock申请锁成功。lockKey:{lockKey} token:{token}", LockKey, _token);
                IsLocked = true;
                _timer.Elapsed += (sender, args) =>
                {
                    Database.LockExtendAsync(LockKey, _token, Expiry);//过期中间，延期
                };
                _timer.Start();
                return;
            }
            await Task.Delay(RetryInterval);
            retry--;
        }
        while (retry > 0);
        Release();
        LogLockError();
    }
    private void LogLockError()
    {
        LogUtil.GetContextLogger()
            .SetLevel(Microsoft.Extensions.Logging.LogLevel.Warning)
            .AddMessage($"RedLock申请锁失败。lockKey:{LockKey} token:{_token}")
            .Save();
    }

    #region IDisposable
    /// <summary>
    /// 手动释放锁
    /// </summary>
    public void Release()
    {
        if (_disposed) return;
        _disposed = true;
        Database.LockReleaseAsync(LockKey, _token).ConfigureAwait(false);
        GC.SuppressFinalize(this);
        _timer.Stop();
        _timer.Dispose();
    }
    private bool _disposed = false;
    public void Dispose()
    {
        Release();
    }

    ~RedLock()
    {
        Dispose();
        LogUtil.Error("RedLock申请锁成功。lockKey:{lockKey} token:{token}", LockKey, _token);
    }

    public void Close()
        => ((IDisposable)this).Dispose();
    #endregion
}