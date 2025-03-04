namespace AiUo.AspNet.Auth.RateLimit;

/// <summary>
/// 滑动窗口算法
/// 估计数 = 前一窗口计数 * (1 - 当前窗口经过时间 / 单位时间) + 当前窗口计数
/// </summary>
internal class SlidingWindow
{
    private readonly object _sync = new object();

    private readonly int _requestIntervalSeconds; 
    private readonly int _requestLimit;

    private DateTime _windowStartTime;
    private int _prevRequestCount;
    private int _requestCount;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="requestLimit">时长内限制的总次数</param>
    /// <param name="requestIntervalSeconds">限制时间长度间隔(秒)</param>
    public SlidingWindow(int requestLimit, int requestIntervalSeconds)
    {
        _windowStartTime = DateTime.Now;
        _requestLimit = requestLimit;
        _requestIntervalSeconds = requestIntervalSeconds;
    }

    public bool PassRequest()
    {
        lock (_sync)
        {
            var currentTime = DateTime.Now;
            var elapsedSeconds = (currentTime - _windowStartTime).TotalSeconds;

            if (elapsedSeconds >= _requestIntervalSeconds * 2)
            {
                _windowStartTime = currentTime;
                _prevRequestCount = 0;
                _requestCount = 0;

                elapsedSeconds = 0;
            }
            else if (elapsedSeconds >= _requestIntervalSeconds)
            {
                _windowStartTime = _windowStartTime.AddSeconds(_requestIntervalSeconds);
                _prevRequestCount = _requestCount;
                _requestCount = 0;

                elapsedSeconds = (currentTime - _windowStartTime).TotalSeconds;
            }

            var requestCount = _prevRequestCount * (1 - elapsedSeconds / _requestIntervalSeconds) + _requestCount + 1;
            if (requestCount <= _requestLimit)
            {
                _requestCount++;
                return true;
            }
        }

        return false;
    }
}