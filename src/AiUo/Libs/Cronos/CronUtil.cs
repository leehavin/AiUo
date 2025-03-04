using System;
using System.Collections.Generic;
using Cronos;
using System.Collections.Concurrent;

namespace AiUo.Cronos;
/*
                                             允许值            允许符号
┌─────────────  second (optional)  0-59              * , - /
│ ┌──────────── minute             0-59              * , - /
│ │ ┌──────────  hour               0-23              * , - /
│ │ │ ┌───────── day of month       1-31              * , - / L W ?
│ │ │ │ ┌───────  month              1-12 or JAN-DEC   * , - /
│ │ │ │ │ ┌────── day of week        0-6  or SUN-SAT   * , - / # L ?
│ │ │ │ │ │
*  *  *  *  *  *
 */

/// <summary>
/// cron表达式辅助类
/// https://github.com/HangfireIO/Cronos
/// </summary>
public static class CronUtil
{
    private static ConcurrentDictionary<string, CronExpression> _crons = new ConcurrentDictionary<string, CronExpression>();
    public static CronExpression Parse(string cron)
    {
        return _crons.GetOrAdd(cron, (key) =>
        {
            var strs = cron.SplitSpace();
            var format = strs.Length == 5 ? CronFormat.Standard : CronFormat.IncludeSeconds;
            return CronExpression.Parse(cron, format);
        });
    }
    /// <summary>
    /// 获取下一个 fromUtc 之后的到期时间
    /// </summary>
    /// <param name="cron"></param>
    /// <param name="fromUtc"></param>
    /// <returns></returns>
    public static DateTime? GetNextUtc(string cron, DateTime fromUtc = default)
    {
        return Parse(cron).GetNextOccurrence(fromUtc == default ? DateTime.UtcNow : fromUtc);
    }
    /// <summary>
    /// 获取下一个 fromLocal 之后的到期时间
    /// </summary>
    /// <param name="cron"></param>
    /// <param name="fromLocal"></param>
    /// <returns></returns>
    public static DateTime? GetNextLocal(string cron, DateTime fromLocal = default)
    {
        var fromDate = fromLocal == default ? DateTime.UtcNow : fromLocal;
        return Parse(cron).GetNextOccurrence(fromDate, TimeZoneInfo.Local);
    }
    /// <summary>
    /// 获取从 fromUtc 到 toUtc 区间的到期时间
    /// </summary>
    /// <param name="cron"></param>
    /// <param name="fromUtc"></param>
    /// <param name="toUtc"></param>
    /// <returns></returns>
    public static IEnumerable<DateTime> GetNextsUtc(string cron, DateTime fromUtc, DateTime toUtc)
    {
        return Parse(cron).GetOccurrences(fromUtc, toUtc);
    }
    /// <summary>
    /// 获取从 fromUtc 到 toUtc 区间的到期时间
    /// </summary>
    /// <param name="cron"></param>
    /// <param name="fromLocal"></param>
    /// <param name="toLocal"></param>
    /// <returns></returns>
    public static IEnumerable<DateTime> GetNextsLocal(string cron, DateTime fromLocal, DateTime toLocal)
    {
        return Parse(cron).GetOccurrences(fromLocal, toLocal, TimeZoneInfo.Local);
    }

}