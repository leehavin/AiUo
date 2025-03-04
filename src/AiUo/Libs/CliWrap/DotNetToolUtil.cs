using System;
using System.Collections.Generic;
using System.IO;

namespace AiUo.CliWrap;

/// <summary>
/// dotnet工具辅助类
/// </summary>
public static class DotNetToolUtil
{
    private static string _toolPath;
    public static string ToolPath
    {
        get
        {
            if (string.IsNullOrEmpty(_toolPath))
            {
                _toolPath = AiUoUtil.IsWindowsOS
                    ? Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), ".dotnet\\tools")
                    : Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".dotnet/tools");
            }
            return _toolPath;
        }
    }
    public static string GetFilePathByName(string name)
        => Path.Combine(ToolPath, AiUoUtil.IsWindowsOS ? $"{name}.exe": name);
    public static string GetFilePathById(string id)
        => GetFilePathByName(GetToolById(id).CommandName);
    /// <summary>
    /// 通过PackageId获取CommandName
    /// </summary>
    /// <param name="id"></param>
    /// <param name="isGlobal"></param>
    /// <returns></returns>
    public static DotNetToolItem GetToolById(string id, bool isGlobal = true)
        => GetToolList(isGlobal).Find(item => item.PackageId == id.ToLower());
    /// <summary>
    /// 通过CommandName获取PackageId
    /// </summary>
    /// <param name="name"></param>
    /// <param name="isGlobal"></param>
    /// <returns></returns>
    public static DotNetToolItem GetToolByName(string name, bool isGlobal = true)
        => GetToolList(isGlobal).Find(item => item.CommandName == name.ToLower());

    public static List<DotNetToolItem> GetToolList(bool isGlobal = true)
    {
        var ret = new List<DotNetToolItem>();
        var result = CliUtil.ExecuteReturn($"dotnet tool list -g");
        //var result = CliUtil.ExecuteReturn($"dotnet tool list {(isGlobal ? " -g" : string.Empty)}");
        var lines = result.Output.SplitNewLine();
        for (int i = lines.Length - 1; i > 0; i--)
        {
            var line = lines[i];
            if (line.StartsWith("-------"))
                break; ;
            var objs = line.SplitSpace();
            var item = new DotNetToolItem {
                PackageId = objs[0].Trim(),
                Version = objs[1].Trim(),
                CommandName = objs[2].Trim()
            };
            ret.Add(item);
        }
        return ret;
    }
}

public class DotNetToolItem
{
    public string PackageId { get; set; }
    public string Version { get; set; }
    public string CommandName { get; set; }
}