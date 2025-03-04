using System.Collections.Concurrent;

namespace AiUo.Randoms;

public class ShuffleSampling
{
    public bool Enabled { get; set; } = true;
    /// <summary>
    /// 范围0-53 => 54
    /// </summary>
    public int Range { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public int Size { get; set; }
    public int SamplingCount { get; set; }
    public ShuffleSampling(int range,int size, int count)
    {
        Range = range;
        Size = size;
        SamplingCount = count;
    }
    private ConcurrentQueue<int[]> _datas = new ConcurrentQueue<int[]>();
    public ConcurrentDictionary<int, int> Samplings = new ConcurrentDictionary<int, int>();
    public bool CanVerify => _datas.Count >= SamplingCount;
    public void Add(int[] rnds)
    {
        if (CanVerify)
        {
            if (_datas.TryDequeue(out int[] result))
            {
                foreach (var item in result)
                {
                    Samplings.AddOrUpdate(item, 0, (k, v) => {
                        return v - 1;
                    });
                }
            }
        }
        _datas.Enqueue(rnds);
        foreach (var item in rnds)
        {
            Samplings.AddOrUpdate(item, 1, (k, v) => {
                return v + 1;
            });
        }
    }
}