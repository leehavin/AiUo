using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.IO;

namespace AiUo.Net;

/// <summary>
/// FTP客户端类，用于简单连接FTP服务器
/// 例如：
/// FtpClient ftp = new FtpClient("172.28.19.61", 21, "Administrator", "create");
/// ftp.UploadFile("/1.txt", "d:\1.txt");
/// </summary>
public class FtpClient
{
    #region Properties
    private NetworkCredential _credential;
    /// <summary>
    /// 获取FTP服务器地址: 192.168.0.1或机器名
    /// </summary>
    public string Server { get; private set; }
    /// <summary>
    /// 获取FTP端口
    /// </summary>
    public int Port { get; private set; }
    /// <summary>
    /// 获取FTP帐户名
    /// </summary>
    public string Username { get; private set; }
    /// <summary>
    /// 获取FTP帐户密码
    /// </summary>
    public string Password { get; private set; }
    /// <summary>
    /// 获取或设置指定文件传输的数据类型，默认为true
    /// </summary>
    public bool UseBinary { get; set; } = true;
    /// <summary>
    /// 获取或设置请求完成后是否关闭连接，默认为true
    /// </summary>
    public bool KeepAlive { get; set; } = true;
    /// <summary>
    /// 获取或设置是否使用被动模式(PASV)[主动模式(PORT)]，默认为true
    /// </summary>
    public bool UsePassive { get; set; } = true;
    /// <summary>
    /// 获取或设置 Boolean，它指定是否使用 SSL 连接。默认为false
    /// </summary>
    public bool EnableSsl { get; set; } = false;
    /// <summary>
    /// 获取或设置用于建立到 FTP 服务器的加密连接的证书
    /// </summary>
    public X509Certificate2 ClientCertificate { get; set; }
    /// <summary>
    /// 获取或设置上传下载时缓存大小，默认2048
    /// </summary>
    public int BufferSize { get; set; } = 2048;
    /// <summary>
    /// 字符集
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <summary>
    /// 获取或设置等待请求的毫秒数。默认30秒
    /// </summary>
    public int Timeout { get; set; } = 30000;

    /// <summary>
    /// 当前服务器路径
    /// 操作时都是基于此路径之上的，如：当前路径/root 调用GetFileInfo("/mydir/1.txt")时最终访问的是服务器/root/mydir/1.txt
    /// </summary>
    public string CurrentPath { get; set; } = "/";
    #endregion

    #region Constructors

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="server">FTP服务器地址</param>
    /// <param name="port">FTP端口,FTP默认端口是21</param>
    /// <param name="userName">FTP帐户名</param>
    /// <param name="password">FTP帐户密码</param>
    public FtpClient(string server, string userName= "anonymous", string password="", int port = 21)
    {
        Server = server;
        Port = port;
        Username = userName;
        Password = password;
        _credential = new NetworkCredential(userName, password);
    }
    #endregion 

    #region Common Methods
    /// <summary>
    /// 获得文件大小
    /// </summary>
    /// <param name="path">FTP相对路径，如：/root/1.txt</param>
    /// <returns></returns>
    public long GetFileSize(string path)
    {
        long ret = -1;
        using (FtpWebResponse response = GetResponse(path, WebRequestMethods.Ftp.GetFileSize))
        {
            response.GetResponseStream();
            ret = response.ContentLength;
        }
        return ret;
    }

    /// <summary>
    /// 获得指定路径的文件信息
    /// </summary>
    /// <param name="path">FTP相对路径，如：/root/1.txt</param>
    /// <returns></returns>
    public FtpFileInfo GetFileInfo(string path)
    {
        FtpFileInfo ret = null;
        using (FtpWebResponse response = GetResponse(path, WebRequestMethods.Ftp.ListDirectoryDetails))
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding))
            {
                ret = GetFileInfoBase(reader.ReadLine());
            }
        }
        return ret;
    }

    /// <summary>
    /// 获得指定路径的FTP详细信息列表
    /// </summary>
    /// <param name="path">FTP相对路径，如：/root/mydir</param>
    /// <returns></returns>
    public IEnumerable<FtpFileInfo> List(string path)
    {
        using (FtpWebResponse response = GetResponse(path, WebRequestMethods.Ftp.ListDirectoryDetails))
        {
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding))
            {
                string line = reader.ReadLine();
                while (line != null)
                {
                    FtpFileInfo info = GetFileInfoBase(line);
                    line = reader.ReadLine();
                    yield return info;
                }
            }
        }
    }

    /// <summary>
    /// 判断指定的路径是否是文件
    /// </summary>
    /// <param name="path">FTP相对路径，如：/root/1.txt</param>
    /// <returns></returns>
    public bool IsFile(string path)
        => path.Contains('.');

    /// <summary>
    /// 判断文件或目录是否存在
    /// </summary>
    /// <param name="path">FTP相对路径，如：/root/1.txt或/root/mydir</param>
    /// <returns></returns>
    public bool IsExist(string path)
    {
        bool ret = true;
        try
        {
            using (FtpWebResponse response = GetResponse(path, WebRequestMethods.Ftp.ListDirectory))
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding))
                {
                    try
                    {
                        string line = reader.ReadLine();
                    }
                    catch
                    {
                        if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            ret = false;
                        }
                        else throw;
                    }
                }
            }
        }
        catch (WebException exp)
        {
            FtpWebResponse currResponse = (FtpWebResponse)exp.Response;
            if (currResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
            {
                ret = false;
            }
            else throw;
        }
        return ret;
    }

    /// <summary>
    /// 创建目录
    /// </summary>
    /// <param name="path">FTP相对路径，如：/root/mydir</param>
    /// <param name="isMakeParent">是否创建父路径</param>
    public void MakeDir(string path, bool isMakeParent = false)
    {
        if (path != "/")
        {
            if (isMakeParent)
            {
                string parentPath = GetParentPath(path);
                if (!IsExist(parentPath))
                    MakeDir(parentPath, true);
            }
            ExecuteNonQuery(path, WebRequestMethods.Ftp.MakeDirectory);
        }
    }
    /// <summary>
    /// 修改目录或文件的名称
    /// </summary>
    /// <param name="oldName">旧名称，必须包含FTP相对路径，如：/root/old.txt或/root/mydir</param>
    /// <param name="newName">新名称，可包含路径（意味着移动），如：/newdir/new.txt或new.txt</param>
    public void Rename(string oldName, string newName)
    {
        oldName = CleanPath(oldName);
        newName = CleanPath(newName);

        FtpWebRequest request = GetRequest(oldName, WebRequestMethods.Ftp.Rename);
        request.RenameTo = newName;
        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse()) { }
    }

    /// <summary>
    /// 移动目录或文件
    /// </summary>
    /// <param name="oldName">源名称，必须包含FTP相对路径，如：/root/1.txt 或/root/source</param>
    /// <param name="newName">新名称，包含路径和目标名称，如：/newroot/1.txt或/newroot/source</param>
    public void Move(string oldName, string newName)
        => Rename(oldName, newName);
    #endregion //Common Methods

    #region Delete
    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="path">FTP相对路径，如：/root/1.txt</param>
    public void DeleteFile(string path)
        => ExecuteNonQuery(path, WebRequestMethods.Ftp.DeleteFile);

    /// <summary>
    /// 删除目录包括子目录
    /// </summary>
    /// <param name="path">FTP相对路径，如：/root/mydir</param>
    public void DeleteDir(string path)
    {
        foreach (FtpFileInfo item in List(path))
        {
            if (item.IsDir)
                DeleteDir($"{path}/{item.Name}");
            else
                DeleteFile($"{path}/{item.Name}");
        }
        DeleteEmptyDir(path);
    }

    // 删除空目录
    private void DeleteEmptyDir(string path)
        => ExecuteNonQuery(path, WebRequestMethods.Ftp.RemoveDirectory);

    /// <summary>
    /// 删除文件或目录
    /// </summary>
    /// <param name="path">FTP相对路径，如：/root/1.txt或/root/mydir</param>
    public void Delete(string path)
    {
        if (IsFile(path))
            DeleteFile(path);
        else
            DeleteDir(path);
    }
    #endregion

    #region Upload

    /// <summary>
    /// 根据数据流上传文件
    /// </summary>
    /// <param name="path">FTP相对路径（包含文件名），如：/root/1.txt</param>
    /// <param name="input">文件数据流</param>
    public void UploadStream(string path, Stream input)
    {
        FtpWebRequest request = GetRequest(path, WebRequestMethods.Ftp.UploadFile);
        request.ContentLength = input.Length;
        byte[] buffer = new byte[BufferSize];
        using (Stream stream = request.GetRequestStream())
        {
            int readCount = input.Read(buffer, 0, BufferSize);
            while (readCount > 0)
            {
                stream.Write(buffer, 0, readCount);
                readCount = input.Read(buffer, 0, BufferSize);
            }
        }
    }

    /// <summary>
    /// 根据字节数组上传文件
    /// </summary>
    /// <param name="path">FTP相对路径（包含文件名），如：/root/1.txt</param>
    /// <param name="data">文件数据</param>
    public void UploadData(string path, byte[] data)
    {
        FtpWebRequest request = GetRequest(path, WebRequestMethods.Ftp.UploadFile);
        request.ContentLength = data.Length;
        using (Stream stream = request.GetRequestStream())
        {
            stream.Write(data, 0, data.Length);
        }
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="serverPath">FTP相对路径（包含文件名），如：/root/1.txt</param>
    /// <param name="localPath">本地文件路径（包含文件名），如：C:\1.txt</param>
    public void UploadFile(string serverPath, string localPath)
    {
        if (!File.Exists(localPath))
            throw new FileNotFoundException("FTP上传时文件未找到。", localPath);
        using (FileStream fs = new FileStream(localPath, FileMode.Open, FileAccess.Read))
        {
            UploadStream(serverPath, fs);
        }
    }

    /// <summary>
    /// 上传目录
    /// </summary>
    /// <param name="serverPath">FTP相对路径（包含目录名），如：/root/mydir</param>
    /// <param name="localPath">本地目录路径（包含目录名），如：C:\mydir</param>
    public void UploadDir(string serverPath, string localPath)
    {
        if (!Directory.Exists(localPath))
            throw new DirectoryNotFoundException(string.Format("FTP上传时目录{0}为找到。", localPath));
        MakeDir(serverPath);
        UploadDirBase(localPath, serverPath, localPath);

    }

    //递归上传
    private void UploadDirBase(string rootPath, string serverPath, string localPath)
    {
        FileSystemInfo[] subs = new DirectoryInfo(localPath).GetFileSystemInfos();
        foreach (FileSystemInfo sub in subs)
        {
            string currServerPath = serverPath + sub.FullName.Substring(rootPath.Length).Replace('\\', '/');
            if (sub is DirectoryInfo)
            {
                MakeDir(currServerPath);
                UploadDirBase(rootPath, serverPath, sub.FullName);
            }
            else
                UploadFile(currServerPath, sub.FullName);
        }
    }

    /// <summary>
    /// 上传文件或目录
    /// </summary>
    /// <param name="serverPath">FTP相对路径（包含文件名或目录名），如：/root/1.txt或/root/mydir</param>
    /// <param name="localPath">本地路径（包含文件名或目录名），如：C:\1.txt或C:\mydir</param>
    public void Upload(string serverPath, string localPath)
    {
        bool isFile = File.Exists(localPath);
        bool isDir = Directory.Exists(localPath);
        if (isFile)
            UploadFile(serverPath, localPath);
        else if (isDir)
            UploadDir(serverPath, localPath);
        else
            throw new Exception("文件或目录不存在.");
    }
    #endregion //Upload

    #region Download
    /// <summary>
    /// 下载文件的数据流
    /// </summary>
    /// <param name="path">FTP相对路径（包含文件名），如：/root/1.txt</param>
    /// <returns></returns>
    public Stream DownloadStream(string path)
    {
        using (FtpWebResponse response = GetResponse(path, WebRequestMethods.Ftp.DownloadFile))
        {
            return response.GetResponseStream();
        }
    }

    /// <summary>
    /// 下载文件的字节数组
    /// </summary>
    /// <param name="path">FTP相对路径（包含文件名），如：/root/1.txt</param>
    /// <returns></returns>
    public byte[] DownloadData(string path)
    {
        using (Stream stream = DownloadStream(path))
        {
            byte[] ret = new byte[stream.Length];
            stream.Read(ret, 0, ret.Length);
            return ret;
        }
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="serverPath">FTP相对路径（包含文件名），如：/root/1.txt</param>
    /// <param name="localPath">本地文件路径（包含文件名），如：C:\1.txt</param>
    public void DownloadFile(string serverPath, string localPath)
    {
        using (FtpWebResponse response = GetResponse(serverPath, WebRequestMethods.Ftp.DownloadFile))
        {
            using (Stream sourceStream = response.GetResponseStream())
            {
                using (FileStream outputStream = new FileStream(localPath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[BufferSize];
                    int readCount = sourceStream.Read(buffer, 0, BufferSize);
                    while (readCount > 0)
                    {
                        outputStream.Write(buffer, 0, readCount);
                        readCount = sourceStream.Read(buffer, 0, BufferSize);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 下载目录
    /// </summary>
    /// <param name="serverPath">FTP相对路径（包含目录名），如：/root/mydir</param>
    /// <param name="localPath">本地目录路径（包含目录名），如：C:\mydir</param>
    public void DownloadDir(string serverPath, string localPath)
    {
        if (!Directory.Exists(localPath))
            Directory.CreateDirectory(localPath);
        DownloadDirBase(serverPath, localPath);
    }

    /// <summary>
    /// 递归下载目录 
    /// /aaa/bbb   /aaa/bbb --> c:\abc\bbb
    /// </summary>
    /// <param name="serverPath"></param>
    /// <param name="localPath"></param>
    public void DownloadDirBase(string serverPath, string localPath)
    {
        foreach (FtpFileInfo sub in List(serverPath))
        {
            string currFtpPath = $"{serverPath}/{sub.Name}";
            string currLocalPath = $"{localPath}\\{sub.Name}";
            if (sub.IsDir)
            {
                if (!Directory.Exists(currLocalPath))
                    Directory.CreateDirectory(currLocalPath);
                DownloadDirBase(currFtpPath, currLocalPath);
            }
            else
            {
                DownloadFile(currFtpPath, currLocalPath);
            }
        }
    }

    /// <summary>
    /// 下载文件或目录
    /// </summary>
    /// <param name="serverPath">FTP相对路径（包含文件名或目录名），如：/root/1.txt或/root/mydir</param>
    /// <param name="localPath">本地路径（包含文件名或目录名），如：C:\1.txt或C:\mydir</param>
    public void Download(string serverPath, string localPath)
    {
        if (IsFile(serverPath))
            DownloadFile(serverPath, localPath);
        else
            DownloadDir(serverPath, localPath);
    }
    #endregion //Download

    #region Utils

    //获得FtpWebRequest
    private FtpWebRequest GetRequest(string path, string method)
    {
        Uri uri = new Uri(GetFtpPath(path));
        if (uri.Scheme != Uri.UriSchemeFtp)
            throw new ArgumentException("FTP的路径错误。", "path");
#pragma warning disable SYSLIB0014 // 类型或成员已过时
        FtpWebRequest ret = (FtpWebRequest)FtpWebRequest.Create(uri);
#pragma warning restore SYSLIB0014 // 类型或成员已过时
        ret.Credentials = _credential;
        ret.KeepAlive = KeepAlive;
        ret.UseBinary = UseBinary;
        ret.Method = method;
        ret.UsePassive = UsePassive;
        ret.EnableSsl = EnableSsl;
        if (ClientCertificate != null)
        {
            ret.ClientCertificates.Clear();
            ret.ClientCertificates.Add(ClientCertificate);
        }
        //ret.ReadWriteTimeout = -1;//支持大文件传输
        ret.Timeout = Timeout;
        return ret;
    }
    //获得FtpWebResponse
    private FtpWebResponse GetResponse(string path, string method)
        => (FtpWebResponse)GetRequest(path, method).GetResponse();

    //执行不需要返回的FTP操作
    private void ExecuteNonQuery(string path, string method)
    {
        using (FtpWebResponse response = GetResponse(path, method)) { }
    }
    /// <summary>
    /// 获得FTP全路径
    /// </summary>
    /// <param name="path">相对路径</param>
    /// <returns></returns>
    public string GetFtpPath(string path)
    {
        path = CleanPath(path);
        if (CurrentPath != "/")
        {
            path = CurrentPath + path;
        }
        return $"ftp://{Server}:{Port}{path}";
    }
    //获得相对ftp父路径，如果为根则返回null
    private string GetParentPath(string path)
    {
        string ret = null;
        path = CleanPath(path);
        if (path != "/")
            ret = path.Substring(0, path.LastIndexOf('/'));
        return ret;
    }
    //整理Path保证除了/以外其他路径都是/aaa/bb或者/aaa/bb.txt格式
    private string CleanPath(string path)
    {
        string ret = null;
        ret = path.Trim().Replace('\\', '/');
        if (ret != "/")
        {
            if (ret[0] != '/')
                ret = "/" + ret.TrimEnd('/');
            else
                ret = ret.TrimEnd('/');
        }
        return ret;
    }

    //根据ListDirectoryDetails返回的详细信息,获得FtpFileInfo对象
    private FtpFileInfo GetFileInfoBase(string line)
    {
        /*
        04-23-08  06:15PM       <DIR>          1
        04-23-08  06:15PM       <DIR>          2
        09-20-07  05:39PM                 2375 TraceLog.asp
        */
        string date = line.Substring(0, 17).Trim();
        string dir = line.Substring(17, 12).Trim();
        string size = line.Substring(29, 9).Trim();
        string name = line.Substring(38).Trim();

        FtpFileInfo ret = new FtpFileInfo();
        ret.ModifyDate = DateTime.ParseExact(date, "MM-dd-yy  hh:mmtt", System.Globalization.CultureInfo.InvariantCulture);
        ret.IsDir = (dir == "<DIR>");
        ret.Size = string.IsNullOrEmpty(size) ? 0 : long.Parse(size);
        ret.Name = name;
        return ret;
    }

    /// <summary>
    /// 获得文件大小字符串表示，以KB MB GB结尾
    /// </summary>
    /// <param name="fileSize"></param>
    /// <returns></returns>
    public static String FormatFileSize(long fileSize)
    {
        if (fileSize < 0)
        {
            throw new ArgumentOutOfRangeException("fileSize");
        }
        else if (fileSize >= 1073741824) // 1073741824 = 1024 * 1024 * 1024
        {
            return string.Format("{0:########0.00} GB", ((Double)fileSize) / 1073741824);
        }
        else if (fileSize >= 1048576) // 1048576 = 1024 * 1024
        {
            return string.Format("{0:####0.00} MB", ((Double)fileSize) / 1048576);
        }
        else if (fileSize >= 1024)
        {
            return string.Format("{0:####0.00} KB", ((Double)fileSize) / 1024);
        }
        else
        {
            return string.Format("{0} bytes", fileSize);
        }
    }
    //获得FTP路径中的文件名
    private string GetFileName(string path)
        => path.Substring(path.LastIndexOf('/') + 1);
    #endregion //Utils

}

#region FtpFileInfo

/// <summary>
/// FTP服务器列表文件信息
/// </summary>
[Serializable]
public class FtpFileInfo
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 文件大小
    /// </summary>
    public long Size { get; set; }
    /// <summary>
    /// 是否是文件夹
    /// </summary>
    public bool IsDir { get; set; }
    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime ModifyDate { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    public FtpFileInfo() { }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="name">文件名</param>
    /// <param name="size">文件大小</param>
    /// <param name="isDir">是否是文件夹</param>
    /// <param name="modifyDate">修改时间</param>
    public FtpFileInfo(string name, long size, bool isDir, DateTime modifyDate)
    {
        Name = name;
        Size = size;
        IsDir = isDir;
        ModifyDate = modifyDate;
    }
}
#endregion