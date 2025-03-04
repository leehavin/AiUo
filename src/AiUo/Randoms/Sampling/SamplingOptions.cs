namespace AiUo.Randoms;

public class SamplingOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = false;
    /// <summary>
    /// 抽取采样总数
    /// </summary>
    public int RouletteSamplingCount { get; set; } = 100000;
    /// <summary>
    /// 洗牌采样总数
    /// </summary>
    public int ShuffleSamplingCount { get; set; } = 100000;
    /// <summary>
    /// 采样偏差: 抽取 - n取1偏差量，20表示偏差20%
    /// </summary>
    public int RouletteDeviation { get; set; } = 20;
    /// <summary>
    /// 采样偏差: 洗牌 - n取size偏差量，20表示偏差20%
    /// </summary>
    public int ShuffleDeviation { get; set; } = 20;
    /// <summary>
    /// 监测间隔
    /// </summary>
    public int CheckInterval { get; set; } = 10000;
}