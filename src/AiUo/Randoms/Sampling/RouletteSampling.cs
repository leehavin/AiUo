using System.Collections.Concurrent;

namespace AiUo.Randoms;

public class RouletteSampling
{
    public bool Enabled { get; set; } = true;
    public int Min { get; set; }
    public int Max { get; set; }
    public int SamplingCount { get; set; }
    public RouletteSampling(int min, int max, int count)
    {
        Min = min;
        Max = max;
        SamplingCount = count;
    }
    private ConcurrentQueue<int> _datas = new ConcurrentQueue<int>();
    public ConcurrentDictionary<int, int> Samplings = new ConcurrentDictionary<int, int>();
    public bool CanVerify => _datas.Count >= SamplingCount;
    public void Add(int value)
    {
        if (CanVerify)
        {
            if (_datas.TryDequeue(out int result))
            {
                Samplings.AddOrUpdate(result, 0, (k, v) => {
                    return v - 1;
                });
            }
        }
        _datas.Enqueue(value);
        Samplings.AddOrUpdate(value, 1, (k, v) => {
            return v + 1;
        });
    }
}