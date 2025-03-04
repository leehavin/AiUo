using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AiUo.Randoms;

/// <summary>
/// 随机数采样容器
/// </summary>
public class SamplingContainer: ISamplingContainer,IDisposable
{
    public SamplingOptions Options;
    /// <summary>
    /// 采样总数
    /// </summary>
    public int RouletteSamplingCount { get; set; } = 10000;
    public int ShuffleSamplingCount { get; set; } = 10000;
    /// <summary>
    /// 采样偏差: 抽取 - n取1偏差量，20表示偏差20%
    /// </summary>
    public int RouletteDeviation { get; set; }
    /// <summary>
    /// 采样偏差: 洗牌 - n取size偏差量，20表示偏差20%
    /// </summary>
    public int ShuffleDeviation { get; set; }
    public ConcurrentDictionary<string, RouletteSampling> RouletteSamplings = new ConcurrentDictionary<string, RouletteSampling>();
    public ConcurrentDictionary<string, ShuffleSampling> ShuffleSamplings = new ConcurrentDictionary<string, ShuffleSampling>();
    private System.Timers.Timer _timer;
    public SamplingContainer(SamplingOptions options)
    {
        Options = options ?? new SamplingOptions();
        if (Options.Enabled)
        {
            _timer = new System.Timers.Timer(Options.CheckInterval);
            _timer.Elapsed += (sender, e) =>
            {
                VerifyRoulette();
                VerifyShuffle();
            };
            _timer.Start();
        }
    }
    public void AddRandom(int min, int max, params byte[] rnds)
        => AddRandom(min, max, rnds.Select(rnd => (int)rnd));
    public void AddRandom(int min, int max, params int[] rnds)
        => AddRandom(min, max, rnds);
    public void AddRandom(int min, int max, IEnumerable<int> rnds)
    {
        if (!Options.Enabled) return;
        var key = $"{min}_{max}";
        RouletteSamplings.AddOrUpdate(key, new RouletteSampling(min, max, RouletteSamplingCount), (k, v) =>
        {
            if (!v.Enabled)
                throw new RandomException("随机数RouletteSamplings验证存在异常");
            foreach (var rnd in rnds)
                v.Add(rnd);
            return v;
        });
    }
    public void AddNotRepeat(int range, int size, params int[] rnds)
    {
        if (!Options.Enabled) return;
        var key = $"{range}_{size}";
        ShuffleSamplings.AddOrUpdate(key, new ShuffleSampling(range, size, ShuffleSamplingCount), (k, v) =>
        {
            if (!v.Enabled)
                throw new RandomException("随机数ShuffleSamplings验证存在异常");
            v.Add(rnds);
            return v;
        });
    }
    private void VerifyRoulette()
    {
        var enumtor = RouletteSamplings.GetEnumerator();
        while (enumtor.MoveNext())
        {
            var sampling = enumtor.Current.Value;
            if (!sampling.CanVerify)
                continue;
            //
            var count = RouletteSamplingCount;
            var avg = count / sampling.Samplings.Count;
            var etor = sampling.Samplings.GetEnumerator();
            while (etor.MoveNext())
            {
                var value = etor.Current.Value;
                var rate = (int)(Math.Abs(value - avg) * 1.0d / count * 100);
                if (rate > RouletteDeviation)
                {
                    sampling.Enabled = false;
                    break;
                }
            }
        }
    }
    private void VerifyShuffle()
    {
        var enumtor = ShuffleSamplings.GetEnumerator();
        while (enumtor.MoveNext())
        {
            var sampling = enumtor.Current.Value;
            if (!sampling.CanVerify)
                continue;
            //
            var count = sampling.SamplingCount * sampling.Size;
            var avg = count / sampling.Samplings.Count;
            var etor = sampling.Samplings.GetEnumerator();
            while (etor.MoveNext())
            {
                var value = etor.Current.Value;
                var rate = (int)(Math.Abs(value - avg) * 1.0d / count * 100);
                if (rate > ShuffleDeviation)
                {
                    sampling.Enabled = false;
                    break;
                }
            }
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}