using System;
using AiUo.CliWrap;

namespace AiUo.Linux;

/// <summary>
/// Linux系统信息
/// </summary>
public static class SystemInfoUtil
{
    /// <summary>
    /// 物理CPU个数
    /// </summary>
    /// <returns></returns>
    public static int GetCpuPhysicalNum()
    {
        var result = CliUtil.ExecuteReturn("bash -c \"grep 'physical id' /proc/cpuinfo | sort | uniq | wc -l\"");
        return result.Output.ToInt32();
    }
    /// <summary>
    /// 逻辑CPU个数
    /// </summary>
    /// <returns></returns>
    public static int GetCpuCoreNum()
        => CliUtil.ExecuteReturn($"bash -c \"grep 'processor' /proc/cpuinfo|wc -l\"").Output.ToInt32();
    public static LinuxMemInfo GetMemInfo()
    {
        var items = CliUtil.ExecuteReturn("free -b").Output.SplitNewLine()[1].SplitSpace();
        var ret = new LinuxMemInfo
        {
            Total = items[1].ToInt64(),
            Used = items[2].ToInt64(),
            Free = items[3].ToInt64() + items[5].ToInt64()
        };
        var perc = Math.Round((decimal)ret.Used / ret.Total * 100);
        ret.UsedPercent = (int)perc;
        return ret;
    }
    public static LinuxDiskInfo GetDiskInfo(string path)
    {
        var items = CliUtil.ExecuteReturn($"df {path}").Output.SplitNewLine()[1].SplitSpace();
        var ret = new LinuxDiskInfo { 
            Total = items[1].ToInt64()*1024,
            Used = items[2].ToInt64() * 1024,
            Free = items[3].ToInt64() * 1024,
        };
        var perc = Math.Round((decimal)ret.Used/ret.Total * 100);
        ret.UsedPercent = (int)perc;
        return ret;
    }
}
public class LinuxMemInfo
{
    public long Total { get; set; }
    public long Used { get; set; }
    public long Free { get; set; }
    public int UsedPercent { get; set; }
}
public class LinuxDiskInfo
{
    public long Total { get; set; }
    public long Used { get; set; }
    public long Free { get; set; }
    public int UsedPercent { get; set; }
}