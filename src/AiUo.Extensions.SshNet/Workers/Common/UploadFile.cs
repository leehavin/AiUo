using System.IO;
using AiUo.IO;

namespace AiUo.Extensions.SshNet.Workers;

public class UploadFile : WorkerBase
{
    public Stream Stream { get; set; }
    public string FileName { get; set; }
    public string ServerDir { get; set; }
    public string ServerPath { get; set; }
    public UploadFile(string localPath, string serverPath, SshClientEx client = null) : base(client)
    {
        Stream = IOUtil.ReadFileToStream(localPath);
        var srvFileName = Path.GetFileName(serverPath);
        FileName = string.IsNullOrEmpty(srvFileName) ? Path.GetFileName(localPath) : srvFileName;
        ServerDir = Path.GetDirectoryName(ServerPath);
        ServerPath = Path.Combine(ServerDir, FileName);
    }

    public UploadFile(Stream stream, string serverPath, string fileName = null, SshClientEx client = null) : base(client)
    {
        Stream = stream;
        FileName = string.IsNullOrEmpty(fileName) ? Path.GetFileName(serverPath) : fileName;
        ServerDir = Path.GetDirectoryName(serverPath).Replace('\\','/');
        ServerPath = Path.Combine(ServerDir, FileName).Replace('\\', '/');
    }
    public override void Execute()
    {
        OutputText($"上传 {ServerPath} 开始", true);
        Client.UploadFile(Stream, ServerPath);
        OutputText($"上传 {ServerPath} 完成", true);
    }
}