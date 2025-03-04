using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AiUo.Logging;
using AiUo.Net;

namespace AiUo.Extensions.SshNet;

public class SshClientEx : IDisposable
{
    #region Constructors
    public ConnectionInfo ConnectionInfo { get; }
    public SshClientEx(string host, string username, string password)
        : this(host, 22, username, password)
    { }
    public SshClientEx(string host, string username, string password, SshPasswordMode mode)
        : this(host, 22, username, password, mode)
    { }
    public SshClientEx(string host, int port, string username, string password, SshPasswordMode mode)
    {
        switch (mode)
        {
            case SshPasswordMode.Password:
                ConnectionInfo = new PasswordConnectionInfo(host, port, username, password);
                break;
            case SshPasswordMode.PrivateKey:
                ConnectionInfo = GetConnectionInfo(password);
                break;
            case SshPasswordMode.PrivateKeyFile:
                ConnectionInfo = GetConnectionInfo(File.ReadAllText(password));
                break;
        }
        ConnectionInfo GetConnectionInfo(string pwd)
        {
            var keyFile = new PrivateKeyFile(new MemoryStream(Encoding.UTF8.GetBytes(pwd)));
            return new PrivateKeyConnectionInfo(host, port, username, keyFile);
        }
    }

    public SshClientEx(string host, string username, params PrivateKeyFile[] keyFiles)
        : this(host, 22, username, keyFiles)
    { }
    public SshClientEx(string host, int port, string username, string password)
    {
        ConnectionInfo = new PasswordConnectionInfo(host, port, username, password);
    }
    public SshClientEx(string host, int port, string username, params PrivateKeyFile[] keyFiles)
    {
        ConnectionInfo = new PrivateKeyConnectionInfo(host, port, username, keyFiles);
    }
    #endregion

    #region GetClient
    private SshClient _sshClient;
    public SshClient GetSshClient(bool connect = true)
    {
        if (_sshClient == null)
            _sshClient = new SshClient(ConnectionInfo);
        if (connect && !_sshClient.IsConnected)
            _sshClient.Connect();
        return _sshClient;
    }
    private SftpClient _sftpClient;
    public SftpClient GetSftpClient(bool connect = true)
    {
        if (_sftpClient == null)
            _sftpClient = new SftpClient(ConnectionInfo);
        if (connect && !_sftpClient.IsConnected)
            _sftpClient.Connect();
        return _sftpClient;
    }
    private ScpClient _scpClient;
    public ScpClient GetScpClient(bool connect = true)
    {
        if (_scpClient == null)
            _scpClient = new ScpClient(ConnectionInfo);
        if (connect && !_scpClient.IsConnected)
            _scpClient.Connect();
        return _scpClient;
    }
    #endregion

    #region Upload & Download
    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="localFile"></param>
    /// <param name="serverPath">服务器文件或路径，是否包含文件名，根据hasFileName参数</param>
    /// <param name="hasFileName">serverPath是否包含文件名。true:路径包含文件名。false:路径不包含文件名</param>
    public void UploadFile(string localFile, string serverPath, bool hasFileName = false)
    {
        if (!hasFileName)
        {
            var name = Path.GetFileName(localFile);
            serverPath = $"{serverPath.TrimEnd('/')}/{name}";
        }
        serverPath = GetServerPath(serverPath);
        GetScpClient().Upload(new FileInfo(localFile), serverPath);
    }
    public void UploadFile(Stream stream, string serverPath)
    {
        GetScpClient().Upload(stream, GetServerPath(serverPath));
    }
    /// <summary>
    /// 上传本地目录下的所有文件（夹）到指定服务器目录(必须存在)
    /// </summary>
    /// <param name="localPath"></param>
    /// <param name="serverPath">远程目录的相对或绝对路径</param>
    public void UploadDir(string localPath, string serverPath)
    {
        GetScpClient().Upload(new DirectoryInfo(localPath), GetServerPath(serverPath));
    }
    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="serverFile">服务器文件</param>
    /// <param name="localPath">本地路径或文件，是否包含文件名根据hasFileName参数</param>
    /// <param name="hasFileName">localPath是否包含文件名。true:路径包含文件名。false:路径不包含文件名</param>
    public void DownloadFile(string serverFile, string localPath, bool hasFileName = false)
    {
        if (!hasFileName)
        {
            var name = Path.GetFileName(serverFile);
            localPath = Path.Combine(localPath, name);
        }
        GetScpClient().Download(GetServerPath(serverFile), new FileInfo(localPath));
    }

    public Stream DownloadFile(string serverFile)
    {
        var ret = new MemoryStream();
        GetScpClient().Download(GetServerPath(serverFile), ret);
        return ret;
    }

    /// <summary>
    /// 下载指定服务器目录下的所有文件(夹)到本地指定目录
    /// </summary>
    /// <param name="serverPath"></param>
    /// <param name="localPath"></param>
    public void DownloadDir(string serverPath, string localPath)
    {
        GetScpClient().Download(GetServerPath(serverPath), new DirectoryInfo(localPath));
    }
    #endregion

    public string GetHomePath()
        => ExecuteCmd("echo $HOME").Result.TrimEnd('\n');
    public string GetServerPath(string serverPath)
        => serverPath.Replace("~", GetHomePath());
    public ResponseResult<string> ExecuteCmd(string commandText)
    {
        ResponseResult<string> ret = new ResponseResult<string>();
        using (var cmd = GetSshClient().RunCommand(commandText))
        {
            if (cmd.ExitStatus == 0 && string.IsNullOrEmpty(cmd.Error))
            {
                ret.Success = true;
                ret.Result = cmd.Result;
                ret.Message = cmd.Result;
                ret.Code = cmd.ExitStatus.ToString();
            }
            else
            {
                ret.Success = false;
                ret.Message = cmd.Result + Environment.NewLine + cmd.Error;
                ret.Code = cmd.ExitStatus.ToString();
            }
        }
        return ret;
    }
    public void ExecuteStream(List<string> commands, Action<string> receive = null)
    {
        string endLine = "$ExecStreamEnd$";
        using (var stream = GetSshClient().CreateShellStream("xterm", 80, 50, 1024, 1024, 1024))
        {
            for (int i = 0; i < commands.Count; i++)
            {
                Thread.Sleep(100);
                stream.WriteLine(commands[i]);
                if (receive != null)
                {
                    Thread.Sleep(100);
                    stream.WriteLine($"echo '{endLine}'");
                    string line;
                    while ((line = stream.ReadLine()) != endLine)
                    {
                        receive(line);
                    }
                }
            }
        }
    }

    public Task<ResponseResult<string>> ExecuteCmdAsync(string commandText)
    {
        return Task.Run(() =>
        {
            return ExecuteCmd(commandText);
        });
    }

    #region IDisposable
    public void Disconnect()
    {
        _sshClient?.Disconnect();
        _sftpClient?.Disconnect();
        _scpClient?.Disconnect();
    }

    private bool _disposed;
    public void Dispose()
    {
        if (!_disposed)
        {
            _sshClient?.Dispose();
            _sftpClient?.Dispose();
            _scpClient?.Dispose();
            GC.SuppressFinalize(this);
            _disposed = true;
        }
    }
    ~SshClientEx()
    {
        Dispose();
        LogUtil.Error("SshClientEx没有释放资源Disconnect。host:{host}", ConnectionInfo.Host);
    }
    #endregion
}
public enum SshPasswordMode
{
    Password,
    PrivateKey,
    PrivateKeyFile
}