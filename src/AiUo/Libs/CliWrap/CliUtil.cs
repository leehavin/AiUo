using CliWrap;
using CliWrap.Buffered;
using CliWrap.EventStream;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AiUo.Xml;

namespace AiUo.CliWrap;

/// <summary>
/// 执行命令辅助类
/// </summary>
public static class CliUtil
{
    #region Execute
    /// <summary>
    /// 执行命令，输出到sdtout
    /// </summary>
    /// <param name="commandLine"></param>
    /// <param name="workDir"></param>
    /// <returns></returns>
    public static CliResult Execute(string commandLine, string workDir = null)
        => ExecuteBaseAsync(commandLine, workDir).Result;
    /// <summary>
    /// 执行命令，输出到outputAction
    /// </summary>
    /// <param name="commandLine"></param>
    /// <param name="outputAction"></param>
    /// <param name="timeout">超时时间，windows网络超时65秒</param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static CliResult ExecuteAction(string commandLine, Action<string, CliStatus> outputAction, int timeout = 70000, Encoding encoding = null)
    {
        var cmd = CreateCommand(commandLine);
        var observer = new CliObserver(outputAction);
        cmd.Observe(encoding ?? Encoding.GetEncoding("gb2312")).Subscribe(observer);
        var task1 = cmd.ExecuteAsync().Task;
        var task2 = Task.Run(() =>
        {
            var tryCount = 0;
            while (true)
            {
                var status = observer.GetStatus();
                if (status == CliStatus.Completed || status == CliStatus.Error)
                {
                    return observer.GetResult();
                }
                tryCount += 1000;
                if (tryCount > timeout)
                {
                    var ret = observer.GetResult();
                    ret.ExitCode = 1;
                    try
                    {
                        Process.GetProcessById(observer.ProcessId).Kill();
                    }
                    catch { }
                    outputAction($"操作超时: {TimeSpan.FromMilliseconds(timeout).TotalSeconds}秒", CliStatus.Error);
                    return ret;
                }
                Thread.Sleep(1000);
            }
        });
        Task.WaitAll(task1, task2);
        return task2.Result;
    }
    /// <summary>
    /// 执行命令，输出内容返回CliResult.Output和.Error
    /// </summary>
    /// <param name="commandLine"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static CliResult ExecuteReturn(string commandLine, Encoding encoding = null)
    {
        var cmd = CreateCommand(commandLine);
        var result = cmd.ExecuteBufferedAsync(encoding ?? Encoding.UTF8).Task.Result;
        var ret = new CliResult
        {
            ExitCode = result.ExitCode,
            StartTime = result.StartTime,
            ExitTime = result.ExitTime,
            RunTime = result.RunTime,
            Output = result.StandardOutput.TrimEnd('\n'),
            Error = result.StandardError
        };
        return ret;
    }
    private static Command CreateCommand(string commandLine, string workDir = null)
    {
        var cmds = commandLine.Split(' ');
        var cmd = cmds[0];
        var args = string.Join(" ", cmds, 1, cmds.Length - 1);
        var command = Cli.Wrap(cmd)
            .WithArguments(args)
            .WithValidation(CommandResultValidation.None);
        if(!string.IsNullOrEmpty(workDir))
            command.WithWorkingDirectory(workDir);
        //.WithWorkingDirectory(Path.GetDirectoryName(cmd));
        return command;
    }
    private static async Task<CliResult> ExecuteBaseAsync(string commandLine, string workDir=null)
    {
        var command = CreateCommand(commandLine, workDir);

        var streamOut = Console.OpenStandardOutput();
        var streamErr = Console.OpenStandardError();
        command = command.WithStandardOutputPipe(PipeTarget.ToStream(streamOut))
            .WithStandardErrorPipe(PipeTarget.ToStream(streamErr));
        var result = await command.ExecuteAsync();
        var ret = new CliResult
        {
            ExitCode = result.ExitCode,
            StartTime = result.StartTime,
            ExitTime = result.ExitTime,
            RunTime = result.RunTime,
            Output = null,
            Error = null
        };
        return ret;
    }
    #endregion

    /*
    /// <summary>
    /// 使用cmd执行windows命令
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public static int ExecuteWinCmd(string cmd, out string output)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "cmd",
            UseShellExecute = false,        //是否使用操作系统shell启动
            RedirectStandardInput = true,   //接受来自调用程序的输入信息
            RedirectStandardOutput = true,  //由调用程序获取输出信息
            RedirectStandardError = true,   //重定向标准错误输出
            CreateNoWindow = true,          //不显示程序窗口
            //WorkingDirectory = guiProjectDirectory
        };
        using (var proc = Process.Start(psi))
        {
            proc.StandardInput.WriteLine($"{cmd} & exit");
            proc.StandardInput.AutoFlush = true;
            output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            return proc.ExitCode;
        }
    }
    */

    /// <summary>
    /// Windows CMD 执行命令
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public static int ExecuteWinCmd(string cmd, out string output)
    {
        var psi = new ProcessStartInfo { 
            FileName = "cmd.exe",
            Arguments = $"/c {cmd}",
            UseShellExecute = false,       //是否使用操作系统shell启动
            RedirectStandardInput = false,  //接受来自调用程序的输入信息
            RedirectStandardOutput = true, //由调用程序获取输出信息
            RedirectStandardError = true,  //重定向标准错误输出
            CreateNoWindow = true,         //不显示程序窗口
        };
        using (var proc = Process.Start(psi))
        {
            output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            return proc.ExitCode;
        }
    }

    /// <summary>
    /// 编译.net项目
    /// </summary>
    /// <param name="projectFile"></param>
    /// <param name="buildPath"></param>
    public static void BuildProject(string projectFile, string buildPath)
    {
        if (!Directory.Exists(buildPath))
            Directory.CreateDirectory(buildPath);
        var cmd = $"dotnet build {projectFile} -c Debug -o {buildPath}";
        var result = ExecuteReturn(cmd);
        if (result.ExitCode != 0)
            throw new Exception($"编译项目失败：{projectFile}{Environment.NewLine}{result.Error}");

        var file = Path.Combine(buildPath, $"{Path.GetFileNameWithoutExtension(projectFile)}.xml");
        if (!File.Exists(file))
        {
            var xml = new XmlWrapper(projectFile);
            file = xml.GetInnerText("/Project/PropertyGroup/DocumentationFile");
            if (!string.IsNullOrEmpty(file))
            {
                file = Path.Combine(Path.GetDirectoryName(projectFile), file);
                if (File.Exists(file))
                {
                    var filename = Path.GetFileName(file);
                    File.Copy(file, Path.Combine(buildPath, filename), true);
                }
            }
        }

        Console.WriteLine($"项目编译成功：{projectFile} => {buildPath}");
    }
    public static void InstallNpmPackage(string cmd, string package, bool enforce=false)
    {
        var filePath = Environment.ExpandEnvironmentVariables($@"%USERPROFILE%\AppData\Roaming\npm\{cmd}");
        if (File.Exists(filePath) && !enforce)
        {
            Console.WriteLine($"NPM安装包{package}已存在");
            return;
        }
        var npmPath = @"C:\Program Files\nodejs\npm";
        if (!File.Exists(npmPath))
            throw new Exception($"npm在 {npmPath} 位置没找到");

        /*
        var cmdLine = $"npm install {package} -g";
        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {cmdLine}",
            UseShellExecute = true,       //是否使用操作系统shell启动
            RedirectStandardInput = false,  //接受来自调用程序的输入信息
            RedirectStandardOutput = false, //由调用程序获取输出信息
            RedirectStandardError = false,  //重定向标准错误输出
            CreateNoWindow = true,         //不显示程序窗口
        };
        using (var proc = Process.Start(psi))
        {
            //proc.StandardInput.WriteLine($"{cmd} & exit");
            //proc.StandardInput.AutoFlush = true;
        }
        int wait = 0;
        while (!File.Exists(filePath))
        {
            wait += 1000;
            Thread.Sleep(1000);
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"等待NPM安装包{package}安装...时间: {TimeSpan.FromMilliseconds(wait).ToString("c")}");
        }

        Console.WriteLine($"NPM安装包{package}安装成功");
        */
        var result = ExecuteWinCmd($"npm install {package} -g", out string output);
        if (result != 0)
            throw new Exception($"npm install {package} 出错：{output}");
        else
            Console.WriteLine(output);
        Console.WriteLine($"NPM安装包{package}安装成功");
    }
}
public enum CliStatus
{
    Start,
    Output,
    Error,
    Timeout,
    Exited,
    Completed
}