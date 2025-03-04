using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using AiUo.CliWrap;
using AiUo.Reflection;

namespace AiUo.Linux;

public static class LinuxUtil
{
    public static void Chmod(string target, string auth) => CliUtil.ExecuteReturn($"sudo chmod {auth} {target} -R");
    public static void ChmodSh(string sh) => CliUtil.ExecuteReturn($"sudo chmod +x {sh}");

    #region Execute
    public static CliResult ExecCmd(string cmd, string workDir = null) => CliUtil.Execute(cmd, workDir);
    public static CliResult ExecCmdReturn(string cmd) => CliUtil.ExecuteReturn(cmd);
    public static CliResult ExecSh(string sh, params object[] args)
    {
        ConsoleEx.WriteLine($"准备执行sh: {sh}");

        if (!File.Exists(sh))
            throw new FileNotFoundException($"sh文件未找到: {sh}");
        ChmodSh(sh);
        var paras = args.Select(item => item.ToString());
        return CliUtil.Execute($"{sh} {string.Join(" ", paras)}","/");
    }
    public static T ExecShReturn<T>(string sh, params object[] args)
    {
        if (!File.Exists(sh))
            throw new FileNotFoundException($"sh文件未找到: {sh}");
        ChmodSh(sh);
        var paras = args.Select(item => item.ToString());
        return CliUtil.ExecuteReturn($"{sh} {string.Join(" ", paras)}").Output.ConvertTo<T>();
    }
    public static CliResult ExecShWithOptions<T>(string sh, T options)
    {
        var args = new List<string>();
        foreach (var prop in options.GetType().GetProperties())
        {
            if (prop.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                continue;
            var value = ReflectionUtil.GetPropertyValue<string>(options, prop.Name);
            args.Add(value);
        }
        return ExecSh(sh, args.ToArray());
    }
    #endregion

    /// <summary>
    /// 当前用户Home路径
    /// </summary>
    /// <returns></returns>
    public static string GetHomePath()
    {
        var ret = Environment.GetEnvironmentVariable("HOME");
        return !string.IsNullOrEmpty(ret) ? ret : "/root";
    }
    public static string GetPwdPath()
        => CliUtil.ExecuteReturn("pwd").Output;
    /// <summary>
    /// 获取相对HOME路径的绝对路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetAbsolutePathOfHome(string path)
        => path.Replace("~", GetHomePath());

    /// <summary>
    /// 替换某文件包含src的行
    /// </summary>
    /// <param name="file"></param>
    /// <param name="src"></param>
    /// <param name="dest"></param>
    /// <param name="beginMatch">true:行必须以src开始匹配 false: 行包含src包含即可</param>
    public static void ReplaceFileLine(string file, string src, string dest, bool beginMatch = false)
    {
        var match = beginMatch ? src : $".*{src}";
        dest = ParseBashString(dest);
        CliUtil.Execute($"/bin/bash -c \"sudo sed -i 's/^{match}.*/{dest}/g' {file}\"");
    }
    /// <summary>
    /// 某文件添加一行
    /// </summary>
    /// <param name="file"></param>
    /// <param name="line"></param>
    public static void AppendFileLine(string file, string line)
    {
        line = ParseBashString(line);
        CliUtil.Execute($"/bin/bash -c \"sudo sed -i '$a\\{line}' {file}\"");
    }
    private static string ParseBashString(string src)
    {
        return src.Replace("/", "\\/");
    }

    /// <summary>
    /// 设置环境变量(~/.bashrc)
    /// </summary>
    /// <param name="variable"></param>
    /// <param name="value"></param>
    public static void SetEnvironmentVariable(string variable, string value)
    {
        var file = $"{GetHomePath()}/.bashrc";
        var sign = $"export {variable}=";
        var envStr = $"{sign}{value}";
        var content = File.ReadAllText(GetAbsolutePathOfHome(file));
        if (content.Contains(sign))
        {
            ReplaceFileLine(file, sign, envStr, true);
        }
        else
        {
            AppendFileLine(file, envStr);
        }
        Environment.SetEnvironmentVariable(variable, value);
        //CliUtil.Execute($"sudo sh -c \"source {file}\"");
    }

    /// <summary>
    /// 添加别名 ~/.bashrc
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="cmd"></param>
    public static void AddAlias(string alias, string cmd)
    {
        var file = $"{GetHomePath()}/.bashrc";
        var input = $"alias {alias}='{cmd}'";
        var sign = $"alias {alias}=";
        var content = File.ReadAllText(file);
        if (content.Contains(sign))
        {
            var sb = new StringBuilder();
            foreach (var line in content.SplitNewLine())
            {
                if (line.StartsWith(sign))
                    sb.AppendLine(input);
                else
                    sb.AppendLine(line);
            }
            File.WriteAllText(file, sb.ToString());
            ConsoleEx.WriteLine($"{file}替换: {input}", ConsoleColor.Yellow);
        }
        else
        {
            File.WriteAllText(file, content + Environment.NewLine + input);
            ConsoleEx.WriteLine($"{file}添加: {input}", ConsoleColor.Blue);
        }
    }
    public static void RemoveAlias(string alias)
    {
        var file = $"{GetHomePath()}/.bashrc";
        var sign = $"alias {alias}=";
        var content = File.ReadAllText(file);
        if (content.Contains(sign))
        {
            var sb = new StringBuilder();
            foreach (var line in content.SplitNewLine())
            {
                if (!line.StartsWith(sign))
                    sb.AppendLine(line);
            }
            File.WriteAllText(file, sb.ToString());
            ConsoleEx.WriteLine($"{file}清除alias: {alias}", ConsoleColor.Red);
        }
    }
}