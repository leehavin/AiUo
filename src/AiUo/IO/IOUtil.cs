using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace AiUo.IO;

/// <summary>
/// IO辅助类
/// file    => string   : File.ReadAllText
/// file    => stream   : IOUtil.ReadFileToStream
/// file    => bytes    : File.ReadAllBytes
/// string  => file     : File.WriteAllText
/// string  => stream   : IOUtil.ReadStreamToString
/// string  => bytes    : Encoding.UTF8.GetBytes
/// stream  => string   : IOUtil.ReadStreamToString
/// stream  => file     : IOUtil.WriteStreamToFile
/// stream  => bytes    : IOUtil.ReadStreamToBytes
/// bytes   => stream   : new MemoryStream(bytes)
/// bytes   => string   : Encoding.UTF8.GetString
/// bytes   => file     : File.WriteAllBytes
/// </summary>
public static class IOUtil
{
    /// <summary>
    /// 读取 Stream 到 byte[]
    /// </summary>
    /// <param name="stream">需要读取的Stream</param>
    /// <param name="isClose">读取完是否关闭Stream</param>
    /// <returns></returns>
    public static byte[] ReadStreamToBytes(Stream stream, bool isClose = false)
    {
        byte[] ret = new byte[stream.Length];
        try
        {
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(ret, 0, (int)stream.Length);
        }
        finally
        {
            if (isClose) stream.Close();
            else stream.Seek(0, SeekOrigin.Begin);
        }
        return ret;
    }

    /// <summary>
    /// 将 Stream 写入文件 
    /// </summary>
    /// <param name="stream">流对象</param>
    /// <param name="path">文件名</param>
    /// <param name="encoding">Stream的字符集</param>
    public static void WriteStreamToFile(Stream stream, string path, Encoding encoding = null)
    {
        byte[] buffer = new byte[2048];
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
        {
            BinaryReader reader = new BinaryReader(stream, encoding ?? Encoding.UTF8);
            int readLength;
            while ((readLength = reader.Read(buffer, 0, buffer.Length)) > 0)
                fs.Write(buffer, 0, readLength);
        }
    }

    /// <summary>
    /// 从文件中读取到MemoryStream
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Stream ReadFileToStream(string path)
    {
        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, (int)fs.Length);
            return new MemoryStream(buffer);
        }
    }

    /// <summary>
    /// 读取流到字符串
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string ReadStreamToString(Stream stream, Encoding encoding = null)
    {
        using (StreamReader reader = new StreamReader(stream, encoding ?? Encoding.UTF8))
        {
            stream.Position = 0;
            return reader.ReadToEnd();
        }
    }

    /// <summary>
    /// 替换文本文件内容
    /// </summary>
    /// <param name="file"></param>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    public static void ReplaceTextFileContent(string file, string oldValue, string newValue)
    {
        var content = File.ReadAllText(file);
        content.Replace(oldValue, newValue);
        File.WriteAllText(file, content);
    }

    /// <summary>
    /// 移除指定文件夹的只读属性(包含这个文件夹下的所有文件)
    /// </summary>
    /// <param name="destDirectoryPath">目标文件夹</param>
    public static void RemoveReadOnlyAttr(string destDirectoryPath)
    {
        Queue<FileSystemInfo> filefolders = new Queue<FileSystemInfo>(new DirectoryInfo(destDirectoryPath).GetFileSystemInfos());
        while (filefolders.Count > 0)
        {
            FileSystemInfo atom = filefolders.Dequeue();
            FileInfo file = atom as FileInfo;
            if (file == null)
            {
                DirectoryInfo directory = atom as DirectoryInfo;
                foreach (FileSystemInfo fi in directory.GetFileSystemInfos())
                    filefolders.Enqueue(fi);
            }
            else
                file.Attributes &= ~FileAttributes.ReadOnly;
        }
    }

    /// <summary>
    /// 判断路径是否是目录
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsDir(string path)
        => (new FileInfo(path).Attributes & FileAttributes.Directory) != 0;

    /// <summary>
    /// 删除目录中的所有
    /// </summary>
    /// <param name="targetDirectory"></param>
    public static void DeleteAll(string targetDirectory)
    {
        var dirInfo = new DirectoryInfo(targetDirectory);
        RecursiveDelete(dirInfo);
        // 递归
        void RecursiveDelete(DirectoryInfo baseDir)
        {
            if (!baseDir.Exists) return;
            foreach (var dir in baseDir.EnumerateDirectories())
                RecursiveDelete(dir);
            if (baseDir.Exists)
                baseDir.Delete(true);
        }
    }
    /// <summary>
    /// 查找目录下某文件，包含子目录
    /// </summary>
    /// <param name="path"></param>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static string FindFile(string path, string filename)
    {
        var helper = new FindFileHelper();
        helper.FindFile(path, filename);
        return helper.Result;
    }
    private class FindFileHelper
    {
        public string Result { get; set; }
        public void FindFile(string path, string filename)
        {
            if (Result != null)
                return;
            var dir = new DirectoryInfo(path);
            var file = dir.GetFiles().FirstOrDefault(f => f.Name == filename);
            if (file != null)
            {
                Result = file.FullName;
                return;
            }
            foreach (var subDir in dir.GetDirectories())
            {
                if (Result != null)
                    return;
                FindFile(subDir.FullName, filename);
            }
        }

    }

    /// <summary>
    /// 复制目录
    /// </summary>
    /// <param name="sourceDir">源目录</param>
    /// <param name="destinationDir">目标目录</param>
    /// <param name="recursive">是否复制子目录</param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive = true)
    {
        var dir = new DirectoryInfo(sourceDir);
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"源目录未找到: {dir.FullName}");
        DirectoryInfo[] dirs = dir.GetDirectories();
        Directory.CreateDirectory(destinationDir);
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }

    /// <summary>
    /// 从Pipe中读取String
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static async Task<string> GetStringFromPipe(PipeReader reader)
    {
        StringBuilder ret = null;
        while (true)
        {
            ReadResult read = await reader.ReadAsync();
            var buffer = read.Buffer;
            if (buffer.IsEmpty && read.IsCompleted)
                break;
            ret = new StringBuilder();
            foreach (var segment in buffer)
            {
                var newData = Encoding.UTF8.GetString(segment.Span);
                ret.Append(newData);
            }
            reader.AdvanceTo(buffer.Start, buffer.End);
            if (read.IsCompleted)
            {
                break;
            }
        }
        return Convert.ToString(ret);
    }
}