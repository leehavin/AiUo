using System;

namespace AiUo.Data;

/// <summary>
/// 死锁信息
/// </summary>
public class DeadLockInfo
{
    /// <summary>
    /// 死锁时间
    /// </summary>
    public DateTime DeadLockDate { get; set; }
    /// <summary>
    /// 事务A语句
    /// </summary>
    public string TransactionA { get; set; }
    /// <summary>
    /// 事务A等待的锁
    /// </summary>
    public string AWaitingFor { get; set; }
    /// <summary>
    /// 事务B语句
    /// </summary>
    public string TransactionB { get; set; }
    /// <summary>
    /// 事务B拥有的锁
    /// </summary>
    public string BHoldsLock { get; set; }
    /// <summary>
    /// 事务B等待的锁
    /// </summary>
    public string BWaitingFor { get; set; }

    /// <summary>
    /// 原始信息
    /// </summary>
    public string Status { get; set; }
}